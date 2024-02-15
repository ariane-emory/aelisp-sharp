using System.Collections.Immutable;
using static System.Console;
using static Ae;
using static Utility;

//======================================================================================================================
class Program
{
  //====================================================================================================================
  static readonly ImmutableArray<TokenType> ExcludedTokenTypes = ImmutableArray.Create(
    TokenType.Whitespace,
    TokenType.LineComment,
    TokenType.MultilineCommentBeginning,
    TokenType.MultilineCommentContent,
    TokenType.MultilineCommentEnd,
    TokenType.Comment,
    TokenType.Newline);

  //====================================================================================================================
  static bool IsExcludedTokenType(Ae.Token token) => ExcludedTokenTypes.Contains(token.TokenType);
  static bool IsIncludedTokenType(Ae.Token token) => !IsExcludedTokenType(token);

  //====================================================================================================================
  static void PrintTokens(IEnumerable<Ae.Token> tokens)
  {
    foreach (var (token, index) in tokens
             //.Where(IsIncludedTokenType)
             .Select((value, index) => (value, index)))
      WriteLine($"#{index}: {token}");
  }

  //====================================================================================================================
  static (string? Input, Ae.TokenizerState State, List<Ae.Token> Tokens)
  TokenizeAndPrintLine(string? input, Ae.TokenizerState? state = null)
  {
    var tokens = new List<Ae.Token>();

    while (!string.IsNullOrEmpty(input)) // infinite loop risk, fix!
    {
      var (newInput, newState, newToken) = Tokenizer.Instance.NextToken(input, state);

      if (newToken is null)
        throw new ApplicationException($"No token at \"{newInput}\"!");

      (input, state) = (newInput, newState);

      tokens.Add(newToken.Value);
    }

    PrintTokens(tokens);

    return (input, state!.Value, tokens);
  }

  //====================================================================================================================
  static (string? Input, Ae.TokenizerState State, List<Ae.Token> Tokens)
  TokenizeAndPrintLines(IEnumerable<string> lines, Ae.TokenizerState? state)
  {
    var tokens = new List<Ae.Token>();

    foreach (var line in lines)
    {
      var (leftoverInput, newState, newTokens) = TokenizeAndPrintLine(line, state);
      state = newState;

      if (!string.IsNullOrEmpty(leftoverInput))
        throw new ApplicationException($"leftover input \"{leftoverInput}\".");

      tokens.AddRange(newTokens);
    }

    return (null, state!.Value, tokens);
  }

  //====================================================================================================================
  enum Mode { LineByLine, EntireFileAtOnce, };

  //====================================================================================================================
  static void DoTokenizeAndPrintLinesTests(string filename)
  {
    foreach (var mode in new[] {
        Mode.LineByLine,
        Mode.EntireFileAtOnce
      })
    {
      var tokenizeResult = mode switch
      {
        Mode.LineByLine => TokenizeAndPrintLines(File.ReadAllLines(filename).Select(s => s + "\n"), null),
        Mode.EntireFileAtOnce => TokenizeAndPrintLine(File.ReadAllText(filename), null),
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
      };

      WriteLine($"Token count: {tokenizeResult.Tokens.Count}");
      WriteLine($"\n\n---\n\n");
    }
  }

  //====================================================================================================================
  static void Main()
  {
    var filename = "data/data.lisp";
    var fileText = File.ReadAllText(filename);
    var stream = new Ae.QueueingTokenStream(fileText, IsExcludedTokenType);
    var take = 32;
    var ary = new Ae.Token[take];
    var read = stream.Read(ary);
    PrintTokens(ary.Take(read));
  }

  //====================================================================================================================
}

