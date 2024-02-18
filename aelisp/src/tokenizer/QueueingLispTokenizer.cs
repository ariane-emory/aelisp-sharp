//====================================================================================================================================================
static partial class Ae
{
   //====================================================================================================================
   // QueueingLispTokenizer class
   //====================================================================================================================
   public class QueueingLispTokenizer : LispTokenizer
   {
      //==================================================================================================================
      // Private fields
      //==================================================================================================================
      private readonly Queue<string> _queuedInputs = new();

      //==================================================================================================================
      // Constructor
      //==================================================================================================================
      public QueueingLispTokenizer(string? input) : base(null)
      {

         if (input is not null)
            _queuedInputs.Enqueue(input);
      }

      //==================================================================================================================
      public QueueingLispTokenizer(string? input, Func<LispToken, bool>? exclude) : base(null, exclude)
      {

         if (input is not null)
            _queuedInputs.Enqueue(input);
      }

      //==================================================================================================================
      // Instance methods
      //==================================================================================================================
      protected override LispToken? NextToken()
      {
         if (string.IsNullOrEmpty(_input) && _queuedInputs.Count > 0)
            _input = _queuedInputs.Dequeue();

         return base.NextToken();
      }

      //==================================================================================================================
      public void Enqueue(string newQueuedInput)
      {
         _queuedInputs.Enqueue(newQueuedInput);
      }

      //==================================================================================================================
      public override void Reset()
      {
         _queuedInputs.Clear();
         base.Reset();
      }

      //==================================================================================================================
   }
}
