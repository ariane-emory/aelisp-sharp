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
}
