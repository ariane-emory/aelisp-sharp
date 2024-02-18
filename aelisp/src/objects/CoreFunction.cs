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
      public delegate LispObject CoreFunc(Env env, LispObject argsList, int argsLength); // ???

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
      public bool UnlimitedMinArgs => MinArgs == 15;

      //================================================================================================================
      // Constructor
      //================================================================================================================
      public CoreFunction(string name, bool special, byte minArgs, byte maxArgs, CoreFunc fun)
      {
         if (string.IsNullOrEmpty(name))
            throw new ArgumentException($"{nameof(name)} cannot be null or empty", nameof(name));

         if (minArgs > 3)
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
         int argsLength = argsList.Length;

         if (((!UnlimitedMinArgs) && argsLength < MinArgs) ||
             ((!UnlimitedMaxArgs) && argsLength > MaxArgs))
         {
            var msgPrefix = $"core '{Name}' requires";

            if (UnlimitedMinArgs && (!UnlimitedMaxArgs))
               throw new ArgumentException($"{msgPrefix} at most {MaxArgs} args, but got {argsLength}");
            else if (UnlimitedMaxArgs && (!UnlimitedMinArgs))
               throw new ArgumentException($"{msgPrefix} at least {MinArgs} args, but got {argsLength}");
            else if (MaxArgs == MinArgs)
               throw new ArgumentException($"{msgPrefix} {MinArgs} args, but got {argsLength}");
            else
               throw new ArgumentException($"{msgPrefix} {MinArgs} to {MaxArgs} args, but got {argsLength}");
         }

         if (!Special)
            argsList = EvalArgs(env, argsList, argsLength);

         return Function(env, argsList, argsLength);
      }

      //================================================================================================================
      private LispObject EvalArgs(Env env, LispObject argsList, int argsLength)
      {
         if (argsList.IsNil)
            return Nil;

         LispObject result = Nil;
         Pair? result_tail_pair = null;
         LispObject current_arg = argsList;

         while (current_arg is Pair current_arg_pair)
         {
            LispObject eval_result = current_arg_pair.Car.Eval(env);

            // TO-DO: add return if errorp? or stick with Exceptions?

            if (result.IsNil)
            {
               result = Cons(eval_result, Nil);
               result_tail_pair = (Pair)result;
            }
            else
            {
               result_tail_pair!.Cdr = Cons(eval_result, Nil);
               result_tail_pair = (Pair)result_tail_pair.Cdr;
            }

            current_arg = current_arg_pair.Cdr;
         }

         // implement evaling tai arg!
         throw new NotImplementedException("Implement this!");
      }

      //================================================================================================================
   }

   //===================================================================================================================
}
