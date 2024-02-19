using static System.Console;
using static Ae;

//======================================================================================================================
static partial class Ae
{
   //====================================================================================================================
   public static partial class Core
   {
      //=================================================================================================================
      private static CoreFun.FuncT UnaryPredicateFun(Func<LispObject, bool> pred)
       => (Env env, LispObject argsList, int argsLength) =>
       {
          if (argsList.IsImproperList)
             throw new ArgumentException("argsList must be a proper list");

          return Truthiness(pred(((Pair)argsList)[0]));
       };

      //=================================================================================================================
      private static CoreFun.FuncT UnaryPredicateFun<T1>(Func<T1, bool> pred)
       => (Env env, LispObject argsList, int argsLength) =>
       {
          if (argsList.IsImproperList)
             throw new ArgumentException("argsList must be a proper list");

          var arg1 = ((Pair)argsList)[0];

          if (arg1 is T1 typedArg1)
             return Truthiness(pred(typedArg1));

          throw new ArgumentException($"Argument must be of type {typeof(T1).Name}");
       };

      //=================================================================================================================
      public static readonly CoreFun.FuncT PositiveP =
         UnaryPredicateFun(num => num switch
         {
            Integer integer => integer.Value > 0,
            Float floatObj => floatObj.Value > 0,
            Rational rational => rational.Numerator > 0,
            _ => throw new ArgumentException($"Argument must be a number, not {num}!")
         });

      //=================================================================================================================
      public static readonly CoreFun.FuncT NegativeP =
         UnaryPredicateFun(num => num switch
         {
            Integer integer => integer.Value < 0,
            Float floatObj => floatObj.Value < 0,
            Rational rational => rational.Numerator < 0,
            _ => throw new ArgumentException($"Argument must be a number, not {num}!")
         });

      //=================================================================================================================
      public static readonly CoreFun.FuncT KeywordP =
         UnaryPredicateFun<Symbol>(sym => sym.IsKeyword);

      //=================================================================================================================
      public static readonly CoreFun.FuncT ProperP =
         UnaryPredicateFun(o => o.IsProperList);

      //=================================================================================================================
      public static readonly CoreFun.FuncT ImproperP =
         UnaryPredicateFun(o => o.IsImproperList);

      //=================================================================================================================
      public static readonly CoreFun.FuncT TailP =
       UnaryPredicateFun(o => o.IsList);

      //=================================================================================================================
      public static readonly CoreFun.FuncT AtomP =
         UnaryPredicateFun(o => o.IsAtom);

      //=================================================================================================================
      public static readonly CoreFun.FuncT NilP =
         UnaryPredicateFun(o => o.IsNil);

      //=================================================================================================================
      public static readonly CoreFun.FuncT Not =
         UnaryPredicateFun(o => o.IsNil);

      //================================================================================================================
   }
   //===================================================================================================================
}
