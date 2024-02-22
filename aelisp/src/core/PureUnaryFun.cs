using static System.Console;
using static Ae;

//======================================================================================================================
static partial class Ae
{
   //====================================================================================================================
   public static partial class Core
   {
      //=================================================================================================================
      private static CoreFun.FuncT PureUnaryFun(Func<LispObject, LispObject> func)
         => (Env env, LispObject argsList) =>
         {
            ThrowUnlessIsProperList("argsList", argsList);

            return func(((Pair)argsList)[0]);
         };

      //=================================================================================================================
      private static CoreFun.FuncT PureUnaryFun<T1>(Func<T1, LispObject> func)
       where T1 : LispObject
       => (Env env, LispObject argsList) =>
       {
          ThrowUnlessIsProperList("argsList", argsList);

          var arg1 = ((Pair)argsList)[0];

          if (arg1 is T1 typedArg1)
             return func(typedArg1);

          throw new ArgumentException($"argument must be of type {typeof(T1).Name}, not {arg1}!");
       };

      //=================================================================================================================
      public static readonly CoreFun.FuncT Plus1 =
         PureUnaryFun(num =>
            num switch
            {
               Integer integer => (LispObject)new Integer(integer.Value + 1),
               Float floatObj => (LispObject)new Float(floatObj.Value + 1),
               Rational rational => (LispObject)new Rational(rational.Numerator + rational.Denominator, rational.Denominator),
               _ => throw new ArgumentException($"argument must be a Number, not {num}!")
            });

      //=================================================================================================================
      public static readonly CoreFun.FuncT Minus1 =
         PureUnaryFun(num =>
            num switch
            {
               Integer integer => (LispObject)new Integer(integer.Value - 1),
               Float floatObj => (LispObject)new Float(floatObj.Value - 1),
               Rational rational => (LispObject)new Rational(rational.Numerator - rational.Denominator, rational.Denominator),
               _ => throw new ArgumentException($"argument must be a Number, not {num}!")
            });

      //=================================================================================================================
      public static readonly CoreFun.FuncT Id = PureUnaryFun(o => o);

      //=================================================================================================================
      public static readonly CoreFun.FuncT Quote =
         PureUnaryFun(o => o);

      //=================================================================================================================
      public static readonly CoreFun.FuncT Length =
         PureUnaryFun(o => new Integer(o.Length));

      //=================================================================================================================
      public static readonly CoreFun.FuncT ObjToString =
         PureUnaryFun(o => new String(o.ToPrincString()));

      //=================================================================================================================
      public static readonly CoreFun.FuncT InternString =
         PureUnaryFun<String>(str => Intern(str.Value));

      //===================================================================================================================
      public static readonly CoreFun.FuncT Type =
         PureUnaryFun(o => Intern(o is Pair
                                  ? ":cons"
                                  : ":" + o.TypeName.ToLower()));

      //================================================================================================================
   }
   //===================================================================================================================
}
