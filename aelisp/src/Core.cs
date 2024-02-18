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
      // public class CoreFun : Function
      // {
      //    //===========================================================================================================
      //    // Core function delegate
      //    //===========================================================================================================
      //    public delegate LispObject CoreFunc(Env env, LispObject argsList, int argsLength); // ???


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
      public static readonly CoreFun.FuncT Unless = (env, argsList, argsLength) =>
    {
       LispObject if_cond = ((Pair)argsList).Car;
       LispObject then_branch = ((Pair)argsList).Cdr;

       return if_cond.Eval(env).IsNil
          ? then_branch.Eval(env)
          : Nil;
    };

      //=================================================================================================================
      public static readonly CoreFun.FuncT When = (env, argsList, argsLength) =>
      {
         LispObject if_cond = ((Pair)argsList).Car;
         LispObject then_branch = ((Pair)argsList).Cdr;

         return !if_cond.Eval(env).IsNil
            ? then_branch.Eval(env)
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
           : else_branch.Eval(env);
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
      public static readonly CoreFun.FuncT Progn = (env, argsList, argsLength) =>
      {
         var result = Nil;

         if (argsList is Pair argsListPair)
            foreach (var elem in argsListPair)
               result = elem.Eval(env);

         return result;
      };

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
   }
   //===================================================================================================================
}
