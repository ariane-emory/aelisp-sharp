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
   public interface IPromotableToRational
   {
      public abstract Rational ToRational();
   }

   //===================================================================================================================
   public interface IPromotableToFloat
   {
      public abstract Float ToFloat();
   }

   //===================================================================================================================
   // Integer class
   //===================================================================================================================
   public class Integer : Number, IPromotableToRational, IPromotableToFloat
   {
      protected override int Rank => 1;
      public int Value { get; }
      public Integer(int value) => Value = value;
      protected override string? StringRepresentation => $"{Value}";
      public override string ToPrincString() => $"{Value}";
      public Rational ToRational() => new Rational(Value, 1);
      public Float ToFloat() => new Float(Value);
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
      // Instance methods
      //================================================================================================================
      protected override string? StringRepresentation => $"{ToPrincString()}";
      public override string ToPrincString() => $"{Numerator}/{Denominator}";
      public Float ToFloat() => new Float((((float)Numerator) / ((float)Denominator)));

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
            result = new Integer(leftTypedNumber.Value + rightTypedNumber.Value);
      }
      //================================================================================================================
      // Integer     Rational     Rational
      //================================================================================================================
      {
         if ((leftNumber is Integer leftTypedNumber) && (rightNumber is Rational rightTypedNumber))
            result = new Rational((leftTypedNumber.Value * rightTypedNumber.Denominator) + rightTypedNumber.Numerator,
                                rightTypedNumber.Denominator);
      }
      //================================================================================================================
      // Integer     Float        Float
      //================================================================================================================
      {
         if ((leftNumber is Integer leftTypedNumber) && (rightNumber is Float rightTypedNumber))
            result = new Float(leftTypedNumber.Value + rightTypedNumber.Value);

      }
      //================================================================================================================
      // Rational     Integer      Rational
      //================================================================================================================
      {
         if ((leftNumber is Rational leftTypedNumber) && (rightNumber is Integer rightTypedNumber))
            result = new Rational(leftTypedNumber.Numerator + rightTypedNumber.Value * leftTypedNumber.Denominator,
                                leftTypedNumber.Denominator);
      }
      //================================================================================================================
      // Rational     Rational     Rational
      //================================================================================================================
      {
         if ((leftNumber is Rational leftTypedNumber) && (rightNumber is Rational rightTypedNumber))
            result = new Rational(((leftTypedNumber.Numerator * rightTypedNumber.Denominator) + (rightTypedNumber.Numerator * leftTypedNumber.Denominator)),
                                (leftTypedNumber.Denominator * rightTypedNumber.Denominator));
      }
      //================================================================================================================
      // Rational     Float        Float
      //================================================================================================================
      {
         if ((leftNumber is Rational leftTypedNumber) && (rightNumber is Float rightTypedNumber))
            result = new Float((((float)leftTypedNumber.Numerator) / ((float)leftTypedNumber.Denominator)) +
                             rightTypedNumber.Value);

      }
      //================================================================================================================
      // Float     Integer      Float
      //================================================================================================================
      {
         if ((leftNumber is Float leftTypedNumber) && (rightNumber is Integer rightTypedNumber))
            result = new Float(leftTypedNumber.Value + rightTypedNumber.Value);
      }
      //================================================================================================================
      // Float     Rational     Float
      //================================================================================================================
      {
         if ((leftNumber is Float leftTypedNumber) && (rightNumber is Rational rightTypedNumber))
            result = new Float(leftTypedNumber.Value + (((float)rightTypedNumber.Numerator) / ((float)rightTypedNumber.Denominator)));
      }
      //================================================================================================================
      // Float     Float        Float
      //================================================================================================================
      {
         if ((leftNumber is Float leftTypedNumber) && (rightNumber is Float rightTypedNumber))
            result = new Float(leftTypedNumber.Value + rightTypedNumber.Value);
      }
      //================================================================================================================

      if (result is Rational resultRational && resultRational.Denominator == 1)
         return new Integer(resultRational.Numerator);

      return result;
   }


   //===================================================================================================================
}
