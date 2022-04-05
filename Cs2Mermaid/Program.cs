using System.Text;
using System.Buffers;
using System.CommandLine;
using System.CommandLine.Invocation;
using Cs2Mermaid;

var rootcmd = CreateCommand();

rootcmd.Invoke(args);

// var f = args[0];

// var enc = new UTF8Encoding(false);
// var txt = File.ReadAllText(args[0]);
// var src = File.OpenRead(args[0]);

// var fstm = File.Create("output.md");
// var sw = new StreamWriter(fstm, new UTF8Encoding(false));
// sw.WriteLine("```csharp");
// sw.WriteLine(txt);
// sw.WriteLine("```");
// sw.WriteLine();
// sw.WriteLine($"```mermaid");
// new Cs2Mermaid.ConvertCsToMermaid().Convert(src, enc, sw);
// sw.WriteLine(new Cs2Mermaid.ConvertCsToMermaid().Convert(txt));
// sw.WriteLine($"```");
// sw.Flush();

RootCommand CreateCommand()
{
    var rootcmd = new RootCommand("C# syntax to mermaid.js diagram");
    // var outputoption = new Option<string>(new string[]{ "--output", "-o" }, "output file(default stdout)");
    // rootcmd.Add(outputoption);
    // var inputoption = new Option<string>(new string[]{ "-i", "--input" }, "input file(default stdin)");
    // rootcmd.Add(inputoption);
    // var inputEncoding = new Option<string>(new string[]{ "--input-encoding", "-ie", }, "input text encoding(default UTF8)");
    // rootcmd.Add(inputEncoding);
    // var outputEncoding = new Option<string>(new string[]{ "--output-encoding", "-oe", }, "output text encoding(default UTF8 no BOM)");
    // rootcmd.Add(outputEncoding);
    // var withMd = new Option<bool>(new string[] { "--md-with-source" }, "output markdown format with source");
    // rootcmd.Add(withMd);
    var mycommand = new MyCommandHandler();
    foreach(var opt in mycommand.GetOptions())
    {
        rootcmd.Add(opt);
    }
    rootcmd.Handler = mycommand;
    return rootcmd;
}

