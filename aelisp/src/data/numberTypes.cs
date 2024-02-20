using System.Collections;
using System.Reflection;
using System.Text;

//====================================================================================================================================================
static partial class Ae
{
   //=================================================================================================================================================
   // Ae's static method
   //=================================================================================================================================================
   public static (Number, Number) ThrowUnlessNumbers(LispObject left, LispObject right)
   {
      if (!(left is Number leftNumber))
         throw new ArgumentException($"left must be a Number, not {left}.");

      if (!(right is Number rightNumber))
         throw new ArgumentException($"right must be a Number, not {right}.");

      return (leftNumber, rightNumber);
   }

   //=================================================================================================================================================
   // Number class
   //=================================================================================================================================================
   public abstract class Number : LispObject
   {
      //==============================================================================================================================================
      // Properties
      //==============================================================================================================================================
      protected abstract int Rank { get; }
      private static Integer Zero => new Integer(0);

      //==============================================================================================================================================
      // Abstract instance methods
      //==============================================================================================================================================
      protected abstract Number Promote();
      protected abstract Number BinaryAddSameType(Number other);
      protected abstract Number BinarySubSameType(Number other);
      protected abstract Number BinaryMulSameType(Number other);
      protected abstract Number BinaryDivSameType(Number other);
      protected abstract bool BinaryCmpEqlSameType(Number other);
      protected abstract bool BinaryCmpLTSameType(Number other);
      protected abstract bool BinaryCmpGTSameType(Number other);
      protected abstract bool BinaryCmpLTESameType(Number other);
      protected abstract bool BinaryCmpGTESameType(Number other);

      //==============================================================================================================================================
      // Instance methods
      //==============================================================================================================================================
      protected static Func<Number, Number, Number> ApplyBinaryOpFun(Func<Number, Number, Number> op)
      {
         return (l, r) =>
         {
            var (left, right) = MatchRanks(l, r);
            return MaybeDemote(op(left, right));
         };
      }

      //==============================================================================================================================================
      protected static Func<Number, Number, bool> ApplyBinaryCmpFun(Func<Number, Number, bool> cmp)
      {
         return (l, r) =>
         {
            var (left, right) = MatchRanks(l, r);
            return cmp(left, right);
         };
      }

      //==============================================================================================================================================
      public bool CmpEqls(Number that)
      {
         var (left, right) = MatchRanks(this, that);
         return left.BinaryCmpEqlSameType(right);
      }

      //==============================================================================================================================================
      public bool CmpLT(Number that)
      {
         var (left, right) = MatchRanks(this, that);
         return left.BinaryCmpLTSameType(right);
      }

      //==============================================================================================================================================
      public bool CmpGT(Number that)
      {
         var (left, right) = MatchRanks(this, that);
         return left.BinaryCmpGTSameType(right);
      }

      //==============================================================================================================================================
      public bool CmpLTE(Number that)
      {
         var (left, right) = MatchRanks(this, that);
         return left.BinaryCmpLTESameType(right);
      }

      //==============================================================================================================================================
      public bool CmpGTE(Number that)
      {
         var (left, right) = MatchRanks(this, that);
         return left.BinaryCmpGTESameType(right);
      }

      //==============================================================================================================================================
      // Static methods
      //==============================================================================================================================================
      protected static Number ApplyVariadicArithmetic(LispObject list,
                                                      int defaultAccum,
                                                      bool forbidArgsEqlToZero,
                                                      Func<Number, Number, Number> op)
      {
         if (!list.IsProperList)
            throw new ArgumentException($"Can't do math on an improper list: {list}");

         Number accum = new Integer(defaultAccum);

         if (!(list is Pair head))
            return accum;

         LispObject current = head;

         if (!head.Cdr.IsNil)
         {
            if (!(head.Car is Number headNum))
               throw new ArgumentException($"head.Car is not a number: {head.Car}");

            accum = headNum;
            current = head.Cdr;
         }

         while (current is Pair currentPair)
         {
            if (!(currentPair.Car is Number currentNumber))
               throw new ArgumentException($"Can't do math on a non-number list: {currentPair.Car}");

            if (forbidArgsEqlToZero && currentNumber.Eql(Zero))
               throw new ArgumentException($"Possible division by zero: {currentNumber}");

            accum = op(accum, currentNumber);
            current = currentPair.Cdr;
         }

         return accum;
      }

      //==============================================================================================================================================
      private enum AssignMode { AssignAnd, AssignOr, };

      //==============================================================================================================================================
      private static bool ApplyVariadicComparison(LispObject list,
                                                  bool defaultResult,
                                                  AssignMode assignMode,
                                                  Func<Number, Number, bool> op)
      {
         if (!list.IsProperList)
            throw new ArgumentException($"Can't do math on an improper list: {list}");

         bool result = defaultResult;

         if (!(list is Pair head))
            throw new ArgumentException($"can't compare zero items: {list}.");

         LispObject current = head;

         if (head.Cdr.IsNil)
            throw new ArgumentException($"can't compare a single item: {list}.");

         if (!(head.Car is Number headNum))
            throw new ArgumentException($"head.Car is not a number: {head.Car}");

         var left = headNum;
         current = head.Cdr;

         while (current is Pair currentPair)
         {
            if (!(currentPair.Car is Number currentNumber))
               throw new ArgumentException($"Can't do math on a non-number list: {currentPair.Car}");

            if (assignMode == AssignMode.AssignAnd)
               result &= op(left, currentNumber);
            else
               result |= op(left, currentNumber);

            result = op(left, currentNumber);

            // TODO: probably okay to short circuit as soon as result becomes false.

            current = currentPair.Cdr;
         }

         return result;
      }

      //==============================================================================================================================================
      public static Number Add(LispObject list) => ApplyVariadicArithmetic(list, 0, false, ApplyBinaryOpFun((l, r) => l.BinaryAddSameType(r)));
      public static Number Sub(LispObject list) => ApplyVariadicArithmetic(list, 0, false, ApplyBinaryOpFun((l, r) => l.BinarySubSameType(r)));
      public static Number Mul(LispObject list) => ApplyVariadicArithmetic(list, 1, false, ApplyBinaryOpFun((l, r) => l.BinaryMulSameType(r)));
      public static Number Div(LispObject list) => ApplyVariadicArithmetic(list, 1, true, ApplyBinaryOpFun((l, r) => l.BinaryDivSameType(r)));
      public static bool CmpEql(LispObject list) => ApplyVariadicComparison(list, true, AssignMode.AssignAnd, ApplyBinaryCmpFun((l, r) => l.CmpEqls(r)));
      public static bool CmpLT(LispObject list) => ApplyVariadicComparison(list, true, AssignMode.AssignAnd, ApplyBinaryCmpFun((l, r) => l.CmpLT(r)));
      public static bool CmpGT(LispObject list) => ApplyVariadicComparison(list, true, AssignMode.AssignAnd, ApplyBinaryCmpFun((l, r) => l.CmpGT(r)));
      public static bool CmpLTE(LispObject list) => ApplyVariadicComparison(list, true, AssignMode.AssignAnd, ApplyBinaryCmpFun((l, r) => l.CmpLTE(r)));
      public static bool CmpGTE(LispObject list) => ApplyVariadicComparison(list, true, AssignMode.AssignAnd, ApplyBinaryCmpFun((l, r) => l.CmpGTE(r)));

      //==============================================================================================================================================
      private static (Number, Number) MatchRanks(LispObject left, LispObject right)
      {
         var (leftNumber, rightNumber) = ThrowUnlessNumbers(left, right);

         while (leftNumber.Rank < rightNumber.Rank)
            leftNumber = leftNumber.Promote();

         while (leftNumber.Rank > rightNumber.Rank)
            rightNumber = rightNumber.Promote();

         return (leftNumber, rightNumber);
      }

      //==============================================================================================================================================
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

      //==============================================================================================================================================
   }

   //=================================================================================================================================================
   // Integer class
   //=================================================================================================================================================
   public class Integer : Number
   {
      //==============================================================================================================================================
      // Constructor
      //==============================================================================================================================================
      public Integer(int value) => Value = value;

      //==============================================================================================================================================
      // Properties
      //==============================================================================================================================================
      protected override int Rank => 1;
      public int Value { get; }
      protected override string? StringRepresentation => $"{Value}";

      //==============================================================================================================================================
      // Instance methods 
      //==============================================================================================================================================
      public override string ToPrincString() => $"{Value}";
      protected override Rational Promote() => new(Value, 1);

      //==============================================================================================================================================
      private Integer ApplyBinaryOp(Number that, Func<int, int, int> op) =>
         new(op(this.Value, ((Integer)that).Value));

      //==============================================================================================================================================
      protected override Integer BinaryAddSameType(Number that) => ApplyBinaryOp(that, (l, r) => l + r);
      protected override Integer BinarySubSameType(Number that) => ApplyBinaryOp(that, (l, r) => l - r);
      protected override Integer BinaryMulSameType(Number that) => ApplyBinaryOp(that, (l, r) => l * r);
      protected override Integer BinaryDivSameType(Number that) => ApplyBinaryOp(that, (l, r) => l / r);
      protected override bool BinaryCmpEqlSameType(Number other) => Value == ((Integer)other).Value;
      protected override bool BinaryCmpLTSameType(Number other) => Value < ((Integer)other).Value;
      protected override bool BinaryCmpGTSameType(Number other) => Value > ((Integer)other).Value;
      protected override bool BinaryCmpLTESameType(Number other) => Value <= ((Integer)other).Value;
      protected override bool BinaryCmpGTESameType(Number other) => Value >= ((Integer)other).Value;

      //==============================================================================================================================================
      private static Integer GreaterThanZero(Integer that, string opName)
      {
         if (that.Value <= 0)
            throw new ArgumentException($"Modulo by zero or negative number: {that}.");

         return that;
      }

      //==============================================================================================================================================
      protected Integer BinaryModSameType(Integer that) => ApplyBinaryOp(GreaterThanZero(that, "modulo"), (l, r) => l % r);
      protected Integer BinaryLsftSameType(Integer that) => ApplyBinaryOp(GreaterThanZero(that, "left shift"), (l, r) => l << r);

      //==============================================================================================================================================
      protected Integer BinaryAndSameType(Integer that) => ApplyBinaryOp(that, (l, r) => l & r);
      protected Integer BinaryOrSameType(Integer that) => ApplyBinaryOp(that, (l, r) => l | r);
      protected Integer BinaryXorSameType(Integer that) => ApplyBinaryOp(that, (l, r) => l ^ r);

      //==============================================================================================================================================
      // Static methods
      //==============================================================================================================================================
      private static Func<LispObject, Integer> ApplyVariadicIntegerArithmeticFun(string opName, Func<Integer, Integer, Integer> op) => (list) =>
         (Integer)ApplyVariadicArithmetic(list, 1, true, (l, r) =>
         {
            if (!((l is Integer lInteger) && (r is Integer rInteger)))
               throw new ArgumentException($"Can't {opName} non-integers: {l} % {r}.");

            return op(lInteger, rInteger);
         });

      //==============================================================================================================================================
      public static readonly Func<LispObject, Integer> Mod = ApplyVariadicIntegerArithmeticFun("modulo", (l, r) => l.BinaryModSameType(r));
      public static readonly Func<LispObject, Integer> Lsft = ApplyVariadicIntegerArithmeticFun("left shift", (l, r) => l.BinaryLsftSameType(r));
      public static readonly Func<LispObject, Integer> BitAnd = ApplyVariadicIntegerArithmeticFun("AND", (l, r) => l.BinaryAndSameType(r));
      public static readonly Func<LispObject, Integer> BitOr = ApplyVariadicIntegerArithmeticFun("OR", (l, r) => l.BinaryOrSameType(r));
      public static readonly Func<LispObject, Integer> BitXor = ApplyVariadicIntegerArithmeticFun("XOR", (l, r) => l.BinaryXorSameType(r));

      //==============================================================================================================================================
   }

   //=================================================================================================================================================
   // Float class
   //=================================================================================================================================================
   public class Float : Number
   {
      //==============================================================================================================================================
      // Properties
      //==============================================================================================================================================
      protected override int Rank => 3;
      public double Value { get; }

      //==============================================================================================================================================
      // Constructor
      //==============================================================================================================================================
      public Float(double value) => Value = value;

      //==============================================================================================================================================
      // Instance methods
      //==============================================================================================================================================
      protected override string? StringRepresentation => $"{ToPrincString()}";
      public override string ToPrincString() => Value.ToString("0.0");

      //==============================================================================================================================================
      protected override Number Promote()
      {
         throw new NotImplementedException("float can't be promoted further");
      }

      //==============================================================================================================================================
      private Float ApplyBinaryOp(Number that, Func<double, double, double> op) =>
         new(op(this.Value, ((Float)that).Value));

      //==============================================================================================================================================
      protected override Float BinaryAddSameType(Number that) => ApplyBinaryOp(that, (l, r) => l + r);
      protected override Float BinarySubSameType(Number that) => ApplyBinaryOp(that, (l, r) => l - r);
      protected override Float BinaryMulSameType(Number that) => ApplyBinaryOp(that, (l, r) => l * r);
      protected override Float BinaryDivSameType(Number that) => ApplyBinaryOp(that, (l, r) => l / r);
      protected override bool BinaryCmpEqlSameType(Number other) => Value == ((Float)other).Value;
      protected override bool BinaryCmpLTSameType(Number other) => Value < ((Float)other).Value;
      protected override bool BinaryCmpGTSameType(Number other) => Value > ((Float)other).Value;
      protected override bool BinaryCmpLTESameType(Number other) => Value <= ((Float)other).Value;
      protected override bool BinaryCmpGTESameType(Number other) => Value >= ((Float)other).Value;

      //==============================================================================================================================================
   }

   //=================================================================================================================================================
   // Rational class
   //=================================================================================================================================================
   public class Rational : Number
   {
      //==============================================================================================================================================
      // Constructor
      //==============================================================================================================================================
      public Rational(int numerator, int denominator)
      {
         if (denominator == 0)
            throw new ArgumentException("denominator can't be 0");

         int gcd = GCD(numerator, denominator);

         Numerator = numerator / gcd;
         Denominator = denominator / gcd;
      }

      //==============================================================================================================================================
      public Rational((int Numerator, int Denominator) pair) : this(pair.Numerator, pair.Denominator) { }

      //==============================================================================================================================================
      // Properties
      //==============================================================================================================================================
      public int Numerator { get; }
      public int Denominator { get; }
      protected override int Rank => 2;

      //==============================================================================================================================================
      // Instance methods
      //==============================================================================================================================================
      protected override string? StringRepresentation => $"{ToPrincString()}";
      public override string ToPrincString() => $"{Numerator}/{Denominator}";
      protected override Float Promote() => new((((float)Numerator) / ((float)Denominator)));

      //==============================================================================================================================================
      private Rational ApplyBinaryOp(Number that, Func<int, int, int, int, (int, int)> op) =>
         new(op(Numerator, Denominator, ((Rational)that).Numerator, ((Rational)that).Denominator));

      // //==============================================================================================================================================
      // private bool ThrowNotImplemented()
      // {
      //    throw new NotImplementedException("Rational can't do this operation yet");
      //    return false;
      // }

      //==============================================================================================================================================
      protected override Rational BinaryAddSameType(Number that) => ApplyBinaryOp(that, (ln, ld, rn, rd) => ((ln * rd) + (rn * ld), ld * rd));
      protected override Rational BinarySubSameType(Number that) => ApplyBinaryOp(that, (ln, ld, rn, rd) => ((ln * rd) + (rn * ld), ld * rd));
      protected override Rational BinaryMulSameType(Number that) => ApplyBinaryOp(that, (ln, ld, rn, rd) => (ln * rn, ld * rd));
      protected override Rational BinaryDivSameType(Number that) => ApplyBinaryOp(that, (ln, ld, rn, rd) => (ln * rd, ld * rn));
      protected override bool BinaryCmpEqlSameType(Number other) => Denominator == ((Rational)other).Denominator && Numerator == ((Rational)other).Numerator;
      //==============================================================================================================================================
      protected override bool BinaryCmpLTSameType(Number other)
      {
         var leftCmpNum = Numerator * ((Rational)other).Denominator;
         var rightCmpNum = Denominator * ((Rational)other).Numerator;
         return leftCmpNum < rightCmpNum;
      }
      //==============================================================================================================================================
      protected override bool BinaryCmpGTSameType(Number other)
      {
         var leftCmpNum = Numerator * ((Rational)other).Denominator;
         var rightCmpNum = Denominator * ((Rational)other).Numerator;
         return leftCmpNum > rightCmpNum;
      }
      //==============================================================================================================================================
      protected override bool BinaryCmpLTESameType(Number other)
      {
         var leftCmpNum = Numerator * ((Rational)other).Denominator;
         var rightCmpNum = Denominator * ((Rational)other).Numerator;
         return leftCmpNum <= rightCmpNum;
      }
      //==============================================================================================================================================
      protected override bool BinaryCmpGTESameType(Number other)
      {
         var leftCmpNum = Numerator * ((Rational)other).Denominator;
         var rightCmpNum = Denominator * ((Rational)other).Numerator;
         return leftCmpNum >= rightCmpNum;
      }

      //==============================================================================================================================================
   }

   //=================================================================================================================================================
}
