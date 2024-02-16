using System.Text.RegularExpressions;
using static System.Console;
using static Ae;
using Pidgin;
using static Pidgin.Parser;
using static Pidgin.Parser<Ae.Token>;
// using TokenParser = Pidgin.Parser<Ae.Token>;

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
    Func<Token, bool> isMultilineCommentContent =  t => t.TokenType == TokenType.MultilineCommentContent;
    Func<Token, bool> isMultilineCommentEnd = t => t.TokenType == TokenType.MultilineCommentEnd;
    Func<Token, bool> isSomeOtherToken = t => !multilineCommentTokenTypes.Contains(t.TokenType);

    // var multilineCommentBeginningParser = Parser<Token>.Token(isMultilineCommentBeginning);
    // var multilineCommentContentParser = Parser<Token>.Token(isMultilineCommentContent);
    // var multilineCommentEndParser = Parser<Token>.Token(isMultilineCommentEnd);
    // var someOtherTokenParser = Parser<Token>.Token(isSomeOtherToken);

    static Parser<Token, Token> IsTokenType(TokenType tokenType) => Parser<Token>.Token(t => t.TokenType == tokenType);

    var multilineCommentBeginning = IsTokenType(TokenType.MultilineCommentBeginning);
    var multilineCommentContent = IsTokenType(TokenType.MultilineCommentContent);
    var multilineCommentEnd = IsTokenType(TokenType.MultilineCommentEnd);
    var someOtherToken = Parser<Token>.Token(isSomeOtherToken);

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
    // Assuming you have a method to convert your IEnumerable<Token> into a parser's input stream
    // var tokens = ... // Your token stream here
    // var result = multilineStringParser.ParseOrFallback(tokens, fallbackValue);
    // var commentResult = multilineCommentParser.ParseOrFallback(tokens, fallbackValue);
  }

  //====================================================================================================================
}

