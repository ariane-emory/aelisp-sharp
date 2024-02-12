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

  static void PrintTokens(List<PositionedToken<TokenType>> tokens)
  {
    foreach (var (token, index) in tokens
     .Where(token => token.TokenType != TokenType.Whitespace)
     .Select((value, index) => (value, index)))
      WriteLine($"#{index}: {token}");
  }

  static int TokenizeFile(string filename)
  {
    var lineNumber = 0;
    var tokens = new List<PositionedToken<TokenType>>();
    var improperList = new List<object>();
    var improperLis = new List<object>();
    var totalTokens = 0;

    foreach (var line in File.ReadLines(filename))
    {
      var newTokens = Tokenizer.Get().Tokenize($"{line}", false)
        .Select(t => new PositionedToken<TokenType>(t.TokenType, t.Text, lineNumber, t.Column)).ToList();

      totalTokens += newTokens.Count;

      if (newTokens.Any())
      {
        var lastNewToken = newTokens.ToList()[Math.Max(0, newTokens.Count() - 1)];

        if (lastNewToken.TokenType == TokenType.Garbage)
          Die(1, $"Failed to tokenize the entire input, remaining text: \"{lastNewToken.Text}\"");
      }

      PrintTokens(newTokens);

      tokens.AddRange(newTokens);

      // Die if we tokenized nothing:
      if (!tokens.Any())
        Die(1, "No tokens!");

      // Die if we didn't tokenize everything:
      var lastToken = tokens.ToList()[Math.Max(0, tokens.Count() - 1)];

      if (lastToken.TokenType == TokenType.Garbage)
        Die(1, $"Failed to tokenize the entire input -  remaining text: \"{lastToken.Text}\"");

      lineNumber++;
    }

    return totalTokens;
  }

  static void Main()
  {
    var filename = "data.lisp";
    var totalTokens = TokenizeFile(filename);

    Die(0, $"Tokenized all input, {totalTokens} tokens.");

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

