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
         ThrowUnlessIsProperList("argsList", argsList);

         ApplyLog($"argList:        {argsList.ToPrincString()}");

         var arg1 = ((Pair)argsList)[0];

         if (!(argsList is Pair argsListPair))
            throw new ArgumentException($"argsList must be a pair, not {argsList}!");

         ApplyLog($"argListPair:    {argsListPair.ToPrincString()}");

         Pair newExpr = (Pair)Ae.Cons(argsListPair.Car, Nil);
         LispObject newExprTail = newExpr;
         LispObject current = argsListPair.Cdr;
         LispObject evaledArg = Nil;

         ApplyLog($"newExpr:        {newExpr.ToPrincString()}");
         ApplyLog($"newExprTail:    {newExprTail.ToPrincString()}");
         ApplyLog($"current:        {current.ToPrincString()}");
         // ApplyLog($"evaledArg:   {evaledArg.ToPrincString()}");

         while (current is Pair currentPair && !currentPair.Cdr.IsNil)
         {
            var arg = currentPair.Car;
            ApplyLog($"\narg:            {arg.ToPrincString()}");

            var argIsQuoteForm = arg is Pair argPair && argPair.Car == Intern("quote");
            ApplyLog($"argIsQuoteForm: {argIsQuoteForm}");

            // if (argIsQuoteForm)
            // {
            //    arg = ((Pair)(((Pair)arg).Cdr)).Car;
            //    ApplyLog($"arg 2:          {arg.ToPrincString()}");
            // }

            if (!argIsQuoteForm)
            {
               ApplyLog($"evaluating:     {currentPair.Car.ToPrincString()}");
               evaledArg = currentPair.Car.Eval(env);
            }
            else
            {
               evaledArg = currentPair.Car; // 'Eval(env);.Car;
            }

            ApplyLog($"evaledArg:      {evaledArg.ToPrincString()}");

            var elem = Ae.Cons(evaledArg, Nil);

            ApplyLog($"elem:           {elem.ToPrincString()}");

            ((Pair)newExprTail).Cdr = elem;
            newExprTail = ((Pair)newExprTail).Cdr;
            ApplyLog($"newExpr:        {newExpr.ToPrincString()}");
            current = currentPair.Cdr;
         }

         LispObject last = ((Pair)current).Car;

         ApplyLog($"last:           {last.ToPrincString()}");

         var lastIsQuoteForm = last is Pair lastPair && lastPair.Car == Intern("quote");
         ApplyLog($"lastIsQForm:    {lastIsQuoteForm}");

         last = lastIsQuoteForm ? ((Pair)((Pair)last).Cdr).Car : last.Eval(env);
         ApplyLog($"last 2:         {last.ToPrincString()}");

         ThrowUnlessIsProperList("last", last);
         
         while (!last.IsNil)
         {
            evaledArg = ((Pair)last).Car;
            
            if (! lastIsQuoteForm)
               evaledArg = evaledArg.Eval(env);
            
            ((Pair)newExprTail).Cdr = Ae.Cons(evaledArg, Nil);
            newExprTail = ((Pair)newExprTail).Cdr;
            last = ((Pair)last).Cdr;
         }

         ApplyLog($"last 3:         {last.ToPrincString()}");
         ApplyLog($"newExpr:        {newExpr.ToPrincString()}");

         return newExpr.Eval(env);
         // return Nil;
      }

      //================================================================================================================
   }
   //===================================================================================================================
}
