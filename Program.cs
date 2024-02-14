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
  static void PrintTokens(IEnumerable<AeToken> tokens)
  {
    foreach (var (token, index) in tokens
             .Where(token => !ExcludedTokenTypes.Contains(token.TokenType))
             .Select((value, index) => (value, index)))
      WriteLine($"#{index}: {token}");
  }

  //====================================================================================================================
  static (string? Input, AeLispTokenizerState State, List<AeToken> Tokens)
  TokenizeAndPrintLine(string? input, AeLispTokenizerState? state = null)
  {
    var tokens = new List<AeToken>();

    while (!string.IsNullOrEmpty(input)) // infinite loop risk, fix!
    {
      var (newInput, newState, newToken) = Tokenizer.Get().NextToken(input, state);

      if (newToken is null)
        throw new ApplicationException($"No token at \"{newInput}\"!");

      (input, state) = (newInput, newState);

      tokens.Add(newToken.Value);
    }

    PrintTokens(tokens);

    return (input, state!.Value, tokens);
  }

  //====================================================================================================================
  static (string? Input, AeLispTokenizerState State, List<AeToken> Tokens)
  TokenizeAndPrintLines(IEnumerable<string> lines, AeLispTokenizerState? state = null)
  {
    var tokens = new List<AeToken>();

    foreach (var line in lines)
    {
      var (leftoverInput, newState, newTokens) = TokenizeAndPrintLine(line, state);
      state = newState;

      if (!string.IsNullOrEmpty(leftoverInput))
        throw new ApplicationException($"leftover input \"{leftoverInput}\".");

      tokens.AddRange(newTokens);
    }

    return (null, state!.Value, tokens);
  }

  //====================================================================================================================
  public class AeLispTokenizerTokenStream : Pidgin.ITokenStream<AeToken>
  {
    public void Return(ReadOnlySpan<AeToken> leftovers) { }
    public int ChunkSizeHint => 16;

    private readonly List<AeToken> _waiting;
    private string? _input;

#pragma warning disable CS0169
    private AeLispTokenizerState? _state;
#pragma warning restore CS0169

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

    foreach (var mode in new[] {
        Mode.LineByLine,
        Mode.EntireFileAtOnce
      })
    {
      var tokenizeResult = mode switch
      {
        Mode.LineByLine => TokenizeAndPrintLines(File.ReadAllLines(filename).Select(s => s + "\n"), null),
        Mode.EntireFileAtOnce => TokenizeAndPrintLine(File.ReadAllText(filename), null),
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
      };

      WriteLine($"Token count: {tokenizeResult.Tokens.Count}");

      // if (tokenizeResult.Tokens is not null)
      //   WriteLine($"Token count: {tokenizeResult.Tokens.Count}.");

      WriteLine($"\n\n---\n\n");
    }
  }
}

