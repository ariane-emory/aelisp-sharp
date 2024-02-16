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
    private static Parser<Token, Token> TypedToken(TokenType tokenType) => Parser<Token>.Token(t => t.Type == tokenType);

    private static Parser<Token, Token>
    MergeSequence(TokenType mergedType, TokenType beginType, TokenType contentType, TokenType endType) =>
      from beginToken in TypedToken(beginType)
      from contentsTokens in TypedToken(contentType).Many()
      from endToken in TypedToken(endType)
      select new Token(
        mergedType,
        string.Join("", new[] { beginToken }.Concat(contentsTokens).Append(endToken).Select(t => t.Text)),
        beginToken.Line,
        beginToken.Column,
        beginToken.ParenDepth);

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

    public static readonly Parser<Token, Ae.Object> ParseCStyleChar =
      TypedToken(TokenType.CStyleChar).Select(t => (Ae.Object)new Ae.String(t.Text));

    public static readonly Parser<Token, Ae.Object> ParseLispStyleChar =
      TypedToken(TokenType.LispStyleChar).Select(t => (Ae.Object)new Ae.String(t.Text));

    public static readonly Parser<Token, Ae.Object> ParseString =
      TypedToken(TokenType.String).Select(t => (Ae.Object)new Ae.String(t.Text));

    public static readonly Parser<Token, Ae.Object> ParseSymbol =
      TypedToken(TokenType.Symbol).Select(t => (Ae.Object)new Ae.Symbol(t.Text));

    public static readonly Parser<Token, Ae.Object> ParseNil =
      TypedToken(TokenType.Nil).Select(t => (Ae.Object)Nil);

    public static readonly Parser<Token, Ae.Object> ParseInteger =
      TypedToken(TokenType.Integer).Select(t => (Ae.Object)new Ae.Integer(int.Parse(t.Text)));

    public static readonly Parser<Token, Ae.Object> ParseFloat =
      TypedToken(TokenType.Float).Select(t => (Ae.Object)new Ae.Float(float.Parse(t.Text)));

    public static readonly Parser<Token, Ae.Object> ParseRational =
      TypedToken(TokenType.Rational).Select(t =>
      {
        // We trust the tokenizer and assume that t.Text will contain the expected format.
        var parts = t.Text.Split('/');
        var numerator = int.Parse(parts[0]);
        var denominator = int.Parse(parts[1]);
        return (Ae.Object)new Ae.Rational(numerator, denominator);
      });

    public static readonly Parser<Token, Ae.Object> ParseAtom =
      OneOf(
        ParseSymbol,
        ParseNil,
        ParseInteger,
        // ParseString,
        ParseFloat,
        ParseRational,
        ParseCStyleChar,
        ParseLispStyleChar
        );


    //======================================================================================================================================
  }
  //========================================================================================================================================
}
