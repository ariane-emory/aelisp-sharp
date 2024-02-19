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

            if (arg1 is Integer integer)
               return Truthiness(val == integer.Value);

            if (arg1 is Float floatObj)
               return Truthiness(val == floatObj.Value);

            if (arg1 is Rational rational)
               return Truthiness(val == rational.Numerator);

            throw new ArgumentException($"Argument must be a symbol, not {arg1}!");
         };

      //=================================================================================================================
      public static readonly CoreFun.FuncT ZeroP = NumericEqualityPredicateFun(0);

      //=================================================================================================================
      public static readonly CoreFun.FuncT OneP = NumericEqualityPredicateFun(1);

      //================================================================================================================
   }
   //===================================================================================================================
}
