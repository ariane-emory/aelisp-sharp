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
      public static LispObject WriteOutput(Env env, LispObject argsList, Func<LispObject, string> fun)
      {
         ThrowUnlessIsProperList("argsList", argsList);

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
      // TODO: Needs quoting corrected!
      public static LispObject WriteObj(Env env, LispObject argsList) =>
         WriteOutput(env, argsList,
                     o => o switch
                     {
                        String s => $"\"{s.ToPrincString()}\"",
                        Char c => $"'{c.ToPrincString()}'",
                        _ => o.ToPrincString()
                     });

      //================================================================================================================
      public static LispObject PrincObj(Env env, LispObject argsList) =>
         WriteOutput(env, argsList, o => o.ToPrincString());

      //================================================================================================================
      public static LispObject PrintObj(Env env, LispObject argsList) =>
         WriteOutput(env, argsList, o => $"\n{o.ToPrintString()}");

      //================================================================================================================
      public static LispObject RawObj(Env env, LispObject argsList) =>
         WriteOutput(env, argsList, o => o.ToString());

      //================================================================================================================
      public static LispObject Newline(Env env, LispObject argList)
      {
         WriteLine("");

         return new Integer(1);
      }

      //================================================================================================================
   }
   //===================================================================================================================
}
