using System.Collections;
using System.Reflection;
using System.Text;

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
      public UserFunction(LispObject parameters, LispObject body, Env environment)
      {
         Parameters = parameters;
         Body = body;
         Environment = environment;
      }

      //================================================================================================================
      // Protected instance properties
      //================================================================================================================
      protected override string? StringRepresentation => $"{Parameters.Princ()} {Body.Princ()}";

      //================================================================================================================
      // Instance methods
      //================================================================================================================
      public override string Princ() => ToString();
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
      public Lambda(LispObject parameters, LispObject body, Env env) : base(parameters, body, env) { }

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

         return Body.Eval(Environment.Spawn(Parameters, argsList));
      }

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
      public Macro(LispObject parameters, LispObject body, Env env) : base(parameters, body, env) { }

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
