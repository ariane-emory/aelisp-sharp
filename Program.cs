using static System.Console;
using static Ae;
using static Ae.Parser;
using Pidgin;

//================================================================================================================================
class Program
{
  //==============================================================================================================================
  static void Main()
  {
    // EnableDebugWrite = true;

    var path = "data/data.lisp"; // "~/lib.lisp";
    var expandedPath = path.Expand();
    var fileText = File.ReadAllText(expandedPath);
    var stream = new TokenStream(fileText, exclude: IsUninterestingToken);

    var tokens = stream.ReadAll();
    WriteLine("\nSTEP 1 - Before parse: ");
    tokens.Print();

    var mergedResult = MergeMultilineTokens.ParseOrThrow(tokens);
    WriteLine("\nSTEP 2 - Result after merging: ");
    mergedResult.Print();

    var resultExcludingComments = mergedResult.ExcludingComments();
    WriteLine("\nSTEP 3 - Result after excluding comments: ");
    resultExcludingComments.Print();
  }

  //==============================================================================================================================
}
