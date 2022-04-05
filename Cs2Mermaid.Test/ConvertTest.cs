using Xunit;
using Xunit.Abstractions;
using System.Text;
using System.IO;
using System;

namespace Cs2Mermaid.Test;

public class ConvertTest
{
    ITestOutputHelper _OutputHelper;
    public ConvertTest(ITestOutputHelper outputHelper)
    {
        _OutputHelper = outputHelper;
    }
    [Fact]
    public void ConvertFullClass()
    {
        const string resname = "Cs2Mermaid.Test.TestClasses.Test1.cs";
        using var stm = typeof(ConvertTest).Assembly.GetManifestResourceStream(resname);
        if(stm == null)
        {
            throw new ArgumentNullException(resname);
        }
        using var sr = new StreamReader(stm, Encoding.UTF8);
        var code = sr.ReadToEnd();
        var generated = ConvertCsToMermaid.Convert(code, new ConvertOptions());
        _OutputHelper.WriteLine(generated);
        Assert.NotNull(generated);
    }
    [Fact]
    public void IfDef()
    {
        var options = new ConvertOptions()
        {
            PreprocessorSymbols = new string[] { "ABC" }
        };
        var code = @"
        #if ABC
        1 + 1
        #endif
        ";
        var generated = ConvertCsToMermaid.Convert(code, options);
        _OutputHelper.WriteLine(generated);
        Assert.NotNull(generated);
    }
}