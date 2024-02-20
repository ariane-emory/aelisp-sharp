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
}
