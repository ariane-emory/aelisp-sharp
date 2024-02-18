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
      public virtual bool IsSelfEvaluating => true;
      public bool IsList => this == Nil || this is Pair;
      public bool IsAtom => !IsList;
      public int Id { get; }
      protected abstract string? StringRepresentation { get; }
 
      //================================================================================================================
      public bool IsProperList
      {
         get
         {
            LispObject current = this;

            while (current is Pair cons)
            {
               if (cons.Cdr == Ae.Nil)
                  return true;

               current = cons.Cdr;
            }

            return current == Ae.Nil;
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
      public abstract string SPrinc();
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
      public override string SPrinc() => $"\"{Value}\"";
   }

   //===================================================================================================================
   // Error class
   //===================================================================================================================

   public class Error : LispObjectWithStringValue
   {
      public Error(string value) : base(value) { }
      protected override string? StringRepresentation => $"\"{Value.EscapeChars()}\"";
      public override string SPrinc() => ToString();
   }

   //===================================================================================================================
   // Char class
   //===================================================================================================================
   public class Char : LispObject
   {
      public char Value { get; }
      public Char(char value) => Value = value;
      protected override string? StringRepresentation => $"\'{Value}\'";
      public override string SPrinc() => $"{Value}";
   }

   //===================================================================================================================
   // Integer class
   //===================================================================================================================
   public class Integer : LispObject
   {
      public int Value { get; }
      public Integer(int value) => Value = value;
      protected override string? StringRepresentation => $"{Value}";
      public override string SPrinc() => $"{Value}";
   }

   //===================================================================================================================
   // Float class
   //===================================================================================================================
   public class Float : LispObject
   {
      public double Value { get; }
      public Float(double value) => Value = value;
      protected override string? StringRepresentation => $"{Value}";
      public override string SPrinc() => $"{Value}";
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
      protected override string? StringRepresentation => $"{SPrinc()}";
      public override string SPrinc() => $"{Numerator}/{Denominator}";
   }

   //===================================================================================================================
}
