using System.Collections.Immutable;
using Pidgin;
using static Pidgin.Parser;
using static Pidgin.Parser<Ae.LispToken>;
using static Ae;
using LispObjectParser = Pidgin.Parser<Ae.LispToken, Ae.LispObject>;
using LispTokenParser = Pidgin.Parser<Ae.LispToken, Ae.LispToken>;

//==========================================================================================================================================
static partial class Ae
{
   //=======================================================================================================================================
   // Parsers
   //=======================================================================================================================================
   public static class LispParsers
   {
      //=====================================================================================================================================
      // Private static methods
      //=====================================================================================================================================
      private static LispTokenParser
         TypedToken(LispTokenType tokenType) =>
         Parser<LispToken>.Token(t => t.Type == tokenType);

      //=====================================================================================================================================
      private static LispTokenParser
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

      //=====================================================================================================================================
      // Private Parsers
      //=====================================================================================================================================
      private static readonly LispObjectParser ParseCStyleChar =
        TypedToken(LispTokenType.CStyleChar).Select(t => (LispObject)new Char(t.Text![0]));

      private static readonly LispObjectParser ParseLispStyleChar =
        TypedToken(LispTokenType.LispStyleChar).Select(t => (LispObject)new Char(t.Text![0]));

      private static readonly LispObjectParser ParseString =
        TypedToken(LispTokenType.String).Select(t => (LispObject)new String(t.Text!));

      private static readonly LispObjectParser ParseSymbol =
        TypedToken(LispTokenType.Symbol).Select(t => Intern(t.Text!));

      private static readonly LispObjectParser ParseNil =
         TypedToken(LispTokenType.Nil).Select(t => Nil);

      private static readonly LispObjectParser ParseInteger =
         TypedToken(LispTokenType.Integer).Select(t => (LispObject)new Integer(int.Parse(t.Text!)));

      private static readonly LispObjectParser ParseFloat =
        TypedToken(LispTokenType.Float).Select(t => (LispObject)new Float(float.Parse(t.Text!)));

      //=====================================================================================================================================
      private static readonly LispObjectParser ParseRational =
        TypedToken(LispTokenType.Rational).Select(t =>
        {
           // We trust the tokenizer and assume that t.Text will contain the expected format.
           var parts = t.Text!.Split('/');
           return (LispObject)new Rational(int.Parse(parts[0]), int.Parse(parts[1]));
        });

      //=====================================================================================================================================
      private static readonly LispObjectParser ParseAtom =
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

      //=====================================================================================================================================
      // Public Parsers
      //=====================================================================================================================================
      public static readonly Parser<LispToken, IEnumerable<LispToken>> MergeMultilineTokens =
        OneOf(Token(t => (!MultilineCommentLispTokenTypes.Contains(t.Type)) && (!MultilineStringLispTokenTypes.Contains(t.Type))),
              MergeSequence(LispTokenType.Comment, true,
                            LispTokenType.MultilineCommentBegin, LispTokenType.MultilineCommentContent, LispTokenType.MultilineCommentEnd),
              MergeSequence(LispTokenType.String, false,
                            LispTokenType.MultilineStringBegin, LispTokenType.MultilineStringContent, LispTokenType.MultilineStringEnd))
        .Many();

      //=====================================================================================================================================
      public static LispObjectParser ParseSExp = Rec(() => OneOf(
          ParseAtom,
          ParseList!,
          ParseQuotedSExp!,
          ParseQuasiQuotedSExp!,
          ParseUnquotedSExp!,
          ParseSplicedSExp!,
          ParseLitListSExp!
      ));

      //=====================================================================================================================================
      public static readonly LispObjectParser  ParseProgram =
         ParseSExp.Many()
         .Select(sexps => Cons(Intern("progn"),
                               (LispObject)sexps.Reverse().Aggregate((LispObject)Nil, (acc, sexp) => Cons(sexp, acc))))
         .Before(End);

      //=====================================================================================================================================
      // More private Parsers
      //=====================================================================================================================================
      private static LispObjectParser ParseListElements =
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

      private static readonly LispObjectParser ParseList =
        TypedToken(LispTokenType.LParen)
        .Then(ParseListElements)
        .Before(TypedToken(LispTokenType.RParen));

      private static readonly LispObjectParser ParseQuotedSExp =
        TypedToken(LispTokenType.Quote)
        .Then(ParseSExp, (_, exp) => Cons(Intern("quote"), Cons(exp, Nil)));

      private static readonly LispObjectParser ParseQuasiQuotedSExp =
        TypedToken(LispTokenType.Backtick)
        .Then(ParseSExp, (_, exp) => Cons(Intern("quasiquote"), Cons(exp, Nil)));

      private static readonly LispObjectParser ParseUnquotedSExp =
        TypedToken(LispTokenType.Comma)
        .Then(ParseSExp, (_, exp) => Cons(Intern("unquote"), Cons(exp, Nil)));

      private static readonly LispObjectParser ParseSplicedSExp =
        TypedToken(LispTokenType.CommaAt)
        .Then(ParseSExp, (_, exp) => Cons(Intern("unquote-splicing"), Cons(exp, Nil)));

      private static readonly LispObjectParser ParseLitListSExp =
        TypedToken(LispTokenType.Dollar)
        .Then(ParseSExp, (_, exp) => Cons(Intern("list"), exp));
   }

   //=======================================================================================================================================
}
