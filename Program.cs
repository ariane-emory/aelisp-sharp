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
  static Parser<Token, Token> TokenWithTokenType(TokenType tokenType) => Parser<Token>.Token(t => t.TokenType == tokenType);

  //====================================================================================================================
  static void Main()
  {
    var multilineCommentTokenTypes =
      new[] { TokenType.MultilineCommentBegin, TokenType.MultilineCommentContent, TokenType.MultilineCommentEnd };
    var multilineStringTokenTypes =
      new[] { TokenType.MultilineStringBegin, TokenType.MultilineStringContent, TokenType.MultilineStringEnd };

    var multilineCommentBegin = TokenWithTokenType(TokenType.MultilineCommentBegin);
    var multilineCommentContent = TokenWithTokenType(TokenType.MultilineCommentContent);
    var multilineCommentEnd = TokenWithTokenType(TokenType.MultilineCommentEnd);
    var multilineStringBegin = TokenWithTokenType(TokenType.MultilineStringBegin);
    var multilineStringContent = TokenWithTokenType(TokenType.MultilineStringContent);
    var multilineStringEnd = TokenWithTokenType(TokenType.MultilineStringEnd);
    var someOtherToken = Token(t => (!multilineCommentTokenTypes.Contains(t.TokenType))
                               && (!multilineStringTokenTypes.Contains(t.TokenType)));

    var multilineComment =
      from begin in multilineCommentBegin
      from contents in multilineCommentContent.Many()
      from end in multilineCommentEnd
      select new Token(
        TokenType.Comment,
        string.Join("", new[] { begin }.Concat(contents).Append(end).Select(t => t.Text)),
        begin.Line,
        begin.Column
        );

    var multilineString =
      from begin in multilineStringBegin
      from contents in multilineStringContent.Many()
      from end in multilineStringEnd
      select new Token(
        TokenType.String,
        string.Join("", new[] { begin }.Concat(contents).Append(end).Select(t => t.Text)),
        begin.Line,
        begin.Column
        );

    //==================================================================================================================

    var filename = "data/data.lisp";
    var fileText = File.ReadAllText(filename);
    var stream = new TokenStream(fileText, exclude: IsUninterestingToken);
    var tokens = stream.ReadAll();

    WriteLine("\nBefore parse: ");
    tokens.Print(); // tokens are printed correctly..

    var result = OneOf(
      someOtherToken,
      multilineComment,
      multilineString
    ).Many().ParseOrThrow(tokens);

    WriteLine("\nResult of parse: ");
    result.Print();
  }

  //====================================================================================================================
}
