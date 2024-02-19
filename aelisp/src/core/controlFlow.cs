using static System.Console;
using static Ae;

//======================================================================================================================
static partial class Ae
{
   //====================================================================================================================
   public static partial class Core
   {
      //=================================================================================================================
      public static readonly CoreFun.FuncT Repeat = (env, argsList) =>
      {
         if (argsList.IsImproperList)
            throw new ArgumentException($"argsList must be a proper list, not {argsList}!");

         var first_arg = ((Pair)argsList).Car.Eval(env);
         var body = ((Pair)argsList).Cdr;
         var result = Nil;

         if (body.IsImproperList)
            throw new ArgumentException("body must be a proper list");

         if (!((first_arg is Integer times) && (times.Value >= 0)))
            throw new ArgumentException($"repeat requires a positive integer as its first argument, not {first_arg}!");

         for (int ix = 0; ix < times.Value; ix++)
            result = Progn(env, body);

         return result;
      };

      //=================================================================================================================
      private static CoreFun.FuncT WhileOrUntilSpecialFun(Func<LispObject, bool> pred) =>
         (Env env, LispObject argsList) =>
         {
            if (argsList.IsImproperList)
               throw new ArgumentException($"argsList must be a proper list, not {argsList}!");

            var test = ((Pair)argsList).Car;
            var body = ((Pair)argsList).Cdr;
            var result = Nil;

            if (body.IsImproperList)
               throw new ArgumentException("body must be a proper list");

            while (pred(test.Eval(env)))
               result = Progn(env, body);

            return result;
         };

      //=================================================================================================================
      public static readonly CoreFun.FuncT Until =
         WhileOrUntilSpecialFun(o => o.IsNil);

      //=================================================================================================================
      public static readonly CoreFun.FuncT While =
         WhileOrUntilSpecialFun(o => !o.IsNil);

      //=================================================================================================================
      private static CoreFun.FuncT WhenOrUnlessSpecialFun(Func<LispObject, bool> pred) =>
         (Env env, LispObject argsList) =>
         {
            if (argsList.IsImproperList)
               throw new ArgumentException($"argsList must be a proper list, not {argsList}!");

            var test = ((Pair)argsList).Car;
            var body = ((Pair)argsList).Cdr;

            if (body.IsImproperList)
               throw new ArgumentException("body must be a proper list");

            return pred(test.Eval(env))
               ? Progn(env, body)
               : Nil;
         };

      //=================================================================================================================
      public static readonly CoreFun.FuncT When =
         WhenOrUnlessSpecialFun(o => !o.IsNil);

      //=================================================================================================================
      public static readonly CoreFun.FuncT Unless =
         WhenOrUnlessSpecialFun(o => o.IsNil);

      //=================================================================================================================
      public static readonly CoreFun.FuncT If = (env, argsList) =>
      {
         if (argsList.IsImproperList)
            throw new ArgumentException($"argsList must be a proper list, not {argsList}!");

         var if_cond = ((Pair)argsList).Car;
         var then_branch = ((Pair)((Pair)argsList).Cdr).Car;
         var else_branch = ((Pair)((Pair)argsList).Cdr).Cdr;

         if (else_branch.IsImproperList)
            throw new ArgumentException("else body must be a proper list");

         return !if_cond.Eval(env).IsNil
            ? then_branch.Eval(env)
            : Progn(env, else_branch);
      };

      //================================================================================================================
      public static readonly CoreFun.FuncT Cond = (env, argsList) =>
      {
         if (argsList.IsImproperList)
            throw new ArgumentException($"argsList must be a proper list, not {argsList}!");

         // First pass: Validation
         bool elseFound = false;
         var current = argsList;

         if (current.IsImproperList)
            throw new ArgumentException("cond arguments must be a proper list");

         while (current is Pair currentPair)
         {
            var condItem = currentPair.Car;

            if (!(condItem is Pair condItemPair) || condItemPair.Cdr.IsNil)
               throw new ArgumentException("cond arguments must be proper lists with at least two elements");

            var itemCar = condItemPair.Car;

            if (itemCar.Equals(Intern("else")))
            {
               if (elseFound)
                  throw new ArgumentException("Only one else clause is allowed in a cond expression");

               elseFound = true;

               if (!(currentPair.Cdr.IsNil))
                  throw new ArgumentException("If used, else clause must be the last clause in a cond expression");
            }
            current = currentPair.Cdr;
         }

         // Second pass: Evaluation
         current = argsList;

         while (current is Pair currentClausePair)
         {
            var condClause = currentClausePair.Car;

            if (condClause is Pair condClausePair)
            {
               var condition = condClausePair.Car;
               var actions = condClausePair.Cdr;

               var conditionResult = Nil;

               if (condition.Equals(Intern("else")))
                  conditionResult = True;
               else
                  conditionResult = condition.Eval(env);

               if (!conditionResult.IsNil)
                  return Progn(env, actions);
            }
            current = currentClausePair.Cdr;
         }

         return Nil;
      };

      //================================================================================================================
      public static readonly CoreFun.FuncT Case = (env, argsList) =>
      {
         if (argsList.IsImproperList)
            throw new ArgumentException($"argsList must be a proper list, not {argsList}!");

         // if (!(argsList is Pair argsPair))
         //   throw new ArgumentException($"{nameof(argsList)} is not a list, something has gone wrong.");

         var argsPair = (Pair)argsList;
         var keyForm = argsPair.Car.Eval(env);
         var caseForms = argsPair.Cdr;

         // First pass: Validation and check for multiple 'else' clauses
         bool elseFound = false;
         var current = caseForms;

         if (caseForms.IsImproperList)
            throw new ArgumentException("case forms must be a proper list");

         while (current is Pair currentPair)
         {
            var caseItem = currentPair.Car;

            if (!(caseItem is Pair caseItemPair && caseItemPair.IsProperList))
               throw new ArgumentException("case forms elements must be proper lists");

            var caseItemCar = caseItemPair.Car;

            if (caseItemCar == Intern("else"))
            {
               if (elseFound)
                  throw new ArgumentException("Only one else clause is allowed in a case expression");

               elseFound = true;
            }

            current = currentPair.Cdr;
         }

         // Second pass: Evaluation
         current = caseForms;

         while (current is Pair currentCasePair)
         {
            var caseClause = currentCasePair.Car;

            if (caseClause is Pair caseClausePair)
            {
               var caseKeys = caseClausePair.Car;
               var caseActions = caseClausePair.Cdr;

               if (caseKeys == Intern("else") ||
                   caseKeys.Eql(keyForm) ||
                   (caseKeys is Pair caseKeysPair && caseKeysPair.ToList().Contains(keyForm)))
                  return Progn(env, caseActions);
            }

            current = currentCasePair.Cdr;
         }

         return Nil; // No matching case found
      };

      //================================================================================================================
   }
   //===================================================================================================================
}
