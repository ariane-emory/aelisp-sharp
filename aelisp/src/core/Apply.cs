using static System.Console;
using static Ae;

//======================================================================================================================
static partial class Ae
{
   //====================================================================================================================
   public static partial class Core
   {

      //================================================================================================================
      private static LispObject Requote(LispObject obj) =>
         Ae.Cons(Intern("quote"), Ae.Cons(obj, Nil));

      //================================================================================================================
      public static bool ApplyLogEnabled { get; set; } = true;

      //================================================================================================================
      public static void ApplyLog(string msg)
      {
         if (ApplyLogEnabled)
            WriteLine(msg);
      }

      //================================================================================================================
      public static LispObject Apply(Env env, LispObject argsList)
      {
         if (!argsList.IsProperList)
            throw new ArgumentException($"argsList must be a proper list, not {argsList}!");

         ApplyLog($"argList:        {argsList.PrincString()}");

         var arg1 = ((Pair)argsList)[0];

         if (!(argsList is Pair argsListPair))
            throw new ArgumentException($"argsList must be a pair, not {argsList}!");

         ApplyLog($"argListPair:    {argsListPair.PrincString()}");

         Pair newExpr = (Pair)Ae.Cons(argsListPair.Car, Nil);
         LispObject newExprTail = newExpr;
         LispObject current = argsListPair.Cdr;
         LispObject evaledArg = Nil;

         ApplyLog($"newExpr:        {newExpr.PrincString()}");
         ApplyLog($"newExprTail:    {newExprTail.PrincString()}");
         ApplyLog($"current:        {current.PrincString()}");
         // ApplyLog($"evaledArg:   {evaledArg.PrincString()}");

         while (current is Pair currentPair && !currentPair.Cdr.IsNil)
         {
            var arg = currentPair.Car;
            ApplyLog($"\narg:            {arg.PrincString()}");

            var argIsQuoteForm = arg is Pair argPair && argPair.Car == Intern("quote");
            ApplyLog($"argIsQuoteForm: {argIsQuoteForm}");

            // if (argIsQuoteForm)
            // {
            //    arg = ((Pair)(((Pair)arg).Cdr)).Car;
            //    ApplyLog($"arg 2:          {arg.PrincString()}");
            // }

            if (!argIsQuoteForm)
            {
               ApplyLog($"evaluating:     {currentPair.Car.PrincString()}");
               evaledArg = currentPair.Car.Eval(env);
            }
            else
            {
               evaledArg = currentPair.Car; // 'Eval(env);.Car;
            }

            ApplyLog($"evaledArg:      {evaledArg.PrincString()}");

            var elem = Ae.Cons(evaledArg, Nil);

            ApplyLog($"elem:           {elem.PrincString()}");

            ((Pair)newExprTail).Cdr = elem;
            newExprTail = ((Pair)newExprTail).Cdr;
            ApplyLog($"newExpr:        {newExpr.PrincString()}");
            current = currentPair.Cdr;
         }

         LispObject last = ((Pair)current).Car;

         ApplyLog($"last:           {last.PrincString()}");

         var lastIsQuoteForm = last is Pair lastPair && lastPair.Car == Intern("quote");
         ApplyLog($"lastIsQForm:    {lastIsQuoteForm}");

         last = lastIsQuoteForm ? ((Pair)((Pair)last).Cdr).Car : last.Eval(env);
         ApplyLog($"last 2:         {last.PrincString()}");

         if (!last.IsProperList)
            throw new ArgumentException($"last must be a proper list, not {last}!");
         
         while (!last.IsNil)
         {
            evaledArg = ((Pair)last).Car;
            
            if (! lastIsQuoteForm)
               evaledArg = evaledArg.Eval(env);
            
            ((Pair)newExprTail).Cdr = Ae.Cons(evaledArg, Nil);
            newExprTail = ((Pair)newExprTail).Cdr;
            last = ((Pair)last).Cdr;
         }

         ApplyLog($"last 3:         {last.PrincString()}");
         ApplyLog($"newExpr:        {newExpr.PrincString()}");

         return newExpr.Eval(env);
         // return Nil;
      }

      //================================================================================================================
   }
   //===================================================================================================================
}
