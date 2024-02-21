using System.Collections;
using System.Reflection;
using System.Text;
using static System.Console;

//====================================================================================================================================================
static partial class Ae
{

   //=================================================================================================================================================
   // Rational class
   //=================================================================================================================================================
   public class Rational : Number
   {
      //==============================================================================================================================================
      // Properties
      //==============================================================================================================================================
      protected override int Rank => 2;
      public int Numerator { get; }
      public int Denominator { get; }

      //==============================================================================================================================================
      // Constructor
      //==============================================================================================================================================
      public Rational((int Numerator, int Denominator) pair) : this(pair.Numerator, pair.Denominator) { }
      public Rational(int numerator, int denominator)
      {
         if (denominator == 0)
            throw new ArgumentException("denominator can't be 0");

         int gcd = GCD(numerator, denominator);

         Numerator = numerator / gcd;
         Denominator = denominator / gcd;
      }

      //==============================================================================================================================================
      // Instance methods
      //==============================================================================================================================================
      protected override string? StringRepresentation => $"{ToPrincString()}";
      public override string ToPrincString() => $"{Numerator}/{Denominator}";
      protected override Float Promote() => new((((float)Numerator) / ((float)Denominator)));

      //==============================================================================================================================================
      private Rational ApplyBinaryOp(Number that, Func<int, int, int, int, (int, int)> op) =>
         new(op(Numerator, Denominator, ((Rational)that).Numerator, ((Rational)that).Denominator));
      
      //==============================================================================================================================================
      protected override Rational BinaryAdd(Number that) => ApplyBinaryOp(that, (ln, ld, rn, rd) => ((ln * rd) + (rn * ld), ld * rd));
      protected override Rational BinarySub(Number that) => ApplyBinaryOp(that, (ln, ld, rn, rd) => ((ln * rd) + (rn * ld), ld * rd));
      protected override Rational BinaryMul(Number that) => ApplyBinaryOp(that, (ln, ld, rn, rd) => (ln * rn, ld * rd));
      protected override Rational BinaryDiv(Number that) => ApplyBinaryOp(that, (ln, ld, rn, rd) => (ln * rd, ld * rn));

      //==================================================== ==========================================================================================
      protected override bool BinaryCmpEql(Number other) => ApplyBinaryCmp(other, (l, r) => l == r);
      protected override bool BinaryCmpLT(Number other) => ApplyBinaryCmp(other, (l, r)  => l < r);
      protected override bool BinaryCmpGT(Number other) => ApplyBinaryCmp(other, (l, r)  => l > r);
      protected override bool BinaryCmpLTE(Number other) => ApplyBinaryCmp(other, (l, r)  => l <= r);
      protected override bool BinaryCmpGTE(Number other) => ApplyBinaryCmp(other, (l, r)  => l >= r);

      //==============================================================================================================================================
      private bool ApplyBinaryCmp(Number other, Func<int, int, bool> cmp) => 
         cmp(Numerator * ((Rational)other).Denominator, Denominator * ((Rational)other).Numerator);

      //==============================================================================================================================================
   }

   //=================================================================================================================================================
}
