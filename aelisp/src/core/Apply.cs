using static System.Console;
using static Ae;

//======================================================================================================================
static partial class Ae
{
   //====================================================================================================================
   public static partial class Core
   {
      //================================================================================================================
      public static LispObject Apply(Env env, LispObject argsList)
      {
         if (!argsList.IsProperList)
            throw new ArgumentException($"argsList must be a proper list, not {argsList}!");

         var arg1 = ((Pair)argsList)[0];

         if (!(argsList is Pair argsListPair))
            throw new ArgumentException($"argsList must be a pair, not {argsList}!");

         LispObject current = argsListPair;
         var stash = Nil;
         
         while (current is Pair currentPair && currentPair.Cdr is Pair)
         {
            stash = Ae.Cons(currentPair.Car, stash);
            current = currentPair.Cdr;
         }

         WriteLine($"Stash: {stash.Princ()}");
         WriteLine($"current: {current.Princ()}");

         return Nil;
        }

      //================================================================================================================
   }
   //===================================================================================================================
}
