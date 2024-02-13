using static System.Console;
using System.Text.RegularExpressions;

//======================================================================================================================
// Token class
//======================================================================================================================
public record struct Token<TTokenType>(TTokenType TokenType, string Text);

//======================================================================================================================
// Tokenizer class
//======================================================================================================================
public abstract class StringTokenizer<TTokenType, TToken, TTokenizerState> where TTokenizerState : struct
{
  //====================================================================================================================
  // Types
  //====================================================================================================================
  // public enum StringTokenizerResetMode { Auto, Manual };

  //====================================================================================================================
  // Delegates 
  //====================================================================================================================
  protected delegate (TTokenizerState, TToken) ProcesTokenFun((TTokenizerState State, TToken Token) tup);
  protected delegate bool TokenDefinitionIsActiveFun(TTokenizerState state);
  protected delegate TToken CreateTokenFun(TTokenType type, string text);
  protected delegate TTokenizerState CreateTokenizerStateFun();

  //====================================================================================================================
  // Public properties
  //====================================================================================================================
  // public StringTokenizerResetMode ResetMode { get; set; }

  //====================================================================================================================
  // Private fields
  //====================================================================================================================
  private readonly CreateTokenFun _createToken;
  private readonly CreateTokenizerStateFun _createTokenizerState;

  // private TTokenizerState _state;

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
  protected StringTokenizer(CreateTokenFun createToken, CreateTokenizerStateFun createTokenizerStateFun) //, StringTokenizerResetMode resetMode)
  {
    _createToken = createToken;
    _createTokenizerState = createTokenizerStateFun;
    // _state = _createTokenizerState();
    // ResetMode = resetMode;
  }

  //====================================================================================================================
  // Instance methods
  //====================================================================================================================
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

  // public (List<TToken> Tokens, TTokenizerState State) Tokenize(string input) => Tokenize(input, _createTokenizerState());
  
  public (List<TToken> Tokens, TTokenizerState State) Tokenize(string input, TTokenizerState? state = null)
  {
    if (state is null)
      state = _createTokenizerState();
  
    var tokens = new List<TToken>();
    
    while (!string.IsNullOrEmpty(input))
    {
      bool foundMatch = false;

      foreach (var definition in _tokenDefinitions)
      {
        if (definition.DefinitionIsActive is not null && !definition.DefinitionIsActive(state.Value))
          continue;

        var match = definition.Pattern.Match(input);

        if (match.Success)
        {
          var token = _createToken(definition.Type, match.Value);

          if (match.Length == 0)
            throw new Exception($"Zero-length match found: {token}, which could lead to an infinite loop.");

          if (definition.ProcessToken is not null)
            (state, token) = definition.ProcessToken((state.Value, token));

          tokens.Add(token);
          
          foundMatch = true;
          input = input.Substring(match.Length);

          break;
        }
      }

      if (!foundMatch)
        break;

    }
    
    return (tokens, state.Value);
  }
}
