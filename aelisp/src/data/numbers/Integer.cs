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
      private Integer ApplyBinaryOp(Number that, Func<int, int, int> op) =>
         new(op(this.Value, ((Integer)that).Value));

      //==============================================================================================================================================
      protected override Integer BinaryAdd(Number that) => ApplyBinaryOp(that, (l, r) => l + r);
      protected override Integer BinarySub(Number that) => ApplyBinaryOp(that, (l, r) => l - r);
      protected override Integer BinaryMul(Number that) => ApplyBinaryOp(that, (l, r) => l * r);
      protected override Integer BinaryDiv(Number that) => ApplyBinaryOp(that, (l, r) => l / r);

      //==============================================================================================================================================
      protected override bool BinaryCmpEql(Number other) => ApplyBinaryCmp(other, (l, r) => l == r);
      protected override bool BinaryCmpLT(Number other) => ApplyBinaryCmp(other, (l, r)  => l < r);
      protected override bool BinaryCmpGT(Number other) => ApplyBinaryCmp(other, (l, r)  => l > r);
      protected override bool BinaryCmpLTE(Number other) => ApplyBinaryCmp(other, (l, r)  => l <= r);
      protected override bool BinaryCmpGTE(Number other) => ApplyBinaryCmp(other, (l, r)  => l >= r);

      //==============================================================================================================================================
      private bool ApplyBinaryCmp(Number other, Func<int, int,bool> cmp) => 
         cmp(Value, ((Integer)other).Value);

      //=============================================================================================================================================
      private static Integer ThrowwUnlessGreaterThanZero(Integer that, string opName)
      {
         if (that.Value <= 0)
            throw new ArgumentException($"Modulo by zero or negative number: {that}.");

         return that;
      }

      //==============================================================================================================================================
      protected Integer BinaryAnd(Integer that) => ApplyBinaryOp(that, (l, r) => l & r);
      protected Integer BinaryOr(Integer that) => ApplyBinaryOp(that, (l, r) => l | r);
      protected Integer BinaryXor(Integer that) => ApplyBinaryOp(that, (l, r) => l ^ r);
      protected Integer BinaryMod(Integer that) => ApplyBinaryOp(ThrowwUnlessGreaterThanZero(that, "modulo"), (l, r) => l % r);
      protected Integer BinaryLsft(Integer that) => ApplyBinaryOp(ThrowwUnlessGreaterThanZero(that, "left shift"), (l, r) => l << r);
      protected Integer BinaryRsft(Integer that) => ApplyBinaryOp(ThrowwUnlessGreaterThanZero(that, "right shift"), (l, r) => l >> r);

      //==============================================================================================================================================
      // Impl operators
      //==============================================================================================================================================
      public static Integer operator % (Integer left, Integer right) => left.BinaryMod(right);
      public static Integer operator & (Integer left, Integer right) => left.BinaryAnd(right);
      public static Integer operator | (Integer left, Integer right) => left.BinaryOr(right);
      public static Integer operator ^ (Integer left, Integer right) => left.BinaryXor(right);
      public static Integer operator << (Integer left, Integer right) => left.BinaryLsft(right);
      public static Integer operator >> (Integer left, Integer right) => left.BinaryRsft(right);
      
      //==============================================================================================================================================
      // Static methods
      //==============================================================================================================================================
      public static readonly Func<LispObject, Integer> BitAnd = ApplyVariadicIntegerArithmeticFun("AND", (l, r) => l.BinaryAnd(r));
      public static readonly Func<LispObject, Integer> BitOr = ApplyVariadicIntegerArithmeticFun("OR", (l, r) => l.BinaryOr(r));
      public static readonly Func<LispObject, Integer> BitXor = ApplyVariadicIntegerArithmeticFun("XOR", (l, r) => l.BinaryXor(r));
      public static readonly Func<LispObject, Integer> Mod = ApplyVariadicIntegerArithmeticFun("modulo", (l, r) => l.BinaryMod(r));
      public static readonly Func<LispObject, Integer> Lsft = ApplyVariadicIntegerArithmeticFun("left shift", (l, r) => l.BinaryLsft(r));
      public static readonly Func<LispObject, Integer> Rsft = ApplyVariadicIntegerArithmeticFun("right shift", (l, r) => l.BinaryRsft(r));
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
