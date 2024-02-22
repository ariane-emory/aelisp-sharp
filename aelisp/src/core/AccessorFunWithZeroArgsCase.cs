using static System.Console;
using static Ae;

//======================================================================================================================
static partial class Ae
{
   //====================================================================================================================
   public static partial class Core
   {
      //=================================================================================================================
      private static CoreFun.FuncT
         AccessorFunWithZeroArgsCase<ThisLispObjT, FieldT>(Func<ThisLispObjT, FieldT> getField,
                                                           Func<FieldT, LispObject> construct,
                                                           Func<LispObject> thunk) =>
         (Env env, LispObject argsList) =>
          (argsList.Length == 0)
               ? thunk()
               : AccessorFun(getField, construct)(env, argsList);


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

      //================================================================================================================
   }
   //===================================================================================================================
}
