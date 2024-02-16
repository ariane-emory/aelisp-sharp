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
    var objects = ParseAtom.Many().ParseOrThrow(resultExcludingComments);

    if (objects.Count() == 0)
      Die(1, "No objects!");
    
    foreach (var obj in objects)
      WriteLine(obj);
  }

  //==============================================================================================================================
}
