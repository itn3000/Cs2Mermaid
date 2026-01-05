using System.Text;
using ConsoleAppFramework;

using Cs2Mermaid;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
var app = ConsoleApp.Create();
app.Add<Command>();
await app.RunAsync(args);
