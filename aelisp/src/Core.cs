using static Ae;
using LispObjectParser = Pidgin.Parser<Ae.LispToken, Ae.LispObject>;
using LispTokenParser = Pidgin.Parser<Ae.LispToken, Ae.LispToken>;

//======================================================================================================================
static partial class Ae
{
   //====================================================================================================================
   public static class Core
   {
      // public class CoreFunction : Function
      // {
      //    //===========================================================================================================
      //    // Core function delegate
      //    //===========================================================================================================
      //    public delegate LispObject CoreFunc(Env env, LispObject argsList, int argsLength); // ???

      //=================================================================================================================
      public static CoreFunction.FuncT Car = (env, argsList, argsLength) =>
      {
         LispObject arg0 = ((Pair)argsList)[0];

         if (!arg0.IsList)
            throw new ArgumentException("Argument must be a list!");

         if (arg0.IsNil)
            return Nil;

         return ((Pair)arg0).Car;
      };

      //=================================================================================================================
      public static CoreFunction.FuncT Cdr = (env, argsList, argsLength) =>
      {
         LispObject arg0 = ((Pair)argsList)[0];

         if (!arg0.IsList)
            throw new ArgumentException("Argument must be a list!");

         if (arg0.IsNil)
            return Nil;

         return ((Pair)arg0).Cdr;
      };

      //=================================================================================================================
      public static CoreFunction.FuncT Cons = (env, argsList, argsLength) =>
      {
         LispObject arg0 = ((Pair)argsList)[0];
         LispObject arg1 = ((Pair)argsList)[1];

         return Ae.Cons(arg0, arg1);
      };

      //=================================================================================================================
      public static CoreFunction.FuncT EqP = (env, argsList, argsLength)
         => ((Pair)argsList)[0] == ((Pair)argsList)[1] ? True : Nil;

   }
   //====================================================================================================================
}
