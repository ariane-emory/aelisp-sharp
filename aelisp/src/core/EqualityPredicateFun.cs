using static System.Console;
using static Ae;

//======================================================================================================================
static partial class Ae
{
   //====================================================================================================================
   public static partial class Core
   {
      //=================================================================================================================
      private static CoreFun.FuncT EqualityPredicateFun(Func<LispObject, LispObject, bool> pred) =>
         (env, argsList) =>
         {
            if (argsList.IsImproperList)
               throw new ArgumentException($"argsList must be a proper list, not {argsList}!");

            var arg0 = ((Pair)argsList).Car;
            var current = ((Pair)argsList).Cdr;

            while (current is Pair currentPair)
            {
               // WriteLine($"Compare {arg0} to {((Pair)currentPair).Car}.");

               if (!pred(arg0, ((Pair)currentPair).Car))
                  return Nil;

               current = ((Pair)currentPair).Cdr;
            }

            return True;
         };

      //=================================================================================================================
      public static readonly CoreFun.FuncT EqP =
        EqualityPredicateFun((o1, o2) => o1 == o2);

      //=================================================================================================================
      public static readonly CoreFun.FuncT EqlP =
         EqualityPredicateFun((o1, o2) => o1.Eql(o2));

      //================================================================================================================
   }
   //===================================================================================================================
}
