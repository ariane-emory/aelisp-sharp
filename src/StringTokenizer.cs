using static System.Console;
using System.Text.RegularExpressions;

//======================================================================================================================
// Token class
//======================================================================================================================
public record struct Token<TTokenType>(TTokenType TokenType, string Text);

//======================================================================================================================
// Tokenizer class
//======================================================================================================================
public abstract class StringTokenizer<TTokenType, TToken, TTokenizerState>
{
  //====================================================================================================================
  // Delegates 
  //====================================================================================================================
  protected delegate (TTokenizerState, TToken) ProcesTokenFun((TTokenizerState State, TToken Token) tup);
  protected delegate bool TokenDefinitionIsActiveFun(TTokenizerState state);
  protected delegate TToken CreateTokenFun(TTokenType type, string text);
  protected delegate TTokenizerState CreateTokenizerStateFun();

  //====================================================================================================================
  // Private fields
  //====================================================================================================================
  private readonly CreateTokenFun _createToken;
  private readonly CreateTokenizerStateFun _createTokenizerState;

  private TTokenizerState _state;

  private List<(TTokenType Type,
                Regex Pattern,
                ProcesTokenFun? ProcessToken,
                TokenDefinitionIsActiveFun? DefinitionIsActive)>
  _tokenDefinitions =
    new List<(TTokenType Type,
              Regex Pattern,
              ProcesTokenFun? ProcessToken,
              TokenDefinitionIsActiveFun? DefinitionIsActive)>();

  //====================================================================================================================
  // Constructor
  //====================================================================================================================
  protected StringTokenizer(CreateTokenFun createToken, CreateTokenizerStateFun createTokenizerStateFun)
  {
    _createToken = createToken;
    _createTokenizerState = createTokenizerStateFun;
    _state = _createTokenizerState();
  }

  // Instance methods
  protected void Add(TTokenType type,
                     string pattern,
                     ProcesTokenFun? processToken = null,
                     TokenDefinitionIsActiveFun? definitionIsActive = null)
  {
    pattern = "(?:" + pattern + ")";

    if (!pattern.StartsWith("^"))
      pattern = "^" + pattern;

    _tokenDefinitions.Add((type, new Regex(pattern, RegexOptions.Singleline), processToken, definitionIsActive));
  }

  private void Restart() {
    _state = _createTokenizerState();
  }

  public IEnumerable<TToken> Tokenize(string input)
  {
    Restart();

    while (!string.IsNullOrEmpty(input))
    {
      // WriteLine($"Enter while at \"{input}\".");

      bool foundMatch = false;

      // foreach (var (tokenType, regex, processToken) in _tokenDefinitions)
      foreach (var definition in _tokenDefinitions)
      {
        // WriteLine($"Try matching a {tokenType} token with \"{regex}\" at \"{input}\".");

        if (!definition.DefinitionIsActive(_state))
          continue;
 
        var match = definition.Pattern.Match(input);

        if (match.Success)
        {
          // WriteLine($"Matched a {tokenType} token: \"{match.Value}\"");

          var token = _createToken(definition.Type, match.Value);

          if (match.Length == 0)
            throw new Exception($"Zero-length match found: {token}, which could lead to an infinite loop.");

          if (definition.ProcessToken is not null)
            (_state, token) = definition.ProcessToken((_state, token));

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
