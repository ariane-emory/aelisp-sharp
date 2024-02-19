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
         if (argsLength % 2 != 0)
            throw new ArgumentException("setq requires an even number of arguments!");

         if (argsList.IsImproperList)
            throw new ArgumentException("argsList must be a proper list");

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
         ((Pair)argsList)[0].Eval(env);

      //=================================================================================================================
      public static readonly CoreFun.FuncT BoundP = (env, argsList, argsLength) =>
      {
         var arg1 = ((Pair)argsList)[0];

         if (arg1 is not Symbol sym)
            throw new ArgumentException($"Argument must be a symbol, not {arg1}!");

         var (found, _) = env.Lookup(Env.LookupMode.Nearest, sym);

         return Truthiness(found);
      };

      //=================================================================================================================
      private static CoreFun.FuncT RplacaOrRplacdFun(Action<Pair, LispObject> action) =>
         (Env env, LispObject argsList, int argsLength) =>
         {
            var arg1 = ((Pair)argsList)[0];
            var arg2 = ((Pair)argsList)[1];

            if (arg1 is not Pair pair)
               throw new ArgumentException("First argument must be a cons cell!");

            action(pair, arg2);

            return arg2;
         };

      //=================================================================================================================
      public static readonly CoreFun.FuncT Rplacd =
         RplacaOrRplacdFun((pair, arg1) => pair.Cdr = arg1);

      //=================================================================================================================
      public static readonly CoreFun.FuncT Rplaca =
         RplacaOrRplacdFun((pair, arg1) => pair.Car = arg1);

      //=================================================================================================================
      private static CoreFun.FuncT CarOrCdrFun(Func<LispObject, LispObject> func) =>
         (Env env, LispObject argsList, int argsLength) =>
         {
            return PureUnaryFun((o) =>
            {
               if (!o.IsList)
                  throw new ArgumentException($"Argument must be a list, not {o}!");

               if (o.IsNil)
                  return Nil;
               
               return func((Pair)o);

            })(env, argsList, argsLength);
            
         };

      //=================================================================================================================
      public static readonly CoreFun.FuncT Car = CarOrCdrFun(o => ((Pair)o).Car);

      //=================================================================================================================
      public static readonly CoreFun.FuncT Cdr = CarOrCdrFun(o => ((Pair)o).Cdr);

      //=================================================================================================================
      public static readonly CoreFun.FuncT List = (env, argsList, argsLength) => argsList;

      //================================================================================================================
   }
   //===================================================================================================================
}
