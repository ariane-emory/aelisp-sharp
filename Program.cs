using System.Collections.Immutable;
using System.Text.RegularExpressions;
using static System.Console;
using static Ae;
using Pidgin;
using static Pidgin.Parser;
using static Pidgin.Parser<Ae.Token>;
// using TokenParser = Pidgin.Parser<Ae.Token>;

//================================================================================================================================
class Program
{
  //==============================================================================================================================
  static Parser<Token, Token> TokenWithType(TokenType tokenType) => Parser<Token>.Token(t => t.Type == tokenType);

  static Parser<Token, Token> MergeMultilineSequence(TokenType beginToken, TokenType contentToken, TokenType endToken) =>
    from begin in TokenWithType(beginToken)
    from contents in TokenWithType(contentToken).Many()
    from end in TokenWithType(endToken)
    select new Token(
      TokenType.Comment,
      string.Join("", new[] { begin }.Concat(contents).Append(end).Select(t => t.Text)),
      begin.Line,
      begin.Column
      );

  //==============================================================================================================================
  static readonly ImmutableArray<TokenType> MultilineCommentTokenTypes =
    ImmutableArray.Create(TokenType.MultilineCommentBegin, TokenType.MultilineCommentContent, TokenType.MultilineCommentEnd);

  static readonly ImmutableArray<TokenType> MultilineStringTokenTypes =
    ImmutableArray.Create(TokenType.MultilineStringBegin, TokenType.MultilineStringContent, TokenType.MultilineStringEnd);

  //==============================================================================================================================
  // static readonly Parser<Token, Token> MultilineCommentBeginToken = TokenWithType(TokenType.MultilineCommentBegin);
  // static readonly Parser<Token, Token> MultilineCommentContentToken = TokenWithType(TokenType.MultilineCommentContent);
  // static readonly Parser<Token, Token> MultilineCommentEndToken = TokenWithType(TokenType.MultilineCommentEnd);
  // static readonly Parser<Token, Token> MultilineStringBeginToken = TokenWithType(TokenType.MultilineStringBegin);
  // static readonly Parser<Token, Token> MultilineStringContentToken = TokenWithType(TokenType.MultilineStringContent);
  // static readonly Parser<Token, Token> MultilineStringEndToken = TokenWithType(TokenType.MultilineStringEnd);
  static readonly Parser<Token, Token> NotAMultilineToken = Token(t => (!MultilineCommentTokenTypes.Contains(t.Type))
                                                              && (!MultilineStringTokenTypes.Contains(t.Type)));

  static readonly Parser<Token, Token> MergeMultilineCommentTokens =
    MergeMultilineSequence(TokenType.MultilineCommentBegin, TokenType.MultilineCommentContent, TokenType.MultilineCommentEnd);

  static readonly Parser<Token, Token> MergeMultilineStringTokens =
    MergeMultilineSequence(TokenType.MultilineStringBegin, TokenType.MultilineStringContent, TokenType.MultilineStringEnd);

  static readonly Parser<Token, IEnumerable<Token>> MergeMultilineTokens =
    OneOf(NotAMultilineToken, MergeMultilineCommentTokens, MergeMultilineStringTokens).Many();

  //==============================================================================================================================
  static void Main()
  {
    //============================================================================================================================

    var filename = "data/data.lisp";
    var fileText = File.ReadAllText(filename);
    var stream = new TokenStream(fileText, exclude: IsUninterestingToken);
    var tokens = stream.ReadAll();

    WriteLine("\nBefore parse: ");
    tokens.Print(); // tokens are printed correctly..

    var result = MergeMultilineTokens.ParseOrThrow(tokens);

    WriteLine("\nResult of parse: ");
    result.Print();
  }

  //==============================================================================================================================
}
