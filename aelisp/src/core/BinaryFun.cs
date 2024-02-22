using static System.Console;
using static Ae;

//======================================================================================================================
static partial class Ae
{
   //====================================================================================================================
   public static partial class Core
   {
      //=================================================================================================================
      private static CoreFun.FuncT BinaryFun(Func<LispObject, LispObject, LispObject> func)
         => (Env env, LispObject argsList) =>
          {
             ThrowUnlessIsProperList("argsList", argsList);

             var arg1 = ((Pair)argsList)[0];
             var arg2 = ((Pair)argsList)[1];

             return func(arg1, arg2);
          };

      //=================================================================================================================
      private static CoreFun.FuncT BinaryFun<T1, T2>(Func<T1, T2, LispObject> func)
          where T1 : LispObject
          where T2 : LispObject
          => (Env env, LispObject argsList) =>
          {
             ThrowUnlessIsProperList("argsList", argsList);;

             var arg1 = ((Pair)argsList)[0];
             var arg2 = ((Pair)argsList)[1];

             if (arg1 is T1 typedArg1 && arg2 is T2 typedArg2)
                return func(typedArg1, typedArg2);

             throw new ArgumentException($"arguments must be of types {typeof(T1).Name} and {typeof(T2).Name}!");
          };

      //=================================================================================================================
      public static readonly CoreFun.FuncT Cons =
         BinaryFun((arg1, arg2) => Ae.Cons(arg1, arg2));

      //=================================================================================================================
      public static readonly CoreFun.FuncT NewRational =
         BinaryFun<Integer, Integer>((num, den) => new Rational(num.Value, den.Value));

      //================================================================================================================
   }
   //===================================================================================================================
}
