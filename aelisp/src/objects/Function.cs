//======================================================================================================================
static partial class Ae
{
   //===================================================================================================================
   // Function abstract class
   //===================================================================================================================
   public abstract class Function : LispObject
   {
      //================================================================================================================
      // Public instance methods
      //================================================================================================================
      public LispObject Apply(Env env, LispObject args)
      {
         if (!args.IsProperList) // for the moment, args must be proper.
            throw new NotImplementedException($"Using improper lists as {nameof(args)} is not permitted yet!");

         if (!args.IsList)
            throw new ArgumentException($"{nameof(args)} must be a list!");

         return ImplApply(env, args);
      }

      //================================================================================================================
      // Protected instance methods
      //================================================================================================================
      protected abstract LispObject ImplApply(Env env, LispObject args);
   }

   //===================================================================================================================
}
