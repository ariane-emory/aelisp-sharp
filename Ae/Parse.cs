using System.Collections.Immutable;
using System.Text.RegularExpressions;
using static System.Console;
using Pidgin;
using static Pidgin.Parser;
using static Pidgin.Parser<Ae.Token>;

//==========================================================================================================================================
static partial class Ae
{
  //========================================================================================================================================
  private static Parser<Token, Token> TokenWithType(TokenType tokenType) => Parser<Token>.Token(t => t.Type == tokenType);

  private static Parser<Token, Token> MergeMultilineSequence(TokenType beginToken, TokenType contentToken, TokenType endToken) =>
    from begin in TokenWithType(beginToken)
    from contents in TokenWithType(contentToken).Many()
    from end in TokenWithType(endToken)
    select new Token(
      TokenType.Comment,
      string.Join("", new[] { begin }.Concat(contents).Append(end).Select(t => t.Text)),
      begin.Line,
      begin.Column
      );

  //========================================================================================================================================
  private static readonly ImmutableArray<TokenType> MultilineCommentTokenTypes =
    ImmutableArray.Create(TokenType.MultilineCommentBegin, TokenType.MultilineCommentContent, TokenType.MultilineCommentEnd);

  private static readonly ImmutableArray<TokenType> MultilineStringTokenTypes =
    ImmutableArray.Create(TokenType.MultilineStringBegin, TokenType.MultilineStringContent, TokenType.MultilineStringEnd);

  //========================================================================================================================================
  public static readonly Parser<Token, IEnumerable<Token>> MergeMultilineTokens =
    OneOf(Token(t => (!MultilineCommentTokenTypes.Contains(t.Type))
                && (!MultilineStringTokenTypes.Contains(t.Type))),
          MergeMultilineSequence(TokenType.MultilineCommentBegin, TokenType.MultilineCommentContent, TokenType.MultilineCommentEnd),
          MergeMultilineSequence(TokenType.MultilineStringBegin, TokenType.MultilineStringContent, TokenType.MultilineStringEnd)).Many();

  //========================================================================================================================================
}

