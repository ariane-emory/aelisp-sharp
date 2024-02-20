using System.Collections;
using System.Reflection;
using System.Text;

//======================================================================================================================
static partial class Ae
{
   //===================================================================================================================
   // Symbol class
   //===================================================================================================================
   public class Symbol : LispObjectWithStringValue
   {
      //================================================================================================================
      // Constructor
      //================================================================================================================
      public Symbol(string value) : base(value)
      {
         if (string.IsNullOrEmpty(Value))
            throw new ArgumentException($"{nameof(Value)} cannot be null or empty", nameof(Value));
      }

      //================================================================================================================
      // Instance properties
      //================================================================================================================
      protected override string? StringRepresentation => $"{Value}";
      public override bool IsSelfEvaluating => IsKeyword || this == Nil || this == True;
      public bool IsSpecial => Value[0] == '*' && Value[Value.Length - 1] == '*';
      public bool IsKeyword => Value[0] == ':';
      public bool IsSetBindable => !IsKeyword && !IsSelfEvaluating;
      public bool IsLetBindable => !IsSpecial && IsSetBindable;
      
      //==================================================================W==============================================
      // Public methods
      //================================================================================================================
      public override string PrincString() => Value;

      //================================================================================================================
      public override LispObject Eval(Env env)
      {
         if (IsSelfEvaluating)
            return this;

         var lookupMode = IsSpecial ? Env.LookupMode.Global : Env.LookupMode.Nearest;
         var (found, obj) = env.Lookup(lookupMode, this);

         if (!found)
            throw new ApplicationException($"Unbound symbol '{this}!");

         return obj;
      }

      //================================================================================================================
   }

   //===================================================================================================================
}
