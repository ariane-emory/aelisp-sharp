using static System.Console;
using static Ae;

//======================================================================================================================
static partial class Ae
{
   //====================================================================================================================
   public static partial class Core
   {
      //=================================================================================================================
      public static readonly CoreFun.FuncT Setq = (env, argsList, argsLength) =>
      {
         if (argsList.IsImproperList)
            throw new ArgumentException($"argsList must be a proper list, not {argsList}!");

         if (argsLength % 2 != 0)
            throw new ArgumentException("setq requires an even number of arguments!");

         var result = Nil;

         while (argsList is Pair currentPair)
         {
            var pairHead = currentPair.Car;
            var valueExpression = ((Pair)currentPair.Cdr).Car;

            if (!(pairHead is Symbol sym))
               throw new ArgumentException($"The first element of each pair must be a symbol, not {pairHead}.");

            if (sym.IsKeyword || sym.IsSelfEvaluating)
               throw new ArgumentException($"symbol {sym} may not be set.");

            var evaluatedValue = valueExpression.Eval(env);

            result = valueExpression.Eval(env);

            var mode = sym.IsSpecial ? Env.LookupMode.Global : Env.LookupMode.Nearest;

            env.Set(mode, sym, result);

            argsList = ((Pair)currentPair.Cdr).Cdr;
         }

         return result;
      };

      //=================================================================================================================
      public static readonly CoreFun.FuncT Eval = (env, argsList, argsLength) =>
      {
         if (argsList.IsImproperList)
            throw new ArgumentException($"argsList must be a proper list, not {argsList}!");

         return ((Pair)argsList)[0].Eval(env);
      };

      //=================================================================================================================
      public static readonly CoreFun.FuncT List = (env, argsList, argsLength) => argsList;

      //=================================================================================================================
      public static readonly CoreFun.FuncT BoundP = (env, argsList, argsLength) =>
      {
         if (argsList.IsImproperList)
            throw new ArgumentException($"argsList must be a proper list, not {argsList}!");

         var arg1 = ((Pair)argsList)[0];

         if (arg1 is not Symbol sym)
            throw new ArgumentException($"Argument must be a symbol, not {arg1}!");

         var (found, _) = env.Lookup(Env.LookupMode.Nearest, sym);

         return Truthiness(found);
      };

      //================================================================================================================
   }
   //===================================================================================================================
}
