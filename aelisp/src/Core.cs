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

         if (argsList.IsImproperList)
            throw new ArgumentException("prog body must be a proper list");

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
      public static readonly CoreFun.FuncT Repeat = (env, argsList, argsLength) =>
      {
         var first_arg = ((Pair)argsList).Car.Eval(env);
         var body = ((Pair)argsList).Cdr;
         var bodyLength = body.Length;
         var result = Nil;

         if (body.IsImproperList)
            throw new ArgumentException("body must be a proper list");

         if (!((first_arg is Integer times) && (times.Value >= 0)))
            throw new ArgumentException($"repeat requires a positive integer as its first argument, not {first_arg}!");

         for (int ix = 0; ix < times.Value; ix++)
            result = Progn(env, body, bodyLength);

         return result;
      };

      //=================================================================================================================
      public static readonly CoreFun.FuncT Until = (env, argsList, argsLength) =>
      {
         var while_cond = ((Pair)argsList).Car;
         var do_branch = ((Pair)argsList).Cdr;
         var result = Nil;

         if (do_branch.IsImproperList)
            throw new ArgumentException("body must be a proper list");

         while (while_cond.Eval(env).IsNil)
            result = Progn(env, do_branch, do_branch.Length);

         return result;
      };

      //=================================================================================================================
      public static readonly CoreFun.FuncT While = (env, argsList, argsLength) =>
      {
         var while_cond = ((Pair)argsList).Car;
         var do_branch = ((Pair)argsList).Cdr;
         var result = Nil;

         if (do_branch.IsImproperList)
            throw new ArgumentException("body must be a proper list");

         while (!while_cond.Eval(env).IsNil)
            result = Progn(env, do_branch, do_branch.Length);

         return result;
      };

      //=================================================================================================================
      public static readonly CoreFun.FuncT Unless = (env, argsList, argsLength) =>
      {
         var if_cond = ((Pair)argsList).Car;
         var then_branch = ((Pair)argsList).Cdr;

         if (then_branch.IsImproperList)
            throw new ArgumentException("body must be a proper list");

         return if_cond.Eval(env).IsNil
            ? Progn(env, then_branch, then_branch.Length)
            : Nil;
      };

      //=================================================================================================================
      public static readonly CoreFun.FuncT When = (env, argsList, argsLength) =>
      {
         var if_cond = ((Pair)argsList).Car;
         var then_branch = ((Pair)argsList).Cdr;

         if (then_branch.IsImproperList)
            throw new ArgumentException("body must be a proper list");

         return !if_cond.Eval(env).IsNil
            ? Progn(env, then_branch, then_branch.Length)
            : Nil;
      };

      //=================================================================================================================
      public static readonly CoreFun.FuncT If = (env, argsList, argsLength) =>
      {
         var if_cond = ((Pair)argsList).Car;
         var then_branch = ((Pair)((Pair)argsList).Cdr).Car;
         var else_branch = ((Pair)((Pair)argsList).Cdr).Cdr;

         if (else_branch.IsImproperList)
            throw new ArgumentException("else body must be a proper list");

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
         var arg0 = ((Pair)argsList)[0];
         var arg1 = ((Pair)argsList)[1];

         if (arg0 is not Pair pair)
            throw new ArgumentException("First argument must be a cons cell!");

         pair.Cdr = arg1;

         return arg1;
      };

      //=================================================================================================================
      public static readonly CoreFun.FuncT Rplaca = (env, argsList, argsLength) =>
      {
         var arg0 = ((Pair)argsList)[0];
         var arg1 = ((Pair)argsList)[1];

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
         var arg0 = ((Pair)argsList)[0];

         if (!arg0.IsList)
            throw new ArgumentException("Argument must be a list!");

         if (arg0.IsNil)
            return Nil;

         return ((Pair)arg0).Car;
      };

      //=================================================================================================================
      public static readonly CoreFun.FuncT Cdr = (env, argsList, argsLength) =>
      {
         var arg0 = ((Pair)argsList)[0];

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
         var arg0 = ((Pair)argsList)[0];
         var arg1 = ((Pair)argsList)[1];

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

         return new Lambda(lambdaList, body, env);
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

         return new Macro(lambdaList, body, env);
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

      // //===================================================================================================================
      // public static readonly CoreFun.FuncT LetRev = (env, argsList, argsLength) =>
      // {
      //    static ae_obj_t* dummy = NULL;

      //    if (!dummy)
      //       dummy = KW("DUMMY");

      //    ae_obj_t * const varlist = CAR(args);

      //    REQUIRE(PROPERP(varlist), "varlist must be a proper list");
      //    // REQUIRE(LENGTH(varlist) > 0, "empty varlist");

      //    FOR_EACH(varlist_item, varlist) {
      //       REQUIRE(SYMBOLP(varlist_item) || (CONSP(varlist_item) && LENGTH(varlist_item) == 2),
      //               "varlist items must be symbols or lists with two elements");

      //       ae_obj_t * const sym = SYMBOLP(varlist_item) ? varlist_item : CAR(varlist_item);

      //       REQUIRE((!SPECIAL_SYMP(sym)), "let forms cannot bind special symbols");

      //    }

      //    ae_obj_t * const body    = CDR(args);

      //    REQUIRE(PROPERP(body), "body must be a proper list");

      //    ae_obj_t * const new_env = NEW_ENV(env, NIL, NIL);

      //    FOR_EACH(varlist_item, varlist)


      //     ENV_SET_L(new_env, CAR(varlist_item), dummy);  // DUMMY_VALUE can be anything like NIL or a specific uninitialized marker.

      //    FOR_EACH(varlist_item, varlist) {
      //       if (log_core)
      //          LOG(varlist_item, "letrec varlist item");

      //       INDENT;

      //       ae_obj_t* val = SYMBOLP(varlist_item)
      //         ? NIL
      //         : RETURN_IF_ERRORP(EVAL(new_env, CADR(varlist_item)));

      //       // let only sets the last-bound-to property if it's not already set.
      //       if ((LAMBDAP(val) || MACROP(val)) && !HAS_PROP("last-bound-to", CAR(varlist_item)))
      //          PUT_PROP(CAR(varlist_item), "last-bound-to", val);

      //       if (log_core)
      //       {
      //          if (SYMBOLP(varlist_item))
      //             LOG(varlist_item, "binding symbol");
      //          else
      //             LOG(CAR(varlist_item), "binding symbol");

      //          LOG(val, "to value");
      //       }

      //       ENV_SET_L(new_env, SYMBOLP(varlist_item) ? varlist_item : CAR(varlist_item), val);

      //       OUTDENT;
      //    }

      //    if (log_core)
      //    {
      //       LOG(ENV_SYMS(new_env), "new_env syms");
      //       LOG(ENV_VALS(new_env), "new_env vals");
      //    }

      //    RETURN(ae_core_progn(new_env, body, LENGTH(body)));

      //    END_DEF_CORE_FUN;
      // };

      private static void ValidateLetArguments(LispObject arg0, LispObject body)
      {
         if (!(arg0 is Pair varlist))
            throw new ArgumentException($"varlist must be a list, not {arg0.Princ()}!");

         if (varlist.IsImproperList)
            throw new ArgumentException($"varlist must be a proper list, not {varlist.Princ()}!");

         LispObject current = varlist;

         while (!current.IsNil)
         {
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
               throw new ArgumentException($"varlist items must be symbols or pairs whose first element is a let bindable symbol, not {current}");
            }
         }

         if (body.IsImproperList)
            throw new ArgumentException($"body must be a proper list, not {body.Princ()}!");

         if (body.IsNil)
            throw new ArgumentException("body cannot be empty");
      }

      //===================================================================================================================
      private static LispObject LetInternal(Env env, LispObject argsList, int argsLength, bool bindInNewEnv)
      {
         var arg0 = ((Pair)argsList).Car;
         var body = ((Pair)argsList).Cdr;

         ValidateLetArguments(arg0, body);

         var varlist = (Pair)arg0;
         LispObject current = varlist;
         var newEnv = new Env(env, Nil, Nil);

         current = varlist;

         while (!current.IsNil)
         {
            var sym = Nil;
            var val = Nil;

            if (current is Pair currentVarPair)
            {
               sym = currentVarPair.Car;
               val = ((Pair)currentVarPair.Cdr).Car.Eval(bindInNewEnv ? newEnv : env);
            }
            else if (current is Symbol currentVarSym)
            {
               sym = currentVarSym;
            }

            newEnv.Set(Env.LookupMode.Local, sym, val);
         }

         return Core.Progn(newEnv, body, body.Length);
      }

      //================================================================================================================
      // private static List<LispObject> ToList(this Pair obj)
      // {
      //    var list = new List<LispObject>();
      //    LispObject current = obj;

      //    while (current is Pair pair)
      //    {
      //       list.Add(pair.Car);
      //       current = pair.Cdr;
      //    }

      //    if (current != Nil)
      //       list.Add(current);

      //    return list;
      // }

      //================================================================================================================
   }
   //===================================================================================================================
}
