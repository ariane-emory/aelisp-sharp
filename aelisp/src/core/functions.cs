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
         ThrowUnlessIsProperList("argsList", argsList);

         var lambdaList = ((Pair)argsList)[0];
         var body = ((Pair)argsList).Cdr;

         // WriteLine($"body is: {body.ToPrincString()}");

         if (!(lambdaList.IsList || IsPermittedParamSymbol(lambdaList)))
            throw new ArgumentException($"lambda list must be a list or a symbol, not {lambdaList.ToPrincString()}!");

         if (lambdaList is Symbol symbol && (symbol.IsSpecial || symbol.IsKeyword || symbol == True))
            throw new ArgumentException($"can't use {symbol.ToPrincString()} as a parameter!");

         ThrowUnlessIsProperList("body", body);

         var currentParam = lambdaList;

         while (currentParam is Pair currentParamPair)
         {
            if (!IsPermittedParamSymbol(currentParamPair.Car))
               throw new ArgumentException($"can't use {currentParamPair.Car.ToPrincString()} as a parameter!");

            currentParam = currentParamPair.Cdr;
         }

         if (currentParam != Nil && !IsPermittedParamSymbol(currentParam))
            throw new ArgumentException($"can't use {currentParam.ToPrincString()} as a parameter!");

         return create(env, lambdaList, Ae.Cons(Intern("progn"), body));
      }

      //================================================================================================================
   }
   //===================================================================================================================
}
