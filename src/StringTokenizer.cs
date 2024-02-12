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
public abstract class StringTokenizer<TTokenType, TToken> // where TToken : IToken<TTokenType>
{
  // Private fields
  private readonly Func<TTokenType, string, TToken> _tokenCreator;

  private List<(TTokenType, Regex, Func<TToken, TToken>?)> TokenDefinitions { get; } =
    new List<(TTokenType, Regex, Func<TToken, TToken>?)>();

  public StringTokenizer(Func<TTokenType, string, TToken> tokenCreator)
  {
    _tokenCreator = tokenCreator;
  }

  // Instance methods
  protected void Add(TTokenType token, string pattern, Func<TToken, TToken>? fun = null)
  {
    pattern = "(?:" + pattern + ")";

    if (!pattern.StartsWith("^"))
      pattern = "^" + pattern;

    TokenDefinitions.Add((token, new Regex(pattern), fun));
  }

  public IEnumerable<TToken> Tokenize(string input)
  {
    while (!string.IsNullOrEmpty(input))
    {
      bool foundMatch = false;

      foreach (var (tokenType, regex, fun) in TokenDefinitions)
      {
        var match = regex.Match(input);

        if (match.Success)
        {
          if (match.Length == 0)
            throw new Exception("Zero-length match found, which could lead to an infinite loop.");

          WriteLine($"Matched a {tokenType} token: \"{match.Value}\"");
          
          var token = _tokenCreator(tokenType, match.Value);

          if (fun is not null)
            token = fun(token);
          
          WriteLine($"Yield the {tokenType}.");
          
          yield return token;

          input = input.Substring(match.Length);
          foundMatch = true;

          break; // Successfully matched and processed, move to next segment of input
        }
      }

      if (!foundMatch)
        break; // No more matches found, exit the loop
    }
  }
}
