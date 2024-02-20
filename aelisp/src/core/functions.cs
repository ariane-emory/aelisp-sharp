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
      public static LispObject NewLambda(Env env, LispObject argsList)
          => NewFunction<Lambda>(env, argsList, (e, p, b) => new Lambda(e, p, b));

      //=================================================================================================================
      public static LispObject NewMacro(Env env, LispObject argsList)
         => NewFunction<Macro>(env, argsList, (e, p, b) => new Macro(e, p, b));

      //=================================================================================================================
      private static LispObject NewFunction<T>(Env env, LispObject argsList, Func<Env, LispObject, LispObject, LispObject> create)
      {
         if (argsList.IsImproperList)
            throw new ArgumentException($"argsList must be a proper list, not {argsList}!");

         var lambdaList = ((Pair)argsList)[0];
         var body = ((Pair)argsList)[1];

         if (!(lambdaList.IsList || IsPermittedParamSymbol(lambdaList)))
            throw new ArgumentException($"Lambda list must be a list or a symbol, not {lambdaList.PrincString()}!");

         if (lambdaList is Symbol symbol && (symbol.IsSpecial || symbol.IsKeyword || symbol == True))
            throw new ArgumentException($"Can't use {symbol.PrincString()} as a parameter!");

         if (!body.IsProperList)
            throw new ArgumentException($"body must be a proper list, not {body.PrincString()}");

         var currentParam = lambdaList;

         while (currentParam is Pair currentParamPair)
         {
            if (!IsPermittedParamSymbol(currentParamPair.Car))
               throw new ArgumentException($"Can't use {currentParamPair.Car.PrincString()} as a parameter!");

            currentParam = currentParamPair.Cdr;
         }

         if (currentParam != Nil && !IsPermittedParamSymbol(currentParam))
            throw new ArgumentException($"Can't use {currentParam.PrincString()} as a parameter!");

         return create(env, lambdaList, body);
      }

      //================================================================================================================
   }
   //===================================================================================================================
}
