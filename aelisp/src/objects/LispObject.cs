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
      protected string TypeName => GetType().Name;
      protected abstract string? StringRepresentation { get; }
      public virtual bool IsSelfEvaluating => true;
      public bool IsNil => this == Nil;
      public bool IsList => IsNil || this is Pair;
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
         Numerator = numerator;
         Denominator = denominator;
      }

      //================================================================================================================
      // Instance methods
      //================================================================================================================
      protected override string? StringRepresentation => $"{Princ()}";
      public override string Princ() => $"{Numerator}/{Denominator}";
   }

   //===================================================================================================================
}
