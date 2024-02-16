using System.Collections.Immutable;
using Pidgin;
using static Pidgin.Parser;
using static Pidgin.Parser<Ae.Token>;

//==========================================================================================================================================
static partial class Ae
{
  //========================================================================================================================================
  // Extension methods
  //========================================================================================================================================
  public static IEnumerable<Token> ExcludingComments(this IEnumerable<Token> self) => 
    self.Where(t => (t.Type != TokenType.Comment) && !MultilineCommentTokenTypes.Contains(t.Type));

  //======================================================================================================================================
  // Private static fields
  //======================================================================================================================================
  private static readonly ImmutableArray<TokenType> MultilineCommentTokenTypes =
    ImmutableArray.Create(TokenType.MultilineCommentBegin, TokenType.MultilineCommentContent, TokenType.MultilineCommentEnd);

  private static readonly ImmutableArray<TokenType> MultilineStringTokenTypes =
    ImmutableArray.Create(TokenType.MultilineStringBegin, TokenType.MultilineStringContent, TokenType.MultilineStringEnd);

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
    MergeSequence(TokenType outToken, TokenType beginToken, TokenType contentToken, TokenType endToken) =>
      from begin in TokenWithType(beginToken)
      from contents in TokenWithType(contentToken).Many()
      from end in TokenWithType(endToken)
      select new Token(
        outToken,
        string.Join("", new[] { begin }.Concat(contents).Append(end).Select(t => t.Text)),
        begin.Line,
        begin.Column
        );

    //======================================================================================================================================
    // Public Parsers
    //======================================================================================================================================
    public static readonly Parser<Token, IEnumerable<Token>> MergeMultilineTokens =
      OneOf(Token(t => (!MultilineCommentTokenTypes.Contains(t.Type)) && (!MultilineStringTokenTypes.Contains(t.Type))),
            MergeSequence( TokenType.Comment,
                          TokenType.MultilineCommentBegin, TokenType.MultilineCommentContent, TokenType.MultilineCommentEnd),
            MergeSequence(TokenType.String,
                          TokenType.MultilineStringBegin, TokenType.MultilineStringContent, TokenType.MultilineStringEnd))
      .Many();
    //======================================================================================================================================
  }
  //========================================================================================================================================
}
