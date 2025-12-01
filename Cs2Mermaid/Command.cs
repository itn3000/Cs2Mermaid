using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using ConsoleAppFramework;

namespace Cs2Mermaid
{
    internal class Command
    {
        public async Task<int> DoConvert(string? output = null,
            string? input = null,
            string? inputEncoding = null,
            string? outputEncoding = null,
            bool mdWithSource = false,
            string? langVersion = null,
            bool availableVersion = false,
            string[]? ppSymbol = null,
            string? chartOrientation = null,
            bool asScript = false,
            bool outputDiagnostics = false,
            CancellationToken token = default)
        {
            await Task.Yield();
            if (availableVersion)
            {
                foreach (var ver in Cs2Mermaid.ConvertCsToMermaid.AvailableVersions())
                {
                    Console.WriteLine(ver);
                }
                return 0;
            }
            var diagnosticsWriter = outputDiagnostics switch
            {
                true => Console.Error,
                false => null
            };
            var convertOption = new ConvertOptions()
            {
                LangVersion = langVersion,
                AsScript = asScript,
                ChartOrientation = chartOrientation,
                DiagnosticsWriter = diagnosticsWriter,
                PreprocessorSymbols = ppSymbol,
            };
            if(mdWithSource)
            {
                await Util.ProcessWithMd(output, outputEncoding, input, inputEncoding, convertOption, diagnosticsWriter, token);
            }
            else
            {
                Util.ProcessNoMd(output, outputEncoding, input, inputEncoding, convertOption, diagnosticsWriter, token);
            }
            return 0;
        }
    }
    static class Util
    {
        public static async Task ProcessWithMd(string? output, string? outputEncoding, string? input, string? inputEncoding, ConvertOptions convertOptions, TextWriter? diagnosticWriter, CancellationToken token)
        {
            var ie = GetEncoding(inputEncoding);
            string sourceText = "";
            using (var inputstm = CreateInputStream(input))
            using (var tr = new StreamReader(inputstm, ie))
            {
                sourceText = await tr.ReadToEndAsync(token);
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
                var diagnostics = await ConvertCsToMermaid.ConvertAsync(tr, tw, convertOptions, token);
                tw.WriteLine("```");
                tw.Flush();
                if (diagnosticWriter != null)
                {
                    foreach (var diag in diagnostics)
                    {
                        diagnosticWriter.WriteLine(diag.ToString());
                    }
                }
            }
        }
        public static void ProcessNoMd(string? output, string? outputEncoding, string? input, string? inputEncoding, ConvertOptions convertOptions, TextWriter? diagnosticWriter, CancellationToken token)
        {
            using var inputstm = CreateInputStream(input);
            using var outputstm = CreateOutputStream(output);
            var ie = GetEncoding(inputEncoding);
            var oe = GetEncoding(outputEncoding);
            using var tr = new StreamReader(inputstm, ie);
            using var tw = new StreamWriter(outputstm, oe);
            var diags = Cs2Mermaid.ConvertCsToMermaid.Convert(inputstm, ie, tw, convertOptions);
            tw.Flush();
            if (diagnosticWriter != null)
            {
                foreach (var diag in diags)
                {
                    diagnosticWriter.WriteLine(diag.ToString());
                }
            }

        }
        public static Stream CreateInputStream(string? input)
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
        public static Stream CreateOutputStream(string? output)
        {
            if (string.IsNullOrEmpty(output))
            {
                return Console.OpenStandardOutput();
            }
            else
            {
                var dirname = Path.GetDirectoryName(output);
                if (!string.IsNullOrEmpty(dirname) && !Directory.Exists(dirname))
                {
                    Directory.CreateDirectory(dirname);
                }
                return File.Create(output);
            }
        }
        public static Encoding GetEncoding(string? name)
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
}
