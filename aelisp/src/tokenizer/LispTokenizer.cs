using Pidgin;
using static Pidgin.Parser;
using static Pidgin.Parser<Ae.LispToken>;
using static System.Console;

//======================================================================================================================
static partial class Ae
{
   //====================================================================================================================
   // LispTokenizer class
   //====================================================================================================================
   private class LispTokenizer
   {
      //==================================================================================================================
      // Protected fields
      //==================================================================================================================
      protected string? _input;

      //==================================================================================================================
      // Private fields
      //==================================================================================================================
      private readonly Func<LispToken, bool>? _exclude;
      private LispTokenizerState? _state;

      //==================================================================================================================
      // Public properties
      //==================================================================================================================
      public int ChunkSizeHint => 16;

      //==================================================================================================================
      // Constructor
      //==================================================================================================================
      //==================================================================================================================
      public LispTokenizer(string? input, Func<LispToken, bool>? exclude) : this(input) => _exclude = exclude;
      public LispTokenizer(string? input)
      {
         _input = input;
         _exclude ??= IsWhitespaceOrCommentToken;
        }

      //==================================================================================================================
      // Instance methods
      //==================================================================================================================
      public List<LispToken> ReadAll()
      {
         var list = new List<LispToken>();
         LispToken? token = NextToken();

         while (token is not null)
         {
            list.Add(token.Value);
            token = NextToken();
         }

         return list;
      }

      //==================================================================================================================
      public int Read(Span<LispToken> buffer)
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

            var token = NextToken();

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
      protected virtual LispToken? NextToken()
      {
         DebugWrite($"\nStream.Next:                Get next token...");

      Next:
         if (string.IsNullOrEmpty(_input))
         {
            DebugWrite($"Stream.Next:                Input is null, not getting token!");
            return null;
         }

         DebugWrite($"Stream.Next:                Get token at: \"{_input.FirstLine()}\".");
         var (newInput, newState, newToken) = PureLispTokenizer.Instance.NextToken(_input, _state);
         (_state, _input) = (newState, newInput);

         if (newToken is not null && _exclude is not null && _exclude(newToken.Value))
         {
            DebugWrite($"Stream.Next:                Got excluded token: {newToken}, try again!");
            goto Next;
         }

         if (newToken is null)
         {
            DebugWrite($"Stream.Next:                Return no token!");
            throw new ApplicationException($"Bad input on line {_state.Value.Line}, "
                                           + $"column {_state.Value.Column} at \""
                                           + _input!.FirstLine()
                                           + "\".");
         }
         else
         {
            DebugWrite($"Stream.Next:                Return token:  {newToken}.");
         }

         return newToken;
      }

      //==================================================================================================================
   }
   //=====================================================================================================================
}
