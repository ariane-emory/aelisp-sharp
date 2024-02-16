using static System.Console;
using static Ae;
using static Ae.Parser;
using Pidgin;
using static Pidgin.Parser;
using static Pidgin.Parser<Ae.Token>;
// using TokenParser = Pidgin.Parser<Ae.Token>;

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
    tokens.Print(); // tokens are printed correctly..

    var mergedResult = MergeMultilineTokens.ParseOrThrow(tokens);

    WriteLine("\nResult after merging: ");
    mergedResult.Print();

    var resultExcludingComments = mergedResult.ExcludingComments();
    
    WriteLine("\nResult after excluding comments: ");
    resultExcludingComments.Print();
}

  //==============================================================================================================================
}
