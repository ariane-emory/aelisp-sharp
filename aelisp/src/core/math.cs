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

      //================================================================================================================
      public static readonly CoreFun.FuncT NumEquals = (env, argsList) => Truthiness(Number.CmpEqual(argsList));

      //================================================================================================================
      public static readonly CoreFun.FuncT BitAnd = (env, argsList) => Integer.BitAnd(argsList);
      public static readonly CoreFun.FuncT BitOr = (env, argsList) => Integer.BitOr(argsList);
      public static readonly CoreFun.FuncT BitXor = (env, argsList) => Integer.BitXor(argsList);
      public static readonly CoreFun.FuncT BitNot = PureUnaryFun<Integer>(o => new Integer(~o.Value));

      //================================================================================================================
      public static readonly CoreFun.FuncT Rsft = PureBinaryFun<Integer, Integer>((val, sft) => new Integer(val.Value >> sft.Value));

      //================================================================================================================
   }
   //===================================================================================================================
}
