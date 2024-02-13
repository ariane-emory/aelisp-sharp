using static Utility;
using static Ae;
using static System.Console;

class Program
{
  enum Mode
  {
    Line,
    File,
  };

  record struct TokenizeResult(List<PositionedToken<TokenType>> Tokens, bool? TokenizedAllInput);

  static void PrintTokens(List<PositionedToken<TokenType>> tokens)
  {
    foreach (var (token, index) in tokens
     .Where(token => token.TokenType != TokenType.Whitespace)
     .Select((value, index) => (value, index)))
      WriteLine($"#{index}: {token}");
  }

  static bool CheckForGarbage(List<PositionedToken<TokenType>> tokens) =>
    (! tokens.Any()) || tokens[Math.Max(0, tokens.Count() - 1)].TokenType == TokenType.Garbage;

  static TokenizeResult TokenizeFileByLines(string filename)
  {
    var tokenizeResult = new TokenizeResult();
    tokenizeResult.Tokens = new List<PositionedToken<TokenType>>();

    var lineNumber = 0;

    foreach (var line in File.ReadLines(filename))
    {
      var newTokens = Tokenizer.Get().Tokenize($"{line}", false)
        .Select(t => new PositionedToken<TokenType>(t.TokenType, t.Text, lineNumber, t.Column)).ToList();

      if (CheckForGarbage(newTokens))
      {
        tokenizeResult.TokenizedAllInput = false;
        
        return tokenizeResult;
      }

      PrintTokens(newTokens);

      tokenizeResult.Tokens.AddRange(newTokens);

      lineNumber++;
    }

    tokenizeResult.TokenizedAllInput = true;

    return tokenizeResult;
  }

  static void Main()
  {
    var filename = "data.lisp";
    var mode = Mode.Line;
    var tokenizeResult = TokenizeFileByLines(filename);

    // Die(0, $"Tokenized all input, {totalTokens} tokens.");

    // Pair properList = Cons(new Symbol("one"), Cons(new Symbol("two"), Cons(new Symbol("three"), Nil)));
    // Pair improperList = Cons(new Symbol("one"), Cons(new Integer(37), Cons(new Rational(3, 4), new Symbol("four"))));

    // WriteLine(properList);
    // WriteLine(Write(properList));

    // foreach (var obj in properList)
    //   WriteLine(obj);

    // WriteLine(improperList);
    // WriteLine(Write(improperList));

    // foreach (var obj in improperList)
    //   WriteLine(obj);

    // WriteLine(Write(new Lambda(Nil, Nil, Nil)));
    // WriteLine(Write(new Lambda(Nil, Nil, Nil)));
    // WriteLine(Write(new Lambda(Nil, Nil, Nil)));

    // WriteLine("Done.");

    // Ae.Object symbolsList = Cons(new Symbol("one"), Cons(new Symbol("two"), Cons(new Symbol("three"), Nil)));

    // Intern(ref symbolsList, "two");
    // WriteLine(symbolsList);
    // Intern(ref symbolsList, "four");
    // WriteLine(symbolsList);
    // Intern(ref symbolsList, "two");
    // WriteLine(symbolsList);
    // Intern(ref symbolsList, "four");
    // WriteLine(symbolsList);
  }
}

