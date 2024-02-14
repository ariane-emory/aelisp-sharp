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

      while (!string.IsNullOrEmpty(input)) {
         var (newInput, newState, newToken) = Tokenizer.Get().NextToken(input, state);
         (input, state) = (newInput, newState);

         if (newToken is not null) {
            PrintTokens(new [] { newToken.Value}.AsEnumerable());
            
            tokens.Add(newToken.Value);
         }

      }

       return (input, state.Value, tokens);
  }

  //====================================================================================================================
  // static Tokenizer.TokenizeData TokenizeAndPrintLines(IEnumerable<string> lines)
  // {
  //   var (input, state, token) = Tokenizer.Get().NextToken(input, null);
  //   var tokenizeData = new Tokenizer.TokenizeData(null, null, null);

  //   foreach (var line in lines)
  //   {
  //     tokenizeData.Input = line;
  //     tokenizeData = TokenizeAndPrintLine(tokenizeData);

  //     if (!string.IsNullOrEmpty(tokenizeData.Input))
  //       break;
  //   }

  //   return tokenizeData;
  // }

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
//        Mode.LineByLine,
        Mode.EntireFileAtOnce
      })
    {
      var tokenizeResult = mode switch
      {
        // Mode.LineByLine => TokenizeAndPrintLines(File.ReadAllLines(filename).Select(s => s + "\n")),
        Mode.EntireFileAtOnce => TokenizeAndPrintLine(File.ReadAllText(filename), null),
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
      };

      // if (tokenizeResult.Tokens is not null)
      //   WriteLine($"Token count: {tokenizeResult.Tokens.Count}.");

      WriteLine($"\n\n---\n\n");
    }
  }
}

