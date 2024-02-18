using System.Collections;
using System.Reflection;
using System.Text;

//======================================================================================================================
static partial class Ae
{
   //===================================================================================================================
   // Core function class
   //===================================================================================================================
   public class CoreFunction : Function
   {
      //================================================================================================================
      // Core function delegate
      //================================================================================================================
      public delegate LispObject CoreFunc(LispObject arg1, LispObject arg2, int arg3); // ???

      //================================================================================================================
      // Protected instance properties
      //================================================================================================================
      protected override string? StringRepresentation => $"\"{Name}\"";
            
      //================================================================================================================
      // Public properties
      //================================================================================================================
      public string Name { get; }
      public bool Special { get; }
      public CoreFunc Function { get; }

      //================================================================================================================
      // Constructor
      //================================================================================================================
      public CoreFunction(string name, bool special, CoreFunc fun)
      {
         if (string.IsNullOrEmpty(name))
            throw new ArgumentException($"{nameof(name)} cannot be null or empty", nameof(name));

         Name = name;
         Special = special;
         Function = fun;
      }

      //================================================================================================================
      // Public instance methods
      //================================================================================================================
      public override string Princ() => ToString();
      
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
