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
    // [Fact]
    // public void DocComment()
    // {
    //     var options = new ConvertOptions()
    //     {
    //         ParseDocumentComment = true,
    //     };
    //     var code = @"
    //     /// <summary>this is summary</summary>
    //     /// <remarks>this is remarks</remarks>
    //     class C1
    //     {
    //         public void M(int x){}
    //     }
    //     ";
    //     var withdoc = ConvertCsToMermaid.Convert(code, options);
    //     _OutputHelper.WriteLine("withdoc: " + withdoc);
    //     Assert.NotNull(withdoc);
    //     options.ParseDocumentComment = false;
    //     var nodoc = ConvertCsToMermaid.Convert(code, options);
    //     _OutputHelper.WriteLine("nodoc: " + nodoc);
    //     Assert.NotEqual(nodoc, withdoc);
    // }
    [Fact]
    public void SourceCodeKindTest()
    {
        var options = new ConvertOptions()
        {
            AsScript = true
        };
        var code = @"
        #r """"
        var x = 1 + 1;
        ";
        var asscript = ConvertCsToMermaid.Convert(code, options);
        options.AsScript = false;
        var asregular = ConvertCsToMermaid.Convert(code, options);
        _OutputHelper.WriteLine("script: " + asscript);
        _OutputHelper.WriteLine("regular: " + asregular);
        Assert.NotEqual(asregular, asscript);

    }
    [Fact]
    public void LangVersionTest()
    {
        var options = new ConvertOptions()
        {
            LangVersion = "CSharp7_3"
        };
        var code = @"record Abc(string X);";
        var cs7_3 = ConvertCsToMermaid.Convert(code, options);
        _OutputHelper.WriteLine("cs7.3 = " + cs7_3);
        options.LangVersion = "CSharp10";
        var cs10_0 = ConvertCsToMermaid.Convert(code, options);
        _OutputHelper.WriteLine("cs10 = " + cs10_0);
    }
}