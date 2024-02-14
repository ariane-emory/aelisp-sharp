using static Utility;
using static Ae;
using static System.Console;

class Program
{

  static readonly List<TokenType> ExcludedTokenTypes = new List<TokenType>
  {
    TokenType.Whitespace,
    TokenType.LineComment,
    TokenType.MultilineCommentBeginning,
    TokenType.MultilineCommentContent,
    TokenType.MultilineCommentEnd,
    TokenType.InlineComment,
    TokenType.Newline,
  };

  //====================================================================================================================
  static void PrintTokens(IEnumerable<PositionedToken<TokenType>> tokens)
  {
    foreach (var (token, index) in tokens
             .Where(token => !ExcludedTokenTypes.Contains(token.TokenType))
             .Select((value, index) => (value, index)))
      WriteLine($"#{index}: {token}");
  }

  //====================================================================================================================
  public class WrappedTokenizer
  {
    private AeLispTokenizerState? _state = null;

    public void Reset()
    {
      _state = null;
    }

    // // GPT's idea:
    // public class WrappedTokenizer
    // {
    //   public IEnumerable<PositionedToken<TokenType>> Tokenize(string input, ref AeLispTokenizerState? state)
    //   {
    //     foreach (var (leftoverInput, newState, newToken) in Tokenizer.Get().Tokenize(input, state))
    //     {
    //       state = newState;
    //       if (newToken is null && !string.IsNullOrEmpty(leftoverInput))
    //         yield break;
    //       else if (newToken is not null && !ExcludedTokenTypes.Contains(newToken.Value.TokenType))
    //         yield return newToken.Value;
    //     }
    //   }
    // }

    public IEnumerable<PositionedToken<TokenType>> Tokenize(string input)
    {
      var state = _state;

      foreach (var (leftoverInput, newState, newToken) in Tokenizer.Get().Tokenize(input, state))
      {
        state = newState;

        if (newToken is null && !string.IsNullOrEmpty(leftoverInput))
          yield break;
        else if (newToken is null)
          break;
        else if (newToken is not null && !ExcludedTokenTypes.Contains(newToken.Value.TokenType))
          yield return newToken.Value;
      }

      _state = state;
    }
  }

  //====================================================================================================================
  static (string? Input, AeLispTokenizerState? State, List<PositionedToken<TokenType>> Tokens)
  TokenizeLine(string? input, AeLispTokenizerState? state)
  {
    var tokens = new List<PositionedToken<TokenType>>();

    foreach (var (leftoverInput, newState, newToken) in Tokenizer.Get().Tokenize(input, state))
    {
      (input, state) = (leftoverInput, newState);

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

    AeLispTokenizerState? state = null;

    foreach (var line in lines)
    {
      var (leftoverInput, newState, newTokens) = TokenizeLine(line, state);

      state = newState;

      tokens.AddRange(newTokens);
    }

    return (null, state, tokens);
  }

  //====================================================================================================================
  enum Mode { LineByLine, EntireFileAtOnce, };

  //====================================================================================================================
  static void Main()
  {
    var filename = "data.lisp";
    var wrapped = new WrappedTokenizer();

    foreach (var mode in new[] {
        Mode.LineByLine,
        Mode.EntireFileAtOnce
      })
    {
      // var tokenizeResult = mode switch
      // {
      //   Mode.LineByLine => TokenizeLines(File.ReadAllLines(filename).Select(s => s + "\n")),
      //   Mode.EntireFileAtOnce => TokenizeLine(File.ReadAllText(filename), null),
      //   _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
      // };

      // if (tokenizeResult.Tokens is not null)
      //   WriteLine($"Token count: {tokenizeResult.Tokens.Count}.");

      int count = 0;

      wrapped.Reset();

      switch (mode)
      {
        case Mode.LineByLine:
          foreach (var line in File.ReadAllLines(filename).Select(s => s + "\n")) // experiment with removing the \n and using allowing $ instead of \n in tok defs.
            foreach (var token in wrapped.Tokenize(line))
              WriteLine($"#{count++}: {token}");
          break;
        case Mode.EntireFileAtOnce:
          foreach (var token in wrapped.Tokenize(File.ReadAllText(filename)))
            WriteLine($"#{count++}: {token}");
          break;
      }


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

