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
    var multilineStringTokenTypes = new[] { TokenType.MultilineStringBegin, TokenType.MultilineStringContent, TokenType.MultilineStringEnd };

    Func<Token, bool> isSomeOtherToken = t => (!multilineCommentTokenTypes.Contains(t.TokenType)) && (!multilineStringTokenTypes.Contains(t.TokenType));

    static Parser<Token, Token> IsTokenType(TokenType tokenType) => Parser<Token>.Token(t => t.TokenType == tokenType);

    var multilineCommentBegin = IsTokenType(TokenType.MultilineCommentBegin);
    var multilineCommentContent = IsTokenType(TokenType.MultilineCommentContent);
    var multilineCommentEnd = IsTokenType(TokenType.MultilineCommentEnd);
    var someOtherToken = Parser<Token>.Token(isSomeOtherToken);
    var multilineStringBegin = IsTokenType(TokenType.MultilineStringBegin);
    var multilineStringContent = IsTokenType(TokenType.MultilineStringContent);
    var multilineStringEnd = IsTokenType(TokenType.MultilineStringEnd);

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

    var multilineStringParser =
      from begin in multilineStringBegin
      from contents in multilineStringContent.Many()
      from end in multilineStringEnd
      select new Token(
        Ae.TokenType.String,
        string.Join("", new[] { begin }.Concat(contents).Append(end).Select(t => t.Text)),
        begin.Line,
        begin.Column
        );

    // Recreate the stream.
    stream = new Ae.TokenStream(fileText, exclude: Ae.IsUninterestingToken);
    List<Token> tokens = stream.ReadAll();

    WriteLine("\nBefore parse: ");
    tokens.Print(); // tokens are printed correctly..

    var result = someOtherToken.Or(multilineCommentParser).Or(multilineStringParser).Many().ParseOrThrow(tokens);

    WriteLine("\nResult of parse: ");
    result.Print();

  }

  //====================================================================================================================
}
