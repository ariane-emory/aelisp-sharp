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
