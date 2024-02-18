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
      public static LispObject Car(LispObject obj)
      {
         if (obj == Nil)
            return Nil;

         if (obj is not Pair pair)
            return new Error("Argument must be a pair or nil!");

         return pair.Car;
      }

      //====================================================================================================================
      public static LispObject Cdr(LispObject obj)
      {
         if (obj == Nil)
            return Nil;

         if (obj is not Pair pair)
            return new Error("Argument must be a pair or nil!");

         return pair.Cdr;
      }

      //====================================================================================================================
      public static LispObject Cons(LispObject head, LispObject tail)
           => Ae.Cons(head, tail);
   }
   //====================================================================================================================
}
