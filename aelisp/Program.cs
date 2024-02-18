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

      var root_env = new Env(Nil, Nil, Nil);
      var parent_env = new Env(root_env, Nil, Nil);
      var child_env = new Env(parent_env, Nil, Nil);

      List<(string name, CoreFun.FuncT fun, byte minArgs, byte maxArgs, bool special)> coreFuns = [
         ("let*   ", Core.LetStar, 0000000002, 15, true),
         ("let    ", Core.Let, 00000000000002, 15, true),
         ("repeat ", Core.Repeat, 00000000001, 15, true),
         ("case   ", Core.Cond, 0000000000002, 15, true),
         ("cond   ", Core.Cond, 0000000000001, 15, true),
         ("setq   ", Core.Setq, 0000000000002, 15, true),
         ("until  ", Core.Until, 000000000002, 15, true),
         ("while  ", Core.While, 000000000002, 15, true),
         ("unless ", Core.Unless, 00000000002, 15, true),
         ("when   ", Core.When, 0000000000002, 15, true),
         ("if     ", Core.If, 000000000000002, 15, true),
         ("length ", Core.Length, 00000000001, 01, false),
         ("tail?  ", Core.TailP, 000000000001, 01, false),
         ("nil?   ", Core.NilP, 0000000000001, 01, false),
         ("atom?  ", Core.AtomP, 000000000001, 01, false),
         ("rplacd!", Core.Rplacd, 00000000002, 02, false),
         ("rplaca!", Core.Rplaca, 00000000002, 02, false),
         ("id     ", Core.Id, 000000000000001, 01, false),
         ("not    ", Core.Not, 00000000000001, 01, false),
         ("macro  ", Core.Macro, 000000000002, 15, true),
         ("lambda ", Core.Lambda, 00000000002, 15, true),
         ("progn  ", Core.Progn, 000000000015, 15, true),
         ("eval   ", Core.Eval, 0000000000001, 01, false),
         ("list   ", Core.List, 0000000000015, 15, false),
         ("quote  ", Core.Quote, 000000000001, 01, true),
         ("eql?   ", Core.EqlP, 0000000000002, 02, false),
         ("eq?    ", Core.EqP, 00000000000002, 02, false),
         ("cons   ", Core.Cons, 0000000000002, 02, false),
         ("cdr    ", Core.Cdr, 00000000000001, 01, false),
         ("car    ", Core.Car, 00000000000001, 01, false),
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
      WriteLine(Eval(child_env, "((lambda (x) (cons x x)) 8)", true).Princ());
      WriteLine(Eval(child_env, "(setq qq 88 ww 77 *ee* 66)", true).Princ());
      WriteLine(Eval(child_env, "(progn 2 3 4)", true).Princ());
      
      WriteLine($"\n{root_env}");
      WriteLine($"\n{parent_env}");
      WriteLine($"\n{child_env}");

      WriteLine(Eval(child_env, "(cond (nil 8) (else 14))", true).Princ());

      foreach (var o in ((Pair)Eval(child_env, "(list 2 4 6)", true)).ToList())
            WriteLine(o);

        // }
    }

   //==============================================================================================================================
}

