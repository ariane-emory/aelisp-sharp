using Pidgin;
using static Pidgin.Parser;
using static Pidgin.Parser<Ae.Token>;

static partial class Ae
{
  //====================================================================================================================
  public class TokenStream : Pidgin.ITokenStream<Token>
  {
    //==================================================================================================================
    // Constructer
    //==================================================================================================================
    public TokenStream(string input, Func<Token, bool>? exclude = null)
    {
      _input = input;
      _exclude = exclude;
    }

    //==================================================================================================================
    // Private fields
    //==================================================================================================================
    private string? _input;
    private readonly Func<Token, bool>? _exclude;
    private TokenizerState? _state;

    //==================================================================================================================
    // Public properties
    //==================================================================================================================
    public int ChunkSizeHint => 16;

    //==================================================================================================================
    // Public instance methods
    //==================================================================================================================
    public void Return(ReadOnlySpan<Token> leftovers) { throw new NotImplementedException(); }
    
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

    private Token? Next()
    {
    Next:
      var (newInput, newState, newToken) = Tokenizer.Get().NextToken(_input, _state);
      (_state, _input) = (newState, newInput);

      if (newToken is not null && _exclude is not null && _exclude(newToken.Value))
        goto Next;

      return newToken;
    }
  }
}
