using static Ae;
using static System.Console;

//======================================================================================================================
static class Junk
{
    //====================================================================================================================
    static (string? Input, LispTokenizerState State, List<Token> Tokens)
    TokenizeAndPrintLine(string? input, LispTokenizerState? state = null)
    {
        var tokens = new List<Token>();

        while (!string.IsNullOrEmpty(input)) // infinite loop risk, fix!
        {
            var (newInput, newState, newToken) = PureLispTokenizer.Instance.NextToken(input, state);

            if (newToken is null)
                throw new ApplicationException($"No token at \"{newInput}\"!");

            (input, state) = (newInput, newState);

            tokens.Add(newToken.Value);
        }

        tokens.Print();

        return (input, state!.Value, tokens);
    }

    //====================================================================================================================
    static (string? Input, LispTokenizerState State, List<Token> Tokens)
    TokenizeAndPrintLines(IEnumerable<string> lines, LispTokenizerState? state)
    {
        var tokens = new List<Token>();

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
  
   //===================================================================================================================
}
