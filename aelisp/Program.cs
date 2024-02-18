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

      List<(string name, CoreFunction.FuncT fun, byte minArgs, byte maxArgs, bool special)> coreFuns = [
         ("macro ", Core.Macro, 002, 15, true),
         ("lambda", Core.Lambda, 02, 15, true),
         ("progn ", Core.Progn, 015, 15, false),
         ("eval  ", Core.Eval, 0001, 01, false),
         ("list  ", Core.List, 0015, 15, false),
         ("quote ", Core.Quote, 001, 01, true),
         ("eql?  ", Core.EqlP, 0002, 02, false),
         ("eq?   ", Core.EqP, 00002, 02, false),
         ("cons  ", Core.Cons, 0002, 02, false),
         ("cdr   ", Core.Cdr, 00001, 01, false),
         ("car   ", Core.Car, 00001, 01, false),
      ];

      foreach ((string name, CoreFunction.FuncT fun, byte minArgs, byte maxArgs, bool special) in coreFuns)
         root_env.Set(Env.LookupMode.Global, Intern(name.Trim()), new CoreFunction(name, fun, minArgs, maxArgs, special));

      WriteLine(root_env);

      // var (found, fobj) = root_env.Lookup(Env.LookupMode.Global, Intern("cons"));
      // var args = Cons(new Integer(1), Cons(new Integer(2), Nil));
      // WriteLine(((CoreFunction)fobj).Apply(root_env, args).Princ());

      // WriteLine(new Rational(2, 4).Princ());

      ConsDebugWrite = false;
      // Core.ConsDebugWrite = true;
      
      WriteLine("");

      // while (true)
      // {
           WriteLine(Eval(child_env, "(cons 2 (cons 3 (cons 4 (list 5 6 (progn 8 7)))))", false).Princ());
           WriteLine(Eval(child_env, "(progn 10 20 30)", true).Princ());
           WriteLine(Eval(child_env, "(lambda (x) (+ x x))", true).Princ());
      // }
    }

   //==============================================================================================================================
}

