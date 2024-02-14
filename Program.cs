using static Utility;
using static Ae;
using static System.Console;

class Program
{

  static readonly List<TokenType> Excluded = new List<TokenType>
  {
    TokenType.Whitespace,
    TokenType.LineComment,
    TokenType.MultilineCommentB,
    TokenType.MultilineCommentC,
    TokenType.MultilineCommentE,
    TokenType.MultilineCommentN,
    TokenType.MultilineCommentS,
    TokenType.Newline,
  };

  //====================================================================================================================
  static void PrintTokens(IEnumerable<PositionedToken<TokenType>> tokens)
  {
    foreach (var (token, index) in tokens
             .Where(token => !Excluded.Contains(token.TokenType))
             .Select((value, index) => (value, index)))
      WriteLine($"#{index}: {token}");
  }

  //====================================================================================================================
  static (string? Input, AeLispTokenizerState? State, List<PositionedToken<TokenType>> Tokens)
  TokenizeLine(string? input, AeLispTokenizerState? state)
  {
    var tokens = new List<PositionedToken<TokenType>>();

    foreach (var (newInput, newState, newToken) in Tokenizer.Get().Tokenize(input, state))
    {
      (input, state) = (newInput, newState);

      if (newToken is not null)
        tokens.Add(newToken.Value);
    }

    PrintTokens(tokens);

    return (input, state, tokens);
  }


  //====================================================================================================================
  static (string? Input, AeLispTokenizerState? State, List<PositionedToken<TokenType>> Tokens)
  TokenizeLines(IEnumerable<string> lines)
  {
    var tokens = new List<PositionedToken<TokenType>>();
    
    (string? input, AeLispTokenizerState? state) = (null, null);

    foreach (var line in lines)
    {
      var (newInput, newState, newTokens) = TokenizeLine(line, state);

      (input, state) = (newInput, newState);

      tokens.AddRange(newTokens);
    }

    return (input, state, tokens);
  }

  //====================================================================================================================
  enum Mode { LineByLine, EntireFileAtOnce, };

  //====================================================================================================================
  static void Main()
  {
    var filename = "data.lisp";

    foreach (var mode in new[] {
        Mode.LineByLine,
        Mode.EntireFileAtOnce
      })
    {
      var tokenizeResult = mode switch
      {
        Mode.LineByLine => TokenizeLines(File.ReadAllLines(filename).Select(s => s + "\n")),
        Mode.EntireFileAtOnce => TokenizeLine(File.ReadAllText(filename), null),
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
      };

      if (tokenizeResult.Tokens is not null)
        WriteLine($"Token count: {tokenizeResult.Tokens.Count}.");

      WriteLine($"\n\n---\n\n");
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

