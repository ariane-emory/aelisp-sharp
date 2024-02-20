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
      protected override Integer BinaryAdd(Number that) => ApplyBinaryOp(that, (l, r) => l + r);
      protected override Integer BinarySub(Number that) => ApplyBinaryOp(that, (l, r) => l - r);
      protected override Integer BinaryMul(Number that) => ApplyBinaryOp(that, (l, r) => l * r);
      protected override Integer BinaryDiv(Number that) => ApplyBinaryOp(that, (l, r) => l / r);
      protected override bool BinaryCmpEql(Number other) => Value == ((Integer)other).Value;
      protected override bool BinaryCmpLT(Number other) => Value < ((Integer)other).Value;
      protected override bool BinaryCmpGT(Number other) => Value > ((Integer)other).Value;
      protected override bool BinaryCmpLTE(Number other) => Value <= ((Integer)other).Value;
      protected override bool BinaryCmpGTE(Number other) => Value >= ((Integer)other).Value;

      //==============================================================================================================================================
      private static Integer GreaterThanZero(Integer that, string opName)
      {
         if (that.Value <= 0)
            throw new ArgumentException($"Modulo by zero or negative number: {that}.");

         return that;
      }

      //==============================================================================================================================================
      protected Integer BinaryMod(Integer that) => ApplyBinaryOp(GreaterThanZero(that, "modulo"), (l, r) => l % r);
      protected Integer BinaryLsft(Integer that) => ApplyBinaryOp(GreaterThanZero(that, "left shift"), (l, r) => l << r);

      //==============================================================================================================================================
      protected Integer BinaryAnd(Integer that) => ApplyBinaryOp(that, (l, r) => l & r);
      protected Integer BinaryOr(Integer that) => ApplyBinaryOp(that, (l, r) => l | r);
      protected Integer BinaryXor(Integer that) => ApplyBinaryOp(that, (l, r) => l ^ r);

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
      public static readonly Func<LispObject, Integer> Mod = ApplyVariadicIntegerArithmeticFun("modulo", (l, r) => l.BinaryMod(r));
      public static readonly Func<LispObject, Integer> Lsft = ApplyVariadicIntegerArithmeticFun("left shift", (l, r) => l.BinaryLsft(r));
      public static readonly Func<LispObject, Integer> BitAnd = ApplyVariadicIntegerArithmeticFun("AND", (l, r) => l.BinaryAnd(r));
      public static readonly Func<LispObject, Integer> BitOr = ApplyVariadicIntegerArithmeticFun("OR", (l, r) => l.BinaryOr(r));
      public static readonly Func<LispObject, Integer> BitXor = ApplyVariadicIntegerArithmeticFun("XOR", (l, r) => l.BinaryXor(r));

      //==============================================================================================================================================
   }

   //=================================================================================================================================================
}
