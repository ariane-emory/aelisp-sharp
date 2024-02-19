using System.Text;
using static System.Console;
using static Ae;

//======================================================================================================================
static partial class Ae
{
   //===================================================================================================================
   public static partial class Core
   {
      //================================================================================================================
      public static LispObject OutputFun(Env env, LispObject argsList, Func<LispObject, string> fun)
      {
         if (argsList.IsImproperList)
            throw new ArgumentException($"argsList must be a proper list, not {argsList}!");

         int written = 0;
         LispObject current = argsList;

         while (current is Pair pair)
         {
            var elem = pair.Car;
            var str = fun(elem);
            
            Write(str);
            written += str.Length;

            current = pair.Cdr;
         }

         return new Integer(written);
      }

      //================================================================================================================
      public static LispObject WriteObj(Env env, LispObject argsList)
      {
         return OutputFun(env, argsList, o => o.Princ());
      }

      //================================================================================================================
      public static LispObject PutObj(Env env, LispObject argsList)
      {
         if (argsList.IsImproperList)
            throw new ArgumentException($"argsList must be a proper list, not {argsList}!");

         int written = 0;
         LispObject current = argsList;

         while (current is Pair pair)
         {
            var elem = pair.Car;
            var str = elem.ToString();

            Write(str);
            written += str.Length;

            current = pair.Cdr;
         }

         return new Integer(written);
      }

      //================================================================================================================
      public static LispObject Newline(Env env, LispObject argList)
      {
         WriteLine("");

         return Nil;
      }

      //================================================================================================================
   }
   //===================================================================================================================
}
