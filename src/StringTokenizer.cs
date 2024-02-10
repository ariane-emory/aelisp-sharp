
using System.Text.RegularExpressions;
using System.Collections.Generic;

// Tokenizer class
public class StringTokenizer<TTokenType>
{
  // Token class
  public class Token : IEquatable<Token>
  {
    // Fields
    public TTokenType TokenType { get; }
    public string Text { get; }

    // Constructor
    public Token(TTokenType token, string value)
    {
      TokenType = token;
      Text = value;
    }

    // Implement IEquatable<Token>.Equals
    public bool Equals(Token? other)
    {
      if (other is null)
      {
        return false;
      }

      // Use EqualityComparer<T>.Default to handle equality comparison for TTokenType,
      // which works for both value and reference types and respects IEquatable<T> if implemented.
      bool typeEquals = EqualityComparer<TTokenType>.Default.Equals(TokenType, other.TokenType);
      bool textEquals = Text == other.Text;

      return typeEquals && textEquals;
    }

    // Override object.Equals
    public override bool Equals(object? obj)
    {
      return obj is Token token && Equals(token);
    }

    // Override object.GetHashCode
    public override int GetHashCode()
    {
      // Use System.HashCode if available (.NET Core 2.1+), for better hashing.
      // Otherwise, you can use a simple approach like XOR (^) or a combination of field hash codes.
      // Adjust the hashing strategy based on your needs and .NET version.

#if NETCOREAPP2_1_OR_GREATER
      return System.HashCode.Combine(TokenType, Text);
#else
        unchecked // Overflow is fine, just wrap
        {
            int hash = 17;
            // Suitable nullity checks etc, of course :)
            hash = hash * 23 + TokenType.GetHashCode();
            hash = hash * 23 + (Text != null ? Text.GetHashCode() : 0);
            return hash;
        }
#endif
    }

    // Implement equality operator
    public static bool operator ==(Token left, Token right)
    {
      if (ReferenceEquals(left, null))
      {
        return ReferenceEquals(right, null);
      }

      return left.Equals(right);
    }

    // Implement inequality operator
    public static bool operator !=(Token left, Token right)
    {
      return !(left == right);
    }
  }

  // Private fields
  private List<(TTokenType, Regex, Func<string, string>?)> _tokenDefinitions { get; set; }

  // Constructor
  public StringTokenizer() => _tokenDefinitions = new List<(TTokenType, Regex, Func<string, string>?)>();

  // Instance methods
  public void Add(TTokenType token, string pattern, Func<string, string>? fun = null)
  {
    pattern = "(?:" + pattern + ")";

    if (!pattern.StartsWith("^"))
      pattern = "^" + pattern;

    _tokenDefinitions.Add((token, new Regex(pattern), fun));
  }

  public IEnumerable<Token> Tokenize(string input)
  {
    while (!string.IsNullOrEmpty(input))
    {
      bool foundMatch = false;

      foreach (var (tokenType, regex, fun) in _tokenDefinitions)
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
