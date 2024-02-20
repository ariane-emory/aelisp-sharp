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
            if (argsList.IsImproperList)
               throw new ArgumentException($"argsList must be a proper list, not {argsList}!");

            return func(((Pair)argsList)[0]);
         };

      //=================================================================================================================
      private static CoreFun.FuncT PureUnaryFun<T1>(Func<T1, LispObject> func)
       where T1 : LispObject
       => (Env env, LispObject argsList) =>
       {
          if (argsList.IsImproperList)
             throw new ArgumentException($"argsList must be a proper list, not {argsList}!");

          var arg1 = ((Pair)argsList)[0];

          if (arg1 is T1 typedArg1)
             return func(typedArg1);

          throw new ArgumentException($"Argument must be of type {typeof(T1).Name}");
       };

      //=================================================================================================================
      public static readonly CoreFun.FuncT Plus1 =
         PureUnaryFun(num =>
            num switch
            {
               Integer integer => (LispObject)new Integer(integer.Value + 1),
               Float floatObj => (LispObject)new Float(floatObj.Value + 1),
               Rational rational => (LispObject)new Rational(rational.Numerator + rational.Denominator, rational.Denominator),
               _ => throw new ArgumentException($"Argument must be a number, not {num}!")
            });

      //=================================================================================================================
      public static readonly CoreFun.FuncT Minus1 =
         PureUnaryFun(num =>
            num switch
            {
               Integer integer => (LispObject)new Integer(integer.Value - 1),
               Float floatObj => (LispObject)new Float(floatObj.Value - 1),
               Rational rational => (LispObject)new Rational(rational.Numerator - rational.Denominator, rational.Denominator),
               _ => throw new ArgumentException($"Argument must be a number, not {num}!")
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
         PureUnaryFun(o => new String(o.PrincString()));

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
