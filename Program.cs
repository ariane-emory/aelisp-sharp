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
    // var take_count = 16;
    // var ary = new Ae.Token[take_count];
    // var read_count = stream.Read(ary);
    // ary.Take(read_count).Print();

    var multilineCommentTokenTypes = new[] { TokenType.MultilineCommentBegin, TokenType.MultilineCommentContent, TokenType.MultilineCommentEnd };

    Func<Token, bool> isMultilineCommentBegin = t => t.TokenType == TokenType.MultilineCommentBegin;
    Func<Token, bool> isMultilineCommentContent = t => t.TokenType == TokenType.MultilineCommentContent;
    Func<Token, bool> isMultilineCommentEnd = t => t.TokenType == TokenType.MultilineCommentEnd;
    Func<Token, bool> isSomeOtherToken = t => !multilineCommentTokenTypes.Contains(t.TokenType);

    // var multilineCommentBeginParser = Parser<Token>.Token(isMultilineCommentBegin);
    // var multilineCommentContentParser = Parser<Token>.Token(isMultilineCommentContent);
    // var multilineCommentEndParser = Parser<Token>.Token(isMultilineCommentEnd);
    // var someOtherTokenParser = Parser<Token>.Token(isSomeOtherToken);

    static Parser<Token, Token> IsTokenType(TokenType tokenType) => Parser<Token>.Token(t => t.TokenType == tokenType);

    var multilineCommentBegin = IsTokenType(TokenType.MultilineCommentBegin);
    var multilineCommentContent = IsTokenType(TokenType.MultilineCommentContent);
    var multilineCommentEnd = IsTokenType(TokenType.MultilineCommentEnd);
    var someOtherToken = Parser<Token>.Token(isSomeOtherToken);

    var multilineCommentParser =
      from begin in multilineCommentBegin
      from contents in multilineCommentContent.Many()
      from end in multilineCommentEnd
      select new Token(
        Ae.TokenType.Comment,
        string.Join("", new[] { begin }.Concat(contents).Append(end).Select(t => t.Text)),
        begin.Line,
        begin.Column
        );

    // Recreate the stream.
    stream = new Ae.TokenStream(fileText, exclude: Ae.IsUninterestingToken);
    List<Token> tokens = stream.ReadAll();

    WriteLine("\nBefore parse: ");
    tokens.Print(); // tokens are printed correctly..

    var result = someOtherToken.Or(multilineCommentParser).Many().ParseOrThrow(tokens);

    WriteLine("\nResult of parse: ");
    result.Print();

  }

  //====================================================================================================================
}
