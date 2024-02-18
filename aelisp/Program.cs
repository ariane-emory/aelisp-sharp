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
       ("eql?", Core.EqlP, 02, 2, true),
       ("eq?", Core.EqP, 002, 2, true),
       ("cons", Core.Cons, 02, 2, true),
       ("cdr ", Core.Cdr, 001, 1, true),
       ("car ", Core.Car, 001, 1, true),
      ];

      foreach ((string name, CoreFunction.FuncT fun, byte minArgs, byte maxArgs, bool special) in coreFuns)
         root_env.Set(Env.LookupMode.Global, Intern(name.Trim()), new CoreFunction(name, fun, minArgs, maxArgs, special));

      WriteLine(root_env);

      // var (found, fobj) = root_env.Lookup(Env.LookupMode.Global, Intern("cons"));
      // var args = Cons(new Integer(1), Cons(new Integer(2), Nil));
      // WriteLine(((CoreFunction)fobj).Apply(root_env, args).Princ());

      // WriteLine(new Rational(2, 4).Princ());

      WriteLine(Eval(child_env, "(cons 2 (cons 3 (cons 4 nil)))").Princ());
   }

   //==============================================================================================================================
}
