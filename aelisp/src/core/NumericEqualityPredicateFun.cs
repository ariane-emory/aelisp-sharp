using static System.Console;
using static Ae;

//======================================================================================================================
static partial class Ae
{
   //====================================================================================================================
   public static partial class Core
   {
      //=================================================================================================================
      private static CoreFun.FuncT NumericEqualityPredicateFun(int val) =>
         (Env env, LispObject argsList) =>
         {
            var arg1 = ((Pair)argsList)[0];

            if (arg1 is Number number)
               return Truthiness(new Integer(val) == number);

            throw new ArgumentException($"argument must be a Number, not {arg1.ToPrincString()}!");
          };

      //=================================================================================================================
      public static readonly CoreFun.FuncT ZeroP = NumericEqualityPredicateFun(0);

      //=================================================================================================================
      public static readonly CoreFun.FuncT OneP = NumericEqualityPredicateFun(1);

      //================================================================================================================
   }
   //===================================================================================================================
}
