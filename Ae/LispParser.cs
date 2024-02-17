using System.Collections.Immutable;
using Pidgin;
using static Pidgin.Parser;
using static Pidgin.Parser<Ae.LispToken>;
using static Ae;

//====================================================================================================================================================
static partial class Ae
{
   //==================================================================================================================================================
   // Ae's static fields
   //==================================================================================================================================================
   private static readonly ImmutableArray<LispTokenType> MultilineCommentLispTokenTypes =
     ImmutableArray.Create(LispTokenType.MultilineCommentBegin, LispTokenType.MultilineCommentContent, LispTokenType.MultilineCommentEnd);

   private static readonly ImmutableArray<LispTokenType> MultilineStringLispTokenTypes =
     ImmutableArray.Create(LispTokenType.MultilineStringBegin, LispTokenType.MultilineStringContent, LispTokenType.MultilineStringEnd);

   //==================================================================================================================================================
   // Extension methods
   //==================================================================================================================================================
   // This one might belong in another file?
   public static IEnumerable<LispToken> ExcludingComments(this IEnumerable<LispToken> self) =>
     self.Where(t => (t.Type != LispTokenType.Comment) && (t.Type != LispTokenType.LineComment) && !MultilineCommentLispTokenTypes.Contains(t.Type));

   //==================================================================================================================================================
   // Parsers
   //==================================================================================================================================================
   public static class LispParser
   {
      //================================================================================================================================================
      // Private static methods
      //================================================================================================================================================
      private static Parser<LispToken, LispToken> TypedToken(LispTokenType tokenType) =>
         Parser<LispToken>.Token(t => t.Type == tokenType);

      //================================================================================================================================================
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

      //================================================================================================================================================
      // Private Parsers
      //================================================================================================================================================
      private static readonly Parser<LispToken, LispObject> ParseCStyleChar =
        TypedToken(LispTokenType.CStyleChar).Select(t => (LispObject)new Char(t.Text![0]));

      private static readonly Parser<LispToken, LispObject> ParseLispStyleChar =
        TypedToken(LispTokenType.LispStyleChar).Select(t => (LispObject)new Char(t.Text![0]));

      private static readonly Parser<LispToken, LispObject> ParseString =
        TypedToken(LispTokenType.String).Select(t => (LispObject)new String(t.Text!));

      private static readonly Parser<LispToken, LispObject> ParseSymbol =
        TypedToken(LispTokenType.Symbol).Select(t => Intern(t.Text!));

      private static readonly Parser<LispToken, LispObject> ParseNil =
        TypedToken(LispTokenType.Nil).Select(t => (LispObject)Nil);

      private static readonly Parser<LispToken, LispObject> ParseInteger =
        TypedToken(LispTokenType.Integer).Select(t => (LispObject)new Integer(int.Parse(t.Text!)));

      private static readonly Parser<LispToken, LispObject> ParseFloat =
        TypedToken(LispTokenType.Float).Select(t => (LispObject)new Float(float.Parse(t.Text!)));

      //================================================================================================================================================
      private static readonly Parser<LispToken, LispObject> ParseRational =
        TypedToken(LispTokenType.Rational).Select(t =>
        {
           // We trust the tokenizer and assume that t.Text will contain the expected format.
           var parts = t.Text!.Split('/');
           return (LispObject)new Rational(int.Parse(parts[0]), int.Parse(parts[1]));
        });

      //================================================================================================================================================
      private static readonly Parser<LispToken, LispObject> ParseAtom =
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

      //================================================================================================================================================
      // Public Parsers
      //================================================================================================================================================
      public static readonly Parser<LispToken, IEnumerable<LispToken>> MergeMultilineTokens =
        OneOf(Token(t => (!MultilineCommentLispTokenTypes.Contains(t.Type)) && (!MultilineStringLispTokenTypes.Contains(t.Type))),
              MergeSequence(LispTokenType.Comment, true,
                            LispTokenType.MultilineCommentBegin, LispTokenType.MultilineCommentContent, LispTokenType.MultilineCommentEnd),
              MergeSequence(LispTokenType.String, false,
                            LispTokenType.MultilineStringBegin, LispTokenType.MultilineStringContent, LispTokenType.MultilineStringEnd))
        .Many();

      //================================================================================================================================================
      public static Parser<LispToken, LispObject> ParseSExp = Rec(() => OneOf(
          ParseAtom,
          ParseList!,
          ParseQuotedSExp!,
          ParseQuasiQuotedSExp!,
          ParseUnquotedSExp!,
          ParseSplicedSExp!,
          ParseLitListSExp!
      ));

      //================================================================================================================================================
      public static readonly Parser<LispToken, LispObject> ParseProgram =
         ParseSExp.Many()
         .Select(sexps => Cons(Intern("progn"), (LispObject)sexps.Reverse().Aggregate((LispObject)Nil, (acc, sexp) => Cons(sexp, acc))));

      //================================================================================================================================================
      // More private Parsers
      //================================================================================================================================================
      private static Parser<LispToken, LispObject> ParseListElements =
         Rec(() =>
             ParseSExp.Many()
             .Then(
                TypedToken(LispTokenType.Dot)
                .Then(OneOf(
                         ParseQuotedSExp!.Select(exp => Cons(exp, Nil)),
                         ParseQuasiQuotedSExp!.Select(exp => Cons(exp, Nil)),
                         ParseUnquotedSExp!.Select(exp => Cons(exp, Nil)),
                         ParseSplicedSExp!.Select(exp => Cons(exp, Nil)),
                         ParseLitListSExp!.Select(exp => Cons(exp, Nil)),
                         ParseSExp),
                      (_, tailExpr) => tailExpr).Optional(),
                (exprs, optionalTailExpr) =>
                {
                   LispObject list = Nil;

                   if (optionalTailExpr.HasValue)
                      list = optionalTailExpr.Value;

                   foreach (var expr in exprs.Reverse())
                      list = Cons(expr, list);

                   return list;
                }
             )
         );

      private static readonly Parser<LispToken, LispObject> ParseList =
        TypedToken(LispTokenType.LParen)
        .Then(ParseListElements)
        .Before(TypedToken(LispTokenType.RParen));

      private static readonly Parser<LispToken, LispObject> ParseQuotedSExp =
        TypedToken(LispTokenType.Quote)
        .Then(ParseSExp, (_, exp) => Cons(Intern("quote"), Cons(exp, Nil)));

      private static readonly Parser<LispToken, LispObject> ParseQuasiQuotedSExp =
        TypedToken(LispTokenType.Backtick)
        .Then(ParseSExp, (_, exp) => Cons(Intern("quasiquote"), Cons(exp, Nil)));

      private static readonly Parser<LispToken, LispObject> ParseUnquotedSExp =
        TypedToken(LispTokenType.Comma)
        .Then(ParseSExp, (_, exp) => Cons(Intern("unquote"), Cons(exp, Nil)));

      private static readonly Parser<LispToken, LispObject> ParseSplicedSExp =
        TypedToken(LispTokenType.CommaAt)
        .Then(ParseSExp, (_, exp) => Cons(Intern("unquote-splicing"), Cons(exp, Nil)));

      private static readonly Parser<LispToken, LispObject> ParseLitListSExp =
        TypedToken(LispTokenType.Dollar)
        .Then(ParseSExp, (_, exp) => Cons(Intern("list"), exp));
   }
   //==================================================================================================================================================
}
