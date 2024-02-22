using System.Collections;
using System.Reflection;
using System.Text;
using static System.Console;

//====================================================================================================================================================
static partial class Ae
{
   //=================================================================================================================================================
   // Number class
   //=================================================================================================================================================
   public abstract class Number : LispObject, IEquatable<Number>, IComparable<Number>
   {
      //==============================================================================================================================================
      // Private types
      //==============================================================================================================================================
      private enum AssignMode { AssignAnd, AssignOr, };

      //==============================================================================================================================================
      // Properties
      //==============================================================================================================================================
      private static Integer Zero => new Integer(0);
      
      //==============================================================================================================================================
      // Abstract properties
      //==============================================================================================================================================
      protected abstract int Rank { get; }

      //==============================================================================================================================================
      // Impl IEquatable
      //==============================================================================================================================================
      public bool Equals(Number? other) => other is null ? false : BinaryCmpEql(other);

      //==============================================================================================================================================
      public override bool Equals(object? other) => other switch {
         null => false,
         Number otherNumber => BinaryCmpEql(otherNumber),
         _ => false
      };
      
      //==============================================================================================================================================
      public override int GetHashCode() =>  base.GetHashCode();

      //==============================================================================================================================================
      // Impl IComparable
      //==============================================================================================================================================
      public int CompareTo(Number? other) {
         if (other is null)
            return 1;
         
         if (BinaryCmpEql(other))
            return 0;
         
         if (BinaryCmpLT(other))
            return -1;
         
         return 1;
      }

      //==============================================================================================================================================
      // Impl operators
      //==============================================================================================================================================
      public static Number operator + (Number left, Number right) => ApplyBinaryOpFun((l, r) => l.BinaryAdd(r))(left, right);
      public static Number operator - (Number left, Number right) => ApplyBinaryOpFun((l, r) => l.BinarySub(r))(left, right);
      public static Number operator * (Number left, Number right) => ApplyBinaryOpFun((l, r) => l.BinaryMul(r))(left, right);
      public static Number operator / (Number left, Number right) => ApplyBinaryOpFun((l, r) => l.BinaryDiv(r))(left, right);
      
      //==============================================================================================================================================
      public static bool operator == (Number left, Number right) => ApplyBinaryCmpFun((l, r) => l.BinaryCmpEql(r))(left, right);
      public static bool operator < (Number left, Number right) => ApplyBinaryCmpFun((l, r) => l.BinaryCmpLT(r))(left, right);
      public static bool operator <= (Number left, Number right) => (left < right) || (left == right);
      public static bool operator > (Number left, Number right) => ! (left <= right);
      public static bool operator >= (Number left, Number right) => (left == right) || (left > right);
      public static bool operator != (Number left, Number right) => ! (left == right);

      //==============================================================================================================================================
      // Abstract instance methods
      //==============================================================================================================================================
      protected abstract Number Promote();

      //==============================================================================================================================================
      protected abstract Number BinaryAdd(Number that);
      protected abstract Number BinarySub(Number that);
      protected abstract Number BinaryMul(Number that);
      protected abstract Number BinaryDiv(Number that);

      //==============================================================================================================================================
      protected abstract bool BinaryCmpEql(Number that);
      protected abstract bool BinaryCmpLT(Number that);
      protected abstract bool BinaryCmpGT(Number that);
      protected abstract bool BinaryCmpLTE(Number that);
      protected abstract bool BinaryCmpGTE(Number that);

      //==============================================================================================================================================
      // Static methods
      //==============================================================================================================================================
      public static Number VariadicAdd(LispObject list) => ApplyVariadicArithmetic(list, 0, false, ApplyBinaryOpFun((l, r) => l.BinaryAdd(r)));
      public static Number VariadicSub(LispObject list) => ApplyVariadicArithmetic(list, 0, false, ApplyBinaryOpFun((l, r) => l.BinarySub(r)));
      public static Number VariadicMul(LispObject list) => ApplyVariadicArithmetic(list, 1, false, ApplyBinaryOpFun((l, r) => l.BinaryMul(r)));
      public static Number VariadicDiv(LispObject list) => ApplyVariadicArithmetic(list, 1, true, ApplyBinaryOpFun((l, r) => l.BinaryDiv(r)));

      //==============================================================================================================================================
      // public static bool CmpEql(LispObject list) => ApplyVariadicCmp(list, true, AssignMode.AssignAnd, ApplyBinaryCmpFun((l, r) => l.BinaryCmpEql(r)));
      // public static bool CmpLT(LispObject list) => ApplyVariadicCmp(list, true, AssignMode.AssignAnd, ApplyBinaryCmpFun((l, r) => l.BinaryCmpLT(r)));
      // public static bool CmpLTE(LispObject list) => ApplyVariadicCmp(list, true, AssignMode.AssignAnd, ApplyBinaryCmpFun((l, r) => l.BinaryCmpLTE(r)));
      // public static bool CmpGT(LispObject list) => ApplyVariadicCmp(list, true, AssignMode.AssignAnd, ApplyBinaryCmpFun((l, r) => l.BinaryCmpGT(r)));
      // public static bool CmpGTE(LispObject list) => ApplyVariadicCmp(list, true, AssignMode.AssignAnd, ApplyBinaryCmpFun((l, r) => l.BinaryCmpGTE(r)));

      //==============================================================================================================================================
      public static bool VariadicEql(LispObject list) => ApplyVariadicCmp(list, true, AssignMode.AssignAnd, (l, r) => l == r);
      public static bool VariadicLT(LispObject list) => ApplyVariadicCmp(list, true, AssignMode.AssignAnd, (l, r) => l < r);
      public static bool VariadicLTE(LispObject list) => ApplyVariadicCmp(list, true, AssignMode.AssignAnd, (l, r) => l <= r);
      public static bool VariadicGT(LispObject list) => ApplyVariadicCmp(list, true, AssignMode.AssignAnd, (l, r) => l > r);
      public static bool VariadicGTE(LispObject list) => ApplyVariadicCmp(list, true, AssignMode.AssignAnd, (l, r) => l >= r);

      //==============================================================================================================================================
      private static Func<Number, Number, Number> ApplyBinaryOpFun(Func<Number, Number, Number> op)
      {
         return (l, r) =>
         {
            var (left, right) = MatchRanks(l, r);
            return MaybeDemote(op(left, right));
         };
      }

      //=================================================================================================================================================
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
      private static Func<Number, Number, bool> ApplyBinaryCmpFun(Func<Number, Number, bool> cmp)
      {
         return (l, r) =>
         {
            var (left, right) = MatchRanks(l, r);
            return cmp(left, right);
         };
      }

      //==============================================================================================================================================
      private static bool ApplyVariadicCmp(LispObject list,
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
               throw new ArgumentException($"Can't do math on a list with non-numbers: {currentPair.Car}");

            var tmp = op(left, currentNumber);
            
            result = assignMode == AssignMode.AssignAnd
               ? result & tmp
               : result | tmp;
            
            if (!result)
               return false;

            left = currentNumber;
            current = currentPair.Cdr;
         }

         return result;
      }

      //==============================================================================================================================================
      private static (Number, Number) MatchRanks(LispObject left, LispObject right)
      {
         if (!(left is Number leftNumber))
            throw new ArgumentException($"left must be a Number, not {left}.");

         if (!(right is Number rightNumber))
            throw new ArgumentException($"right must be a Number, not {right}.");

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
}
