using System.Collections.Immutable;
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
  static readonly ImmutableArray<TokenType> multilineCommentTokenTypes =
    ImmutableArray.Create(TokenType.MultilineCommentBegin, TokenType.MultilineCommentContent, TokenType.MultilineCommentEnd);
  static readonly ImmutableArray<TokenType> multilineStringTokenTypes =
    ImmutableArray.Create(TokenType.MultilineStringBegin, TokenType.MultilineStringContent, TokenType.MultilineStringEnd);

  static readonly Parser<Token, Token> multilineCommentBegin = TokenWithTokenType(TokenType.MultilineCommentBegin);
  static readonly Parser<Token, Token> multilineCommentContent = TokenWithTokenType(TokenType.MultilineCommentContent);
  static readonly Parser<Token, Token> multilineCommentEnd = TokenWithTokenType(TokenType.MultilineCommentEnd);
  static readonly Parser<Token, Token> multilineStringBegin = TokenWithTokenType(TokenType.MultilineStringBegin);
  static readonly Parser<Token, Token> multilineStringContent = TokenWithTokenType(TokenType.MultilineStringContent);
  static readonly Parser<Token, Token> multilineStringEnd = TokenWithTokenType(TokenType.MultilineStringEnd);
  static readonly Parser<Token, Token> someOtherToken = Token(t => (!multilineCommentTokenTypes.Contains(t.TokenType))
                                                              && (!multilineStringTokenTypes.Contains(t.TokenType)));

  static readonly Parser<Token, Token> multilineComment =
    from begin in multilineCommentBegin
    from contents in multilineCommentContent.Many()
    from end in multilineCommentEnd
    select new Token(
      TokenType.Comment,
      string.Join("", new[] { begin }.Concat(contents).Append(end).Select(t => t.Text)),
      begin.Line,
      begin.Column
      );

  static readonly Parser<Token, Token> multilineString =
    from begin in multilineStringBegin
    from contents in multilineStringContent.Many()
    from end in multilineStringEnd
    select new Token(
      TokenType.String,
      string.Join("", new[] { begin }.Concat(contents).Append(end).Select(t => t.Text)),
      begin.Line,
      begin.Column
      );

  //====================================================================================================================
  static void Main()
  {
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
