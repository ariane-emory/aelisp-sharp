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
      public static readonly CoreFun.FuncT List = (env, argsList, argsLength) => argsList;

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
