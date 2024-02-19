using static System.Console;
using static Ae;

//======================================================================================================================
static partial class Ae
{
   //====================================================================================================================
   public static partial class Core
   {
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

      //================================================================================================================
   }
   //===================================================================================================================
}
