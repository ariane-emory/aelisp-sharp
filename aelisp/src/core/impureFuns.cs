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
         ThrowUnlessIsProperList("argsList", argsList);

         if (argsList.Length > 1)
            throw new ArgumentException($"argsList must have exactly one element, not {argsList.ToPrincString()}!");
         
         return ((Pair)argsList)[0].Eval(env);
      };

      //=================================================================================================================
      public static readonly CoreFun.FuncT List = (env, argsList) => argsList;

      //=================================================================================================================
      public static readonly CoreFun.FuncT ThrowNotImplemented = (env, argsList) => 
      {
         throw new NotImplementedException("not implemented!");
      };

   //=================================================================================================================
   public static readonly CoreFun.FuncT BoundP = (env, argsList) =>
     {
      ThrowUnlessIsList("argsList", argsList);

        var arg1 = ((Pair)argsList)[0];

         if (arg1 is not Symbol sym)
           throw new ArgumentException($"argument must be a symbol, not {arg1.ToPrincString()}!");

        var (found, _) = env.Lookup(Env.LookupMode.Nearest, sym);

        return Truthiness(found);
     };

   //================================================================================================================
}
   //===================================================================================================================
}
