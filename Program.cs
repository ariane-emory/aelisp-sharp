using static Utility;
using static Ae;
using static System.Console;

class Program
{
  static void PrintTokens(IEnumerable<PositionedToken<TokenType>> tokens)
  {
    foreach (var (token, index) in tokens
     .Where(token => token.TokenType != TokenType.Whitespace)
     .Select((value, index) => (value, index)))
      WriteLine($"#{index}: {token}");
  }

  static bool EndsWithGarbage(List<PositionedToken<TokenType>> tokens) =>
    tokens.Any() && tokens[Math.Max(0, tokens.Count() - 1)].TokenType == TokenType.Garbage;

  record TokenizeResult(List<PositionedToken<TokenType>> Tokens, bool TokenizedAllInput, AeLispTokenizerState? State);

  static TokenizeResult TokenizeLines(IEnumerable<string> lines)
  {
    var tokens = new List<PositionedToken<TokenType>>();
    var lineNumber = 0;

    AeLispTokenizerState? state = null;
    
    foreach (var line in lines)
    {
      var result = TokenizeLine(line, state);

      state = result.State;
      
      tokens.AddRange(result.Tokens.Select(t => new PositionedToken<TokenType>(t.TokenType, t.Text, lineNumber, t.Column)).ToList());

      lineNumber++;
    }

    return new TokenizeResult(tokens, true, state);
  }

  static TokenizeResult TokenizeLine(string line, AeLispTokenizerState? state)
  {
    var result = Tokenizer.Get().Tokenize(line, state);

    PrintTokens(result.Tokens);
    
    if (EndsWithGarbage(result.Tokens))
      return new TokenizeResult(result.Tokens, false, result.State);

    return new TokenizeResult(result.Tokens, true, result.State);
  }

  enum Mode
  {
    LineByLine,
    EntireFileAtOnce,
  };

  static void Main()
  {
    var filename = "data.lisp";

    foreach (var mode in new[] { Mode.LineByLine, Mode.EntireFileAtOnce })
    {
      var tokenizeResult = mode switch
      {
        Mode.LineByLine => TokenizeLines(File.ReadAllLines(filename).Select(s => s + "\n")),
        Mode.EntireFileAtOnce => TokenizeLine(File.ReadAllText(filename), null),
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
      };

      WriteLine($"Token count: {tokenizeResult.Tokens.Count}.\n\n");
    }
  }

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
  // }
}

