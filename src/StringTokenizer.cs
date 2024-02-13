using static System.Console;
using System.Text.RegularExpressions;

//======================================================================================================================
// Token class
//======================================================================================================================
public record struct Token<TTokenType>(TTokenType TokenType, string Text);

//======================================================================================================================
// StringTokenizer class
//=====================================================================================================================
public abstract class StringTokenizer<TTokenType, TToken, TTokenizeState> where TTokenizeState : struct
{
  //====================================================================================================================
  // Delegates 
  //====================================================================================================================
  protected delegate TToken CreateTokenFun(TTokenType type, string text);
  protected delegate TTokenizeState CreateTokenizerStateFun();
  protected delegate (TTokenizeState, TToken) ProcesTokenFun((TTokenizeState State, TToken Token) tup);
  protected delegate bool TokenDefinitionIsActiveFun(TTokenizeState state);

  //====================================================================================================================
  // Private fields
  //====================================================================================================================
  private readonly CreateTokenFun _createToken;
  private readonly CreateTokenizerStateFun _createTokenizerState;

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

  public record struct TokenizeData(string? Input, List<TToken>? Tokens, TTokenizeState? State);

  public TokenizeData Tokenize(string? Input, List<TToken>? Tokens = null, TTokenizeState? State = null) =>
    Tokenize(new TokenizeData(Input, Tokens, State));

  public TokenizeData Tokenize(TokenizeData tokenizeData)
  {
    if (tokenizeData.State is null)
      tokenizeData.State = _createTokenizerState();

    if (tokenizeData.Tokens is null)
      tokenizeData.Tokens = new List<TToken>();

    if (tokenizeData.Input is null)
      return tokenizeData;

    while (!string.IsNullOrEmpty(tokenizeData.Input))
    {
      var foundMatch = false;

      foreach (var definition in _tokenDefinitions)
      {
        if (definition.DefinitionIsActive is not null && !definition.DefinitionIsActive(tokenizeData.State.Value))
          continue;

        var match = definition.Pattern.Match(tokenizeData.Input);

        if (match.Success)
        {
          if (match.Length == 0)
            throw new Exception($"Zero-length match found: \"{match.Value}\", which could lead to an infinite loop.");

          foundMatch = true;

          var token = _createToken(definition.Type, match.Value);

          if (definition.ProcessToken is not null)
            (tokenizeData.State, token) = definition.ProcessToken((tokenizeData.State.Value, token));

          tokenizeData.Tokens.Add(token);
          tokenizeData.Input = tokenizeData.Input.Substring(match.Length);

          break;
        }
      }

      if (!foundMatch)
        break;
    }

    return tokenizeData;
  }
}
