using static System.Console;
using static Ae;
using static Ae.LispParsers;
using Pidgin;
using static Pidgin.Parser;
using static Pidgin.Parser<Ae.LispToken>;
using System.Text.RegularExpressions;

//================================================================================================================================
class Program
{
   //==============================================================================================================================
   static void Main()
   {
      // EnableDebugWrite = true;

      var path = "data/data.lisp"; // "~/lib.lisp";
      var expandedPath = path.ExpandTilde();
      var fileText = File.ReadAllText(expandedPath);

      // WriteLine("File contents:");
      // WriteLine(fileText);
      // WriteLine("EOF");

      var tokenizer = new QueueingLispTokenizer(fileText, exclude: IsWhitespaceToken);

      // WriteLine("\nSTEP 1 - Raw tokens: ");
      IEnumerable<LispToken> tokens = tokenizer.ReadAll();
      // tokens.Print();

      // WriteLine("\nSTEP 2 - Tokens after merging multiline tokens: ");
      tokens = MergeMultilineTokens.ParseOrThrow(tokens);
      //tokens.Print();

      // WriteLine("\nSTEP 3 - Tokens after excluding comments: ");
      tokens = tokens.ExcludingComments();
      // tokens.Print();

      WriteLine("\nSTEP 4 - Parsed objects: ");
      try
      {
         var obj = ParseProgram.ParseOrThrow(tokens);

         // WriteLine($"obj.toString(): {obj}");
         // WriteLine($"obj.Write(): {obj.Write()}");

         // if (obj is Pair pair)
         //    foreach (var o in pair)
         //       WriteLine(o.Write());
         // else
         WriteLine(obj.Write());
      }
      catch (ParseException e)
      {
         var re = new Regex(@"unexpected[\s\S]*at line \d+, col (\d+)", RegexOptions.Multiline);
         var match = re.Match(e.Message);

         if (match.Success)
         {
            // ParseException reports 1-based index, convert it to 0-based
            var ix = int.Parse(match.Groups[1].Value) - 1;

            if (ix >= 0 && ix < tokens.Count())
            {
               var tok = tokens.ElementAt(ix);
               WriteLine($"ERROR: Unexpected token at line {tok.Line + 1}, column {tok.Column}: {tok}.");
            }
            else
            {
               WriteLine($"ERROR: Error at a position that could not be directly mapped to a token: {e.Message}");
            }
         }
         else
         {
            WriteLine($"ERROR: Parse error: {e.Message}");
         }

         Die(2, "Dying due to parse error.");
      }

      var root_env = new Env(
         Nil,
         Cons(Intern("x"), Cons(Intern("y"), Cons(Intern("z"), Nil))),
         Cons(new Integer(1), Cons(new Integer(2), Cons(new Integer(3), Nil))));

      var parent_env = new Env(
         root_env,
         Cons(Intern("xx"), Cons(Intern("yy"), Cons(Intern("zz"), Nil))),
         Cons(new Integer(11), Cons(new Integer(22), Cons(new Integer(33), Nil))));

      var child_env = new Env(
         parent_env,
         Cons(Intern("xxx"), Cons(Intern("yyy"), Cons(Intern("zzz"), Nil))),
         Cons(new Integer(111), Cons(new Integer(222), Cons(new Integer(333), Nil))));

      WriteLine(child_env.Write());

      var (found, fobj) = child_env.Lookup(Env.LookupMode.Nearest, (Symbol)Intern("x"));
      WriteLine($"found: {found}, fobj: {fobj}");

      (found, fobj) = child_env.Lookup(Env.LookupMode.Nearest, (Symbol)Intern("y"));
      WriteLine($"found: {found}, fobj: {fobj}");
      
      (found, fobj) = child_env.Lookup(Env.LookupMode.Nearest, (Symbol)Intern("z"));
      WriteLine($"found: {found}, fobj: {fobj}");
      
      (found, fobj) = child_env.Lookup(Env.LookupMode.Nearest, (Symbol)Intern("xx"));
      WriteLine($"found: {found}, fobj: {fobj}");
      
      (found, fobj) = child_env.Lookup(Env.LookupMode.Nearest, (Symbol)Intern("nil"));
      WriteLine($"found: {found}, fobj: {fobj}");
      
      (found, fobj) = child_env.Lookup(Env.LookupMode.Nearest, (Symbol)Intern("t"));
      WriteLine($"found: {found}, fobj: {fobj}");

      (found, fobj) = child_env.Lookup(Env.LookupMode.Nearest, (Symbol)Intern("yyy"));
      WriteLine($"found: {found}, fobj: {fobj}");

      WriteLine("");
      WriteLine($"root:   {root_env}");
      WriteLine($"parent: {parent_env}");
      WriteLine($"child:  {child_env}");
      
      child_env.Set(Env.LookupMode.Global, (Symbol)Intern("xx"), new Integer(9999));

      WriteLine("");
      WriteLine($"root:   {root_env}");
      WriteLine($"parent: {parent_env}");
      WriteLine($"child:  {child_env}");

      child_env.Set(Env.LookupMode.Nearest, (Symbol)Intern("xx"), new Integer(3333));

      WriteLine("");
      WriteLine($"root:   {root_env}");
      WriteLine($"parent: {parent_env}");
      WriteLine($"child:  {child_env}");
      
      child_env.Set(Env.LookupMode.Local, (Symbol)Intern("xx"), new Integer(6666));

      WriteLine("");
      WriteLine($"root:   {root_env}");
      WriteLine($"parent: {parent_env}");
      WriteLine($"child:  {child_env}"); 
      
      WriteLine("\nStop.");
   }

   //==============================================================================================================================
}
