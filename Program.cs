using static System.Console;
using static Ae;
using static Ae.LispParser;
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

    WriteLine("File contents:");
    WriteLine(fileText);
    WriteLine("EOF");

    var tokenizer = new QueueingLispTokenizer(fileText, exclude: IsUninterestingToken);

    // WriteLine("\nSTEP 1 - Raw tokens: ");
    IEnumerable<LispToken> tokens = tokenizer.ReadAll();
    // tokens.Print();

    // WriteLine("\nSTEP 2 - Tokens after merging multiline tokens: ");
    tokens = MergeMultilineTokens.ParseOrThrow(tokens);
    //tokens.Print();

    WriteLine("\nSTEP 3 - Tokens after excluding comments: ");
    tokens = tokens.ExcludingComments();
    tokens.Print();

    WriteLine("\nSTEP 4 - Parsed objects: ");
    try
    {
      var obj = ParseProgram.ParseOrThrow(tokens);
      WriteLine($"obj.toString(): {obj}");
      WriteLine($"obj.Write(): {obj.Write()}");

      // // (1 2 3 4)
      // var list = new Pair(new Integer(1), new Pair(new Integer(2), new Pair(new Integer(3), new Pair(new Integer(4), Nil))));
      // WriteLine($"list.toString(): {list}");
      // WriteLine($"list.Write(): {list.Write()}");

      // // (1 2 3 . 4)
      // var improper_list = new Pair(new Integer(1), new Pair(new Integer(2), new Pair(new Integer(3), new Integer(4))));
      // WriteLine($"improper_list.toString(): {improper_list}");
      // WriteLine($"improper_list.Write(): {improper_list.Write()}");

      // var objects = ParseAtom.Many().Then(End, (objects, end) => objects).ParseOrThrow(tokens);
      
      // if (objects.Count() == 0)
      //   Die(1, "No objects!");
      
      // foreach (var obj in objects)
      //   WriteLine(obj);
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
          WriteLine($"ERROR: Unexpected token at line {tok.Line}, column {tok.Column}: {tok}.");
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
  }

  //==============================================================================================================================
}
