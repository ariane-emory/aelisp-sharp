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
      public static readonly CoreFun.FuncT NumEql = (env, argsList) => Truthiness(Number.CmpEql(argsList));
      public static readonly CoreFun.FuncT NumNotEql = (env, argsList) => Truthiness(!Number.CmpEql(argsList));
      public static readonly CoreFun.FuncT NumLT = (env, argsList) => Truthiness(Number.CmpLT(argsList));
      public static readonly CoreFun.FuncT NumGT = (env, argsList) => Truthiness(Number.CmpGT(argsList));
      public static readonly CoreFun.FuncT NumLTE = (env, argsList) => Truthiness(Number.CmpLTE(argsList));
      public static readonly CoreFun.FuncT NumGTE = (env, argsList) => Truthiness(Number.CmpGTE(argsList));

      //================================================================================================================
      public static readonly CoreFun.FuncT BitAnd = (env, argsList) => Integer.BitAnd(argsList);
      public static readonly CoreFun.FuncT BitOr = (env, argsList) => Integer.BitOr(argsList);
      public static readonly CoreFun.FuncT BitXor = (env, argsList) => Integer.BitXor(argsList);
      public static readonly CoreFun.FuncT BitNot = PureUnaryFun<Integer>(o => ~o);

      //================================================================================================================
      public static readonly CoreFun.FuncT Floor = PureUnaryFun<Number>(num =>
       num switch
       {
          Integer numInteger => numInteger,
          Float numFloat => new Integer((int)Math.Floor(numFloat.Value)),
          Rational numRational => new Integer((int)Math.Floor((double)numRational.Numerator / numRational.Denominator)),
          _ => throw new ArgumentException($"Argument must be a number, not {num}!")
       });


      //================================================================================================================
   }
   //===================================================================================================================
}
