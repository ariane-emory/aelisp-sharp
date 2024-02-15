using System.Text.RegularExpressions;
using static System.Console;
using static Ae;
using static Utility;
using TokenParser = Pidgin.Parser<Ae.Token, Ae.Token>;

//======================================================================================================================
class Program
{
  //====================================================================================================================
  static void Main()
  {
    var filename = "data/data.lisp";
    var fileText = File.ReadAllText(filename);
    var stream = new TokenStream(fileText, exclude: Ae.IsUninterestingToken);
    var take = 32;
    var ary = new Token[take];
    var read = stream.Read(ary);

    WriteLine($"");
    ary.Take(read).Print();

    Func<Token, bool> isMultilineCommentBegin = t => t.TokenType == TokenType.MultilineCommentBeginning;
    Func<Token, bool> isMultilineCommentContent = t => t.TokenType == Ae.TokenType.MultilineCommentContent;
    Func<Token, bool> isMultilineCommentEnd = t => t.TokenType == Ae.TokenType.MultilineCommentEnd;
    Func<Token, bool> isSomeOtherToken = t => t.TokenType == Ae.TokenType.MultilineCommentEnd;
    
        // var multilineCommentParser =
        //   from begin in multilineCommentBegin
        //   from contents in multilineCommentContent.Many()
        //   from end in multilineCommentEnd
        //   select new Ae.PositionedToken<Ae.TokenType>(
        //     Ae.TokenType.Comment,
        //     string.Join("", new[] { begin }.Concat(contents).Append(end).Select(t => t.Text)),
        //     begin.Line,
        //     begin.Column
        //     );

        // Example usage
        // Assuming you have a method to convert your IEnumerable<AeToken> into a parser's input stream
        // var tokens = ... // Your token stream here
        // var result = multilineStringParser.ParseOrFallback(tokens, fallbackValue);
        // var commentResult = multilineCommentParser.ParseOrFallback(tokens, fallbackValue);

    }

  //====================================================================================================================
}

