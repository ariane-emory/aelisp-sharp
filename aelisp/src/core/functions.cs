using static System.Console;
using static Ae;

//======================================================================================================================
static partial class Ae
{
   //====================================================================================================================
   public static partial class Core
   {
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
   }
   //===================================================================================================================
}
