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
      public static readonly CoreFun.FuncT Concat = (env, argsList) =>
      {
         var stringBuilder = new StringBuilder();
         LispObject current = argsList;

         while (current != Nil)
         {
            if (current is Pair currentPair)
            {
               var item = currentPair.Car;

               // Check if the item is not Nil and is either a String or can be represented as a String.
               if (item != Nil && (item is String str || !(item is Symbol)))
               {
                  stringBuilder.Append(item.Princ());
               }

               current = currentPair.Cdr;
            }
            else
            {
               // If the list is improper (i.e., the loop was exited not because we reached Nil,
               // but because the last Cdr was not a Pair), add the last item if it's not Nil.
               if (current != Nil && (current is String str || !(current is Symbol)))
               {
                  stringBuilder.Append(current.Princ());
               }
               break;
            }
         }

         return new String(stringBuilder.ToString());
      };

      //================================================================================================================
   }
   //===================================================================================================================
}
