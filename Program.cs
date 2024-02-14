using static System.Console;
using static Ae;
using static Utility;
using Pidgin;
using static Pidgin.Parser;
using static Pidgin.Parser<Ae.PositionedToken<Ae.TokenType>>; // no error
using AeToken = Ae.PositionedToken<Ae.TokenType>;

class Program
{
  static readonly List<TokenType> ExcludedTokenTypes = new List<TokenType>
  {
    TokenType.Whitespace,
    TokenType.LineComment,
    TokenType.MultilineCommentBeginning,
    TokenType.MultilineCommentContent,
    TokenType.MultilineCommentEnd,
    TokenType.Comment,
    TokenType.Newline,
  };

  //====================================================================================================================
  static (string? Input, AeLispTokenizerState? State, List<AeToken> Tokens)
  TokenizeAndPrintLine(string? input, AeLispTokenizerState? state)
  {
    var tokens = new List<AeToken>();

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
  static void PrintTokens(IEnumerable<AeToken> tokens)
  {
    foreach (var (token, index) in tokens
             .Where(token => !ExcludedTokenTypes.Contains(token.TokenType))
             .Select((value, index) => (value, index)))
      WriteLine($"#{index}: {token}");
  }

  //====================================================================================================================
  static (string? Input, AeLispTokenizerState? State, List<AeToken> Tokens)
  TokenizeAndPrintLines(IEnumerable<string> lines)
  {
    var tokens = new List<AeToken>();

    AeLispTokenizerState? state = null;

    foreach (var line in lines)
    {
      var (leftoverInput, newState, newTokens) = TokenizeAndPrintLine(line, state);

      state = newState;

      tokens.AddRange(newTokens);
    }

    return (null, state, tokens);
  }

  //====================================================================================================================
  // // GPT's idea:
  // public class WrappedTokenizer
  // {
  //   public IEnumerable<AeToken> Tokenize(string input, ref AeLispTokenizerState? state)
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

  //====================================================================================================================
  public class WrappedTokenizer
  {
    private AeLispTokenizerState? _state = null;

    public void Reset()
    {
      _state = null;
    }

    public IEnumerable<AeToken> Tokenize(string input)
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
  public class AeLispTokenizerTokenStream : Pidgin.ITokenStream<AeToken>
  {
    public void Return(ReadOnlySpan<AeToken> leftovers) { }
    public int ChunkSizeHint => 16;

    private readonly List<AeToken> _waiting;
    private string? _input;
    private AeLispTokenizerState? _state;

    public AeLispTokenizerTokenStream(string input)
    {
      _input = input;
      _waiting = new List<AeToken>();
    }

    public int Read(Span<AeToken> buffer)
    {
      var requested = buffer.Length;

      while (_waiting.Count < requested)
      {
        /* do stuff */
      }

      return 0;
    }
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

            if (true)
            {
                var tokenizeResult = mode switch
                {
                    Mode.LineByLine => TokenizeAndPrintLines(File.ReadAllLines(filename).Select(s => s + "\n")),
                    Mode.EntireFileAtOnce => TokenizeAndPrintLine(File.ReadAllText(filename), null),
                    _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
                };

                if (tokenizeResult.Tokens is not null)
                    WriteLine($"Token count: {tokenizeResult.Tokens.Count}.");

                WriteLine($"\n\n---\n\n");
            }
            else
            {
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
  }
}

