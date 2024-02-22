using static System.Console;
using static Ae;

//======================================================================================================================
static partial class Ae
{
   //====================================================================================================================
   public static partial class Core
   {
      //=================================================================================================================
      private static CoreFun.FuncT UnaryPredicateFun(Func<LispObject, bool> pred) =>
         (Env env, LispObject argsList) =>
       {
          ThrowUnlessIsProperList("argsList", argsList);
          return Truthiness(pred(((Pair)argsList)[0]));
       };

      //=================================================================================================================
      private static CoreFun.FuncT UnaryPredicateFun<T1>(Func<T1, bool> pred) =>
         (Env env, LispObject argsList) =>
       {
          ThrowUnlessIsProperList("argsList", argsList);
          
          var arg1 = ((Pair)argsList)[0];

          if (arg1 is T1 typedArg1)
             return Truthiness(pred(typedArg1));

          throw new ArgumentException($"argument must be of type {typeof(T1).Name}, not {arg1.ToPrincString()}!");
       };

      //=================================================================================================================
      public static LispObject PositiveP(Env env, LispObject argsList) =>
         UnaryPredicateFun(num => num switch
         {
            Integer integer => integer.Value >= 0,
            Float floatObj => floatObj.Value >= 0,
            Rational rational => rational.Numerator >= 0,
            _ => throw new ArgumentException($"argument must be a Number, not {num.ToPrincString()}!")
         })(env, argsList);

      //=================================================================================================================
      public static LispObject NegativeP(Env env, LispObject argsList) =>
       UnaryPredicateFun(num => num switch
       {
          Integer integer => integer.Value < 0,
          Float floatObj => floatObj.Value < 0,
          Rational rational => rational.Numerator < 0,
          _ => throw new ArgumentException($"argument must be a Number, not {num.ToPrincString()}!")
       })(env, argsList);

      //=================================================================================================================
      public static LispObject KeywordP(Env env, LispObject argsList) =>
         UnaryPredicateFun<Symbol>(sym => sym.IsKeyword)
         (env, argsList);

      //=================================================================================================================
      public static LispObject ProperP(Env env, LispObject argsList) =>
         UnaryPredicateFun(o => o.IsProperList)
         (env, argsList);

      //=================================================================================================================
      public static LispObject ImproperP(Env env, LispObject argsList) =>
         UnaryPredicateFun(o => o.IsImproperList)
         (env, argsList);

      //=================================================================================================================
      public static LispObject TailP(Env env, LispObject argsList) =>
         UnaryPredicateFun(o => o.IsList)
         (env, argsList);

      //=================================================================================================================
      public static LispObject AtomP(Env env, LispObject argsList) =>
         UnaryPredicateFun(o => o.IsAtom)
         (env, argsList);

      //=================================================================================================================
      public static LispObject NilP(Env env, LispObject argsList) =>
         UnaryPredicateFun(o => o.IsNil)
         (env, argsList);

      //=================================================================================================================
      public static LispObject Not(Env env, LispObject argsList) =>
         UnaryPredicateFun(o => o.IsNil)
         (env, argsList);

      //================================================================================================================
   }
   //===================================================================================================================
}
