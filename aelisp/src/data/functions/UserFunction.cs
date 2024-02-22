using System.Collections;
using System.Reflection;
using System.Text;
using static System.Console;

//======================================================================================================================
static partial class Ae
{
   //===================================================================================================================
   // User function class
   //===================================================================================================================
   public abstract class UserFunction : Function
   {
      //================================================================================================================
      // Public properties
      //================================================================================================================
      public LispObject Parameters { get; }
      public LispObject Body { get; }
      public Env Environment { get; }
      public override string Name => ToString();

      //================================================================================================================
      // Constructor
      //================================================================================================================
      public UserFunction(Env environment, LispObject parameters, LispObject body)
      {
         Parameters = parameters;
         Body = body;
         Environment = environment;
      }

      //================================================================================================================
      // Protected instance properties
      //================================================================================================================
      protected override string? StringRepresentation => $"{Parameters.ToPrincString()} {Body.ToPrincString()}";

      //================================================================================================================
      // Instance methods
      //================================================================================================================
      public override string ToPrincString() => ToString();

      //================================================================================================================
      // Protected instance methods
      //================================================================================================================
      protected override LispObject ImplApply(Env env, LispObject argsList)
      {
         if ((Parameters is Pair pair) &&
             ((argsList.Length < Parameters.Length) ||
              (Parameters.IsProperList && argsList.Length > Parameters.Length)))
            throw new ArgumentException($"user fun requires {(Parameters.IsProperList ? "exactly" : "at least")} "
                                        + $"{Parameters.Length} args, but got {argsList.Length}");

         if (!Special)
            argsList = EvalArgs(env, argsList);

         // PUT_PROP(fun, "fun", env);

         // WriteLine($"evaluating body: {Body.ToPrincString()}");
         
         return Body.Eval(Environment.Spawn(Parameters, argsList));
      }

   }

   //===================================================================================================================
   // Lambda class
   //===================================================================================================================
   public class Lambda : UserFunction
   {
      //================================================================================================================
      // Public properties
      //================================================================================================================
      public override bool Special => false;

      //================================================================================================================
      // Constructor
      //================================================================================================================
      public Lambda(Env env, LispObject parameters, LispObject body) : base(env, parameters, body) { }

      //================================================================================================================
   }

   //===================================================================================================================
   // Macro class
   //===================================================================================================================
   public class Macro : UserFunction
   {
      //================================================================================================================
      // Public properties
      //================================================================================================================
      public override bool Special => true;

      //================================================================================================================
      // Constructor
      //================================================================================================================
      public Macro(Env env, LispObject parameters, LispObject body) : base(env, parameters, body) { }

      //================================================================================================================
      // Protected instance methods
      //================================================================================================================
      protected override LispObject ImplApply(Env env, LispObject args)
      {
         throw new NotImplementedException("Implement this!");
      }

      //================================================================================================================      
   }

   //===================================================================================================================
}
