﻿using static System.Console;
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
         ("print        ", PrintObj, 00000001, 15, false),
         ("princ        ", PrincObj, 00000001, 15, false),
         ("put-raw      ", PutObj, 00000001, 15, false),
         ("write        ", WriteObj, 00000001, 15, false), // add quoting to write!
         ("nl           ", Newline, 00000000, 00, false),
         ("string       ", ObjToString, 00000001, 01, false),
         ("+            ", NotImplemented, 00000, 15, false),
         ("intern       ", InternString, 0000001, 01, false),
         // set-props
         // props
         ("message      ", ErrorMessage, 0000001, 01, false),
         ("symbol-name  ", SymbolName, 000000001, 01, false),
         ("rational     ", NewRational, 00000002, 02, false),
         ("denom        ", Denominator, 00000001, 01, false),
         ("numer        ", Numerator, 0000000001, 01, false),
         ("body         ", UserFunctionBody, 001, 01, false),
         ("params       ", UserFunctionParams, 1, 01, false),
         ("env          ", EnvOrFunEnv, 00000000, 01, false),
         ("syms         ", EnvSymbols, 000000000, 01, false),
         ("vals         ", EnvValues, 0000000000, 01, false),
         ("set          ", Setq, 000000000000002, 15, false),
         ("concat       ", Concat, 0000000000000, 15, false),
         ("length       ", Length, 0000000000001, 01, false),
         ("id           ", Core.Id, 000000000001, 01, false),
         ("eval         ", Core.Eval, 0000000001, 01, false),
         ("apply        ", Core.Apply, 000000002, 15, true),
         ("macro        ", Core.NewMacro, 000002, 15, true),
         ("improper?    ", ImproperP, 0000000001, 01, false),
         ("proper?      ", ProperP, 000000000001, 01, false),
         ("tail?        ", TailP, 00000000000001, 01, false),
         ("atom?        ", AtomP, 00000000000001, 01, false),
         ("nil?         ", NilP, 000000000000001, 01, false),
         ("bound?       ", BoundP, 0000000000001, 01, false),
         ("zero?        ", ZeroP, 00000000000001, 01, false),
         ("one?         ", OneP, 000000000000001, 01, false),
         ("positive?    ", PositiveP, 0000000001, 01, false),
         ("negative?    ", NegativeP, 0000000001, 01, false),
         ("keyword?     ", KeywordP, 00000000001, 01, false),
         ("1-           ", Minus1, 0000000000001, 01, false),
         ("1+           ", Plus1, 00000000000001, 01, false),
         ("lambda       ", Core.NewLambda, 00002, 15, true),
         ("rplacd!      ", Rplacd, 0000000000002, 02, false),
         ("rplaca!      ", Rplaca, 0000000000002, 02, false),
         ("type         ", Core.Type, 0000000001, 01, false),
         ("setq         ", Setq, 000000000000002, 15, true),
         ("id           ", Id, 00000000000000001, 01, false),
         ("not          ", Core.Not, 00000000001, 01, false),
         ("or           ", Or, 00000000000000001, 15, false),
         ("and          ", And, 0000000000000001, 15, false),
         ("repeat       ", Repeat, 0000000000001, 15, true),
         ("case         ", Cond, 000000000000002, 15, true),
         ("cond         ", Cond, 000000000000001, 15, true),
         ("until        ", Until, 00000000000002, 15, true),
         ("while        ", While, 00000000000002, 15, true),
         ("unless       ", Unless, 0000000000002, 15, true),
         ("when         ", When, 000000000000002, 15, true),
         ("if           ", If, 00000000000000002, 15, true),
         ("list         ", List, 000000000000000, 15, false),
         ("quote        ", Quote, 00000000000001, 01, true),
         ("letrec       ", Letrec, 0000000000002, 15, true),
         ("let*         ", LetStar, 000000000002, 15, true),
         ("let          ", Let, 0000000000000002, 15, true),
         ("eql?         ", EqlP, 000000000000002, 15, false),
         ("eq?          ", EqP, 0000000000000002, 15, false),
         ("cons         ", Core.Cons, 0000000002, 02, false),
         ("cdr          ", Cdr, 0000000000000001, 01, false),
         ("car          ", Car, 0000000000000001, 01, false),
         ("progn        ", Progn, 00000000000001, 15, true),
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

      //==============================================================================================================================
      void Do(string input) => WriteLine(Eval(child_env, input, true).Princ());

      WriteLine("");

      Do("((lambda (x) (cons x x)) 8)");
      Do("(setq qq 88 ww 77 *ee* 66)");
      Do("(progn 2 3 4)");

      WriteLine($"\n{Root}");
      WriteLine($"\n{parent_env}");
      WriteLine($"\n{child_env}");

      Do("(cond (nil 8) (else 14))");
      Do("(type 22)");
      Do("(type 4.3)");
      Do("(type (lambda (x) (x x)))");
      Do("(type '(1 . 2))");
      Do("(eq? :asd :asd :asd)");
      Do("(eq? :asd :asd :qwe)");
      Do("(eql? 2 2 2/1)");
      Do("(eql? 2 2 3)");
      Do("(bound? :asd)");
      Do("(rational 3 4)");
      Do("(car (cons 3 4))");
      Do("(cdr (cons 3 4))");
      Do("(cdr (cdr (cons 3 (cons 4 nil))))");
      Do("(concat)");
      Do("(concat \"hello\" \"world\")");
      Do("(concat \"hello\" nil \"world\")");
      Do("(concat nil \"hello\" nil \"world\")");
      Do("(setq x 2)");
      Do("(setq y 3)");
      Do("(setq lst '(3 4))");
      Do("(write 1 ?  \"hello\" ?  33 ?  :nil ?\n)");
      Do("(put-raw 1 ?  \"hello\" ?  33 ?  :nil ?\n)");
      Do("$($(1 2) $(3 4) $(5 6) '(1 2 . 3))");
      WriteLine("----");
      Do("(apply list '1 'x x '(y 4))");
      WriteLine("----");
      Do("(apply list 1 x (list y 4))");
      WriteLine("----");
      Do("(apply list 1 x '(y 4))");
      WriteLine("----");
      Do("(setq z '(2 3 4))");
      Do("(apply list 1 z)");
      WriteLine("");

      // Do("(raw \"hello\")");
      Do("(write \"hello\")");
      Do("(princ \"hello\")");
      Do("(print \"hello\")");
      // Do("(raw 'hello)");
      Do("(write 'hello)");
      Do("(princ 'hello)");
      Do("(print 'hello)");
      // Do("(raw '(1 \"hello\" 'world))");
      Do("(write '(1 \"hello\" 'world))");
      Do("(princ '(1 \"hello\" 'world))");
      Do("(print '(1 \"hello\" 'world))");

   }

   //==============================================================================================================================
}


