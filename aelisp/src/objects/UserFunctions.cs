using System.Collections;
using System.Reflection;
using System.Text;

//======================================================================================================================
static partial class Ae
{
   //====================================================================================================================
   // User function class
   //====================================================================================================================
   public abstract class UserFunction : LispObject
   {
      //==================================================================================================================
      // Public properties
      //==================================================================================================================
      public LispObject Parameters { get; }
      public LispObject Body { get; }
      public LispObject Env { get; }

      //==================================================================================================================
      // Constructor
      //==================================================================================================================
      public UserFunction(LispObject parameters, LispObject body, LispObject env)
      {
         Parameters = parameters;
         Body = body;
         Env = env;
      }

      //================================================================================================================
      // Protected instance properties
      //================================================================================================================
      protected override string? StringRepresentation => "";

      //==================================================================================================================
      // Instance methods
      //==================================================================================================================
      public override string Write() => ToString();
   }

   //====================================================================================================================
   // Lambda class
   //====================================================================================================================
   public class Lambda : UserFunction
   {
      public Lambda(LispObject parameters, LispObject body, LispObject env) : base(parameters, body, env) { }
   }

   //====================================================================================================================
   // Macro class
   //====================================================================================================================
   public class Macro : UserFunction
   {
      public Macro(LispObject parameters, LispObject body, LispObject env) : base(parameters, body, env) { }
   }
   
   //====================================================================================================================
}
