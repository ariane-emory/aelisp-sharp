using System.Collections;
using System.Reflection;
using System.Text;

//======================================================================================================================
static partial class Ae
{
   //===================================================================================================================
   // Core function class
   //===================================================================================================================
   public class CoreFun : Function
   {
      //================================================================================================================
      // Core function delegate
      //================================================================================================================
      public delegate LispObject FuncT(Env env, LispObject argsList, int argsLength);

      //================================================================================================================
      // Protected instance properties
      //================================================================================================================
      protected override string? StringRepresentation => $"\"{Name}\"";

      //================================================================================================================
      // Private properties
      //================================================================================================================
      private string CoreName { get; set; }
      private bool CoreSpecial { get; set; }

      //================================================================================================================
      // Public properties
      //================================================================================================================
      public override string Name => CoreName;
      public override bool Special => CoreSpecial;
      public byte MinArgs { get; }
      public byte MaxArgs { get; }
      public FuncT Function { get; }
      public bool UnlimitedMaxArgs => MaxArgs == 15;

      //================================================================================================================
      // Constructor
      //================================================================================================================
      public CoreFun(string name, FuncT fun, byte minArgs, byte maxArgs, bool special)
      {
         if (string.IsNullOrEmpty(name))
            throw new ArgumentException($"{nameof(name)} cannot be null or empty", nameof(name));

         if (minArgs > 15)
            throw new ArgumentOutOfRangeException(nameof(minArgs));

         if (maxArgs > 15)
            throw new ArgumentOutOfRangeException(nameof(maxArgs));

         if (minArgs > maxArgs)
            throw new ArgumentException($"{nameof(minArgs)} {minArgs} greater than {nameof(maxArgs)} {maxArgs}!");

         CoreName = name;
         CoreSpecial = special;
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
         int argsLength = argsList.Length;

         if ((argsLength < MinArgs) || ((!UnlimitedMaxArgs) && argsLength > MaxArgs))
         {
            var msgPrefix = $"core '{Name}' requires";

            if (UnlimitedMaxArgs)
               throw new ArgumentException($"{msgPrefix} at least {MinArgs} args, but got {argsLength}");
            else if (MaxArgs == MinArgs)
               throw new ArgumentException($"{msgPrefix} {MinArgs} args, but got {argsLength}");
            else
               throw new ArgumentException($"{msgPrefix} {MinArgs} to {MaxArgs} args, but got {argsLength}");
         }

         if (!Special)
            argsList = EvalArgs(env, argsList);

         return Function(env, argsList, argsLength);
      }

      //================================================================================================================
   }

   //===================================================================================================================
}
