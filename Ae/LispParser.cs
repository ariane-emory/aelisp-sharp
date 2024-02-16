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
  private static readonly ImmutableArray<LispTokenType> MultilineCommentLispTokenTypes =
    ImmutableArray.Create(LispTokenType.MultilineCommentBegin, LispTokenType.MultilineCommentContent, LispTokenType.MultilineCommentEnd);

  private static readonly ImmutableArray<LispTokenType> MultilineStringLispTokenTypes =
    ImmutableArray.Create(LispTokenType.MultilineStringBegin, LispTokenType.MultilineStringContent, LispTokenType.MultilineStringEnd);

  //========================================================================================================================================
  // Extension methods
  //========================================================================================================================================
  public static IEnumerable<Token> ExcludingComments(this IEnumerable<Token> self) =>
    self.Where(t => (t.Type != LispTokenType.Comment) && !MultilineCommentLispTokenTypes.Contains(t.Type));

  //========================================================================================================================================
  // Parsers
  //========================================================================================================================================
  public static class LispParser
  {
    //======================================================================================================================================
    // Private static methods
    //======================================================================================================================================
    private static Parser<Token, Token> TypedToken(LispTokenType tokenType) => Parser<Token>.Token(t => t.Type == tokenType);

    private static Parser<Token, Token>
    MergeSequence(LispTokenType mergedType, LispTokenType beginType, LispTokenType contentType, LispTokenType endType) =>
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
      OneOf(Token(t => (!MultilineCommentLispTokenTypes.Contains(t.Type)) && (!MultilineStringLispTokenTypes.Contains(t.Type))),
            MergeSequence(LispTokenType.Comment,
                          LispTokenType.MultilineCommentBegin, LispTokenType.MultilineCommentContent, LispTokenType.MultilineCommentEnd),
            MergeSequence(LispTokenType.String,
                          LispTokenType.MultilineStringBegin, LispTokenType.MultilineStringContent, LispTokenType.MultilineStringEnd))
      .Many();

    public static readonly Parser<Token, Ae.LispObject> ParseCStyleChar =
      TypedToken(LispTokenType.CStyleChar).Select(t => (Ae.LispObject)new Ae.Char(t.Text[0]));

    public static readonly Parser<Token, Ae.LispObject> ParseLispStyleChar =
      TypedToken(LispTokenType.LispStyleChar).Select(t => (Ae.LispObject)new Ae.Char(t.Text[0]));

    public static readonly Parser<Token, Ae.LispObject> ParseString =
      TypedToken(LispTokenType.String).Select(t => (Ae.LispObject)new Ae.String(t.Text));

    public static readonly Parser<Token, Ae.LispObject> ParseSymbol =
      TypedToken(LispTokenType.Symbol).Select(t => (Ae.LispObject)new Ae.Symbol(t.Text));

    public static readonly Parser<Token, Ae.LispObject> ParseNil =
      TypedToken(LispTokenType.Nil).Select(t => (Ae.LispObject)Nil);

    public static readonly Parser<Token, Ae.LispObject> ParseInteger =
      TypedToken(LispTokenType.Integer).Select(t => (Ae.LispObject)new Ae.Integer(int.Parse(t.Text)));

    public static readonly Parser<Token, Ae.LispObject> ParseFloat =
      TypedToken(LispTokenType.Float).Select(t => (Ae.LispObject)new Ae.Float(float.Parse(t.Text)));

    public static readonly Parser<Token, Ae.LispObject> ParseRational =
      TypedToken(LispTokenType.Rational).Select(t =>
      {
        // We trust the tokenizer and assume that t.Text will contain the expected format.
        var parts = t.Text.Split('/');
        var numerator = int.Parse(parts[0]);
        var denominator = int.Parse(parts[1]);
        return (Ae.LispObject)new Ae.Rational(numerator, denominator);
      });

    public static readonly Parser<Token, Ae.LispObject> ParseAtom =
      OneOf(
        ParseSymbol,
        ParseNil,
        ParseInteger,
        ParseString,
        ParseFloat,
        ParseRational,
        ParseCStyleChar,
        ParseLispStyleChar
        );


    //======================================================================================================================================
  }
  //========================================================================================================================================
}
