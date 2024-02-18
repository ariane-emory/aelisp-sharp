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

      var (found, fobj) = child_env.Lookup(Env.LookupMode.Nearest, Intern("x"));
      WriteLine($"found: {found}, fobj: {fobj}");

      (found, fobj) = child_env.Lookup(Env.LookupMode.Nearest, Intern("y"));
      WriteLine($"found: {found}, fobj: {fobj}");
      
      (found, fobj) = child_env.Lookup(Env.LookupMode.Nearest, Intern("z"));
      WriteLine($"found: {found}, fobj: {fobj}");
      
      (found, fobj) = child_env.Lookup(Env.LookupMode.Nearest, Intern("xx"));
      WriteLine($"found: {found}, fobj: {fobj}");
      
      (found, fobj) = child_env.Lookup(Env.LookupMode.Nearest, Intern("nil"));
      WriteLine($"found: {found}, fobj: {fobj}");
      
      (found, fobj) = child_env.Lookup(Env.LookupMode.Nearest, Intern("t"));
      WriteLine($"found: {found}, fobj: {fobj}");

      (found, fobj) = child_env.Lookup(Env.LookupMode.Nearest, Intern("yyy"));
      WriteLine($"found: {found}, fobj: {fobj}");

      WriteLine("");
      WriteLine($"root:   {root_env}");
      WriteLine($"parent: {parent_env}");
      WriteLine($"child:  {child_env}");
      
      child_env.Set(Env.LookupMode.Global, Intern("xx"), new Integer(9999));

      WriteLine("");
      WriteLine($"root:   {root_env}");
      WriteLine($"parent: {parent_env}");
      WriteLine($"child:  {child_env}");

      child_env.Set(Env.LookupMode.Nearest, Intern("xx"), new Integer(3333));

      WriteLine("");
      WriteLine($"root:   {root_env}");
      WriteLine($"parent: {parent_env}");
      WriteLine($"child:  {child_env}");
      
      child_env.Set(Env.LookupMode.Local, Intern("xx"), new Integer(6666));

      WriteLine("");
      WriteLine($"root:   {root_env}");
      WriteLine($"parent: {parent_env}");
      WriteLine($"child:  {child_env}");

      child_env.Set(Env.LookupMode.Local, Intern("xxyyxx"), new Integer(6666));

      WriteLine("");
      WriteLine($"root:   {root_env}");
      WriteLine($"parent: {parent_env}");
      WriteLine($"child:  {child_env}");

      child_env.Set(Env.LookupMode.Local, Intern("yyxxyy"), new Integer(6666));

      WriteLine("");
      WriteLine($"root:   {root_env}");
      WriteLine($"parent: {parent_env}");
      WriteLine($"child:  {child_env}");

      child_env.Set(Env.LookupMode.Global, Intern("yyxxyy"), new Integer(6666));

      WriteLine("");
      WriteLine($"root:   {root_env}");
      WriteLine($"parent: {parent_env}");
      WriteLine($"child:  {child_env}");

      Symbol sym1 = (Symbol)Intern("xx");
      LispObject sym2 = Intern("xx");
      var (found1, obj1) = child_env.Lookup(Env.LookupMode.Nearest, sym1);
      var (found2, obj2) = child_env.Lookup(Env.LookupMode.Nearest, sym2);
      
      WriteLine(obj1);
      WriteLine(obj2);

      WriteLine("\nStop.");
 
      var proper = (Pair)Cons(new Integer(1), Cons(new Integer(2), Cons(new Integer(3), Cons(new Integer(4), Nil))));
      var improper = (Pair)Cons(new Integer(1), Cons(new Integer(2), Cons(new Integer(3), new Integer(4))));

      int len = 0;
      foreach (var o in proper)
         len++;

      int len2 = 0;
      foreach (var o in improper)
         len2++;

      WriteLine(len);
      WriteLine(len2);
      WriteLine(proper.Length);
      WriteLine(improper.Length);

      WriteLine(Intern("xx").Eval(child_env));
      WriteLine(Intern(":xx").Eval(child_env));
      WriteLine(Intern("nil").Eval(child_env));
      WriteLine(Nil.Eval(child_env));
    }

   //==============================================================================================================================
}
