using System.CommandLine.Invocation;
using System.CommandLine;
using System.Text;

namespace Cs2Mermaid;

class MyCommandHandler : ICommandHandler
{
    Option<string> outputoption = new Option<string>(new string[]{ "--output", "-o" }, "output file(default stdout)");
    Option<string> inputoption = new Option<string>(new string[]{ "-i", "--input" }, "input file(default stdin)");
    Option<string> inputEncodingOption = new Option<string>(new string[]{ "--input-encoding", "-ie", }, "input text encoding(default UTF8)");
    Option<string> outputEncodingOption = new Option<string>(new string[]{ "--output-encoding", "-oe", }, "output text encoding(default UTF8 no BOM)");
    Option<bool> withMdOption = new Option<bool>(new string[] { "--md-with-source" }, "output markdown format with source");
    Option<string> langVersion = new Option<string>(new string[] { "--lang-version" }, "C# language version");
    Option<bool> showAvailableVersion = new Option<bool>("--available-version", "output available C# version and exit");
    public Option[] GetOptions()
    {
        return new Option[]
        {
            outputoption,
            inputoption,
            inputEncodingOption,
            outputEncodingOption,
            withMdOption,
            langVersion,
            showAvailableVersion,
        };
    }
    public MyCommandHandler()
    {

    }
    public Task<int> InvokeAsync(InvocationContext context)
    {
        var showAvailable = context.ParseResult.GetValueForOption<bool>(showAvailableVersion);
        if(showAvailable)
        {
            foreach(var ver in Cs2Mermaid.ConvertCsToMermaid.AvailableVersions())
            {
                context.Console.WriteLine(ver);
            }
            return Task.FromResult(1);
        }
        var output = context.ParseResult.GetValueForOption<string>(outputoption);
        var outputEncoding = context.ParseResult.GetValueForOption<string>(outputEncodingOption);
        var input = context.ParseResult.GetValueForOption<string>(inputoption);
        var inputEncoding = context.ParseResult.GetValueForOption<string>(inputEncodingOption);
        var withMd = context.ParseResult.GetValueForOption<bool>(withMdOption);
        if(withMd)
        {
            ProcessWithMd(output, outputEncoding, input, inputEncoding);
        }
        else
        {
            ProcessNoMd(output, outputEncoding, input, inputEncoding);
        }
        return Task.FromResult(1);
    }
    void ProcessWithMd(string? output, string? outputEncoding, string? input, string? inputEncoding)
    {
        var ie = GetEncoding(inputEncoding);
        string sourceText = "";
        using(var inputstm = CreateInputStream(input))
        using(var tr = new StreamReader(inputstm, ie))
        {
            sourceText = tr.ReadToEnd();
        }
        sourceText = sourceText ?? "";
        using(var tr = new StringReader(sourceText))
        using(var outputstm = CreateOutputStream(output))
        using(var tw = new StreamWriter(outputstm, GetEncoding(outputEncoding)))
        {
            tw.WriteLine("```csharp");
            tw.WriteLine(sourceText);
            tw.WriteLine("```");
            tw.WriteLine();
            tw.WriteLine("```mermaid");
            new Cs2Mermaid.ConvertCsToMermaid().Convert(tr, tw);
            tw.WriteLine("```");
            tw.Flush();
        }
    }
    void ProcessNoMd(string? output, string? outputEncoding, string? input, string? inputEncoding)
    {
        using var inputstm = CreateInputStream(input);
        using var outputstm = CreateOutputStream(output);
        var ie = GetEncoding(inputEncoding);
        var oe = GetEncoding(outputEncoding);
        using var tr = new StreamReader(inputstm, ie);
        using var tw = new StreamWriter(outputstm, oe);
        new Cs2Mermaid.ConvertCsToMermaid().Convert(inputstm, ie, tw);
        tw.Flush();
    }
    Stream CreateInputStream(string? input)
    {
        if(string.IsNullOrEmpty(input))
        {
            return Console.OpenStandardInput();
        }
        else
        {
            return File.OpenRead(input);
        }
    }
    Stream CreateOutputStream(string? output)
    {
        if(string.IsNullOrEmpty(output))
        {
            return Console.OpenStandardOutput();
        }
        else
        {
            return File.Create(output);
        }
    }
    Encoding GetEncoding(string? name)
    {
        if(string.IsNullOrEmpty(name))
        {
            return new UTF8Encoding(false);
        }
        else
        {
            return Encoding.GetEncoding(name);
        }
    }
}

class Cs2MermaidOptions
{
    public string? Input { get; set; }
    public string? Output { get; set; }
    public string? InputEncoding { get; set; }
    public string? OUtputEncoding { get; set; }
    public bool? WithMd { get; set; }
}
