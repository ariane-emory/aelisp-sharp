using static System.Console;
using static Ae;

//======================================================================================================================
static partial class Ae
{
   //====================================================================================================================
   public static partial class Core
   {
      //=================================================================================================================
      public static readonly CoreFun.FuncT Add = (env, argsList) => Number.Add(argsList);
      public static readonly CoreFun.FuncT Sub = (env, argsList) => Number.Sub(argsList);
      public static readonly CoreFun.FuncT Mul = (env, argsList) => Number.Mul(argsList);
      public static readonly CoreFun.FuncT Div = (env, argsList) => Number.Div(argsList);
      public static readonly CoreFun.FuncT Mod = (env, argsList) => Integer.Mod(argsList);
      public static readonly CoreFun.FuncT Lsft = (env, argsList) => Integer.Lsft(argsList);
      public static readonly CoreFun.FuncT Rsft = (env, argsList) => Integer.Rsft(argsList);

      //================================================================================================================
   }
   //===================================================================================================================
}
