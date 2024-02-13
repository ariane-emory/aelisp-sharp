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
             .Where(token => ! Excluded.Contains(token.TokenType))
             .Select((value, index) => (value, index)))
      WriteLine($"#{index}: {token}");
  }

  //====================================================================================================================
  static Tokenizer.TokenizeData TokenizeLines(IEnumerable<string> lines)
  {
    var tokenizeData = new Tokenizer.TokenizeData(null, null, null);

    foreach (var line in lines)
    {
      tokenizeData.Input = line;
      tokenizeData = TokenizeLine(tokenizeData);

      if (!string.IsNullOrEmpty(tokenizeData.Input))
        break;
    }

    return tokenizeData;
  }

  //====================================================================================================================
  static Tokenizer.TokenizeData TokenizeLine(Tokenizer.TokenizeData tokenizeData)
  {
    tokenizeData = Tokenizer.Get().Tokenize(tokenizeData);

    if (tokenizeData.Tokens is not null)
      PrintTokens(tokenizeData.Tokens);

    return tokenizeData;
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
        Mode.EntireFileAtOnce => TokenizeLine(new Tokenizer.TokenizeData(File.ReadAllText(filename), null, null)),
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

