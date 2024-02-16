﻿using System.Text.RegularExpressions;
using static System.Console;
using static Ae;
using Pidgin;
using static Pidgin.Parser;
using static Pidgin.Parser<Ae.Token>;
using TokenParser = Pidgin.Parser<Ae.Token>;
using AeToken = Ae.Token; // If 'using static Ae;' is applied

//======================================================================================================================
class Program
{
  //====================================================================================================================
  static void Main()
  {
    var filename = "data/data.lisp";
    var fileText = File.ReadAllText(filename);
    var stream = new Ae.TokenStream(fileText, exclude: Ae.IsUninterestingToken);
    var take_count = 16;
    var ary = new Ae.Token[take_count];
    var read_count = stream.Read(ary);
    ary.Take(read_count).Print();

    var multilineCommentTokenTypes = new[] { TokenType.MultilineCommentBeginning, TokenType.MultilineCommentContent, TokenType.MultilineCommentEnd };

    Func<Token, bool> isMultilineCommentBeginning = t => t.TokenType == TokenType.MultilineCommentBeginning;
    Func<Token, bool> isMultilineCommentContent = t => t.TokenType == Ae.TokenType.MultilineCommentContent;
    Func<Token, bool> isMultilineCommentEnd = t => t.TokenType == Ae.TokenType.MultilineCommentEnd;
    Func<Token, bool> isSomeOtherToken = t => !multilineCommentTokenTypes.Contains(t.TokenType);

    static Parser<AeToken, AeToken> IsTokenType(TokenType tokenType) => Parser<AeToken>.Token(t => t.TokenType == tokenType);

    var p = Parser<Ae.Token>.Token(t => true);
    List<string> x = new();

    // var multilineCommentBeginningParser = Parser.Token<Token>(isMultilineCommentBeginning);
    // var multilineCommentContentParser = Parser.Token<Token>(isMultilineCommentContent);
    // var multilineCommentEndParser = Parser.Token<Token>(isMultilineCommentEnd);

    // var multilineCommentParser =
    //   from begin in multilineCommentBegin
    //   from contents in multilineCommentContent.Many()
    //   from end in multilineCommentEnd
    //   select new Ae.PositionedToken<Ae.TokenType>(
    //     Ae.TokenType.Comment,
    //     string.Join("", new[] { begin }.Concat(contents).Append(end).Select(t => t.Text)),
    //     begin.Line,
    //     begin.Column
    //     );

    // Example usage
    // Assuming you have a method to convert your IEnumerable<AeToken> into a parser's input stream
    // var tokens = ... // Your token stream here
    // var result = multilineStringParser.ParseOrFallback(tokens, fallbackValue);
    // var commentResult = multilineCommentParser.ParseOrFallback(tokens, fallbackValue);
  }

  //====================================================================================================================
}

