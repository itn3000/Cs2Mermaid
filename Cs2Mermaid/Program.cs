using System.Text;
using ConsoleAppFramework;

//using System.Buffers;
//using System.CommandLine;
//using System.CommandLine.Invocation;
using Cs2Mermaid;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
var app = ConsoleApp.Create();
await app.RunAsync(args);

//var rootcmd = CreateCommand();

//var retcode = rootcmd.Invoke(args);

//Environment.ExitCode = retcode;

//RootCommand CreateCommand()
//{
//    var rootcmd = new RootCommand("C# syntax to mermaid.js diagram");
//    var mycommand = new MyCommandHandler();
//    foreach(var opt in mycommand.GetOptions())
//    {
//        rootcmd.Add(opt);
//    }
//    rootcmd.Handler = mycommand;
//    return rootcmd;
//}

