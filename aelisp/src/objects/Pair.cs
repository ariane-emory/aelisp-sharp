using System.Collections;
using System.Reflection;
using System.Text;

//======================================================================================================================
static partial class Ae
{
   //====================================================================================================================
   // Pair class
   //====================================================================================================================
   public class Pair : LispObject, IEnumerable<LispObject>
   {
      //==================================================================================================================
      // Public properties
      //==================================================================================================================
      public LispObject Car { get; set; }
      public LispObject Cdr { get; set; }

      //==================================================================================================================
      // Constructor
      //==================================================================================================================
      public Pair(LispObject car, LispObject cdr)
      {
         Car = car;
         Cdr = cdr;
      }
      
      //================================================================================================================
      // Protected instance properties
      //================================================================================================================
      protected override string? StringRepresentation => $"{Car}, {Cdr}";

      //==================================================================================================================
      // Public instance methods
      //==================================================================================================================
      public override string SPrinc()
      {
         var sb = new StringBuilder();

         sb.Append("(");

         LispObject current = this;

         while (current is Pair currentCons)
         {
            sb.Append(currentCons.Car.SPrinc());

            if (currentCons.Cdr is Pair)
            {
               sb.Append(" ");
               current = currentCons.Cdr;
            }
            else if (currentCons.Cdr != Nil)
            {

               sb.Append(" . ");
               sb.Append(currentCons.Cdr.SPrinc());

               break;
            }
            else
            {
               break;
            }
         }

         sb.Append(")");

         return sb.ToString();
      }

      //==================================================================================================================
      public IEnumerator<LispObject> GetEnumerator()
      {
         LispObject current = this;

         while (current != Nil)
            if (current is Pair pair)
            {
               yield return pair.Car;

               current = pair.Cdr;
            }
            else
            {
               // yield return current;
               yield break;

               // throw new InvalidOperationException("Enumerated improper list!");
            }
      }

      //==================================================================================================================
      IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

      //==================================================================================================================
      public int Length
      {
         get
         {
            int length = 0;
            LispObject current = this;

            while (current is Pair pair)
            {
               length++;
               current = pair.Cdr;

               if (!(current is Pair) && current != Nil)
                  break;
            }

            return length;
         }
      }
   }

   //====================================================================================================================
}
