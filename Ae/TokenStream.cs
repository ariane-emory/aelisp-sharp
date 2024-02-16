using Pidgin;
using static Pidgin.Parser;
using static Pidgin.Parser<Ae.Token>;
using static System.Console;

//======================================================================================================================
static partial class Ae
{
  //====================================================================================================================
  // TokenStream class
  //====================================================================================================================
  public class TokenStream : Pidgin.ITokenStream<Token>
  {
    //==================================================================================================================
    // Protected fields
    //==================================================================================================================
    protected string? _input;

    //==================================================================================================================
    // Private fields
    //==================================================================================================================
    private readonly Func<Token, bool>? _exclude;
    private TokenizerState? _state;

    //==================================================================================================================
    // Public properties
    //==================================================================================================================
    public int ChunkSizeHint => 16;

    //==================================================================================================================
    // Constructor
    //==================================================================================================================
    public TokenStream(string? input, Func<Token, bool>? exclude = null)
    {
      _input = input;
      _exclude = exclude;
    }

    //==================================================================================================================
    // Instance methods
    //==================================================================================================================
    public void Return(ReadOnlySpan<Token> leftovers) { throw new NotImplementedException(); }

    //==================================================================================================================
    public List<Token> ReadAll()
    {
      var list = new List<Token>();
      Token? token = Next();

      while (token is not null)
      {
        list.Add(token.Value);
        token = Next();
      }

      return list;
    }

    //==================================================================================================================
    public int Read(Span<Token> buffer)
    {
      var ix = 0;

      DebugWrite($"\nStream.Read:                Fill {buffer.Length} slots.");

      for (; ix < buffer.Length; ix++)
      {
        if (_input is null)
          DebugWrite($"\nStream._input               null");
        else
          DebugWrite($"\nStream._input               \"{_input.ReplaceNewlinesWithEscaped()}\"");

        DebugWrite($"Stream.Read:                Try to set slot #{ix}.");

        var token = Next();

        if (token is null)
        {
          DebugWrite($"\nStream.Read:                Got null from Next(), breaking after filling {ix} elements!");
          break;
        }

        buffer[ix] = token.Value;
        DebugWrite($"Stream.Read:                Set slot #{ix} to {token.Value}.");
      }

      return ix;
    }

    //==================================================================================================================
    public virtual void Reset()
    {
      _state = null;
    }

    //==================================================================================================================
    protected virtual Token? Next()
    {
      if (_input is null)
      {
        DebugWrite($"\nStream.Next:                Input is null, not getting token!");
        return null;
      }

      DebugWrite($"\nStream.Next:                Get token at: \"{_input.ReplaceNewlinesWithEscaped()}\".");

    Next:
      var (newInput, newState, newToken) = Tokenizer.Instance.NextToken(_input, _state);
      (_state, _input) = (newState, newInput);

      if (newToken is not null && _exclude is not null && _exclude(newToken.Value))
      {
        DebugWrite($"Stream.Next:                Got excluded token: {newToken}, try again!");
        goto Next;
      }

      if (newToken is null)
      {
        DebugWrite($"Stream.Next:                Return no token!");

        throw new ApplicationException($"Encountered bad input on line {_state.Value.Line}, "
                                       + $"column {_state.Value.Column} at \"{_input}\"!");
      }
      else
      {
        DebugWrite($"Stream.Next:                Return token:  {newToken}.");
      }

      return newToken;
    }

    //==================================================================================================================
  }
}
