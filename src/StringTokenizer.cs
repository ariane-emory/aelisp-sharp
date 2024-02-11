using System.Text.RegularExpressions;

// Tokenizer class
public abstract class StringTokenizer<TTokenType>
{
  // Token class
  public record Token(TTokenType TokenType, string Text);

  // // Token class
  // public class Token : IEquatable<Token>
  // {
  //   // Fields
  //   public TTokenType TokenType { get; }
  //   public string Text { get; }

  //   // Constructor
  //   public Token(TTokenType token, string value)
  //   {
  //     TokenType = token;
  //     Text = value;
  //   }

  //   // Instance methods
  //   public bool Equals(Token? other)
  //   {
  //     if (other is null)
  //       return false;

  //     bool typeEquals = EqualityComparer<TTokenType>.Default.Equals(TokenType, other.TokenType);
  //     bool textEquals = Text == other.Text;

  //     return typeEquals && textEquals;
  //   }

  //   public override bool Equals(object? obj) => obj is Token token && Equals(token);

  //   public override int GetHashCode() => System.HashCode.Combine(TokenType, Text);

  //   public static bool operator ==(Token left, Token right) =>
  //     (left is null && right is null) || (left is not null && left.Equals(right));

  //   public static bool operator !=(Token left, Token right) => !(left == right);
  // }
  
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
