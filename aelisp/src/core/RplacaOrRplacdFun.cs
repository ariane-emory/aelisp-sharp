using static System.Console;
using static Ae;

//======================================================================================================================
static partial class Ae
{
   //====================================================================================================================
   public static partial class Core
   {
      //=================================================================================================================
      private static CoreFun.FuncT RplacaOrRplacdFun(Action<Pair, LispObject> action) =>
         (Env env, LispObject argsList) =>
         {
            ThrowUnlessIsProperList("argsList", argsList);

            var arg1 = ((Pair)argsList)[0];
            var arg2 = ((Pair)argsList)[1];

            if (arg1 is not Pair pair)
               throw new ArgumentException("first argument must be a cons cell, not {arg1.ToPrincString()}!");

            action(pair, arg2);

            return arg2;
         };

      //=================================================================================================================
      public static readonly CoreFun.FuncT Rplacd =
         RplacaOrRplacdFun((pair, arg1) => pair.Cdr = arg1);

      //=================================================================================================================
      public static readonly CoreFun.FuncT Rplaca =
         RplacaOrRplacdFun((pair, arg1) => pair.Car = arg1);

      //================================================================================================================
   }
   //===================================================================================================================
}
