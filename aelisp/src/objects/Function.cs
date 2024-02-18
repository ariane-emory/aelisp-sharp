using static System.Console;

//======================================================================================================================
static partial class Ae
{
   //===================================================================================================================
   // Function abstract class
   //===================================================================================================================
   public abstract class Function : LispObject
   {
      //================================================================================================================
      // Public properties
      //================================================================================================================
      public abstract string Name { get; }
      public abstract bool Special { get; }

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

      //================================================================================================================
      protected LispObject EvalArgs(Env env, LispObject argsList)
      {
         WriteLine($"EvalArgs: {argsList.Princ()}");

         if (argsList.IsNil)
            return Nil;

         LispObject result = Nil;
         Pair? resultLastPair = null;
         LispObject currentArg = argsList;

         int index = 0;

         while (currentArg is Pair currentArg_pair)
         {
            WriteLine($"| Arg #{++index}:  {currentArg_pair.Car.Princ()}");

            LispObject evaledArg = currentArg_pair.Car.Eval(env);

            WriteLine($"> Arg #{index} = {evaledArg.Princ()}");

            // TO-DO: add return if evaledArg is Error? or stick with Exceptions?

            if (result.IsNil)
            {
               result = Cons(evaledArg, Nil);
               resultLastPair = (Pair)result;
            }
            else
            {
               resultLastPair!.Cdr = Cons(evaledArg, Nil);
               resultLastPair = (Pair)resultLastPair.Cdr;
            }

            currentArg = currentArg_pair.Cdr;
         }

         if (!currentArg.IsNil) // dotted tail arg is present.
         {
            LispObject evaledArg = currentArg.Eval(env);
            // TO-DO: add return if evaledArg is Error? or stick with Exceptions?

            resultLastPair!.Cdr = evaledArg;
         }

         return result;
      }

   }

   //===================================================================================================================
}
