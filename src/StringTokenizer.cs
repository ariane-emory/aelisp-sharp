using static System.Console;
using System.Text.RegularExpressions;

//======================================================================================================================
// Token class
//======================================================================================================================
public record struct Token<TTokenType>(TTokenType TokenType, string Text);

//======================================================================================================================
// StringTokenizer class
//=====================================================================================================================
public abstract class StringTokenizer<TTokenType, TToken, TTokenizeState>
  where TTokenType : struct
  where TToken : struct
  where TTokenizeState : struct
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

  private readonly List<(TTokenType Type,
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

  //====================================================================================================================
  public IEnumerable<(string? Input, TTokenizeState State, TToken? Token)>
  Tokenize(string? input, TTokenizeState? state = null)
  {
    if (state is null)
      state = _createTokenizerState();

    if (string.IsNullOrEmpty(input))
    {
      yield return (input, state.Value, null);
      yield break;
    }

    while (!string.IsNullOrEmpty(input))
    {
      var foundMatch = false;

      foreach (var definition in _tokenDefinitions)
      {
        if (definition.DefinitionIsActive is not null && !definition.DefinitionIsActive(state.Value))
          continue;

        var match = definition.Pattern.Match(input);

        if (match.Success)
        {
          if (match.Length == 0)
            throw new Exception($"Zero-length match found: \"{match.Value}\", which could lead to an infinite loop.");

          foundMatch = true;

          var token = _createToken(definition.Type, match.Value);

          if (definition.ProcessToken is not null)
            (state, token) = definition.ProcessToken((state.Value, token));

          input = input.Substring(match.Length);

          yield return (input, state.Value, token);

          break;
        }
      }

      if (!foundMatch)
        yield break;
    }

    yield return (input, state.Value, null);
  }
}
