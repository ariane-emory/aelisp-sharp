using static System.Console;
using static Ae;

//======================================================================================================================
static partial class Ae
{
   //====================================================================================================================
   public static partial class Core
   {
      //=================================================================================================================
      public static readonly CoreFun.FuncT Eval = (env, argsList) =>
      {
         if (argsList.IsImproperList)
            throw new ArgumentException($"argsList must be a proper list, not {argsList}!");

         return ((Pair)argsList)[0].Eval(env);
      };

      //=================================================================================================================
      public static readonly CoreFun.FuncT List = (env, argsList) => argsList;

      //=================================================================================================================
      public static readonly CoreFun.FuncT BoundP = (env, argsList) =>
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
