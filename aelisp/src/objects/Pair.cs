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
      public override LispObject Eval(Env env)
      {
            throw new NotImplementedException("Implement this!");
      }

      //================================================================================================================
      public override string Princ()
      {
         var sb = new StringBuilder();

         sb.Append("(");

         LispObject current = this;

         while (current is Pair currentCons)
         {
            sb.Append(currentCons.Car.Print());

            if (currentCons.Cdr is Pair)
            {
               sb.Append(" ");
               current = currentCons.Cdr;
            }
            else if (currentCons.Cdr != Nil)
            {

               sb.Append(" . ");
               sb.Append(currentCons.Cdr.Princ());

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
      public int PairListLength
      {
         get
         {
            // NOTE: Remember, the Listlength of both (1 2 3) and (1 2 3 . 4 ) is 3 in this language at present.

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
