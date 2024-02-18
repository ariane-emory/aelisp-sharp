using static System.Console;
using static Ae;
using LispObjectParser = Pidgin.Parser<Ae.LispToken, Ae.LispObject>;
using LispTokenParser = Pidgin.Parser<Ae.LispToken, Ae.LispToken>;

//======================================================================================================================
static partial class Ae
{
   //====================================================================================================================
   public static class Core
   {
      //=================================================================================================================
      // public static readonly CoreFun.FuncT Progn = (env, argsList, argsLength) =>
      // {
      //    var result = Nil;

      //    if (argsList is Pair argsListPair)
      //       foreach (var elem in argsListPair)
      //          result = elem.Eval(env);

      //    return result;
      // };


      //=================================================================================================================
      public static readonly CoreFun.FuncT Progn = (env, argsList, argsLength) =>
      {
         var result = Nil;

         while (argsList is Pair argsListPair)
         {
            result = argsListPair.Car.Eval(env);
            argsList = argsListPair.Cdr;
         }

         return result;
      };

      //=================================================================================================================
      public static readonly CoreFun.FuncT Setq = (env, argsList, argsLength) =>
      {
         if (argsLength % 2 != 0)
            throw new ArgumentException("setq requires an even number of arguments!");

         var result = Nil;

         while (argsList is Pair currentPair)
         {
            var pairHead = currentPair.Car;
            var valueExpression = ((Pair)currentPair.Cdr).Car;

            if (!(pairHead is Symbol sym))
               throw new ArgumentException($"The first element of each pair must be a symbol, not {pairHead}.");

            if (sym.IsKeyword || sym.IsSelfEvaluating)
               throw new ArgumentException($"symbol {sym} may not be set.");

            LispObject evaluatedValue = valueExpression.Eval(env);

            result = valueExpression.Eval(env);

            var mode = sym.IsSpecial ? Env.LookupMode.Global : Env.LookupMode.Nearest;

            env.Set(mode, sym, result);

            argsList = ((Pair)currentPair.Cdr).Cdr;
         }

         return result;
      };

      //=================================================================================================================
      public static readonly CoreFun.FuncT Repeat = (env, argsList, argsLength) =>
      {
         var first_arg = ((Pair)argsList).Car.Eval(env);
         var body = ((Pair)argsList).Cdr;
         var result = Nil;
         var bodyLength = body.Length;

         if (!((first_arg is Integer times) && (times.Value >= 0)))
            throw new ArgumentException($"repeat requires a positive integer as its first argument, not {first_arg}!");

         for (int ix = 0; ix < times.Value; ix++)
            result = Progn(env, body, bodyLength);

         return result;
      };

      //=================================================================================================================
      public static readonly CoreFun.FuncT Until = (env, argsList, argsLength) =>
      {
         LispObject while_cond = ((Pair)argsList).Car;
         LispObject do_branch = ((Pair)argsList).Cdr;
         
         var result = Nil;
         
         while (while_cond.Eval(env).IsNil)
            result = Progn(env, do_branch, do_branch.Length);
         
         return result;
      };

      //=================================================================================================================
      public static readonly CoreFun.FuncT While = (env, argsList, argsLength) =>
      {
         LispObject while_cond = ((Pair)argsList).Car;
         LispObject do_branch = ((Pair)argsList).Cdr;

         var result = Nil;

         while (!while_cond.Eval(env).IsNil)
            result = Progn(env, do_branch, do_branch.Length);

         return result;
      };

      //=================================================================================================================
      public static readonly CoreFun.FuncT Unless = (env, argsList, argsLength) =>
      {
         LispObject if_cond = ((Pair)argsList).Car;
         LispObject then_branch = ((Pair)argsList).Cdr;

         return if_cond.Eval(env).IsNil
            ? Progn(env, then_branch, then_branch.Length)
            : Nil;
      };

      //=================================================================================================================
      public static readonly CoreFun.FuncT When = (env, argsList, argsLength) =>
      {
         LispObject if_cond = ((Pair)argsList).Car;
         LispObject then_branch = ((Pair)argsList).Cdr;

         return !if_cond.Eval(env).IsNil
            ? Progn(env, then_branch, then_branch.Length)
            : Nil;
      };

      //=================================================================================================================
      public static readonly CoreFun.FuncT If = (env, argsList, argsLength) =>
      {
         LispObject if_cond = ((Pair)argsList).Car;
         LispObject then_branch = ((Pair)((Pair)argsList).Cdr).Car;
         LispObject else_branch = ((Pair)((Pair)argsList).Cdr).Cdr;

         return !if_cond.Eval(env).IsNil
            ? then_branch.Eval(env)
            : Progn(env, else_branch, else_branch.Length);
      };

      //=================================================================================================================
      public static readonly CoreFun.FuncT Length = (env, argsList, argsLength)
       => new Integer(((Pair)argsList)[0].Length);

      //=================================================================================================================
      public static readonly CoreFun.FuncT TailP = (env, argsList, argsLength)
       => Truthiness(((Pair)argsList)[0].IsList);

      //=================================================================================================================
      public static readonly CoreFun.FuncT AtomP = (env, argsList, argsLength)
         => Truthiness(((Pair)argsList)[0].IsAtom);

      //=================================================================================================================
      public static readonly CoreFun.FuncT NilP = (env, argsList, argsLength)
         => Truthiness(((Pair)argsList)[0].IsNil);

      //=================================================================================================================
      public static readonly CoreFun.FuncT Rplacd = (env, argsList, argsLength) =>
      {
         LispObject arg0 = ((Pair)argsList)[0];
         LispObject arg1 = ((Pair)argsList)[1];

         if (arg0 is not Pair pair)
            throw new ArgumentException("First argument must be a cons cell!");

         pair.Cdr = arg1;

         return arg1;
      };

      //=================================================================================================================
      public static readonly CoreFun.FuncT Rplaca = (env, argsList, argsLength) =>
      {
         LispObject arg0 = ((Pair)argsList)[0];
         LispObject arg1 = ((Pair)argsList)[1];

         if (arg0 is not Pair pair)
            throw new ArgumentException("First argument must be a cons cell!");

         pair.Car = arg1;

         return arg1;
      };

      //=================================================================================================================
      public static readonly CoreFun.FuncT Id = (env, argsList, argsLength)
         => ((Pair)argsList)[0];

      //=================================================================================================================
      public static readonly CoreFun.FuncT Not = (env, argsList, argsLength)
         => Truthiness(((Pair)argsList)[0].IsNil);

      //=================================================================================================================
      public static readonly CoreFun.FuncT Car = (env, argsList, argsLength) =>
      {
         LispObject arg0 = ((Pair)argsList)[0];

         if (!arg0.IsList)
            throw new ArgumentException("Argument must be a list!");

         if (arg0.IsNil)
            return Nil;

         return ((Pair)arg0).Car;
      };

      //=================================================================================================================
      public static readonly CoreFun.FuncT Cdr = (env, argsList, argsLength) =>
      {
         LispObject arg0 = ((Pair)argsList)[0];

         if (!arg0.IsList)
            throw new ArgumentException("Argument must be a list!");

         if (arg0.IsNil)
            return Nil;

         return ((Pair)arg0).Cdr;
      };

      //=================================================================================================================
      public static bool ConsDebugWrite { get; set; } = false;

      public static readonly CoreFun.FuncT Cons = (env, argsList, argsLength) =>
      {
         LispObject arg0 = ((Pair)argsList)[0];
         LispObject arg1 = ((Pair)argsList)[1];

         if (Core.ConsDebugWrite)
            WriteLine($"Core Cons({arg0.Princ()}, {arg1.Princ()})");

         return Ae.Cons(arg0, arg1);
      };

      //=================================================================================================================
      public static readonly CoreFun.FuncT Eval = (env, argsList, argsLength)
         => ((Pair)argsList)[0];

      //=================================================================================================================
      public static readonly CoreFun.FuncT List = (env, argsList, argsLength)
         => argsList;

      //=================================================================================================================
      public static readonly CoreFun.FuncT Quote = (env, argsList, argsLength)
         => ((Pair)argsList)[0];

      //=================================================================================================================
      public static readonly CoreFun.FuncT EqP = (env, argsList, argsLength)
         => Truthiness(((Pair)argsList)[0] == ((Pair)argsList)[1]);

      //=================================================================================================================
      public static readonly CoreFun.FuncT EqlP = (env, argsList, argsLength)
         => Truthiness(((Pair)argsList)[0].Eql(((Pair)argsList)[1]));

      //=================================================================================================================
      private static bool IsPermittedParamSymbol(LispObject obj) =>
           obj is Symbol symbol && (!(symbol.IsSpecial || symbol.IsKeyword || symbol == True || symbol == Nil));

      //=================================================================================================================
      public static readonly CoreFun.FuncT Lambda = (env, argsList, argsLength) =>
      {
         LispObject lambdaList = ((Pair)argsList)[0];
         LispObject body = ((Pair)argsList)[1];

         if (!(lambdaList.IsList || IsPermittedParamSymbol(lambdaList)))
            throw new ArgumentException($"Lambda list must be a list or a symbol, not {lambdaList.Princ()}!");

         if (lambdaList is Symbol symbol && (symbol.IsSpecial || symbol.IsKeyword || symbol == True))
            throw new ArgumentException($"Can't use {symbol.Princ()} as a parameter!");

         if (body is not Pair)
            throw new ArgumentException($"Body argument must be a list, not {body.Princ()}!");

         LispObject currentParam = lambdaList;

         while (currentParam is Pair currentParamPair)
         {
            if (!IsPermittedParamSymbol(currentParamPair.Car))
               throw new ArgumentException($"Can't use {currentParamPair.Car.Princ()} as a parameter!");

            currentParam = currentParamPair.Cdr;
         }

         if (currentParam != Nil && !IsPermittedParamSymbol(currentParam))
            throw new ArgumentException($"Can't use {currentParam.Princ()} as a parameter!");

         return new Lambda(lambdaList, body, env);
      };

      //=================================================================================================================
      public static readonly CoreFun.FuncT Macro = (env, argsList, argsLength) =>
      {
         LispObject lambdaList = ((Pair)argsList)[0];
         LispObject body = ((Pair)argsList)[1];

         if (!(lambdaList.IsList || IsPermittedParamSymbol(lambdaList)))
            throw new ArgumentException($"Lambda list must be a list or a symbol, not {lambdaList.Princ()}!");

         if (lambdaList is Symbol symbol && (symbol.IsSpecial || symbol.IsKeyword || symbol == True))
            throw new ArgumentException($"Can't use {symbol.Princ()} as a parameter!");

         if (body is not Pair)
            throw new ArgumentException($"Body argument must be a list, not {body.Princ()}!");

         LispObject currentParam = lambdaList;

         while (currentParam is Pair currentParamPair)
         {
            if (!IsPermittedParamSymbol(currentParamPair.Car))
               throw new ArgumentException($"Can't use {currentParamPair.Car.Princ()} as a parameter!");

            currentParam = currentParamPair.Cdr;
         }

         if (currentParam != Nil && !IsPermittedParamSymbol(currentParam))
            throw new ArgumentException($"Can't use {currentParam.Princ()} as a parameter!");

         return new Macro(lambdaList, body, env);
      };

      //================================================================================================================
      public static readonly CoreFun.FuncT Cond = (env, argsList, argsLength) =>
      {
         // First pass: Validation
         bool elseFound = false;
         var current = argsList;

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

               LispObject conditionResult = Nil;

               if (condition.Equals(Intern("else")))
                  conditionResult = True;
               else
                  conditionResult = condition.Eval(env);

               if (!conditionResult.IsNil)
                  return Progn(env, actions, actions.Length);
            }
            current = currentClausePair.Cdr;
         }

         return Nil;
      };

      //================================================================================================================
      public static readonly CoreFun.FuncT Case = (env, argsList, argsLength) =>
      {
         // if (!(argsList is Pair argsPair))
         //   throw new ArgumentException($"{nameof(argsList)} is not a list, something has gone wrong.");

         var argsPair = (Pair)argsList;
         LispObject keyForm = argsPair.Car.Eval(env);
         LispObject caseForms = argsPair.Cdr;

         // First pass: Validation and check for multiple 'else' clauses
         bool elseFound = false;
         var current = caseForms;

         while (current is Pair currentPair)
         {
            var caseItem = currentPair.Car;

            if (!(caseItem is Pair caseItemPair))
               throw new ArgumentException("case forms must be proper lists");

            var caseItemCar = caseItemPair.Car;

            if (caseItemCar.Equals(Intern("else")))
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
                  return Progn(env, caseActions, caseActions.Length);
            }

            current = currentCasePair.Cdr;
         }

         return Nil; // No matching case found
      };

      //================================================================================================================
   }

   //================================================================================================================
   private static List<LispObject> ToList(this Pair obj)
   {
      var list = new List<LispObject>();
      LispObject current = obj;

      while (current is Pair pair)
      {
         list.Add(pair.Car);
         current = pair.Cdr;
      }

      if (current != Nil)
         list.Add(current);

      return list;
   }

   //===================================================================================================================
}
