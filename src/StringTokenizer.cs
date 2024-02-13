using static System.Console;
using System.Text.RegularExpressions;

//======================================================================================================================
// Token class
//======================================================================================================================
public record struct Token<TTokenType>(TTokenType TokenType, string Text);

//======================================================================================================================
// StringTokenizer class
//======================================================================================================================
public abstract class StringTokenizer<TTokenType, TToken, TTokenizerState> where TTokenizerState : struct
{
  //====================================================================================================================
  // Delegates 
  //====================================================================================================================
  protected delegate TToken CreateTokenFun(TTokenType type, string text);
  protected delegate TTokenizerState CreateTokenizerStateFun();
  protected delegate (TTokenizerState, TToken) ProcesTokenFun((TTokenizerState State, TToken Token) tup);
  protected delegate bool TokenDefinitionIsActiveFun(TTokenizerState state);

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

  public record struct Arg(string? Input, List<TToken>? Tokens, TTokenizerState? State);

  public Arg Tokenize(string? Input, List<TToken>? Tokens, TTokenizerState? State) => Tokenize(new Arg(Input, Tokens, State));

  public Arg Tokenize(Arg arg)
  {
    if (arg.State is null)
      arg.State = _createTokenizerState();

    if (arg.Tokens is null)
      arg.Tokens = new List<TToken>();

    if (arg.Input is null)
      return arg;

    while (!string.IsNullOrEmpty(arg.Input))
    {
      bool foundMatch = false;

      foreach (var definition in _tokenDefinitions)
      {
        if (definition.DefinitionIsActive is not null && !definition.DefinitionIsActive(arg.State.Value))
          continue;

        var match = definition.Pattern.Match(arg.Input);

        if (match.Success)
        {
          var token = _createToken(definition.Type, match.Value);

          if (match.Length == 0)
            throw new Exception($"Zero-length match found: {token}, which could lead to an infinite loop.");

          if (definition.ProcessToken is not null)
            (arg.State, token) = definition.ProcessToken((arg.State.Value, token));

          arg.Tokens.Add(token);

          foundMatch = true;
          arg.Input = arg.Input.Substring(match.Length);

          break;
        }
      }

      if (!foundMatch)
        break;
    }

    return arg;
  }
}
