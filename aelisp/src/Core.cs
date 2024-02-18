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
         LispObject arg1 = ((Pair)argsList).Car;

         if (! arg1.IsList)
            throw new ArgumentException("Argument must be a list!");

         if (arg1.IsNil)
            return Nil;

         return ((Pair)arg1).Car;
      };
      
      //=================================================================================================================
      public static CoreFunction.FuncT Cdr = (env, argsList, argsLength) =>
      {
         LispObject arg1 = ((Pair)argsList).Car;

         if (! arg1.IsList)
            throw new ArgumentException("Argument must be a list!");

         if (arg1.IsNil)
            return Nil;

         return ((Pair)arg1).Cdr;
      };
      
      //=================================================================================================================
      public static CoreFunction.FuncT Cons = (env, argsList, argsLength) =>
      {
         LispObject arg1 = ((Pair)argsList).Car;
         LispObject arg2 = ((Pair)((Pair)argsList).Cdr).Car;

         return Ae.Cons(arg1, arg2);
      };
      
   }
   //====================================================================================================================
}
