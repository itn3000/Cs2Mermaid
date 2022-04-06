using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using System.IO;
namespace Cs2Mermaid;

public class ConvertOptions
{
    public string[]? PreprocessorSymbols { get; set; }
    public string? LangVersion { get; set; }
    public string? ChartOrientation { get; set; }
    public string? SourceKind { get; set; }
    public bool ParseDocumentComment { get; set; }
}
public static class ConvertCsToMermaid
{
    public static string[] AvailableVersions()
    {
        return Enum.GetNames<LanguageVersion>();
    }
    static CSharpParseOptions CreateParseOption(ConvertOptions options)
    {
        var ret = CSharpParseOptions.Default;
        if (!string.IsNullOrEmpty(options.LangVersion))
        {
            try
            {
                var langver = Enum.Parse<LanguageVersion>(options.LangVersion, true);
                ret = ret.WithLanguageVersion(langver);
            }
            catch (Exception e)
            {
                throw new Exception($"failed to parse langveresion: {options.LangVersion}", e);
            }
        }
        if(!string.IsNullOrEmpty(options.SourceKind))
        {
            try
            {
                ret = ret.WithKind(Enum.Parse<SourceCodeKind>(options.SourceKind, true));
            }
            catch(Exception e)
            {
                throw new Exception($"failed to parse sourcecode kind: {options.SourceKind}", e);
            }
        }
        if (options.PreprocessorSymbols != null && options.PreprocessorSymbols.Length != 0)
        {
            ret = ret.WithPreprocessorSymbols(options.PreprocessorSymbols);
        }
        if(options.ParseDocumentComment)
        {
            ret = ret.WithDocumentationMode(DocumentationMode.Parse);
        }
        return ret;
    }
    public static void Convert(Stream input, Encoding inputEncoding, TextWriter tw, ConvertOptions convertOptions)
    {
        var src = SourceText.From(input, inputEncoding);
        var opt = CreateParseOption(convertOptions);
        var node = CSharpSyntaxTree.ParseText(src, opt);
        var rootNode = node.GetRoot();
        WriteHeader(tw, convertOptions.ChartOrientation);
        var kinds = new Dictionary<string, int>();
        var mermaidNodeName = GetMermaidNodeName(rootNode.Kind(), kinds);
        ConvertInternal(tw, mermaidNodeName, rootNode, 1, kinds);
    }

    public static void Convert(TextReader tr, TextWriter tw, ConvertOptions convertOptions)
    {
        var src = SourceText.From(tr.ReadToEnd());
        var opt = CreateParseOption(convertOptions);
        var node = CSharpSyntaxTree.ParseText(src, opt);
        var rootNode = node.GetRoot();
        WriteHeader(tw, convertOptions.ChartOrientation);
        var kinds = new Dictionary<string, int>();
        var mermaidNodeName = GetMermaidNodeName(rootNode.Kind(), kinds);
        ConvertInternal(tw, mermaidNodeName, rootNode, 1, kinds);
    }
    public static string Convert(string code, ConvertOptions convertOptions)
    {
        var opt = CreateParseOption(convertOptions);
        var node = CSharpSyntaxTree.ParseText(code, opt);
        var rootNode = node.GetRoot();
        var sb = new StringBuilder();
        var sw = new StringWriter(sb);
        WriteHeader(sw, convertOptions.ChartOrientation);
        var kinds = new Dictionary<string, int>();
        var mermaidNodeName = GetMermaidNodeName(rootNode.Kind(), kinds);
        ConvertInternal(sw, mermaidNodeName, rootNode, 1, kinds);
        return sb.ToString();
    }
    static void WriteHeader(TextWriter tw, string? orientation)
    {
        orientation = orientation ?? "LR";
        tw.WriteLine($"flowchart {orientation}");
    }
    static string GetMermaidNodeName(SyntaxKind kind, Dictionary<string, int> kinds)
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
    static void ConvertInternal(TextWriter tw, string currentNodeName, SyntaxNode node, int depth, Dictionary<string, int> kinds)
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
                tw.WriteLine($"{indent}{currentNodeName} --> {childNodeName}[\"{child.Kind()} {child.ToString()}\"]");
            }
        }
        if (!hasChild)
        {
            tw.WriteLine($"{indent}{currentNodeName}[\"{node.Kind()}\"]");
        }
    }
}