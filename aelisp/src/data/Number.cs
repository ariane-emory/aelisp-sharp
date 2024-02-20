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
            var result = cmp(left, right);
            // WriteLine($"cmp {left.ToPrincString()} <=> {right.ToPrincString()} result: {result}");
            return result;
         };
      }

      //==============================================================================================================================================
      // Static methods
      //==============================================================================================================================================
      public static (Number, Number) ThrowUnlessNumbers(LispObject left, LispObject right)
      {
         if (!(left is Number leftNumber))
            throw new ArgumentException($"left must be a Number, not {left}.");

         if (!(right is Number rightNumber))
            throw new ArgumentException($"right must be a Number, not {right}.");

         return (leftNumber, rightNumber);
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
               throw new ArgumentException($"Can't do math on a list with non-numbers: {currentPair.Car}");

            if (assignMode == AssignMode.AssignAnd)
            {
               var tmp = op(left, currentNumber);
               // WriteLine($"assign {result} & {tmp}");
               result &= tmp;
            }
            else
            {
               var tmp = op(left, currentNumber);
               // WriteLine($"assign {result} | {tmp}");
               result |= tmp;
            }

            if (!result)
               return false;

            left = currentNumber;
            current = currentPair.Cdr;
         }

         return result;
      }

      //==============================================================================================================================================
      public static Number Add(LispObject list) => ApplyVariadicArithmetic(list, 0, false, ApplyBinaryOpFun((l, r) => l.BinaryAddSameType(r)));
      public static Number Sub(LispObject list) => ApplyVariadicArithmetic(list, 0, false, ApplyBinaryOpFun((l, r) => l.BinarySubSameType(r)));
      public static Number Mul(LispObject list) => ApplyVariadicArithmetic(list, 1, false, ApplyBinaryOpFun((l, r) => l.BinaryMulSameType(r)));
      public static Number Div(LispObject list) => ApplyVariadicArithmetic(list, 1, true, ApplyBinaryOpFun((l, r) => l.BinaryDivSameType(r)));

      //==============================================================================================================================================
      public static bool CmpEql(LispObject list) => ApplyVariadicComparison(list, true, AssignMode.AssignAnd, ApplyBinaryCmpFun((l, r) => l.BinaryCmpEqlSameType(r)));
      public static bool CmpLT(LispObject list) => ApplyVariadicComparison(list, true, AssignMode.AssignAnd, ApplyBinaryCmpFun((l, r) => l.BinaryCmpLTSameType(r)));
      public static bool CmpGT(LispObject list) => ApplyVariadicComparison(list, true, AssignMode.AssignAnd, ApplyBinaryCmpFun((l, r) => l.BinaryCmpGTSameType(r)));
      public static bool CmpLTE(LispObject list) => ApplyVariadicComparison(list, true, AssignMode.AssignAnd, ApplyBinaryCmpFun((l, r) => l.BinaryCmpLTESameType(r)));
      public static bool CmpGTE(LispObject list) => ApplyVariadicComparison(list, true, AssignMode.AssignAnd, ApplyBinaryCmpFun((l, r) => l.BinaryCmpGTESameType(r)));

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
}
