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
      public byte MinArgs { get; }
      public byte MaxArgs { get; }
      public CoreFunc Function { get; }
      public bool UnlimitedMaxArgs => MaxArgs == 15;
      
      //================================================================================================================
      // Constructor
      //================================================================================================================
      public CoreFunction(string name, bool special, byte minArgs, byte maxArgs, CoreFunc fun)
      {
         if (string.IsNullOrEmpty(name))
            throw new ArgumentException($"{nameof(name)} cannot be null or empty", nameof(name));

         if (minArgs > 15)
            throw new ArgumentOutOfRangeException(nameof(minArgs));

         if (maxArgs > 15)
            throw new ArgumentOutOfRangeException(nameof(maxArgs));

         if (minArgs > maxArgs)
            throw new ArgumentException($"{nameof(minArgs)} {minArgs} greater than {nameof(maxArgs)} {maxArgs}!");
         
         Name = name;
         Special = special;
         MinArgs = minArgs;
         MaxArgs = maxArgs;
         Function = fun;
      }

      //================================================================================================================
      // Public instance methods
      //================================================================================================================
      public override string Princ() => ToString();

      //================================================================================================================
      // Protected instance methods
      //================================================================================================================
      protected override LispObject ImplApply(Env env, LispObject argsList)
      {


         throw new NotImplementedException("Implement this!");
      }

      //================================================================================================================
   }

   //===================================================================================================================
}
