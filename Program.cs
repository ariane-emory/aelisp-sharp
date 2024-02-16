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
    var stream = new TokenStream(fileText, exclude: IsUninterestingToken);

    WriteLine("File contents:");
    WriteLine(fileText);
    WriteLine("EOF");

    var tokens = stream.ReadAll();
    WriteLine("\nSTEP 1 - Before parse: ");
    tokens.Print();

    var mergedResult = MergeMultilineTokens.ParseOrThrow(tokens);
    WriteLine("\nSTEP 2 - Result after merging: ");
    mergedResult.Print();

    var resultExcludingComments = mergedResult.ExcludingComments();
    WriteLine("\nSTEP 3 - Result after excluding comments: ");
    resultExcludingComments.Print();

    WriteLine("\nSTEP 4 - Parse objects: ");

    try
    {
      var completeParser = ParseAtom.Many().Then(End, (objects, end) => objects);
      var objects = completeParser.ParseOrThrow(resultExcludingComments);

      if (objects.Count() == 0)
        Die(1, "No objects!");

      foreach (var obj in objects)
        WriteLine(obj);
    }
    catch (ParseException e)
    {
      var re = new Regex(@"^\s+at line \d+, col (\d+)", RegexOptions.Multiline);
      var match = re.Match(e.Message);

      if (match.Success)
      {
        var ix = int.Parse(match.Groups[1].Value) - 1;
        var tok = resultExcludingComments.ToList()[ix];

        WriteLine($"Error at line {tok.Line}, column {tok.Column} at token: {tok}.");
        Die(2, "Parse error!");
      }
      else
      {
        Die(2, $"Parse error: {e}");
      }

    }
  }

  //==============================================================================================================================
}
