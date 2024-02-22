using static System.Console;
using static Ae;

//======================================================================================================================
static partial class Ae
{
   //====================================================================================================================
   public static partial class Core
   {
      //=================================================================================================================
      private static CoreFun.FuncT
         AccessorFun<ThisLispObjT, FieldT>(Func<ThisLispObjT, FieldT> getField, Func<FieldT, LispObject> construct)
       => PureUnaryFun(o =>
       {
          if (o is ThisLispObjT typed)
             return construct(getField(typed));

          throw new ArgumentException($"argument must be a string, not {o}!");
       });

      //=================================================================================================================
      public static readonly CoreFun.FuncT UserFunctionBody =
         AccessorFun<UserFunction, LispObject>(fun => fun.Body, n => n);

      //=================================================================================================================
      public static readonly CoreFun.FuncT UserFunctionParams =
         AccessorFun<UserFunction, LispObject>(fun => fun.Parameters, n => n);

      //=================================================================================================================
      public static readonly CoreFun.FuncT Numerator =
         AccessorFun<Rational, int>(rat => rat.Numerator, n => new Integer(n));

      //=================================================================================================================
      public static readonly CoreFun.FuncT Denominator =
         AccessorFun<Rational, int>(rat => rat.Denominator, n => new Integer(n));

      //=================================================================================================================
      public static readonly CoreFun.FuncT ErrorMessage =
         AccessorFun<Error, string>(err => err.Value, s => new Error(s));

      //=================================================================================================================
      public static readonly CoreFun.FuncT SymbolName =
         AccessorFun<Symbol, string>(sym => sym.Value, s => new String(s));

      //================================================================================================================
   }
   //===================================================================================================================
}
