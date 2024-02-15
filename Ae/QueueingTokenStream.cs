static partial class Ae
{
  //====================================================================================================================
  // QueueinngTokenStream class
  //====================================================================================================================
  public class QueueingTokenStream : TokenStream
  {
    //==================================================================================================================
    // Private fields
    //==================================================================================================================
    private readonly Queue<string> _queuedInputs = new();

    //==================================================================================================================
    // Constructor
    //==================================================================================================================
    public QueueingTokenStream(string? input, Func<Token, bool>? exclude = null) : base(null, exclude)
    {
      if (input is not null)
        _queuedInputs.Enqueue(input);
    }

    //==================================================================================================================
    // Protected instance methods
    //==================================================================================================================
    protected override Token? Next()
    {
      if (string.IsNullOrEmpty(_input) && _queuedInputs.Count > 0)
        _input = _queuedInputs.Dequeue();

      return base.Next();
    }

    //==================================================================================================================
    // Public instance methods
    //==================================================================================================================
    public void Enqueue(string newQueuedInput)
    {
      _queuedInputs.Enqueue(newQueuedInput);
    }
    
    public override void Reset()
    {
      _queuedInputs.Clear();
      base.Reset();
    }    
  }
}
