using System.Collections.Immutable;
using Pidgin;
using static Pidgin.Parser;
using static Pidgin.Parser<Ae.Token>;

//==========================================================================================================================================
static partial class Ae
{
  //==================================================================================================================================================
  // Ae's static fields
  //==================================================================================================================================================
  private static readonly ImmutableArray<TokenType> MultilineCommentTokenTypes =
    ImmutableArray.Create(TokenType.MultilineCommentBegin, TokenType.MultilineCommentContent, TokenType.MultilineCommentEnd);

  private static readonly ImmutableArray<TokenType> MultilineStringTokenTypes =
    ImmutableArray.Create(TokenType.MultilineStringBegin, TokenType.MultilineStringContent, TokenType.MultilineStringEnd);

  //========================================================================================================================================
  // Extension methods
  //========================================================================================================================================
  public static IEnumerable<Token> ExcludingComments(this IEnumerable<Token> self) =>
    self.Where(t => (t.Type != TokenType.Comment) && !MultilineCommentTokenTypes.Contains(t.Type));

  //========================================================================================================================================
  // Parsers
  //========================================================================================================================================
  public static class Parser
  {
    //======================================================================================================================================
    // Private static methods
    //======================================================================================================================================
    private static Parser<Token, Token> TokenWithType(TokenType tokenType) => Parser<Token>.Token(t => t.Type == tokenType);

    private static Parser<Token, Token>
    MergeSequence(TokenType mergedType, TokenType beginType, TokenType contentType, TokenType endType) =>
      from beginToken in TokenWithType(beginType)
      from contentsTokens in TokenWithType(contentType).Many()
      from endToken in TokenWithType(endType)
      select new Token(
        mergedType,
        string.Join("", new[] { beginToken }.Concat(contentsTokens).Append(endToken).Select(t => t.Text)),
        beginToken.Line,
        beginToken.Column);

    //======================================================================================================================================
    // Public Parsers
    //======================================================================================================================================
    public static readonly Parser<Token, IEnumerable<Token>> MergeMultilineTokens =
      OneOf(Token(t => (!MultilineCommentTokenTypes.Contains(t.Type)) && (!MultilineStringTokenTypes.Contains(t.Type))),
            MergeSequence(TokenType.Comment,
                          TokenType.MultilineCommentBegin, TokenType.MultilineCommentContent, TokenType.MultilineCommentEnd),
            MergeSequence(TokenType.String,
                          TokenType.MultilineStringBegin, TokenType.MultilineStringContent, TokenType.MultilineStringEnd))
      .Many();
    //======================================================================================================================================
  }
  //========================================================================================================================================
}
