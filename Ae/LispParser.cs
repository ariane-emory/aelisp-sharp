using System.Collections.Immutable;
using Pidgin;
using static Pidgin.Parser;
using static Pidgin.Parser<Ae.LispToken>;
using static Ae;

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
  public static IEnumerable<LispToken> ExcludingComments(this IEnumerable<LispToken> self) =>
    self.Where(t => (t.Type != LispTokenType.Comment) && !MultilineCommentLispTokenTypes.Contains(t.Type));

  //========================================================================================================================================
  // Parsers
  //========================================================================================================================================
  public static class LispParser
  {
    //======================================================================================================================================
    // Private static methods
    //======================================================================================================================================
    private static Parser<LispToken, LispToken> TypedToken(LispTokenType tokenType) => Parser<LispToken>.Token(t => t.Type == tokenType);

    private static Parser<LispToken, LispToken>
    MergeSequence(LispTokenType mergedType, bool trim, LispTokenType beginType, LispTokenType contentType, LispTokenType endType) =>
      from beginToken in TypedToken(beginType)
      from contentsTokens in TypedToken(contentType).Many()
      from endToken in TypedToken(endType)
      select new LispToken(
        mergedType,
        string.Join("", new[] { beginToken }.Concat(contentsTokens).Append(endToken).Select(t => t.Text)).Trim(trim),
        beginToken.Line,
        beginToken.Column,
        beginToken.ParenDepth);

    //======================================================================================================================================
    // Public Parsers
    //======================================================================================================================================
    public static readonly Parser<LispToken, IEnumerable<LispToken>> MergeMultilineTokens =
      OneOf(Token(t => (!MultilineCommentLispTokenTypes.Contains(t.Type)) && (!MultilineStringLispTokenTypes.Contains(t.Type))),
            MergeSequence(LispTokenType.Comment, true,
                          LispTokenType.MultilineCommentBegin, LispTokenType.MultilineCommentContent, LispTokenType.MultilineCommentEnd),
            MergeSequence(LispTokenType.String, false,
                          LispTokenType.MultilineStringBegin, LispTokenType.MultilineStringContent, LispTokenType.MultilineStringEnd))
      .Many();

    public static readonly Parser<LispToken, LispObject> ParseCStyleChar =
      TypedToken(LispTokenType.CStyleChar).Select(t => (LispObject)new Char(t.Text![0]));

    public static readonly Parser<LispToken, LispObject> ParseLispStyleChar =
      TypedToken(LispTokenType.LispStyleChar).Select(t => (LispObject)new Char(t.Text![0]));

    public static readonly Parser<LispToken, LispObject> ParseString =
      TypedToken(LispTokenType.String).Select(t => (LispObject)new String(t.Text!));

    public static readonly Parser<LispToken, LispObject> ParseSymbol =
      TypedToken(LispTokenType.Symbol).Select(t => Intern(t.Text!));

    public static readonly Parser<LispToken, LispObject> ParseNil =
      TypedToken(LispTokenType.Nil).Select(t => (LispObject)Nil);

    public static readonly Parser<LispToken, LispObject> ParseInteger =
      TypedToken(LispTokenType.Integer).Select(t => (LispObject)new Integer(int.Parse(t.Text!)));

    public static readonly Parser<LispToken, LispObject> ParseFloat =
      TypedToken(LispTokenType.Float).Select(t => (LispObject)new Float(float.Parse(t.Text!)));

    public static readonly Parser<LispToken, LispObject> ParseRational =
      TypedToken(LispTokenType.Rational).Select(t =>
      {
        // We trust the tokenizer and assume that t.Text will contain the expected format.
        var parts = t.Text!.Split('/');
        var numerator = int.Parse(parts[0]);
        var denominator = int.Parse(parts[1]);
        return (LispObject)new Rational(numerator, denominator);
      });

    public static readonly Parser<LispToken, LispObject> ParseAtom =
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

    public static Parser<LispToken, LispObject> ParseSExp = Rec(() => OneOf(
        ParseAtom,
        ParseList!,
        QuotedSExp!,
        QuasiQuotedSExp!,
        UnquotedSExp!,
        SplicedSExp!,
        LitListSExp!
    ));

    public static readonly Parser<LispToken, LispToken> ParseDot =
        TypedToken(LispTokenType.Dot);


    public static Parser<LispToken, LispObject> ParseListElements = Rec<LispToken, LispObject>(() =>
        ParseSExp.Many()
        .Then(
            // Expecting a dot followed by an S-expression or a quoted S-expression
            ParseDot.Then(OneOf(
                QuotedSExp!,
                ParseSExp), (_, tailExpr) => tailExpr).Optional(),
            (exprs, optionalTailExpr) =>
            {
              LispObject list = Nil;

              // If there's a tail expression, it's the final part of an improper list.
              if (optionalTailExpr.HasValue)
                list = optionalTailExpr.Value;

              foreach (var expr in exprs.Reverse())
                list = Cons(expr, list);

              return list;
            }
        )
    );

    public static readonly Parser<LispToken, LispObject> ParseList =
        TypedToken(LispTokenType.LParen)
        .Then(ParseListElements)
        .Before(TypedToken(LispTokenType.RParen));

    static Parser<LispToken, LispObject> QuotedSExp =
        TypedToken(LispTokenType.Quote)
        .Then(ParseSExp, (_, exp) => (LispObject)Cons(Intern("quote"), Cons(exp, Nil)));

    static Parser<LispToken, LispObject> QuasiQuotedSExp =
        TypedToken(LispTokenType.Backtick)
        .Then(ParseSExp, (_, exp) => (LispObject)Cons(Intern("quasiquote"), Cons(exp, Nil)));

    static Parser<LispToken, LispObject> UnquotedSExp =
        TypedToken(LispTokenType.Comma)
        .Then(ParseSExp, (_, exp) => (LispObject)Cons(Intern("unquote"), Cons(exp, Nil)));

    static Parser<LispToken, LispObject> SplicedSExp =
        TypedToken(LispTokenType.CommaAt)
        .Then(ParseSExp, (_, exp) => (LispObject)Cons(Intern("unquote-splicing"), Cons(exp, Nil)));

    static Parser<LispToken, LispObject> LitListSExp =
        TypedToken(LispTokenType.Dollar)
        .Then(ParseSExp, (_, exp) => (LispObject)Cons(Intern("list"), exp));
  }
  //========================================================================================================================================
}
