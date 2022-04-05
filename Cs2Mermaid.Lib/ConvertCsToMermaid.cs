using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using System.IO;
namespace Cs2Mermaid;

public class ConvertCsToMermaid
{
    public static string[] AvailableVersions()
    {
        return Enum.GetNames<LanguageVersion>();
    }
    public void Convert(Stream input, Encoding inputEncoding, TextWriter tw)
    {
        var src = SourceText.From(input, inputEncoding);
        var node = CSharpSyntaxTree.ParseText(src);
        var rootNode = node.GetRoot();
        tw.WriteLine("flowchart LR");
        var kinds = new Dictionary<string, int>();
        var mermaidNodeName = GetMermaidNodeName(rootNode.Kind(), kinds);
        ConvertInternal(tw, mermaidNodeName, rootNode, 1, kinds);
    }

    public void Convert(TextReader tr, TextWriter tw)
    {
        var src = SourceText.From(tr.ReadToEnd());
        var node = CSharpSyntaxTree.ParseText(src);
        var rootNode = node.GetRoot();
        tw.WriteLine("flowchart LR");
        var kinds = new Dictionary<string, int>();
        var mermaidNodeName = GetMermaidNodeName(rootNode.Kind(), kinds);
        ConvertInternal(tw, mermaidNodeName, rootNode, 1, kinds);
    }
    public string Convert(string code)
    {
        var node = CSharpSyntaxTree.ParseText(code);
        var rootNode = node.GetRoot();
        var sb = new StringBuilder();
        var sw = new StringWriter(sb);
        sw.WriteLine("flowchart LR");
        var kinds = new Dictionary<string, int>();
        var mermaidNodeName = GetMermaidNodeName(rootNode.Kind(), kinds);
        ConvertInternal(sw, mermaidNodeName, rootNode, 1, kinds);
        return sb.ToString();
    }
    string GetMermaidNodeName(SyntaxKind kind, Dictionary<string, int> kinds)
    {
        var s = kind.ToString();
        if (kinds.TryGetValue(s, out var val))
        {
            kinds[s] += 1;
            return $"{s}_{val + 1}";
        }
        else
        {
            kinds[s] = 0;
            return $"{s}_0";
        }
    }
    void ConvertInternal(TextWriter tw, string currentNodeName, SyntaxNode node, int depth, Dictionary<string, int> kinds)
    {
        var indent = new string(' ', depth * 2);
        bool hasChild = false;
        // var mermaidNodeName = GetMermaidNodeName(node.Kind(), kinds);
        // sb.AppendLine(indent + node.GetText());
        foreach (var child in node.ChildNodesAndTokens())
        {
            if (!hasChild && depth == 1)
            {
                tw.WriteLine($"{indent}{currentNodeName}[\"{node.Kind()}\"]");
                hasChild = true;
            }
            var childNodeName = GetMermaidNodeName(child.Kind(), kinds);
            var childnode = child.AsNode();
            if (childnode != null)
            {
                tw.WriteLine($"{indent}{childNodeName}[\"{childnode.Kind()}\"]");
                tw.WriteLine($"{indent}{currentNodeName} --> {childNodeName}");
                ConvertInternal(tw, childNodeName, childnode, depth + 1, kinds);
            }
            else
            {
                // sb.AppendLine($"{indent}  {child.Kind().ToString()}: {child.ToString()}");
                tw.WriteLine($"{indent}{currentNodeName} --> {childNodeName}[\"{child.Kind()} {child.ToString()}\"]");
            }
        }
        if (!hasChild)
        {
            tw.WriteLine($"{indent}{currentNodeName}[\"{node.Kind()}\"]");
        }
    }
}