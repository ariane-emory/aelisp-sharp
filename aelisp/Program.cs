using static System.Console;
using static Ae;
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

      var root_env = new Env(Nil);
      var parent_env = root_env.Spawn();
      var child_env = parent_env.Spawn();

      List<(string name, CoreFun.FuncT fun, byte minArgs, byte maxArgs, bool special)> coreFuns = [
         ("body         ", Core.UserFunctionBody, 001, 01, false),
         ("params       ", Core.UserFunctionParams,01, 01, false),
         ("env          ", Core.UserFunctionEnv, 0001, 01, false), // 0-arg version not yet supported.
                  
         ("syms         ", Core.EnvSymbols, 000000001, 01, false), // 0-arg version not yet supported.
         ("vals         ", Core.EnvValues, 0000000001, 01, false), // 0-arg version not yet supported.
         ("numer        ", Core.Numerator, 0000000001, 01, false),
         ("denom        ", Core.Denominator, 00000001, 01, false),
         ("message      ", Core.ErrorMessage, 0000001, 01, false),
         ("symbol-name  ", Core.SymbolName, 000000001, 01, false),
         ("intern       ", Core.InternString, 0000001, 01, false),
         ("string       ", Core.ObjToString, 00000001, 01, false),
         // set                                  
         ("length       ", Core.Length, 0000000000001, 01, false),
         ("eval         ", Core.Eval, 000000000000001, 01, false),
         // apply                                
         ("macro        ", Core.Macro, 00000000000002, 15, true),
                                                 
         ("proper?      ", Core.ProperP, 000000000001, 01, false),
         ("tail?        ", Core.TailP, 00000000000001, 01, false),
         ("atom?        ", Core.AtomP, 00000000000001, 01, false),
         ("nil?         ", Core.NilP, 000000000000001, 01, false),
         ("bound?       ", Core.BoundP, 0000000000001, 01, false),
         ("zero?        ", Core.ZeroP, 00000000000001, 01, false), 
         ("one?         ", Core.OneP, 000000000000001, 01, false), 
         ("keyword?     ", Core.KeywordP, 00000000001, 01, false),
                                                 
         ("lambda       ", Core.Lambda, 0000000000002, 15, true),
         ("rplacd!      ", Core.Rplacd, 0000000000002, 02, false),
         ("rplaca!      ", Core.Rplaca, 0000000000002, 02, false),
         ("type         ", Core.Type, 000000000000001, 01, false),
         ("setq         ", Core.Setq, 000000000000002, 15, true),
         ("id           ", Core.Id, 00000000000000001, 01, false),
         ("not          ", Core.Not, 0000000000000001, 01, false),
         ("or           ", Core.Type, 000000000000001, 15, false),
         ("and          ", Core.Type, 000000000000001, 15, false),
                                                 
         ("repeat       ", Core.Repeat, 0000000000001, 15, true),
         ("case         ", Core.Cond, 000000000000002, 15, true),
         ("cond         ", Core.Cond, 000000000000001, 15, true),
         ("until        ", Core.Until, 00000000000002, 15, true),
         ("while        ", Core.While, 00000000000002, 15, true),
         ("unless       ", Core.Unless, 0000000000002, 15, true),
         ("when         ", Core.When, 000000000000002, 15, true),
         ("if           ", Core.If, 00000000000000002, 15, true),
         ("list         ", Core.List, 000000000000015, 15, false),
         ("quote        ", Core.Quote, 00000000000001, 01, true),
         ("letrec       ", Core.Letrec, 0000000000002, 15, true),
         ("let*         ", Core.LetStar, 000000000002, 15, true),
         ("let          ", Core.Let, 0000000000000002, 15, true),
         ("eql?         ", Core.EqlP, 000000000000002, 15, false),
         ("eq?          ", Core.EqP, 0000000000000002, 15, false),
         ("cons         ", Core.Cons, 000000000000002, 02, false),
         ("cdr          ", Core.Cdr, 0000000000000001, 01, false),
         ("car          ", Core.Car, 0000000000000001, 01, false),
         ("progn        ", Core.Progn, 00000000000015, 15, true),
      ];

      foreach ((string name, CoreFun.FuncT fun, byte minArgs, byte maxArgs, bool special) in coreFuns)
         root_env.Set(Env.LookupMode.Global, Intern(name.Trim()), new CoreFun(name.Trim(), fun, minArgs, maxArgs, special));

      WriteLine(root_env);

      // var (found, fobj) = root_env.Lookup(Env.LookupMode.Global, Intern("cons"));
      // var args = Cons(new Integer(1), Cons(new Integer(2), Nil));
      // WriteLine(((CoreFun)fobj).Apply(root_env, args).Princ());

      // WriteLine(new Rational(2, 4).Princ());

      ConsDebugWrite = false;
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

      WriteLine($"\n{root_env}");
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

      // }
   }

   //==============================================================================================================================
   static void Do(Env env, string input) => WriteLine(Eval(env, input, true).Princ());

   //==============================================================================================================================
}


