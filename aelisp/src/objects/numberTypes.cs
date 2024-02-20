using System.Collections;
using System.Reflection;
using System.Text;

//======================================================================================================================
static partial class Ae
{
   //===================================================================================================================
   // Number class
   //===================================================================================================================
   public abstract class Number : LispObject
   {
      protected abstract int Rank { get; }
   }

   //===================================================================================================================
   // Integer class
   //===================================================================================================================
   public class Integer : Number
   {
      protected override int Rank => 1;
      public int Value { get; }
      public Integer(int value) => Value = value;
      protected override string? StringRepresentation => $"{Value}";
      public override string ToPrincString() => $"{Value}";

   }

   //===================================================================================================================
   // Float class
   //===================================================================================================================
   public class Float : Number
   {
      protected override int Rank => 2;
      public double Value { get; }
      public Float(double value) => Value = value;
      protected override string? StringRepresentation => $"{Value}";
      public override string ToPrincString() => $"{Value}";
   }

   //===================================================================================================================
   // Rational class
   //===================================================================================================================
   public class Rational : Number
   {
      //================================================================================================================
      // Properties
      //================================================================================================================
      public int Numerator { get; }
      public int Denominator { get; }
      protected override int Rank => 3;

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
      protected override string? StringRepresentation => $"{ToPrincString()}";
      public override string ToPrincString() => $"{Numerator}/{Denominator}";

   }

   //===================================================================================================================
   static LispObject BinaryAdd(LispObject left, LispObject right)
   {
      if (!(left is Number leftNumber))
         throw new ArgumentException($"left must be a Number, not {left}.");

      if (!(right is Number rightNumber))
         throw new ArgumentException($"right must be a Number, not {right}.");

      var result = Nil;

      //================================================================================================================
      // Integer     Integer      Integer
      //================================================================================================================
      {
         if ((leftNumber is Integer leftTypedNumber) && (rightNumber is Integer rightTypedNumber))
            return new Integer(leftTypedNumber.Value + rightTypedNumber.Value);
      }
      //================================================================================================================
      // Integer     Rational     Rational
      //================================================================================================================
      {
         if ((leftNumber is Integer leftTypedNumber) && (rightNumber is Rational rightTypedNumber))
            return new Rational((leftTypedNumber.Value * rightTypedNumber.Denominator) + rightTypedNumber.Numerator,
                                rightTypedNumber.Denominator);
      }
      //================================================================================================================
      // Integer     Float        Float
      //================================================================================================================
      {
         if ((leftNumber is Integer leftTypedNumber) && (rightNumber is Float rightTypedNumber))
            return new Float(leftTypedNumber.Value + rightTypedNumber.Value);

      }

      // Rational    Integer      Rational
      // Rational    Rational     Rational
      // Rational    Float        Float
      // Float       Integer      Float
      // Float       Rational     Float
      // Float       Float        Float
      // Left Type:  Right Type:  Result Type:

      return result;
   }


   //===================================================================================================================
}
