using static System.Console;
using System.Collections;
using System.Reflection;
using System.Text;

//======================================================================================================================
static partial class Ae
{
   //===================================================================================================================
   // Pair class
   //===================================================================================================================
   public class Pair : LispObject, IEnumerable<LispObject>
   {
      //================================================================================================================
      // Properties
      //================================================================================================================
      public override string TypeName => "Cons";
      public LispObject Car { get; set; }
      public LispObject Cdr { get; set; }
      public bool IsQuoteForm  => Car == Intern("quote");
      
      //================================================================================================================
      protected override string? StringRepresentation => $"#{Car.Id}, #{Cdr.Id}, {Length}";

      //================================================================================================================
      public override int Length
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

      //================================================================================================================
      // Constructor
      //================================================================================================================
      public Pair(LispObject car, LispObject cdr)
      {
         Car = car;
         Cdr = cdr;
      }

      //================================================================================================================
      // Methods
      //================================================================================================================
      public override string ToPrincString()
      {
         if (IsQuoteForm)
            return $"'{((Pair)Cdr).Car.ToPrincString()}";

         var sb = new StringBuilder();

         sb.Append("(");

         LispObject current = this;

         while (current is Pair currentPair)
         {
            sb.Append(currentPair.Car.ToPrintString());

            if (currentPair.Cdr is Pair)
            {
               sb.Append(" ");
               current = currentPair.Cdr;
            }
            else if (currentPair.Cdr != Nil)
            {
               sb.Append(" . ");
               sb.Append(currentPair.Cdr.ToPrintString());

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

      //================================================================================================================
      IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

      //================================================================================================================
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

      //================================================================================================================
      public LispObject ElementAt(int index) => this[index];

      //================================================================================================================
      public LispObject this[int index]
      {
         get
         {
            int currentIndex = 0;
            LispObject current = this;

            while (current is Pair pair)
            {
               if (currentIndex == index)
                  return pair.Car;

               current = pair.Cdr;
               currentIndex++;
            }

            return currentIndex == index ? current : Nil;
         }
         set
         {
            int currentIndex = 0;
            LispObject current = this;
            LispObject previous = Nil;

            while (current is Pair pair)
            {
               if (currentIndex == index)
               {
                  pair.Car = value;
                  return;
               }

               previous = current;
               current = pair.Cdr;
               currentIndex++;
            }

            if (currentIndex == index && previous is Pair prevPair)
            {
               prevPair.Cdr = value;

               return;
            }
            else if (currentIndex <= index)
            {
               throw new IndexOutOfRangeException("index is out of range.");
            }
         }
      }

      //================================================================================================================
      public override LispObject Eval(Env env)
      {
         LispObject head = Car;
         LispObject args = Cdr;

         // WriteLine($"\nhead: {head.ToPrincString()}");
         // WriteLine($"args: {args.ToPrincString()}");

         if (!args.IsList)
            throw new ArgumentException($"{nameof(args)} is {args}, not a list, something has gone very wrong!");

         LispObject fun = head.Eval(env);

         LispObject result = fun switch
            {
               CoreFun coreFun => coreFun.Apply(env, args),
               Lambda lambda => lambda.Apply(env, args),
               Macro macro => macro.Apply(env, args),
               _ => throw new ApplicationException($"result of evaluating head is inapplicable object: {fun} of type: {fun.GetType()}")
            };

         if (fun is Macro)
         {
            if (result.IsAtom)
               result = Cons(Intern("progn"), Cons(result, Nil));

            result = result.Eval(env);
         }

         return result;
      }

      //================================================================================================================
   }
   //===================================================================================================================
}

