using static System.Console;
using static Ae;

//======================================================================================================================
static partial class Ae
{
   //====================================================================================================================
   public static partial class Core
   {
      //=================================================================================================================
      public static readonly CoreFun.FuncT Add = (env, argsList) => Number.VariadicAdd(argsList);
      public static readonly CoreFun.FuncT Sub = (env, argsList) => Number.VariadicSub(argsList);
      public static readonly CoreFun.FuncT Mul = (env, argsList) => Number.VariadicMul(argsList);
      public static readonly CoreFun.FuncT Div = (env, argsList) => Number.VariadicDiv(argsList);

      //================================================================================================================
      public static readonly CoreFun.FuncT NumEql = (env, argsList) => Truthiness(Number.VariadicEql(argsList));
      public static readonly CoreFun.FuncT NumNotEql = (env, argsList) => Truthiness(!Number.VariadicEql(argsList));
      public static readonly CoreFun.FuncT NumLT = (env, argsList) => Truthiness(Number.VariadicLT(argsList));
      public static readonly CoreFun.FuncT NumGT = (env, argsList) => Truthiness(Number.VariadicGT(argsList));
      public static readonly CoreFun.FuncT NumLTE = (env, argsList) => Truthiness(Number.VariadicLTE(argsList));
      public static readonly CoreFun.FuncT NumGTE = (env, argsList) => Truthiness(Number.VariadicGTE(argsList));

      //================================================================================================================
      public static readonly CoreFun.FuncT BitAnd = (env, argsList) => Integer.VariadicBitAnd(argsList);
      public static readonly CoreFun.FuncT BitOr = (env, argsList) => Integer.VariadicBitOr(argsList);
      public static readonly CoreFun.FuncT BitXor = (env, argsList) => Integer.VariadicBitXor(argsList);
      public static readonly CoreFun.FuncT Lsft = (env, argsList) => Integer.VariadicLsft(argsList);
      public static readonly CoreFun.FuncT Rsft = (env, argsList) => Integer.VariadicRsft(argsList);
      public static readonly CoreFun.FuncT Mod = (env, argsList) => Integer.VariadicMod(argsList);

      //================================================================================================================
      public static readonly CoreFun.FuncT BitNot = PureUnaryFun<Integer>(o => ~o);

      //================================================================================================================
      public static readonly CoreFun.FuncT Floor = PureUnaryFun<Number>(num =>
       num switch
       {
          Integer numInteger => numInteger,
          Float numFloat => new Integer((int)Math.Floor(numFloat.Value)),
          Rational numRational => new Integer((int)Math.Floor((double)numRational.Numerator / numRational.Denominator)),
          _ => throw new ArgumentException($"argument must be a number, not {num}!")
       });


      //================================================================================================================
   }
   //===================================================================================================================
}
