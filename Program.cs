using static System.Console;
using static Ae;
using static Ae.Parser;
using Pidgin;
using static Pidgin.Parser;
using static Pidgin.Parser<Ae.Token>;
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
    var stream = new StatefulLispTokenizer(fileText, exclude: IsUninterestingToken);

    WriteLine("File contents:");
    WriteLine(fileText);
    WriteLine("EOF");

    IEnumerable<Token> tokens = stream.ReadAll();
    WriteLine("\nSTEP 1 - Before parse: ");
    tokens.Print();

    tokens = MergeMultilineTokens.ParseOrThrow(tokens);
    WriteLine("\nSTEP 2 - Result after merging: ");
    tokens.Print();

    var tokensExcludingComments = tokens.ExcludingComments();
    WriteLine("\nSTEP 3 - Result after excluding comments: ");
    tokensExcludingComments.Print();

    WriteLine("\nSTEP 4 - Parse objects: ");

    try
    {
      var completeParser = ParseAtom.Many().Then(End, (objects, end) => objects);
      var objects = completeParser.ParseOrThrow(tokensExcludingComments);

      if (objects.Count() == 0)
        Die(1, "No objects!");

      foreach (var obj in objects)
        WriteLine(obj);
    }
    catch (ParseException e)
    {
      var re = new Regex(@"unexpected[\s\S]*at line \d+, col (\d+)", RegexOptions.Multiline);
      var match = re.Match(e.Message);

      if (match.Success)
      {
        // ParseException reports 1-based index, convert it to 0-based
        var ix = int.Parse(match.Groups[1].Value) - 1;

        if (ix >= 0 && ix < tokensExcludingComments.Count())
        {
          var tok = tokensExcludingComments.ElementAt(ix);
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
