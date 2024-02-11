using System.Text.RegularExpressions;

// Tokenizer class
public abstract class StringTokenizer<TTokenType>
{

  // Token class
  public record Token(TTokenType TokenType, string Text);

  // Private fields
  private List<(TTokenType, Regex, Func<string, string>?)> TokenDefinitions { get; } = new List<(TTokenType, Regex, Func<string, string>?)>();

  // Instance methods
  protected void Add(TTokenType token, string pattern, Func<string, string>? fun = null)
  {
    pattern = "(?:" + pattern + ")";

    if (!pattern.StartsWith("^"))
      pattern = "^" + pattern;

    TokenDefinitions.Add((token, new Regex(pattern), fun));
  }

  public IEnumerable<Token> Tokenize(string input)
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

          var str = match.Value;

          if (fun is not null)
            str = fun(str);

          yield return new Token(tokenType, str);

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
