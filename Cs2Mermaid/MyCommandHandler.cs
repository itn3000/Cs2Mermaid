using System.CommandLine.Invocation;
using System.CommandLine;
using System.Text;

namespace Cs2Mermaid;

class MyCommandHandler : AsynchronousCommandLineAction
{
    Option<string> outputoption = new Option<string>(new string[] { "--output", "-o" }, "output file(default stdout)");
    Option<string> inputoption = new Option<string>(new string[] { "-i", "--input" }, "input file(default stdin)");
    Option<string> inputEncodingOption = new Option<string>(new string[] { "--input-encoding", "-ie", }, "input text encoding(default UTF8)");
    Option<string> outputEncodingOption = new Option<string>(new string[] { "--output-encoding", "-oe", }, "output text encoding(default UTF8 no BOM)");
    Option<bool> withMdOption = new Option<bool>(new string[] { "--md-with-source" }, "output markdown format with source");
    Option<string> langVersion = new Option<string>(new string[] { "--lang-version" }, "C# language version");
    Option<bool> showAvailableVersion = new Option<bool>("--available-version", "output available C# version and exit");
    Option<string[]> symbolOption = new Option<string[]>("--pp-symbol", "preprocessor symbol(can be multiple)");
    Option<string> orientationOption = new Option<string>(new string[] { "--chart-orientation", "-co" }, "flowchart orientation(default: LR)");
    Option<bool> asScriptOption = new Option<bool>("--as-script", "parse as C# script");
    Option<bool> outputDiagnosticsOption = new Option<bool>("--output-diagnostics", "output diagnostics to stderr");
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
            symbolOption,
            orientationOption,
            asScriptOption,
            outputDiagnosticsOption,
        };
    }
    public MyCommandHandler()
    {

    }
    public override Task<int> InvokeAsync(ParseResult parseResult, CancellationToken ct)
    {
        var showAvailable = parseResult.GetValue<bool>(showAvailableVersion);
        if (showAvailable)
        {
            foreach (var ver in Cs2Mermaid.ConvertCsToMermaid.AvailableVersions())
            {
                Console.WriteLine(ver);
            }
            return Task.FromResult(0);
        }
        try
        {
            var output = parseResult.GetValue<string>(outputoption);
            var outputEncoding = parseResult.GetValue<string>(outputEncodingOption);
            var input = parseResult.GetValue<string>(inputoption);
            var inputEncoding = parseResult.GetValue<string>(inputEncodingOption);
            var outputDiagnostics = parseResult.GetValue<bool>(outputDiagnosticsOption);
            var withMd = parseResult.GetValue<bool>(withMdOption);
            var diagnosticsWriter = outputDiagnostics switch
            {
                true => Console.Error,
                false => null
            };
            if (withMd)
            {
                ProcessWithMd(output, outputEncoding, input, inputEncoding, CreateConvertOptions(parseResult), diagnosticsWriter);
            }
            else
            {
                ProcessNoMd(output, outputEncoding, input, inputEncoding, CreateConvertOptions(parseResult), diagnosticsWriter);
            }
            return Task.FromResult(0);
        }
        catch (Exception e)
        {
            Console.Error.Write(e.ToString());
            Console.Error.Write(Environment.NewLine);
            return Task.FromResult(1);
        }
    }
    ConvertOptions CreateConvertOptions(ParseResult parseResult)
    {
        var presymbols = parseResult.GetValue<string[]>(symbolOption);
        var langver = parseResult.GetValue<string>(langVersion);
        var orientation = parseResult.GetValue<string>(orientationOption);
        var asscript = parseResult.GetValue<bool>(asScriptOption);
        return new ConvertOptions()
        {
            LangVersion = langver,
            PreprocessorSymbols = presymbols,
            ChartOrientation = orientation,
            AsScript = asscript,
        };
    }
    void ProcessWithMd(string? output, string? outputEncoding, string? input, string? inputEncoding, ConvertOptions convertOptions, TextWriter? diagnosticWriter)
    {
        var ie = GetEncoding(inputEncoding);
        string sourceText = "";
        using (var inputstm = CreateInputStream(input))
        using (var tr = new StreamReader(inputstm, ie))
        {
            sourceText = tr.ReadToEnd();
        }
        sourceText = sourceText ?? "";
        using (var tr = new StringReader(sourceText))
        using (var outputstm = CreateOutputStream(output))
        using (var tw = new StreamWriter(outputstm, GetEncoding(outputEncoding)))
        {
            tw.WriteLine("```csharp");
            tw.WriteLine(sourceText);
            tw.WriteLine("```");
            tw.WriteLine();
            tw.WriteLine("```mermaid");
            var diagnostics = ConvertCsToMermaid.Convert(tr, tw, convertOptions);
            tw.WriteLine("```");
            tw.Flush();
            if(diagnosticWriter != null)
            {
                foreach(var diag in diagnostics)
                {
                    diagnosticWriter.WriteLine(diag.ToString());
                }
            }
        }
    }
    void ProcessNoMd(string? output, string? outputEncoding, string? input, string? inputEncoding, ConvertOptions convertOptions, TextWriter? diagnosticWriter)
    {
        using var inputstm = CreateInputStream(input);
        using var outputstm = CreateOutputStream(output);
        var ie = GetEncoding(inputEncoding);
        var oe = GetEncoding(outputEncoding);
        using var tr = new StreamReader(inputstm, ie);
        using var tw = new StreamWriter(outputstm, oe);
        var diags = Cs2Mermaid.ConvertCsToMermaid.Convert(inputstm, ie, tw, convertOptions);
        tw.Flush();
        if(diagnosticWriter != null)
        {
            foreach(var diag in diags)
            {
                diagnosticWriter.WriteLine(diag.ToString());
            }
        }

    }
    Stream CreateInputStream(string? input)
    {
        if (string.IsNullOrEmpty(input))
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
        if (string.IsNullOrEmpty(output))
        {
            return Console.OpenStandardOutput();
        }
        else
        {
            var dirname = Path.GetDirectoryName(output);
            if(!string.IsNullOrEmpty(dirname) && !Directory.Exists(dirname))
            {
                Directory.CreateDirectory(dirname);
            }
            return File.Create(output);
        }
    }
    Encoding GetEncoding(string? name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return new UTF8Encoding(false);
        }
        else
        {
            return Encoding.GetEncoding(name);
        }
    }
}
