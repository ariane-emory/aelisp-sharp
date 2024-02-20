using static System.Console;
using static Ae;

//======================================================================================================================
static partial class Ae
{
   //====================================================================================================================
   public static partial class Core
   {
      //=================================================================================================================
      private static CoreFun.FuncT PureBinaryFun(Func<LispObject, LispObject, LispObject> func)
         => (Env env, LispObject argsList) =>
          {
             if (argsList.IsImproperList)
                throw new ArgumentException($"argsList must be a proper list, not {argsList}!");

             var arg1 = ((Pair)argsList)[0];
             var arg2 = ((Pair)argsList)[1];

             return func(arg1, arg2);
          };

      //=================================================================================================================
      private static CoreFun.FuncT PureBinaryFun<T1, T2>(Func<T1, T2, LispObject> func)
          where T1 : LispObject
          where T2 : LispObject
          => (Env env, LispObject argsList) =>
          {
             if (argsList.IsImproperList)
                throw new ArgumentException($"argsList must be a proper list, not {argsList}!");

             var arg1 = ((Pair)argsList)[0];
             var arg2 = ((Pair)argsList)[1];

             if (arg1 is T1 typedArg1 && arg2 is T2 typedArg2)
                return func(typedArg1, typedArg2);

             throw new ArgumentException($"Arguments must be of types {typeof(T1).Name} and {typeof(T2).Name}");
          };

      //=================================================================================================================
      public static readonly CoreFun.FuncT Cons =
         PureBinaryFun((arg1, arg2) => Ae.Cons(arg1, arg2));

      //=================================================================================================================
      public static readonly CoreFun.FuncT NewRational =
         PureBinaryFun<Integer, Integer>((num, den) => new Rational(num.Value, den.Value));

      //=================================================================================================================
      public static readonly CoreFun.FuncT Add =
         PureBinaryFun<Number, Number>((left, right) => {
            var (leftNumber, rightNumber) = ThrowUnlessNumbers(left, right);

            return leftNumber.Add(rightNumber);
         });

      //=================================================================================================================
      public static readonly CoreFun.FuncT Sub =
         PureBinaryFun<Number, Number>((left, right) => {
            var (leftNumber, rightNumber) = ThrowUnlessNumbers(left, right);

            return leftNumber.Sub(rightNumber);
         });

      //=================================================================================================================
      public static readonly CoreFun.FuncT Mul =
         PureBinaryFun<Number, Number>((left, right) => {
            var (leftNumber, rightNumber) = ThrowUnlessNumbers(left, right);

            return leftNumber.Mul(rightNumber);
         });

      //=================================================================================================================
      public static readonly CoreFun.FuncT Div =
         PureBinaryFun<Number, Number>((left, right) => {
            var (leftNumber, rightNumber) = ThrowUnlessNumbers(left, right);

            return leftNumber.Div(rightNumber);
         });

      //================================================================================================================
   }
   //===================================================================================================================
}
