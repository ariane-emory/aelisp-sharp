using System.Collections;
using System.Reflection;
using System.Text;
using static System.Console;

//====================================================================================================================================================
static partial class Ae
{
   //=================================================================================================================================================
   // Integer class
   //=================================================================================================================================================
   public class Integer : Number
   {
      //==============================================================================================================================================
      // Properties
      //==============================================================================================================================================
      protected override int Rank => 1;
      public int Value { get; }
      protected override string? StringRepresentation => $"{Value}";

      //==============================================================================================================================================
      // Constructor
      //==============================================================================================================================================
      public Integer(int value) => Value = value;

      //==============================================================================================================================================
      // Instance methods 
      //==============================================================================================================================================
      public override string ToPrincString() => $"{Value}";
      protected override Rational Promote() => new(Value, 1);

      //==============================================================================================================================================
      private Integer ApplyBinaryOp(Integer that, Func<int, int, int> op) => new(op(this.Value, that.Value));
      private Integer CastAndApplyBinaryOp(Number that, Func<int, int, int> op) => ApplyBinaryOp((Integer)that, op);
      //==============================================================================================================================================
      protected override Integer BinaryAdd(Number that) => CastAndApplyBinaryOp(that, (l, r) => l + r);
      protected override Integer BinarySub(Number that) => CastAndApplyBinaryOp(that, (l, r) => l - r);
      protected override Integer BinaryMul(Number that) => CastAndApplyBinaryOp(that, (l, r) => l * r);
      protected override Integer BinaryDiv(Number that) => CastAndApplyBinaryOp(that, (l, r) => l / r);

      //==============================================================================================================================================
      private bool CastAndApplyBinaryCmp(Number that, Func<int, int,bool> cmp) => cmp(Value, ((Integer)that).Value);
      //==============================================================================================================================================
      protected override bool BinaryCmpEql(Number that) => CastAndApplyBinaryCmp(that, (l, r) => l == r);
      protected override bool BinaryCmpLT(Number that) => CastAndApplyBinaryCmp(that, (l, r)  => l < r);

      //=============================================================================================================================================
      private static Integer ThrowwUnlessGreaterThanZero(Integer that, string opName)
      {
         if (that.Value <= 0)
            throw new ArgumentException($"{opName} by zero or negative number: {that}.");

         return that;
      }

      //==============================================================================================================================================
      protected Integer UnaryBitNot() => new Integer(~ Value);
      protected Integer BinaryBitAnd(Integer that) => ApplyBinaryOp(that, (l, r) => l & r);
      protected Integer BinaryBitOr(Integer that) => ApplyBinaryOp(that, (l, r) => l | r);
      protected Integer BinaryBitXor(Integer that) => ApplyBinaryOp(that, (l, r) => l ^ r);
      protected Integer BinaryMod(Integer that) => ApplyBinaryOp(ThrowwUnlessGreaterThanZero(that, "modulo"), (l, r) => l % r);
      protected Integer BinaryLsft(Integer that) => ApplyBinaryOp(ThrowwUnlessGreaterThanZero(that, "left shift"), (l, r) => l << r);
      protected Integer BinaryRsft(Integer that) => ApplyBinaryOp(ThrowwUnlessGreaterThanZero(that, "right shift"), (l, r) => l >> r);

      //==============================================================================================================================================
      // Impl operators
      //==============================================================================================================================================
      public static Number operator ~ (Integer right) => right.UnaryBitNot();
      public static Integer operator % (Integer left, Integer right) => left.BinaryMod(right);
      public static Integer operator & (Integer left, Integer right) => left.BinaryBitAnd(right);
      public static Integer operator | (Integer left, Integer right) => left.BinaryBitOr(right);
      public static Integer operator ^ (Integer left, Integer right) => left.BinaryBitXor(right);
      public static Integer operator << (Integer left, Integer right) => left.BinaryLsft(right);
      public static Integer operator >> (Integer left, Integer right) => left.BinaryRsft(right);
      
      //==============================================================================================================================================
      // Static methods
      //==============================================================================================================================================
      public static readonly Func<LispObject, Integer> VariadicBitAnd = ApplyVariadicIntegerArithmeticFun("AND", (l, r) => l.BinaryBitAnd(r));
      public static readonly Func<LispObject, Integer> VariadicBitOr = ApplyVariadicIntegerArithmeticFun("OR", (l, r) => l.BinaryBitOr(r));
      public static readonly Func<LispObject, Integer> VariadicBitXor = ApplyVariadicIntegerArithmeticFun("XOR", (l, r) => l.BinaryBitXor(r));
      public static readonly Func<LispObject, Integer> VariadicMod = ApplyVariadicIntegerArithmeticFun("modulo", (l, r) => l.BinaryMod(r));
      public static readonly Func<LispObject, Integer> VariadicLsft = ApplyVariadicIntegerArithmeticFun("left shift", (l, r) => l.BinaryLsft(r));
      public static readonly Func<LispObject, Integer> VariadicRsft = ApplyVariadicIntegerArithmeticFun("right shift", (l, r) => l.BinaryRsft(r));

      //==============================================================================================================================================
      private static Func<LispObject, Integer> ApplyVariadicIntegerArithmeticFun(string opName, Func<Integer, Integer, Integer> op) => (list) =>
         (Integer)ApplyVariadicArithmetic(list, 1, true, (l, r) =>
         {
            if (!((l is Integer lInteger) && (r is Integer rInteger)))
               throw new ArgumentException($"Can't {opName} non-integers: {l} % {r}.");

            return op(lInteger, rInteger);
         });

      //==============================================================================================================================================
   }

   //=================================================================================================================================================
}
