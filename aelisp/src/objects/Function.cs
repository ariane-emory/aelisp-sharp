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
      public LispObject Apply(Env env, LispObject argsList)
      {
         if (!argsList.IsList)
            throw new ArgumentException($"{nameof(argsList)} must be a list!");

         if (!argsList.IsProperList) // for the moment, argsList must be proper.
            throw new NotImplementedException($"Using improper lists as {nameof(argsList)} is not permitted yet!");

         return ImplApply(env, argsList);
      }

      //================================================================================================================
      // Protected instance methods
      //================================================================================================================
      protected abstract LispObject ImplApply(Env env, LispObject argsList);
   }

   //===================================================================================================================
}
