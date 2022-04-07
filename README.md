# converter from C# syntax tree to mermaid diagram

This tool converts from C# syntax tree to [mermaid.js diagram.](https://mermaid-js.github.io/mermaid/#/)

nuget package(dotnet tool): https://www.nuget.org/packages/Cs2Mermaid/

# Usage

## Installation

### dotnet global tool

1. install [dotnet sdk 6.0 or later](https://dotnet.microsoft.com/en-us/download)
2. run `dotnet tool install -g Cs2Mermaid`
3. add `$HOME/.dotnet/tools` to your PATH environment variable
4. run `cs2mmd --help` then you will get help message

### single executable binary

1. download zip or tgz binary from [release page](https://github.com/itn3000/Cs2Mermaid/releases)
2. extract binary
3. (if linux or mac) add executable flag to file

# Examples Usage

## Read from file and output to stdout

`cs2mmd -i path/to/cs`

## Read from stdin and output to file

`cat path/to/cs | cs2mmd -o output.mmd`

## Top-Down orientation

`cs2mmd -i path/to/cs --orientation TD`

## parse with Preprocessor

`--pp-symbol` option can be specified multiple.

`cs2mmd -i path/to/cs --pp-symbol ABC --pp-symbol DEF`

## output as markdown with original source(github format)

`cs2mmd -i path/to/cs -o output.md --md-with-source --chart-orientation TD`

Example output is here(please view with raw README)

```csharp
using System;

// this is comment

Console.WriteLine("Hello World");
```

```mermaid
flowchart TD
  CompilationUnit_0["CompilationUnit"]
  UsingDirective_0["UsingDirective"]
  CompilationUnit_0 --> UsingDirective_0
    UsingDirective_0 --> UsingKeyword_0["UsingKeyword using"]
    IdentifierName_0["IdentifierName"]
    UsingDirective_0 --> IdentifierName_0
      IdentifierName_0 --> IdentifierToken_0["IdentifierToken System"]
      IdentifierName_0["IdentifierName"]
    UsingDirective_0 --> SemicolonToken_0["SemicolonToken ;"]
    UsingDirective_0["UsingDirective"]
  GlobalStatement_0["GlobalStatement"]
  CompilationUnit_0 --> GlobalStatement_0
    ExpressionStatement_0["ExpressionStatement"]
    GlobalStatement_0 --> ExpressionStatement_0
      InvocationExpression_0["InvocationExpression"]
      ExpressionStatement_0 --> InvocationExpression_0
        SimpleMemberAccessExpression_0["SimpleMemberAccessExpression"]
        InvocationExpression_0 --> SimpleMemberAccessExpression_0
          IdentifierName_1["IdentifierName"]
          SimpleMemberAccessExpression_0 --> IdentifierName_1
            IdentifierName_1 --> IdentifierToken_1["IdentifierToken Console"]
            IdentifierName_1["IdentifierName"]
          SimpleMemberAccessExpression_0 --> DotToken_0["DotToken ."]
          IdentifierName_2["IdentifierName"]
          SimpleMemberAccessExpression_0 --> IdentifierName_2
            IdentifierName_2 --> IdentifierToken_2["IdentifierToken WriteLine"]
            IdentifierName_2["IdentifierName"]
          SimpleMemberAccessExpression_0["SimpleMemberAccessExpression"]
        ArgumentList_0["ArgumentList"]
        InvocationExpression_0 --> ArgumentList_0
          ArgumentList_0 --> OpenParenToken_0["OpenParenToken ("]
          Argument_0["Argument"]
          ArgumentList_0 --> Argument_0
            StringLiteralExpression_0["StringLiteralExpression"]
            Argument_0 --> StringLiteralExpression_0
              StringLiteralExpression_0 --> StringLiteralToken_0["StringLiteralToken "Hello World""]
              StringLiteralExpression_0["StringLiteralExpression"]
            Argument_0["Argument"]
          ArgumentList_0 --> CloseParenToken_0["CloseParenToken )"]
          ArgumentList_0["ArgumentList"]
        InvocationExpression_0["InvocationExpression"]
      ExpressionStatement_0 --> SemicolonToken_1["SemicolonToken ;"]
      ExpressionStatement_0["ExpressionStatement"]
    GlobalStatement_0["GlobalStatement"]
  CompilationUnit_0 --> EndOfFileToken_0["EndOfFileToken "]
```
