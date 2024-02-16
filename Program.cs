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
    //============================================================================================================================

    var filename = "data/data.lisp";
    var fileText = File.ReadAllText(filename);
    var stream = new TokenStream(fileText, exclude: IsUninterestingToken);

    var tokens = stream.ReadAll();

    WriteLine("\nBefore parse: ");
    tokens.Print();

    var mergedResult = MergeMultilineTokens.ParseOrThrow(tokens);

    WriteLine("\nResult after merging: ");
    mergedResult.Print();

    var resultExcludingComments = mergedResult.ExcludingComments();
    
    WriteLine("\nResult after excluding comments: ");
    resultExcludingComments.Print();
}

  //==============================================================================================================================
}
