using static System.Console;
using static Ae;

//======================================================================================================================
static partial class Ae
{
   //====================================================================================================================
   public static partial class Core
   {
      //=================================================================================================================
      private static LispObject SetInternal(Env env, LispObject argsList, bool evaluateValues)
      {
         ThrowUnlessIsProperList("argsList", argsList);

         if (argsList.Length % 2 != 0)
            throw new ArgumentException("setq requires an even number of arguments!");

         var result = Nil;

         while (argsList is Pair currentPair)
         {
            var pairHead = currentPair.Car;
            var valueOrValueExpression = ((Pair)currentPair.Cdr).Car;

            if (!(pairHead is Symbol sym))
               throw new ArgumentException($"first element of each pair must be a symbol, not {pairHead}!");

            if (sym.IsKeyword || sym.IsSelfEvaluating)
               throw new ArgumentException($"symbol {sym} may not be set!");

            var evaluatedValue = evaluateValues ? valueOrValueExpression.Eval(env) : valueOrValueExpression;

            result = valueOrValueExpression.Eval(env);

            var mode = sym.IsSpecial ? Env.LookupMode.Global : Env.LookupMode.Nearest;

            env.Set(mode, sym, result);

            argsList = ((Pair)currentPair.Cdr).Cdr;
         }

         return result;
      }

      //================================================================================================================
      public static LispObject Set(Env env, LispObject argsList) => SetInternal(env, argsList, false);
      public static LispObject Setq(Env env, LispObject argsList) => SetInternal(env, argsList, true);

      //================================================================================================================
    }
   //===================================================================================================================
}
