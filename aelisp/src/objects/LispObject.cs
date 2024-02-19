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
      // Private static properties
      //==================================================================================================================
      private static int NextId { get; set; } = 0;

      //==================================================================================================================
      // Instance properties
      //==================================================================================================================
      public string TypeName => GetType().Name;
      protected abstract string? StringRepresentation { get; }
      public virtual bool IsSelfEvaluating => true;
      public bool IsNil => this == Nil;
      public bool IsList => IsNil || this is Pair;
      public bool IsImproperList => IsList && !IsProperList;
      public bool IsAtom => this is not Pair;
      public int Id { get; }

      //==================================================================================================================
      public int Length
      {
         get
         {
            if (this == Nil)
               return 0;
            else if (this is Pair pair)
               return pair.PairListLength;
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

         {
            if ((this is Integer thisInteger) &&
                (that is Integer thatInteger) &&
                (thisInteger.Value == thatInteger.Value))
               return true;
         }

         {
            if ((this is Float thisFloat) &&
                (that is Float thatFloat) &&
                (thisFloat.Value == thatFloat.Value))
               return true;
         }

         {
            if ((this is Integer thisInteger) &&
                (that is Float thatFloat) &&
                (thisInteger.Value == thatFloat.Value))
               return true;
         }

         {
            if ((this is Float thisFloat) &&
                (that is Integer thatInteger) &&
                (thisFloat.Value == thatInteger.Value))
               return true;
         }

         {
            if ((this is Rational thisRational) &&
                (that is Rational thatRational))
            {
               return ((thisRational.Numerator == thatRational.Numerator) &&
                       (thisRational.Denominator == thatRational.Denominator));
            }
         }

         {
            if ((this is Rational thisRational) &&
                (that is Integer thatInteger) &&
                (thisRational.Numerator / thisRational.Denominator == thatInteger.Value))
               return true;
         }

         {
            if ((this is Integer thisInteger) &&
                (that is Rational thatRational) &&
                (thisInteger.Value == thatRational.Numerator / thatRational.Denominator))
               return true;
         }

         {
            if ((this is Rational thisRational) &&
                (that is Float thatFloat) &&
                (thisRational.Numerator / thisRational.Denominator == thatFloat.Value))
               return true;
         }

         {
            if ((this is Float thisFloat) &&
                (that is Rational thatRational) &&
                (thisFloat.Value == thatRational.Numerator / thatRational.Denominator))
               return true;
         }

         return false;
      }

      //==================================================================================================================
      // Constructor
      //==================================================================================================================
      protected LispObject()
      {
         Id = NextId++;
      }

      //==================================================================================================================
      // Instance methods
      //==================================================================================================================
      public LispObject Properties { get; set; } = Nil;
      public abstract string Princ();
      public virtual string Print() => Princ();
      public virtual LispObject Eval(Env _) => this;

      //================================================================================================================
      public override string ToString()
      {
         var repr = StringRepresentation;

         return repr is null
            ? $"{TypeName}(#{Id})"
            : $"{TypeName}(#{Id}, {repr})";
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
      public String(string value) : base(value) { }
      protected override string? StringRepresentation => $"\"{Value.EscapeChars()}\"";
      public override string Princ() => $"{Value}";
      public override string Print() => $"\"{Princ()}\"";
   }

   //===================================================================================================================
   // Error class
   //===================================================================================================================

   public class Error : LispObjectWithStringValue
   {
      public Error(string value) : base(value) { }
      protected override string? StringRepresentation => $"\"{Value.EscapeChars()}\"";
      public override string Princ() => ToString();
   }

   //===================================================================================================================
   // Char class
   //===================================================================================================================
   public class Char : LispObject
   {
      public char Value { get; }
      public Char(char value) => Value = value;
      protected override string? StringRepresentation => $"\'{Value}\'";
      public override string Princ() => $"{Value}";
      public override string Print() => $"\'{Princ()}\'";
   }

   //===================================================================================================================
   // Integer class
   //===================================================================================================================
   public class Integer : LispObject
   {
      public int Value { get; }
      public Integer(int value) => Value = value;
      protected override string? StringRepresentation => $"{Value}";
      public override string Princ() => $"{Value}";
   }

   //===================================================================================================================
   // Float class
   //===================================================================================================================
   public class Float : LispObject
   {
      public double Value { get; }
      public Float(double value) => Value = value;
      protected override string? StringRepresentation => $"{Value}";
      public override string Princ() => $"{Value}";
   }

   //===================================================================================================================
   // Rational class
   //===================================================================================================================
   public class Rational : LispObject
   {
      //================================================================================================================
      // Public properties
      //================================================================================================================
      public int Numerator { get; }
      public int Denominator { get; }

      //================================================================================================================
      // Constructor
      //================================================================================================================ 
      public Rational(int numerator, int denominator)
      {
         if (denominator == 0)
            throw new ArgumentException("denominator can't be 0");
         
         int gcd = GCD(numerator, denominator);

         Numerator = numerator / gcd;
         Denominator = denominator / gcd;
      }

      //================================================================================================================
      // Instance methods
      //================================================================================================================
      protected override string? StringRepresentation => $"{Princ()}";
      public override string Princ() => $"{Numerator}/{Denominator}";

   }

   //===================================================================================================================
}
