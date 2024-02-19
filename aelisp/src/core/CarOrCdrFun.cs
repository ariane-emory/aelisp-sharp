using static System.Console;
using static Ae;

//======================================================================================================================
static partial class Ae
{
   //====================================================================================================================
   public static partial class Core
   {
      //=================================================================================================================
      private static CoreFun.FuncT CarOrCdrFun(Func<LispObject, LispObject> func) =>
         (Env env, LispObject argsList, int argsLength) =>
         PureUnaryFun((o) =>
         {
            if (!o.IsList)
               throw new ArgumentException($"Argument must be a list, not {o}!");
            
            if (o.IsNil)
               return Nil;
            
            return func((Pair)o);
         })(env, argsList, argsLength);


      //=================================================================================================================
      public static readonly CoreFun.FuncT Car = CarOrCdrFun(o => ((Pair)o).Car);

      //=================================================================================================================
      public static readonly CoreFun.FuncT Cdr = CarOrCdrFun(o => ((Pair)o).Cdr);

      //================================================================================================================
   }
   //===================================================================================================================
}