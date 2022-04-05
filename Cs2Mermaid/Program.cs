using System.Text;

var f = args[0];

var txt = File.ReadAllText(f, Encoding.UTF8);
var fstm = File.Create("output.md");
var sw = new StreamWriter(fstm, new UTF8Encoding(false));

sw.WriteLine("```csharp");
sw.WriteLine(txt);
sw.WriteLine("```");
sw.WriteLine();
sw.WriteLine($"```mermaid");
sw.WriteLine(new Cs2Mermaid.ConvertCsToMermaid().Convert(txt));
sw.WriteLine($"```");
sw.Flush();