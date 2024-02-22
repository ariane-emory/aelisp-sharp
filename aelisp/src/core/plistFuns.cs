using static System.Console;
using static Ae;

//======================================================================================================================
static partial class Ae
{
   //====================================================================================================================
   public static partial class Core
   {
      //=================================================================================================================
      // private static CoreFun.FuncT TernaryFun(Func<LispObject, LispObject, LispObject, LispObject> func)
      //    => (Env env, LispObject argsList) =>
      //     {
      //        ThrowUnlessIsProperList("argsList", argsList);

      //        var arg1 = ((Pair)argsList)[0];
      //        var arg2 = ((Pair)argsList)[1];
      //        var arg3 = ((Pair)argsList)[2];

      //        return func(arg1, arg2, arg3);
      //     };

      //================================================================================================================
      public static readonly CoreFun.FuncT CorePlistContains =
         BinaryFun((plist, key) => Truthiness(PlistContains(plist, key)));

      //================================================================================================================
      public static readonly CoreFun.FuncT CorePlistGet =
         BinaryFun((plist, key) => PlistGet(plist, key));

      //================================================================================================================
      public static readonly CoreFun.FuncT CorePlistRemove =
         BinaryFun((plist, key) => PurePlistRemove(plist, key));

      //================================================================================================================
      public static readonly CoreFun.FuncT CorePlistRemoveB =
         BinaryFun((plist, key) => MutatingPlistRemove(plist, key));

      //================================================================================================================
      public static readonly CoreFun.FuncT CorePlistSet =
         TernaryFun((plist, key, value) => PurePlistSet(plist, key, value));

      //================================================================================================================
      public static readonly CoreFun.FuncT CorePlistSetB =
         TernaryFun((plist, key, value) => MutatingPlistSet(plist, key, value));

      //================================================================================================================
   }
   //===================================================================================================================
}
