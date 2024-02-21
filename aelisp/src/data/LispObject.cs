using System.Collections;
using System.Reflection;
using System.Text;

//======================================================================================================================
static partial class Ae
{
   //===================================================================================================================
   // Base LispObject abstract class
   //===================================================================================================================
   public abstract class LispObject
   {
      //==================================================================================================================
      // Static properties
      //==================================================================================================================
      private static int NextId { get; set; } = 0;

      //==================================================================================================================
      // Properties
      //==================================================================================================================
      public virtual string TypeName => GetType().Name;
      public virtual bool IsSelfEvaluating => true;
      public int Id { get; }
      protected abstract string? StringRepresentation { get; }

      //==================================================================================================================
      public bool IsNil => this == Nil;
      public bool IsList => IsNil || this is Pair;
      public bool IsImproperList => IsList && !IsProperList;
      public bool IsAtom => this is not Pair;

      //==================================================================================================================
      public virtual int Length
      {
         get
         {
            if (this == Nil)
               return 0;
            // else if (this is Pair pair)
            //    return pair.PairListLength;
            else
               throw new InvalidOperationException("Not a list");
         }
      }

      //================================================================================================================
      public bool IsProperList
      {
         get
         {
            if (this == Nil)
               return true;

            LispObject current = this;

            while (current is Pair cons)
            {
               if (cons.Cdr == Ae.Nil)
                  return true;

               current = cons.Cdr;
            }

            return false;
         }
      }

      //==================================================================================================================
      // Constructor
      //==================================================================================================================
      protected LispObject() => Id = NextId++;
 
      //==================================================================================================================
      // methods
      //==================================================================================================================
      public LispObject Properties { get; set; } = Nil;
      public abstract string ToPrincString();
      public virtual string ToPrintString() => ToPrincString();
      public virtual LispObject Eval(Env _) => this;

      //================================================================================================================
      public override string ToString()
      {
         var repr = StringRepresentation;

          return repr is null
            ? $"{TypeName}(#{Id})"
            : $"{TypeName}(#{Id}, {repr})";
      }

      //==================================================================================================================
      public bool Eql(LispObject that)
      {
         if (this == that)
            return true;

         if ((this is Pair thisPair) &&
             (that is Pair thatPair) &&
             (thisPair.Car == thatPair.Car) &&
             (thisPair.Cdr == thatPair.Cdr))
            return true;

         if ((this is String thisString) &&
             (that is String thatString) &&
             (thisString.Value == thatString.Value))
            return true;

         if ((this is Char thisChar) &&
             (that is Char thatChar) &&
             (thisChar.Value == thatChar.Value))
            return true;

         if ((this is Number thisNumber) &&
             (that is Number thatNumber) &&
             Number.CmpEql(Cons(this, Cons(that, Nil))))
            return true;
         
         return false;
      }

      //================================================================================================================
   }

   //===================================================================================================================
   // LispObjectWithStringValue abstract class
   //===================================================================================================================
   public abstract class LispObjectWithStringValue : LispObject
   {
      public string Value { get; }
      public LispObjectWithStringValue(string value) => Value = value;
   }

   //===================================================================================================================
   // String class
   //===================================================================================================================
   public class String : LispObjectWithStringValue
   {
      protected override string? StringRepresentation => $"\"{Value.EscapeChars()}\"";
      public override string ToPrincString() => $"{Value}";
      public override string ToPrintString() => $"\"{ToPrincString()}\"";
      public String(string value) : base(value) { }
   }

   //===================================================================================================================
   // Error class
   //===================================================================================================================

   public class Error : LispObjectWithStringValue
   {
      protected override string? StringRepresentation => $"\"{Value.EscapeChars()}\"";
      public override string ToPrincString() => ToString();
      public Error(string value) : base(value) { }
   }

   //===================================================================================================================
   // Char class
   //===================================================================================================================
   public class Char : LispObject
   {
      public char Value { get; }
      protected override string? StringRepresentation => $"\'{Value}\'".EscapeChars();
      public Char(char value) => Value = value;
      public override string ToPrincString() => $"{Value}";
      public override string ToPrintString() => $"\'{ToPrincString()}\'";
   }

   //===================================================================================================================
}
