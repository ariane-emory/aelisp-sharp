using System.Collections;
using System.Reflection;
using System.Text;

//======================================================================================================================
static partial class Ae
{
   //===================================================================================================================
   // Integer class
   //===================================================================================================================
   public class Integer : LispObject
   {
      public int Value { get; }
      public Integer(int value) => Value = value;
      protected override string? StringRepresentation => $"{Value}";
      public override string ToPrincString() => $"{Value}";
   }

   //===================================================================================================================
   // Float class
   //===================================================================================================================
   public class Float : LispObject
   {
      public double Value { get; }
      public Float(double value) => Value = value;
      protected override string? StringRepresentation => $"{Value}";
      public override string ToPrincString() => $"{Value}";
   }

   //===================================================================================================================
   // Rational class
   //===================================================================================================================
   public class Rational : LispObject
   {
      //================================================================================================================
      // Public properties
      //================================================================================================================
      public int Numerator { get; }
      public int Denominator { get; }

      //================================================================================================================
      // Constructor
      //================================================================================================================ 
      public Rational(int numerator, int denominator)
      {
         if (denominator == 0)
            throw new ArgumentException("denominator can't be 0");
         
         int gcd = GCD(numerator, denominator);

         Numerator = numerator / gcd;
         Denominator = denominator / gcd;
      }

      //================================================================================================================
      // Instance methods
      //================================================================================================================
      protected override string? StringRepresentation => $"{ToPrincString()}";
      public override string ToPrincString() => $"{Numerator}/{Denominator}";

   }

   //===================================================================================================================
}
