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

      //==================================================================================================================
      // Instance methods
      //==================================================================================================================
      public override string ToString() => $"{TypeName}({Car}, {Cdr})";

      //==================================================================================================================
      public override string Write()
      {
         var sb = new StringBuilder();

         sb.Append("(");

         LispObject current = this;

         while (current is Pair currentCons)
         {
            sb.Append(currentCons.Car.Write());

            if (currentCons.Cdr is Pair)
            {
               sb.Append(" ");
               current = currentCons.Cdr;
            }
            else if (currentCons.Cdr != Nil)
            {

               sb.Append(" . ");
               sb.Append(currentCons.Cdr.Write());

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
      public int Length()
      {
         int length = 0;
         LispObject current = this;

         while (current is Pair pair)
         {
            length += 1; // Count the current element
            current = pair.Cdr; // Move to the next element

            // If we encounter a non-Pair object in the Cdr (indicating an improper list),
            // we stop counting further, treating it as the end of the list for length purposes.
            if (!(current is Pair) && current != Nil)
            {
               break;
            }
         }

         return length;
      }
   }

   //====================================================================================================================
}
