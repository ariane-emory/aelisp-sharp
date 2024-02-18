using System.Collections;
using System.Reflection;
using System.Text;

//======================================================================================================================
static partial class Ae
{
   //===================================================================================================================
   // Core function class
   //===================================================================================================================
   public class CoreFunction : LispObject
   {
      //================================================================================================================
      // Core function delegate
      //================================================================================================================
      public delegate LispObject CoreFunc(LispObject arg1, LispObject arg2, int arg3); // ???

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
      // Instance methods
      //================================================================================================================
      public override string ToString() => $"{TypeName}(#{Id}, \"{Name}\")";
      public override string Write() => ToString();
      //================================================================================================================
   }
   
   //===================================================================================================================
}
