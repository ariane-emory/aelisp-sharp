using static System.Console;
using static Ae;
using static Ae.Core;
using static Ae.LispParsers;
using Pidgin;
using static Pidgin.Parser;
using static Pidgin.Parser<Ae.LispToken>;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Char = Ae.Char;
using String = Ae.String;

//================================================================================================================================
class Program
{
   //==============================================================================================================================
   static void Main()
   {

      // // EnableDebugWrite = true;

      // var path = "data/data.lisp"; // "~/lib.lisp";
      // var expandedPath = path.ExpandTilde();
      // var fileText = File.ReadAllText(expandedPath);

      // // WriteLine("File contents:");
      // // WriteLine(fileText);
      // // WriteLine("EOF");

      // var tokenizer = new QueueingLispTokenizer(fileText, exclude: IsWhitespaceToken);

      // // WriteLine("\nSTEP 1 - Raw tokens: ");
      // IEnumerable<LispToken> tokens = tokenizer.ReadAll();
      // // tokens.Print();

      // // WriteLine("\nSTEP 2 - Tokens after merging multiline tokens: ");
      // tokens = MergeMultilineTokens.ParseOrThrow(tokens);
      // //tokens.Print();

      // // WriteLine("\nSTEP 3 - Tokens after excluding comments: ");
      // tokens = tokens.ExcludingComments();
      // // tokens.Print();

      // WriteLine("\nSTEP 4 - Parsed objects: ");
      // try
      // {
      //    var obj = ParseProgram.ParseOrThrow(tokens);

      //    // WriteLine($"obj.toString(): {obj}");
      //    // WriteLine($"obj.Princ(): {obj.Princ()}");

      //    // if (obj is Pair pair)
      //    //    foreach (var o in pair)
      //    //       WriteLine(o.Princ());
      //    // else
      //    WriteLine(obj.Princ());
      // }
      // catch (ParseException e)
      // {
      //    var re = new Regex(@"unexpected[\s\S]*at line \d+, col (\d+)", RegexOptions.Multiline);
      //    var match = re.Match(e.Message);

      //    if (match.Success)
      //    {
      //       // ParseException reports 1-based index, convert it to 0-based
      //       var ix = int.Parse(match.Groups[1].Value) - 1;

      //       if (ix >= 0 && ix < tokens.Count())
      //       {
      //          var tok = tokens.ElementAt(ix);
      //          WriteLine($"ERROR: Unexpected token at line {tok.Line + 1}, column {tok.Column}: {tok}.");
      //       }
      //       else
      //       {
      //          WriteLine($"ERROR: Error at a position that could not be directly mapped to a token: {e.Message}");
      //       }
      //    }
      //    else
      //    {
      //       WriteLine($"ERROR: Parse error: {e.Message}");
      //    }

      //    Die(2, "Dying due to parse error.");
      // }

      var parent_env = Root.Spawn();
      var child_env = parent_env.Spawn();

      List<(string name, CoreFun.FuncT fun, byte minArgs, byte maxArgs, bool special)> coreFuns = [
         // exit
         ("string       ", ObjToString, 0000001, 01, false),
         ("intern       ", InternString, 000001, 01, false),
         // set-props
         // props
         ("message      ", ErrorMessage, 000001, 01, false),
         ("symbol-name  ", SymbolName, 00000001, 01, false),
         ("rational     ", NewRational, 0000002, 02, false),
         ("denom        ", Denominator, 0000001, 01, false),
         ("numer        ", Numerator, 000000001, 01, false),
         ("body         ", UserFunctionBody, 01, 01, false),
         ("params       ", UserFunctionParams, 1, 01, false),
         ("env          ", EnvOrFunEnv, 0000000, 01, false),
         ("syms         ", EnvSymbols, 00000000, 01, false),
         ("vals         ", EnvValues, 000000000, 01, false),
         // concat
         // set                                 
         ("length       ", Length, 000000000001, 01, false),
         ("id           ", Core.Id, 00000000001, 01, false),
         ("eval         ", Core.Eval, 000000001, 01, false),
         // apply                               
         ("macro        ", Core.Macro, 00000002, 15, true),

         ("improper?    ", ImproperP, 000000001, 01, false),
         ("proper?      ", ProperP, 00000000001, 01, false),
         ("tail?        ", TailP, 0000000000001, 01, false),
         ("atom?        ", AtomP, 0000000000001, 01, false),
         ("nil?         ", NilP, 00000000000001, 01, false),
         ("bound?       ", BoundP, 000000000001, 01, false),
         ("zero?        ", ZeroP, 0000000000001, 01, false),
         ("one?         ", OneP, 00000000000001, 01, false),
         ("positive?    ", PositiveP, 000000001, 01, false),
         ("negative?    ", NegativeP, 000000001, 01, false),
         ("keyword?     ", KeywordP, 0000000001, 01, false),

         ("1-           ", Minus1, 000000000001, 01, false),
         ("1+           ", Plus1, 0000000000001, 01, false),

         ("lambda       ", Core.Lambda, 0000002, 15, true),
         ("rplacd!      ", Rplacd, 000000000002, 02, false),
         ("rplaca!      ", Rplaca, 000000000002, 02, false),
         ("type         ", Core.Type, 000000001, 01, false),
         ("setq         ", Setq, 00000000000002, 15, true),
         ("id           ", Id, 0000000000000001, 01, false),
         ("not          ", Core.Not, 0000000001, 01, false),
         ("or           ", Or, 0000000000000001, 15, false),
         ("and          ", And, 000000000000001, 15, false),

         ("repeat       ", Repeat, 000000000001, 15, true),
         ("case         ", Cond, 00000000000002, 15, true),
         ("cond         ", Cond, 00000000000001, 15, true),
         ("until        ", Until, 0000000000002, 15, true),
         ("while        ", While, 0000000000002, 15, true),
         ("unless       ", Unless, 000000000002, 15, true),
         ("when         ", When, 00000000000002, 15, true),
         ("if           ", If, 0000000000000002, 15, true),
         ("list         ", List, 00000000000015, 15, false),
         ("quote        ", Quote, 0000000000001, 01, true),
         ("letrec       ", Letrec, 000000000002, 15, true),
         ("let*         ", LetStar, 00000000002, 15, true),
         ("let          ", Let, 000000000000002, 15, true),
         ("eql?         ", EqlP, 00000000000002, 15, false),
         ("eq?          ", EqP, 000000000000002, 15, false),
         ("cons         ", Core.Cons, 000000002, 02, false),
         ("cdr          ", Cdr, 000000000000001, 01, false),
         ("car          ", Car, 000000000000001, 01, false),
         ("progn        ", Progn, 0000000000015, 15, true),
      ];

      foreach ((string name, CoreFun.FuncT fun, byte minArgs, byte maxArgs, bool special) in coreFuns)
         Root.Set(Env.LookupMode.Global, Intern(name.Trim()), new CoreFun(name.Trim(), fun, minArgs, maxArgs, special));

      WriteLine(Root);

      // var (found, fobj) = Root.Lookup(Env.LookupMode.Global, Intern("cons"));
      // var args = Cons(new Integer(1), Cons(new Integer(2), Nil));
      // WriteLine(((CoreFun)fobj).Apply(Root, args).Princ());

      // WriteLine(new Rational(2, 4).Princ());

      // ConsDebugWrite = false;
      // Core.ConsDebugWrite = true;

      WriteLine("");

      // while (true)
      // {
      // WriteLine(Eval(child_env, "(cons 2 (cons 3 (cons 4 (list 5 6 (progn 8 7)))))", false).Princ());
      // WriteLine(Eval(child_env, "(progn 10 20 30)", true).Princ());
      // WriteLine(Eval(child_env, "(cons 1 (cons 2 3))").Princ());
      // WriteLine(Eval(child_env, "(length (cons 1 (cons 2 3)))").Princ());
      // WriteLine(Eval(child_env, "(lambda (x) (cons x x))", true).Princ());
      Do(child_env, "((lambda (x) (cons x x)) 8)");
      Do(child_env, "(setq qq 88 ww 77 *ee* 66)");
      Do(child_env, "(progn 2 3 4)");

      WriteLine($"\n{Root}");
      WriteLine($"\n{parent_env}");
      WriteLine($"\n{child_env}");

      Do(child_env, "(cond (nil 8) (else 14))");

      Do(child_env, "(type 22)");
      Do(child_env, "(type 4.3)");
      Do(child_env, "(type (lambda (x) (x x)))");
      Do(child_env, "(type '(1 . 2))");

      Do(child_env, "(eq? :asd :asd :asd)");
      Do(child_env, "(eq? :asd :asd :qwe)");

      Do(child_env, "(eql? 2 2 2/1)");
      Do(child_env, "(eql? 2 2 3)");

      Do(child_env, "(bound? :asd)");

      Do(child_env, "(rational 3 4)");
      Do(child_env, "(car (cons 3 4))");
      Do(child_env, "(cdr (cons 3 4))");
      Do(child_env, "(cdr (cdr (cons 3 (cons 4 nil))))");

      // }
   }

   //==============================================================================================================================
   static void Do(Env env, string input) => WriteLine(Eval(env, input, true).Princ());

   //==============================================================================================================================
}


