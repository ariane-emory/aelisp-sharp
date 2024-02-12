using static System.Console;
using System.Text.RegularExpressions;

//======================================================================================================================
// Token class
//======================================================================================================================
public record Token<TTokenType>(TTokenType TokenType, string Text) // : IToken<TTokenType>
{
  public static Token<TTokenType> Create(TTokenType tokenType, string text) => new Token<TTokenType>(tokenType, text);
}

//======================================================================================================================
// Tokenizer class
//======================================================================================================================
public abstract class StringTokenizer<TTokenType, TToken, TTokenizerState> // where TToken : IToken<TTokenType>
{
  // Private fields
  private readonly Func<TTokenType, string, TToken> _createToken;

  private TTokenizerState _state;

  private List<(TTokenType type, Regex patern, Func<TToken, TToken>? transform)> _tokenDefinitions =
    new List<(TTokenType type, Regex patern, Func<TToken, TToken>? transform)>();

  public StringTokenizer(Func<TTokenType, string, TToken> createToken, TTokenizerState state)
  {
    _createToken = createToken;
    _state = state;
  }

  // Instance methods
  protected void Add(TTokenType token, string pattern, Func<TToken, TToken>? fun = null)
  {
    pattern = "(?:" + pattern + ")";

    if (!pattern.StartsWith("^"))
      pattern = "^" + pattern;

    _tokenDefinitions.Add((token, new Regex(pattern, RegexOptions.Singleline), fun));
  }

  protected virtual void Restart() { }

  public IEnumerable<TToken> Tokenize(string input)
  {
    Restart();

    while (!string.IsNullOrEmpty(input))
    {
      // WriteLine($"Enter while at \"{input}\".");

      bool foundMatch = false;

      foreach (var (tokenType, regex, fun) in _tokenDefinitions)
      {
        // WriteLine($"Try matching a {tokenType} token with \"{regex}\" at \"{input}\".");

        var match = regex.Match(input);

        if (match.Success)
        {
          // WriteLine($"Matched a {tokenType} token: \"{match.Value}\"");

          var token = _createToken(tokenType, match.Value);

          if (match.Length == 0)
            throw new Exception($"Zero-length match found: {token}, which could lead to an infinite loop.");

          if (fun is not null)
            token = fun(token);

          input = input.Substring(match.Length);

          // WriteLine($"Advance input to \"{input}\".");
          foundMatch = true;

          // WriteLine($"Yielding the {tokenType}.");
          yield return token;
          // WriteLine($"Yielded the {tokenType}.");

          break; // Successfully matched and processed, move to next segment of input
        }
      }

      if (!foundMatch)
        break; // No more matches found, exit the loop
    }
  }
}
