
using System.Text.RegularExpressions;
using System.Collections.Generic;

// Tokenizer class
public class StringTokenizer<TokenType>
{
   // Token class
   public class Token
   {
      // Fields
      public TokenType TokenType { get; }
      public string Text { get; }

      // Constructor
      public Token(TokenType token, string value)
      {
         TokenType = token;
         Text = value;
      }
   }

   // Private fields
   private List<(TokenType, Regex, Func<string, string>?)> _tokenDefinitions { get; set; }

   // Constructor
   public StringTokenizer() => _tokenDefinitions = new List<(TokenType, Regex, Func<string, string>?)>();

   // Instance methods
   public void Add(TokenType token, string pattern, Func<string, string>? fun = null)
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
