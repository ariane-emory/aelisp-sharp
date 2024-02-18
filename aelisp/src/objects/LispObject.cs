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
      public LispObject Properties { get; set; } = Nil;
      public abstract override string ToString();
      public abstract string Write();
      protected string TypeName => GetType().Name;

      //================================================================================================================
      public virtual bool IsSelfEvaluating => true;
      public virtual LispObject Eval(Env _) => this;

      //================================================================================================================
      public bool IsList => this == Nil || this is Pair;
      public bool IsAtom => !IsList;

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
   // Symbol class
   //===================================================================================================================
   public class Symbol : LispObjectWithStringValue
   {
      //================================================================================================================
      // Constructor
      //================================================================================================================
      public Symbol(string value) : base(value)
      {
         if (string.IsNullOrEmpty(Value))
            throw new ArgumentException($"{nameof(Value)} cannot be null or empty", nameof(Value));
      }

      //================================================================================================================
      // Public methods
      //================================================================================================================
      public override string ToString() => this == Nil ? "Nil" : $"{TypeName}({Value})";
      public override string Write() => Value;

      public bool IsSpecial => Value[0] == '*' && Value[Value.Length - 1] == '*';
      public bool IsKeyword => Value[0] == ':';
      public override bool IsSelfEvaluating => IsKeyword || this == Nil || this == True;

      //================================================================================================================
      public override LispObject Eval(Env env)
      {
         if (IsSelfEvaluating)
            return this;

         var lookupMode = IsSpecial ? Env.LookupMode.Global : Env.LookupMode.Nearest;
         var (found, obj) = env.Lookup(lookupMode, this);

         if (!found)
            throw new ApplicationException($"Unbound symbol '{Write()}!");

         return obj;
      }
   }

   //===================================================================================================================
   // String class
   //===================================================================================================================
   public class String : LispObjectWithStringValue
   {
      public String(string value) : base(value) { }
      public override string ToString() => $"{TypeName}(\"{Value.EscapeChars()}\")";
      public override string Write() => $"\"{Value}\"";
   }

   //===================================================================================================================
   // Error class
   //===================================================================================================================

   public class Error : LispObjectWithStringValue
   {
      public Error(string value) : base(value) { }
      public override string ToString() => $"{TypeName}(\"{Value.EscapeChars()}\")";
      public override string Write() => ToString();
   }

   //===================================================================================================================
   // Char class
   //===================================================================================================================
   public class Char : LispObject
   {
      public char Value { get; }
      public Char(char value) => Value = value;
      public override string ToString() => $"{TypeName}(\'{Value}\')";
      public override string Write() => $"{Value}";
   }

   //===================================================================================================================
   // Integer class
   //===================================================================================================================
   public class Integer : LispObject
   {
      public int Value { get; }
      public Integer(int value) => Value = value;
      public override string ToString() => $"{TypeName}({Value})";
      public override string Write() => $"{Value}";
   }

   //===================================================================================================================
   // Float class
   //===================================================================================================================
   public class Float : LispObject
   {
      public double Value { get; }
      public Float(double value) => Value = value;
      public override string ToString() => $"{TypeName}({Value})";
      public override string Write() => $"{Value}";
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
      public override string ToString() => $"{TypeName}({Write()})";
      public override string Write() => $"{Numerator}/{Denominator}";
   }

   //===================================================================================================================
}
