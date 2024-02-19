using static System.Console;
using static Ae;

//======================================================================================================================
static partial class Ae
{
   //====================================================================================================================
   public static partial class Core
   {
      //=================================================================================================================
      private static CoreFun.FuncT ShortCircuitingSpecialFun(Func<LispObject, bool> pred) =>
         (env, argsList, argsLength) =>
         {
            if (argsList.IsImproperList)
               throw new ArgumentException("prog body must be a proper list");

            var result = Nil;

            while (argsList is Pair argsListPair)
            {
               result = argsListPair.Car.Eval(env);

               if (pred(result))
                  return result;

               argsList = argsListPair.Cdr;
            }

            return result;
         };

      //       public delegate LispObject FuncT(Env env, LispObject argsList, int argsLength);

      //=================================================================================================================
      public static LispObject And(Env env, LispObject argsList, int argsLength) =>
         ShortCircuitingSpecialFun(o => o.IsNil)(env, argsList, argsLength);

      //=================================================================================================================
      public static LispObject Or(Env env, LispObject argsList, int argsLength) =>
         ShortCircuitingSpecialFun(o => !o.IsNil)(env, argsList, argsLength);

      //=================================================================================================================
      public static LispObject Progn(Env env, LispObject argsList, int argsLength) =>
         ShortCircuitingSpecialFun(o => false)(env, argsList, argsLength);

      //================================================================================================================
   }
   //===================================================================================================================
}
