using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Text;
namespace Cs2Mermaid;

public class ConvertCsToMermaid
{
    public string Convert(string code)
    {
        var node = CSharpSyntaxTree.ParseText(code);
        var rootNode = node.GetRoot();
        var sb = new StringBuilder();
        sb.AppendLine("flowchart LR");
        var kinds = new Dictionary<string, int>();
        var mermaidNodeName = GetMermaidNodeName(rootNode.Kind(), kinds);
        sb.Append(ConvertInternal(mermaidNodeName, rootNode, 1, kinds));
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
    string ConvertInternal(string currentNodeName, SyntaxNode node, int depth, Dictionary<string, int> kinds)
    {
        var sb = new StringBuilder();
        var indent = new string(' ', depth * 2);
        bool hasChild = false;
        // var mermaidNodeName = GetMermaidNodeName(node.Kind(), kinds);
        // sb.AppendLine(indent + node.GetText());
        foreach (var child in node.ChildNodesAndTokens())
        {
            if (!hasChild && depth == 1)
            {
                sb.AppendLine($"{indent}{currentNodeName}[\"{node.Kind()}\"]");
                hasChild = true;
            }
            var childNodeName = GetMermaidNodeName(child.Kind(), kinds);
            var childnode = child.AsNode();
            if (childnode != null)
            {
                sb.AppendLine($"{indent}{childNodeName}[\"{childnode.Kind()}\"]");
                sb.AppendLine($"{indent}{currentNodeName} --> {childNodeName}");
                sb.Append(ConvertInternal(childNodeName, childnode, depth + 1, kinds));
            }
            else
            {
                // sb.AppendLine($"{indent}  {child.Kind().ToString()}: {child.ToString()}");
                sb.AppendLine($"{indent}{currentNodeName} --> {childNodeName}[\"{child.Kind()} {child.ToString()}\"]");
            }
        }
        if (!hasChild)
        {
            sb.AppendLine($"{indent}{currentNodeName}[\"{node.Kind()}\"]");
        }
        return sb.ToString();
    }
}