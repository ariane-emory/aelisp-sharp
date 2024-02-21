using System.Collections;
using System.Reflection;
using System.Text;
using static System.Console;

//====================================================================================================================================================
static partial class Ae
{
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
      protected override Float BinaryAdd(Number that) => ApplyBinaryOp(that, (l, r) => l + r);
      protected override Float BinarySub(Number that) => ApplyBinaryOp(that, (l, r) => l - r);
      protected override Float BinaryMul(Number that) => ApplyBinaryOp(that, (l, r) => l * r);
      protected override Float BinaryDiv(Number that) => ApplyBinaryOp(that, (l, r) => l / r);
      //==============================================================================================================================================
      protected override bool BinaryCmpEql(Number other) => CompareWithSameType(other, (l, r) => l == r);
      protected override bool BinaryCmpLT(Number other) => CompareWithSameType(other, (l, r)  => l < r);
      protected override bool BinaryCmpGT(Number other) => CompareWithSameType(other, (l, r)  => l > r);
      protected override bool BinaryCmpLTE(Number other) => CompareWithSameType(other, (l, r)  => l <= r);
      protected override bool BinaryCmpGTE(Number other) => CompareWithSameType(other, (l, r)  => l >= r);
      //==============================================================================================================================================
      private bool CompareWithSameType(Number other, Func<double, double, bool> cmp) => 
         cmp(Value, ((Float)other).Value);

      //==============================================================================================================================================
   }

   //=================================================================================================================================================
}
