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
    public int Read(Span<Token> buffer)
    {
      var ix = 0;

      for (; ix < buffer.Length; ix++)
      {
        var token = Next();

        if (token is null)
          break;

        buffer[ix] = token.Value;
      }

      return ix;
    }

    //==================================================================================================================
    public virtual void Reset()
    {
      _state = null;
    }
    
    protected virtual Token? Next()
    {
      if (_input is not null)
        WriteLine($"\nGet token at \"{_input.Trim()}\"...");
      
      Next:
      var (newInput, newState, newToken) = Tokenizer.Instance.NextToken(_input, _state);
      (_state, _input) = (newState, newInput);

      if (newToken is not null && _exclude is not null && _exclude(newToken.Value))
        goto Next;

      if (newToken is not null && newToken.Value.TokenType != TokenType.Whitespace && newToken.Value.TokenType != TokenType.Newline)
        WriteLine($"Got token \"{newToken}\".");
      else
        WriteLine($"No token found at \"{_input}\".");
      
      return newToken;
    }

    //==================================================================================================================
  }
}
