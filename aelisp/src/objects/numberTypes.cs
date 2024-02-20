using System.Collections;
using System.Reflection;
using System.Text;

//======================================================================================================================
static partial class Ae
{
   //===================================================================================================================
   // Ae's static method
   //===================================================================================================================
   public static (Number, Number) ThrowUnlessNumbers(LispObject left, LispObject right)
   {
      if (!(left is Number leftNumber))
         throw new ArgumentException($"left must be a Number, not {left}.");

      if (!(right is Number rightNumber))
         throw new ArgumentException($"right must be a Number, not {right}.");

      return (leftNumber, rightNumber);
   }

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

         throw new ApplicationException($"something is wrong, this throw should be unreachable, number is {number}.");
      }

      //================================================================================================================
      // Properties
      //================================================================================================================
      protected abstract int Rank { get; }

      //================================================================================================================
      // Abstract instance methods
      //================================================================================================================
      protected abstract Number BinaryAddSameType(Number other);
      protected abstract Number BinarySubSameType(Number other);
      protected abstract Number BinaryMulSameType(Number other);
      protected abstract Number BinaryDivSameType(Number other);
      protected abstract Number Promote();

      //================================================================================================================
      // Instance methods
      //================================================================================================================
      public Number ApplyBinaryOp(Number other, Func<Number, Number, Number> op)
      {
         var (left, right) = MatchRanks(this, other);
         // return op(left, (right));
         return MaybeDemote(op(left, (right)));
      }

      //================================================================================================================
      public Number BinaryAdd(Number other) => ApplyBinaryOp(other, (l, r) => l.BinaryAddSameType(r));
      public Number BinarySub(Number other) => ApplyBinaryOp(other, (l, r) => l.BinarySubSameType(r));
      public Number BinaryMul(Number other) => ApplyBinaryOp(other, (l, r) => l.BinaryMulSameType(r));
      public Number BinaryDiv(Number other) => ApplyBinaryOp(other, (l, r) => l.BinaryDivSameType(r));

      //================================================================================================================
      public static Number Add(LispObject list)
      {
         if (!list.IsProperList)
            throw new ArgumentException($"Can't do math on an improper list: {list}");

         Number accum = new Integer(0);

         if (!(list is Pair head))
             return accum;

         LispObject current = head;

         while (current is Pair currentPair)
         {
            if (!(currentPair.Car is Number currentNumber))
               throw new ArgumentException($"Can't do math on a non-number list: {currentPair.Car}");

            accum = accum.BinaryAdd(currentNumber);

            current = currentPair.Cdr;
         }

         return accum;
      }

      //================================================================================================================
   }

   //===================================================================================================================
   // Integer class
   //===================================================================================================================
   public class Integer : Number
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
      private Number ApplyBinaryOp(Integer that, Func<int, int, int> op) =>
         new Integer(op(this.Value, that.Value));

      //================================================================================================================
      protected override Number BinaryAddSameType(Number that) => ApplyBinaryOp((Integer)that, (l, r) => l + r);
      protected override Number BinarySubSameType(Number that) => ApplyBinaryOp((Integer)that, (l, r) => l - r);
      protected override Number BinaryMulSameType(Number that) => ApplyBinaryOp((Integer)that, (l, r) => l * r);
      protected override Number BinaryDivSameType(Number that) => ApplyBinaryOp((Integer)that, (l, r) => l / r);

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
      protected override string? StringRepresentation => $"{ToPrincString()}";
      public override string ToPrincString() => Value.ToString("0.0");

      //================================================================================================================
      protected override Number Promote()
      {
         throw new NotImplementedException("float can't be promoted further");
      }

      //================================================================================================================
      private Number ApplyBinaryOp(Float that, Func<double, double, double> op) =>
         new Float(op(this.Value, that.Value));

      //================================================================================================================
      protected override Number BinaryAddSameType(Number that) => ApplyBinaryOp((Float)that, (l, r) => l + r);
      protected override Number BinarySubSameType(Number that) => ApplyBinaryOp((Float)that, (l, r) => l - r);
      protected override Number BinaryMulSameType(Number that) => ApplyBinaryOp((Float)that, (l, r) => l * r);
      protected override Number BinaryDivSameType(Number that) => ApplyBinaryOp((Float)that, (l, r) => l / r);

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
      public Rational((int Numerator, int Denominator) pair) : this(pair.Numerator, pair.Denominator) { }

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
      private Number ApplyBinaryOp(Rational that, Func<int, int, int, int, (int, int)> op) =>
         new Rational(op(Numerator, Denominator, that.Numerator, that.Denominator));

      //================================================================================================================
      protected override Number BinaryAddSameType(Number that) => ApplyBinaryOp((Rational)that, (ln, ld, rn, rd) => ((ln * rd) + (rn * ld), ld * rd));
      protected override Number BinarySubSameType(Number that) => ApplyBinaryOp((Rational)that, (ln, ld, rn, rd) => ((ln * rd) + (rn * ld), ld * rd));
      protected override Number BinaryMulSameType(Number that) => ApplyBinaryOp((Rational)that, (ln, ld, rn, rd) => (ln * rn, ld * rd));
      protected override Number BinaryDivSameType(Number that) => ApplyBinaryOp((Rational)that, (ln, ld, rn, rd) => (ln * rd, ld * rn));

      //================================================================================================================
   }

   //===================================================================================================================
}
