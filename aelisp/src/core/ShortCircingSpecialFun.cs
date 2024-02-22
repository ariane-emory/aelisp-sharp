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
         (env, argsList) =>
         {
            ThrowUnlessIsProperList("argsList", argsList);

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

      //=================================================================================================================
      public static LispObject And(Env env, LispObject argsList) =>
         ShortCircuitingSpecialFun(o => o.IsNil)
         (env, argsList);

      //=================================================================================================================
      public static LispObject Or(Env env, LispObject argsList) =>
         ShortCircuitingSpecialFun(o => !o.IsNil)
         (env, argsList);

      //=================================================================================================================
      public static LispObject Progn(Env env, LispObject argsList) =>
         ShortCircuitingSpecialFun(o => false)
         (env, argsList);

      //================================================================================================================
   }
   //===================================================================================================================
}
