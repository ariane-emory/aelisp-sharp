using static System.Console;
using static Ae;
using LispObjectParser = Pidgin.Parser<Ae.LispToken, Ae.LispObject>;
using LispTokenParser = Pidgin.Parser<Ae.LispToken, Ae.LispToken>;

//======================================================================================================================
static partial class Ae
{
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

   //====================================================================================================================
   public static class Core
   {
      //=================================================================================================================
      private static CoreFun.FuncT ShortCircuitSpecialFun(Func<LispObject, bool> pred) =>
         (env, argsList, argsLength) =>
         {
            if (argsList.IsImproperList)
               throw new ArgumentException("prog body must be a proper list");

            var result = Nil;

            while (argsList is Pair argsListPair)
            {
               result = argsListPair.Car.Eval(env);

               if (pred(result))
                  return result;

               argsList = argsListPair.Cdr;
            }

            return result;
         };

      //=================================================================================================================
      public static readonly CoreFun.FuncT And = ShortCircuitSpecialFun(o => o.IsNil);

      //=================================================================================================================
      public static readonly CoreFun.FuncT Or = ShortCircuitSpecialFun(o => !o.IsNil);

      //=================================================================================================================
      public static readonly CoreFun.FuncT Progn = ShortCircuitSpecialFun(o => false);

      // //=================================================================================================================
      // public static readonly CoreFun.FuncT Progn = (env, argsList, argsLength) =>
      // {
      //    if (argsList.IsImproperList)
      //       throw new ArgumentException("prog body must be a proper list");

      //    var result = Nil;

      //    while (argsList is Pair argsListPair)
      //    {
      //       result = argsListPair.Car.Eval(env);
      //       argsList = argsListPair.Cdr;
      //    }

      //    return result;
      // };

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
      private static CoreFun.FuncT WhileOrUntilSpecialFun(Func<LispObject, bool> pred) =>
         (Env env, LispObject argsList, int argsLength) =>
         {
            var test = ((Pair)argsList).Car;
            var body = ((Pair)argsList).Cdr;
            var result = Nil;

            if (body.IsImproperList)
               throw new ArgumentException("body must be a proper list");

            while (pred(test.Eval(env)))
               result = Progn(env, body, body.Length);

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
         (Env env, LispObject argsList, int argsLength) =>
      {
         var test = ((Pair)argsList).Car;
         var body = ((Pair)argsList).Cdr;

         if (body.IsImproperList)
            throw new ArgumentException("body must be a proper list");

         return pred(test.Eval(env))
            ? Progn(env, body, body.Length)
            : Nil;
      };

      //=================================================================================================================
      public static readonly CoreFun.FuncT When =
         WhenOrUnlessSpecialFun(o => !o.IsNil);

      //=================================================================================================================
      public static readonly CoreFun.FuncT Unless =
         WhenOrUnlessSpecialFun(o => o.IsNil);

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
      private static CoreFun.FuncT NumericEqualityPredicateFun(int val) =>
         (Env env, LispObject argsList, int argsLength) =>
         {
            var arg1 = ((Pair)argsList)[0];

            if (arg1 is Integer integer)
               return Truthiness(val == integer.Value);

            if (arg1 is Float floatObj)
               return Truthiness(val == floatObj.Value);

            if (arg1 is Rational rational)
               return Truthiness(val == rational.Numerator);

            throw new ArgumentException($"Argument must be a symbol, not {arg1}!");
         };

      //=================================================================================================================
      public static readonly CoreFun.FuncT ZeroP = NumericEqualityPredicateFun(0);

      //=================================================================================================================
      public static readonly CoreFun.FuncT OneP = NumericEqualityPredicateFun(1);

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
      public static readonly CoreFun.FuncT Plus1 =
         PureUnaryFun(num =>
            num switch
            {
               Integer integer => (LispObject)new Integer(integer.Value + 1),
               Float floatObj => (LispObject)new Float(floatObj.Value + 1),
               Rational rational => (LispObject)new Rational(rational.Numerator + rational.Denominator, rational.Denominator),
               _ => throw new ArgumentException($"Argument must be a number, not {num}!")
            });


      //=================================================================================================================
      public static readonly CoreFun.FuncT Eval = (env, argsList, argsLength) =>
       ((Pair)argsList)[0].Eval(env);

      // //=================================================================================================================
      // public static readonly CoreFun.FuncT Cons = (env, argsList, argsLength) =>
      // {
      //    var arg0 = ((Pair)argsList)[0];
      //    var arg1 = ((Pair)argsList)[1];

      //    if (Core.ConsDebugWrite)
      //       WriteLine($"Core Cons({arg0.Princ()}, {arg1.Princ()})");

      //    return Ae.Cons(arg0, arg1);
      // };

      //=================================================================================================================
      private static CoreFun.FuncT PureBinaryFun(Func<LispObject, LispObject, LispObject> func)
         => (Env env, LispObject argsList, int argsLength) =>
          {
             if (argsList.IsImproperList)
                throw new ArgumentException("argsList must be a proper list");

             var arg1 = ((Pair)argsList)[0];
             var arg2 = ((Pair)argsList)[1];

             return func(arg1, arg2);
          };

      //=================================================================================================================
      private static CoreFun.FuncT PureBinaryFun<T1, T2>(Func<T1, T2, LispObject> func)
          where T1 : LispObject
          where T2 : LispObject
          => (Env env, LispObject argsList, int argsLength) =>
          {
             if (argsList.IsImproperList)
                throw new ArgumentException("argsList must be a proper list");

             var arg1 = ((Pair)argsList)[0];
             var arg2 = ((Pair)argsList)[1];

             if (arg1 is T1 typedArg1 && arg2 is T2 typedArg2)
                return func(typedArg1, typedArg2);

             throw new ArgumentException($"Arguments must be of types {typeof(T1).Name} and {typeof(T2).Name}");
          };

      //=================================================================================================================
      public static readonly CoreFun.FuncT Cons =
         PureBinaryFun((arg1, arg2) => Ae.Cons(arg1, arg2));

      //=================================================================================================================
      public static readonly CoreFun.FuncT NewRational =
         PureBinaryFun<Integer, Integer>((num, den) => new Rational(num.Value, den.Value));

      //=================================================================================================================
      private static CoreFun.FuncT PureUnaryFun(Func<LispObject, LispObject> func)
         => (Env env, LispObject argsList, int argsLength) =>
         {
            if (argsList.IsImproperList)
               throw new ArgumentException("argsList must be a proper list");

            return func(((Pair)argsList)[0]);
         };

      //=================================================================================================================
      private static CoreFun.FuncT PureUnaryFun<T1>(Func<T1, LispObject> func)
       where T1 : LispObject
       => (Env env, LispObject argsList, int argsLength) =>
       {
          if (argsList.IsImproperList)
             throw new ArgumentException("argsList must be a proper list");

          var arg1 = ((Pair)argsList)[0];

          if (arg1 is T1 typedArg1)
             return func(typedArg1);

          throw new ArgumentException($"Argument must be of type {typeof(T1).Name}");
       };

      //=================================================================================================================
      public static readonly CoreFun.FuncT Id = PureUnaryFun(o => o);

      //=================================================================================================================
      public static readonly CoreFun.FuncT Quote =
         PureUnaryFun(o => o);

      //=================================================================================================================
      public static readonly CoreFun.FuncT Length =
         PureUnaryFun(o => new Integer(o.Length));

      //=================================================================================================================
      public static readonly CoreFun.FuncT ObjToString =
         PureUnaryFun(o => new String(o.Princ()));

      //=================================================================================================================
      private static CoreFun.FuncT AccessorFunWithZeroArgsCase<ThisLispObjT, FieldT>(Func<ThisLispObjT, FieldT> getField,
                                                                                     Func<FieldT, LispObject> construct,
                                                                                     Func<LispObject> thunk) =>
         (Env env, LispObject argsList, int argsLength) =>
          (argsList.Length == 0)
               ? thunk()
               : AccessorFun(getField, construct)(env, argsList, argsLength);

      //=================================================================================================================
      private static CoreFun.FuncT AccessorFun<ThisLispObjT, FieldT>(Func<ThisLispObjT, FieldT> getField, Func<FieldT, LispObject> construct)
       => PureUnaryFun(o =>
       {
          if (o is ThisLispObjT typed)
             return construct(getField(typed));

          throw new ArgumentException($"Argument must be a string, not {o}!");
       });

      //=================================================================================================================
      public static readonly CoreFun.FuncT UserFunctionBody =
         AccessorFun<UserFunction, LispObject>(fun => fun.Body, n => n);

      //=================================================================================================================
      public static readonly CoreFun.FuncT UserFunctionParams =
         AccessorFun<UserFunction, LispObject>(fun => fun.Parameters, n => n);

      //=================================================================================================================
      public static readonly CoreFun.FuncT EnvOrFunEnv =
         AccessorFunWithZeroArgsCase<UserFunction, LispObject>(fun => fun.Environment,
                                                               n => n,
                                                               () => Root);

      //=================================================================================================================
      public static readonly CoreFun.FuncT EnvSymbols =
         AccessorFunWithZeroArgsCase<Env, LispObject>(env => env.Symbols,
                                                      n => n,
                                                      () => Root.Symbols);

      //=================================================================================================================
      public static readonly CoreFun.FuncT EnvValues =
         AccessorFunWithZeroArgsCase<Env, LispObject>(env => env.Values,
                                                      n => n,
                                                      () => Root.Values);

      //=================================================================================================================
      public static readonly CoreFun.FuncT Numerator =
         AccessorFun<Rational, int>(rat => rat.Numerator, n => new Integer(n));

      //=================================================================================================================
      public static readonly CoreFun.FuncT Denominator =
         AccessorFun<Rational, int>(rat => rat.Denominator, n => new Integer(n));

      //=================================================================================================================
      public static readonly CoreFun.FuncT ErrorMessage =
         AccessorFun<Error, string>(err => err.Value, s => new Error(s));

      //=================================================================================================================
      public static readonly CoreFun.FuncT SymbolName =
         AccessorFun<Symbol, string>(sym => sym.Value, s => new String(s));

      //=================================================================================================================
      public static readonly CoreFun.FuncT InternString =
         PureUnaryFun(o =>
         {
            if (o is String str)
               return Intern(str.Value);

            throw new ArgumentException($"Argument must be a string, not {o}!");
         });

      //===================================================================================================================
      public static readonly CoreFun.FuncT Type =
         PureUnaryFun(o => Intern(o is Pair
                                  ? ":cons"
                                  : ":" + o.TypeName.ToLower()));

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
      public static readonly CoreFun.FuncT Not =
         UnaryPredicateFun(o => o.IsNil);

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
      private static CoreFun.FuncT EqualityPredicateFun(Func<LispObject, LispObject, bool> pred) =>
         (env, argsList, argsLength) =>
         {
            if (argsList.IsImproperList)
               throw new ArgumentException("argsList must be a proper list");

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
