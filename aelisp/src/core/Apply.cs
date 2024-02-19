using static System.Console;
using static Ae;

//======================================================================================================================
static partial class Ae
{
   //====================================================================================================================
   public static partial class Core
   {
      //================================================================================================================
      public static LispObject Apply(Env env, LispObject argsList)
      {
         if (!argsList.IsProperList)
            throw new ArgumentException($"argsList must be a proper list, not {argsList}!");

         WriteLine($"argList:     {argsList.Princ()}");

         var arg1 = ((Pair)argsList)[0];

         if (!(argsList is Pair argsListPair))
            throw new ArgumentException($"argsList must be a pair, not {argsList}!");

         WriteLine($"argListPair: {argsListPair.Princ()}");

         Pair newExpr = (Pair)Ae.Cons(argsListPair.Car, Nil);
         Pair newExprTail = newExpr;
         LispObject current = argsListPair.Cdr;
         LispObject evaledArg = Nil;

         WriteLine($"newExpr:     {newExpr.Princ()}");
         WriteLine($"newExprTail: {newExprTail.Princ()}");
         WriteLine($"current:     {current.Princ()}");
         WriteLine($"evaledArg:   {evaledArg.Princ()}");

         while (current is Pair currentPair && ! currentPair.Cdr.IsNil)
         {
            evaledArg = currentPair.Car.Eval(env);
            newExprTail.Cdr = Ae.Cons(evaledArg, Nil);
            newExprTail = (Pair)newExprTail.Cdr;
            current = currentPair.Cdr;
            
            WriteLine($"current:     {current.Princ()}");
            WriteLine($"newExpr:     {newExpr.Princ()}");
            WriteLine($"newExprTail: {newExprTail.Princ()}");
            WriteLine($"current:     {current.Princ()}");
            WriteLine($"evaledArg:   {evaledArg.Princ()}");
         }

         LispObject last = ((Pair)current).Car;
         
         WriteLine($"\nlast:        {last.Princ()}");

         var lastIsQuoteForm = last is Pair lastPair && lastPair.Car == Intern("quote");
            last = lastIsQuoteForm ? ((Pair)((Pair)last).Cdr).Car : last.Eval(env);

            WriteLine($"last:        {last.Princ()}");
         
            // LispObject current = argsListPair;
            // var stash = Nil;

            // while (current is Pair currentPair && currentPair.Cdr is Pair)
            // {
            //    stash = Ae.Cons(currentPair.Car.Eval(env), stash);
            //    current = currentPair.Cdr;
            // }

            // WriteLine($"Stash:   {stash.Princ()}");
            // WriteLine($"current: {current.Princ()}");

            // var last = ((Pair)current).Car;
            // WriteLine($"last:    {last.Princ()}");
            // var lastIsQuoteForm = last is Pair lastPair && lastPair.Car == Intern("quote");
            // WriteLine(lastIsQuoteForm);
            // var lastTail = (Pair)((Pair)last).Cdr;
            // var newExpr = lastIsQuoteForm ? lastTail.Car : lastTail;

            // WriteLine($"newExpr: {newExpr.Princ()}");

            // current = stash;

            // while (current is Pair currentPair)
            // {
            //    newExpr = Ae.Cons(currentPair.Car, newExpr);
            //    current = currentPair.Cdr;
            // }

            // WriteLine($"newExpr after consing: {newExpr.Princ()}");

            // return newExpr.Eval(env);
            return Nil;
        }

      //================================================================================================================
   }
   //===================================================================================================================
}
