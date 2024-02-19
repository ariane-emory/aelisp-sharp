using static System.Console;
using static Ae;

//======================================================================================================================
static partial class Ae
{
   //====================================================================================================================
   public static partial class Core
   {
      //=================================================================================================================
      public static readonly CoreFun.FuncT Apply = (env, argsList) =>
      {
         if (argsList is not Pair argsPair)
            throw new ArgumentException("apply expects at least one argument");

         var func = argsPair.Car;
         var args = argsPair.Cdr;

         if (args.IsImproperList || args.IsNil)
            throw new ArgumentException("apply expects the last argument to be a proper list");

         // Build new expression
         var newExpr = new List<LispObject> { func };
         while (args is Pair argsPairRest && argsPairRest.Cdr is Pair)
         {
            var arg = argsPairRest.Car;
            var evaluatedArg = IsQuoteForm(arg) ? Unquote(arg) : arg.Eval(env);
            newExpr.Add(Requote(evaluatedArg));
            args = argsPairRest.Cdr;
         }

         // Handle last argument, which should be a list
         var lastArg = args is Pair lastPairArg ? lastPairArg.Car : args;
         var lastList = IsQuoteForm(lastArg) ? Unquote(lastArg) : lastArg.Eval(env);
         if (lastList is not Pair && lastList != Nil)
            throw new ArgumentException("apply requires the last argument to be a proper list");

         foreach (var item in ToList(lastList))
         {
            newExpr.Add(item);
         }

         // Evaluate the new expression
         var newExprList = ListToPair(newExpr);

         return newExprList.Eval(env);
      };

      private static bool IsQuoteForm(LispObject obj) =>
          obj is Pair pair && pair.Car.Equals(Intern("quote")) && pair.Cdr is Pair;

      private static LispObject Unquote(LispObject quoted) =>
          ((Pair)((Pair)quoted).Cdr).Car;

      private static LispObject Requote(LispObject obj) =>
          Ae.Cons(Intern("quote"), Ae.Cons(obj, Nil));

      private static IEnumerable<LispObject> ToList(LispObject list)
      {
         while (list is Pair pair)
         {
            yield return pair.Car;
            list = pair.Cdr;
         }
      }

      private static Pair ListToPair(IEnumerable<LispObject> list)
      {
         // Start with an empty pair (Nil . Nil) as the seed for aggregation.
         var seed = new Pair(Nil, Nil);
         // Aggregate the list into a Pair structure.
         var result = list.Aggregate(seed, (current, item) => new Pair(item, current));

         // Since we're always working with Pair instances, the result is guaranteed to be a Pair.
         return result;
      }

      //================================================================================================================
   }
   //===================================================================================================================
}
