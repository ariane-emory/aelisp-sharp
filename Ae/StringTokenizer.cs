using System.Collections.Immutable;
using static System.Console;
using System.Text.RegularExpressions;

//======================================================================================================================
static partial class Ae
{
  //====================================================================================================================
  // StringTokenizer class
  //====================================================================================================================
  public abstract class StringTokenizer<TTokenType, TToken, TTokenizeState>
    where TTokenType : struct
    where TToken : struct
    where TTokenizeState : struct
  {
    //==================================================================================================================
    // Delegates 
    //==================================================================================================================
    protected delegate TToken CreateTokenFun(TTokenType type, string text);
    protected delegate TTokenizeState CreateTokenizerStateFun();
    protected delegate (TTokenizeState, TToken) ProcesTokenFun((TTokenizeState State, TToken Token) tup);
    protected delegate bool TokenDefinitionIsActiveFun(TTokenizeState state);

    //==================================================================================================================
    // Private fields
    //==================================================================================================================
    private readonly CreateTokenFun _createToken;
    private readonly CreateTokenizerStateFun _createTokenizerState;

    //==================================================================================================================
    private ImmutableArray<(TTokenType Type,
                           Regex Pattern,
                           ProcesTokenFun? ProcessToken,
                           TokenDefinitionIsActiveFun? DefinitionIsActive)>
    _tokenDefinitions = ImmutableArray<(TTokenType Type,
                                        Regex Pattern,
                                        ProcesTokenFun? ProcessToken,
                                        TokenDefinitionIsActiveFun? DefinitionIsActive)>.Empty;

    //==================================================================================================================
    // Constructor
    //==================================================================================================================
    protected StringTokenizer(CreateTokenFun createToken, CreateTokenizerStateFun createTokenizerStateFun)
    {
      _createToken = createToken;
      _createTokenizerState = createTokenizerStateFun;
    }

    //==================================================================================================================
    // Instance methods
    //==================================================================================================================
    protected void Add(TTokenType type,
                       string pattern,
                       ProcesTokenFun? processToken = null,
                       TokenDefinitionIsActiveFun? definitionIsActive = null)
    {
      if (!pattern.StartsWith("^"))
        pattern = "^" + pattern;

      pattern = "(?:" + pattern + ")";

      _tokenDefinitions =
        _tokenDefinitions.Add((type,
                               new Regex(pattern, RegexOptions.Singleline), processToken, definitionIsActive));
    }

    //==================================================================================================================
    public (string? Input, TTokenizeState State, TToken? Token)
    NextToken(string? input, TTokenizeState? state)
    {
      state ??= _createTokenizerState();

      if (string.IsNullOrEmpty(input))
      {
        WriteLine("StringTokenizer.NextToken:   No input, retun no token");
        goto ReturnNoToken;
      }
      
      WriteLine($"StringTokenizer.NextToken:  Get token at: \"{input.ReplaceTrailingNewlinesWithEscaped()}\".");
      
      foreach (var definition in _tokenDefinitions)
      {
        if (definition.DefinitionIsActive is not null
            && !definition.DefinitionIsActive(state.Value))
          continue;

        var match = definition.Pattern.Match(input);

        if (match.Success)
        {
          if (match.Length == 0)
            throw new Exception($"Zero-length match found: \"{match.Value}\", which could lead to an infinite loop.");

          var token = _createToken(definition.Type, match.Value);

          if (definition.ProcessToken is not null)
            (state, token) = definition.ProcessToken((state.Value, token));

          input = input.Substring(match.Length);

          return (input, state.Value, token);
        }
      }

    ReturnNoToken:
      return (input, state.Value, null);
    }

    //==================================================================================================================
  }
}
