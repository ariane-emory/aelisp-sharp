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
      //================================================================================================================
      // Static methods
      //================================================================================================================
      private static (Number, Number) MatchRanks(LispObject left, LispObject right)
      {
         var (leftNumber, rightNumber) = ThrowUnlessNumbers(left, right);

         while (leftNumber.Rank < rightNumber.Rank)
            leftNumber = leftNumber.Promote();

         while (leftNumber.Rank > rightNumber.Rank)
            rightNumber = rightNumber.Promote();

         return (leftNumber, rightNumber);
      }

      //================================================================================================================
      private static Number MaybeDemote(Number number)
      {
         if (number is Integer)
            return number;

         if (number is Rational numberRational)
            return numberRational.Denominator == 1
               ? new Integer(numberRational.Numerator)
               : number;

         if (number is Float numberFloat)
         {
            var floor = Math.Floor(numberFloat.Value);

            return (numberFloat.Value == floor) ? new Integer((int)floor) : number;
         }

         throw new ApplicationException($"something is wrong, this throw should be unrachable, number is {number}.");
      }

      //================================================================================================================
      // Properties
      //================================================================================================================
      protected abstract int Rank { get; }

      //================================================================================================================
      // Abstract instance methods
      //================================================================================================================
      protected abstract Number AddSameType(Number other);
      protected abstract Number SubSameType(Number other);
      protected abstract Number Promote();

      //================================================================================================================
      // Instance methods
      //================================================================================================================
      public Number Add(Number other)
      {
         var (left, right) = MatchRanks(this, other);
         return MaybeDemote(left.AddSameType(right));
      }

      //================================================================================================================
      public Number Sub(Number other)
      {
         var (left, right) = MatchRanks(this, other);
         return MaybeDemote(left.SubSameType(right));
      }

      //================================================================================================================
   }

   //===================================================================================================================
   // Integer class
   //===================================================================================================================
   public class Integer : Number //, IPromotableToRational, IPromotableToFloat, IPromotableTo<Rational>
   {
      //================================================================================================================
      // Constructor
      //================================================================================================================
      public Integer(int value) => Value = value;

      //================================================================================================================
      // Properties
      //================================================================================================================
      protected override int Rank => 1;
      public int Value { get; }
      protected override string? StringRepresentation => $"{Value}";

      //================================================================================================================
      // Instance methods 
      //================================================================================================================
      public override string ToPrincString() => $"{Value}";
      protected override Number Promote() => new Rational(Value, 1);

      //================================================================================================================
      protected override Number AddSameType(Number that)
      {
         var addend = (Integer)that;
         return new Integer(this.Value + addend.Value);
      }

      //================================================================================================================
      protected override Number SubSameType(Number that)
      {
         var subtrahend = (Integer)that;
         return new Integer(this.Value - subtrahend.Value);
      }

      //================================================================================================================
   }

   //===================================================================================================================
   // Rational class
   //===================================================================================================================
   public class Rational : Number
   {
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
      // Properties
      //================================================================================================================
      public int Numerator { get; }
      public int Denominator { get; }
      protected override int Rank => 2;

      //================================================================================================================
      // Instance methods
      //================================================================================================================
      protected override string? StringRepresentation => $"{ToPrincString()}";
      public override string ToPrincString() => $"{Numerator}/{Denominator}";
      protected override Number Promote() => new Float((((float)Numerator) / ((float)Denominator)));

      //================================================================================================================
      protected override Number AddSameType(Number that)
      {
         var addend = (Rational)that;

         int commonDenominator = Denominator * addend.Denominator / GCD(Denominator, addend.Denominator);
         int newNumerator = Numerator * (commonDenominator / Denominator);
         int newAddendNumerator = addend.Numerator * (commonDenominator / addend.Denominator);
         int sumNumerator = newNumerator + newAddendNumerator;

         return new Rational(sumNumerator, commonDenominator);
      }

      //================================================================================================================
   }

   //===================================================================================================================
   // Float class
   //===================================================================================================================
   public class Float : Number
   {
      //================================================================================================================
      // Properties
      //================================================================================================================
      protected override int Rank => 3;
      public double Value { get; }

      //================================================================================================================
      // Constructor
      //================================================================================================================
      public Float(double value) => Value = value;

      //================================================================================================================
      // Instance methods
      //================================================================================================================
      protected override string? StringRepresentation => $"{Value}";
      public override string ToPrincString() => Value.ToString("0.0");

      //================================================================================================================
      protected override Number AddSameType(Number that)
      {
         var addend = (Float)that;

         return new Float(Value + addend.Value);
      }

      //================================================================================================================
      protected override Number Promote()
      {
         throw new NotImplementedException("float can't be promoted further");
      }

      //================================================================================================================
   }

   //===================================================================================================================
   public static (Number, Number) ThrowUnlessNumbers(LispObject left, LispObject right)
   {
      if (!(left is Number leftNumber))
         throw new ArgumentException($"left must be a Number, not {left}.");

      if (!(right is Number rightNumber))
         throw new ArgumentException($"right must be a Number, not {right}.");

      return (leftNumber, rightNumber);
   }

   // //===================================================================================================================
   // public static LispObject BinaryAdd(LispObject left, LispObject right)
   // {
   //    var (leftNumber, rightNumber) = ThrowUnlessNumbers(left, right);
   //    var result = Nil;

   //    //================================================================================================================
   //    // Integer     Integer      Integer
   //    //================================================================================================================
   //    {
   //       if ((leftNumber is Integer leftTypedNumber) && (rightNumber is Integer rightTypedNumber))
   //          result = new Integer(leftTypedNumber.Value + rightTypedNumber.Value);
   //    }
   //    //================================================================================================================
   //    // Integer     Rational     Rational
   //    //================================================================================================================
   //    {
   //       if ((leftNumber is Integer leftTypedNumber) && (rightNumber is Rational rightTypedNumber))
   //          result = new Rational((leftTypedNumber.Value * rightTypedNumber.Denominator) + rightTypedNumber.Numerator,
   //                              rightTypedNumber.Denominator);
   //    }
   //    //================================================================================================================
   //    // Integer     Float        Float
   //    //================================================================================================================
   //    {
   //       if ((leftNumber is Integer leftTypedNumber) && (rightNumber is Float rightTypedNumber))
   //          result = new Float(leftTypedNumber.Value + rightTypedNumber.Value);

   //    }
   //    //================================================================================================================
   //    // Rational     Integer      Rational
   //    //================================================================================================================
   //    {
   //       if ((leftNumber is Rational leftTypedNumber) && (rightNumber is Integer rightTypedNumber))
   //          result = new Rational(leftTypedNumber.Numerator + rightTypedNumber.Value * leftTypedNumber.Denominator,
   //                              leftTypedNumber.Denominator);
   //    }
   //    //================================================================================================================
   //    // Rational     Rational     Rational
   //    //================================================================================================================
   //    {
   //       if ((leftNumber is Rational leftTypedNumber) && (rightNumber is Rational rightTypedNumber))
   //          result = new Rational(((leftTypedNumber.Numerator * rightTypedNumber.Denominator) + (rightTypedNumber.Numerator * leftTypedNumber.Denominator)),
   //                              (leftTypedNumber.Denominator * rightTypedNumber.Denominator));
   //    }
   //    //================================================================================================================
   //    // Rational     Float        Float
   //    //================================================================================================================
   //    {
   //       if ((leftNumber is Rational leftTypedNumber) && (rightNumber is Float rightTypedNumber))
   //          result = new Float((((float)leftTypedNumber.Numerator) / ((float)leftTypedNumber.Denominator)) +
   //                           rightTypedNumber.Value);

   //    }
   //    //================================================================================================================
   //    // Float     Integer      Float
   //    //================================================================================================================
   //    {
   //       if ((leftNumber is Float leftTypedNumber) && (rightNumber is Integer rightTypedNumber))
   //          result = new Float(leftTypedNumber.Value + rightTypedNumber.Value);
   //    }
   //    //================================================================================================================
   //    // Float     Rational     Float
   //    //================================================================================================================
   //    {
   //       if ((leftNumber is Float leftTypedNumber) && (rightNumber is Rational rightTypedNumber))
   //          result = new Float(leftTypedNumber.Value + (((float)rightTypedNumber.Numerator) / ((float)rightTypedNumber.Denominator)));
   //    }
   //    //================================================================================================================
   //    // Float     Float        Float
   //    //================================================================================================================
   //    {
   //       if ((leftNumber is Float leftTypedNumber) && (rightNumber is Float rightTypedNumber))
   //          result = new Float(leftTypedNumber.Value + rightTypedNumber.Value);
   //    }
   //    //================================================================================================================

   //    if (result is Rational resultRational && resultRational.Denominator == 1)
   //       return new Integer(resultRational.Numerator);

   //    return result;
   // }

   //===================================================================================================================
}
