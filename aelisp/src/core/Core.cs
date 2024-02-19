using static System.Console;
using static Ae;

//======================================================================================================================
static partial class Ae
{
   //====================================================================================================================
   public static partial class Core
   {
      //=================================================================================================================
      public static readonly CoreFun.FuncT Setq = (env, argsList, argsLength) =>
      {
         if (argsLength % 2 != 0)
            throw new ArgumentException("setq requires an even number of arguments!");

         if (argsList.IsImproperList)
            throw new ArgumentException("argsList must be a proper list");

         var result = Nil;

         while (argsList is Pair currentPair)
         {
            var pairHead = currentPair.Car;
            var valueExpression = ((Pair)currentPair.Cdr).Car;

            if (!(pairHead is Symbol sym))
               throw new ArgumentException($"The first element of each pair must be a symbol, not {pairHead}.");

            if (sym.IsKeyword || sym.IsSelfEvaluating)
               throw new ArgumentException($"symbol {sym} may not be set.");

            var evaluatedValue = valueExpression.Eval(env);

            result = valueExpression.Eval(env);

            var mode = sym.IsSpecial ? Env.LookupMode.Global : Env.LookupMode.Nearest;

            env.Set(mode, sym, result);

            argsList = ((Pair)currentPair.Cdr).Cdr;
         }

         return result;
      };

      //=================================================================================================================
      public static readonly CoreFun.FuncT Eval = (env, argsList, argsLength) =>
         ((Pair)argsList)[0].Eval(env);

      //=================================================================================================================
      public static readonly CoreFun.FuncT BoundP = (env, argsList, argsLength) =>
      {
         var arg1 = ((Pair)argsList)[0];

         if (arg1 is not Symbol sym)
            throw new ArgumentException($"Argument must be a symbol, not {arg1}!");

         var (found, _) = env.Lookup(Env.LookupMode.Nearest, sym);

         return Truthiness(found);
      };

      //=================================================================================================================
      private static CoreFun.FuncT RplacaOrRplacdFun(Action<Pair, LispObject> action) =>
         (Env env, LispObject argsList, int argsLength) =>
         {
            var arg1 = ((Pair)argsList)[0];
            var arg2 = ((Pair)argsList)[1];

            if (arg1 is not Pair pair)
               throw new ArgumentException("First argument must be a cons cell!");

            action(pair, arg2);

            return arg2;
         };

      //=================================================================================================================
      public static readonly CoreFun.FuncT Rplacd =
         RplacaOrRplacdFun((pair, arg1) => pair.Cdr = arg1);

      //=================================================================================================================
      public static readonly CoreFun.FuncT Rplaca =
         RplacaOrRplacdFun((pair, arg1) => pair.Car = arg1);

      //=================================================================================================================
      private static CoreFun.FuncT CarOrCdrFun(Func<LispObject, LispObject> func) =>
         (Env env, LispObject argsList, int argsLength) =>
         {
            var arg1 = ((Pair)argsList)[0];

            if (!arg1.IsList)
               throw new ArgumentException("Argument must be a list!");

            if (arg1.IsNil)
               return Nil;

            return func(arg1);
         };

      //=================================================================================================================
      public static readonly CoreFun.FuncT Car = CarOrCdrFun(o => ((Pair)o).Car);

      //=================================================================================================================
      public static readonly CoreFun.FuncT Cdr = CarOrCdrFun(o => ((Pair)o).Cdr);

      //=================================================================================================================
      public static bool ConsDebugWrite { get; set; } = false;

      //=================================================================================================================
      public static readonly CoreFun.FuncT List = (env, argsList, argsLength)
         => argsList;

      //=================================================================================================================
      private static bool IsPermittedParamSymbol(LispObject obj) =>
           obj is Symbol symbol && (!(symbol.IsSpecial || symbol.IsKeyword || symbol == True || symbol == Nil));

      //=================================================================================================================
      public static readonly CoreFun.FuncT Lambda = (env, argsList, argsLength) =>
      {
         var lambdaList = ((Pair)argsList)[0];
         var body = ((Pair)argsList)[1];

         if (!(lambdaList.IsList || IsPermittedParamSymbol(lambdaList)))
            throw new ArgumentException($"Lambda list must be a list or a symbol, not {lambdaList.Princ()}!");

         if (lambdaList is Symbol symbol && (symbol.IsSpecial || symbol.IsKeyword || symbol == True))
            throw new ArgumentException($"Can't use {symbol.Princ()} as a parameter!");

         if (!body.IsProperList)
            throw new ArgumentException($"body must be a proper list, not {body.Princ()}");

         var currentParam = lambdaList;

         while (currentParam is Pair currentParamPair)
         {
            if (!IsPermittedParamSymbol(currentParamPair.Car))
               throw new ArgumentException($"Can't use {currentParamPair.Car.Princ()} as a parameter!");

            currentParam = currentParamPair.Cdr;
         }

         if (currentParam != Nil && !IsPermittedParamSymbol(currentParam))
            throw new ArgumentException($"Can't use {currentParam.Princ()} as a parameter!");

         return new Lambda(env, lambdaList, body);
      };

      //=================================================================================================================
      public static readonly CoreFun.FuncT Macro = (env, argsList, argsLength) =>
      {
         var lambdaList = ((Pair)argsList)[0];
         var body = ((Pair)argsList)[1];

         if (!(lambdaList.IsList || IsPermittedParamSymbol(lambdaList)))
            throw new ArgumentException($"Lambda list must be a list or a symbol, not {lambdaList.Princ()}!");

         if (lambdaList is Symbol symbol && (symbol.IsSpecial || symbol.IsKeyword || symbol == True))
            throw new ArgumentException($"Can't use {symbol.Princ()} as a parameter!");

         if (!body.IsProperList)
            throw new ArgumentException($"body must be a proper list, not {body.Princ()}");

         var currentParam = lambdaList;

         while (currentParam is Pair currentParamPair)
         {
            if (!IsPermittedParamSymbol(currentParamPair.Car))
               throw new ArgumentException($"Can't use {currentParamPair.Car.Princ()} as a parameter!");

            currentParam = currentParamPair.Cdr;
         }

         if (currentParam != Nil && !IsPermittedParamSymbol(currentParam))
            throw new ArgumentException($"Can't use {currentParam.Princ()} as a parameter!");

         return new Macro(env, lambdaList, body);
      };

      //================================================================================================================
      public static readonly CoreFun.FuncT Cond = (env, argsList, argsLength) =>
      {
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
                  return Progn(env, caseActions, caseActions.Length);
            }

            current = currentCasePair.Cdr;
         }

         return Nil; // No matching case found
      };

      //===================================================================================================================
      public static readonly CoreFun.FuncT Let = (env, argsList, argsLength) =>
         LetInternal(env, argsList, argsLength, false);

      //===================================================================================================================
      public static readonly CoreFun.FuncT LetStar = (env, argsList, argsLength) =>
         LetInternal(env, argsList, argsLength, true);

      //===================================================================================================================
      private static void ValidateLetArguments(LispObject arg0, LispObject body)
      {
         if (!(arg0 is Pair varlist))
            throw new ArgumentException($"varlist must be a list, not {arg0.Princ()}!");

         if (varlist.IsImproperList)
            throw new ArgumentException($"varlist must be a proper list, not {varlist.Princ()}!");

         LispObject current = varlist;

         while (!current.IsNil)
            if (current is Pair currentVarPair)
            {
               if (currentVarPair.Length != 2 || currentVarPair.IsImproperList)
                  throw new ArgumentException($"varlist items must be pairs with two elements, not {currentVarPair}");

               if (currentVarPair.Car is Symbol currentVarSym && !currentVarSym.IsLetBindable)
                  throw new ArgumentException($"let forms cannot bind {currentVarSym}!");
            }
            else if (current is Symbol currentVarSym)
            {
               if (!currentVarSym.IsLetBindable)
                  throw new ArgumentException($"let forms cannot bind {currentVarSym}!");
            }
            else
            {
               throw new ArgumentException($"varlist items must be symbols or pairs whose first element is a "
                                           + $"let-bindable symbol, not {current}");
            }

         if (body.IsImproperList)
            throw new ArgumentException($"body must be a proper list, not {body.Princ()}!");

         if (body.IsNil)
            throw new ArgumentException("body cannot be empty");
      }

      //================================================================================================================
      private static void BindVarlistInEnv(LispObject varlist, Env lookupEnv, Env bindEnv)
      {
         while (!varlist.IsNil)
         {
            var sym = Nil;
            var val = Nil;

            if (varlist is Pair varlistVarPair)
            {
               sym = varlistVarPair.Car;
               val = ((Pair)varlistVarPair.Cdr).Car.Eval(lookupEnv);
            }
            else if (varlist is Symbol varlistVarSym)
            {
               sym = varlistVarSym;
            }

            bindEnv.Set(Env.LookupMode.Local, sym, val);

            varlist = ((Pair)varlist).Cdr;
         }
      }

      //================================================================================================================
      public static readonly CoreFun.FuncT Letrec = (env, argsList, argsLength) =>
      {
         var arg0 = ((Pair)argsList).Car;
         var body = ((Pair)argsList).Cdr;
         ValidateLetArguments(arg0, body);
         var varlist = (Pair)arg0;
         var newEnv = env.Spawn(Nil, Nil);

         LispObject dummy = Intern(":dummy");

         foreach (var varlistElem in varlist)
            newEnv.Set(Env.LookupMode.Local, ((Pair)varlistElem).Car, dummy);

         BindVarlistInEnv(varlist, newEnv, newEnv);

         return Core.Progn(newEnv, body, body.Length);
      };

      //===================================================================================================================
      private static LispObject LetInternal(Env env, LispObject argsList, int argsLength, bool bindInNewEnv)
      {
         var arg0 = ((Pair)argsList).Car;
         var body = ((Pair)argsList).Cdr;
         ValidateLetArguments(arg0, body);
         var varlist = (Pair)arg0;
         var newEnv = env.Spawn(Nil, Nil);

         BindVarlistInEnv(varlist, bindInNewEnv ? newEnv : env, newEnv);

         return Core.Progn(newEnv, body, body.Length);
      }

      //================================================================================================================
   }
   //===================================================================================================================
}
