//======================================================================================================================
static partial class Ae
{
   //===================================================================================================================
   // Environment class
   //===================================================================================================================
   public class Env : LispObject
   {
      //================================================================================================================
      // Public types
      //================================================================================================================
      public enum LookupMode { Local, Global, Nearest, };

      //================================================================================================================
      // Public static properties
      //================================================================================================================
      public static bool EnableDebugWrite { get; set; } = false;

      //================================================================================================================
      // Public static methods
      //================================================================================================================
      private static void DebugWrite(string s)
      {
         if (EnableDebugWrite)
            Console.WriteLine(s);
      }

      //================================================================================================================
      // Public properties
      //================================================================================================================
      public LispObject Parent { get; }
      public LispObject Symbols { get; private set;  }
      public LispObject Values { get; private set; }

        //================================================================================================================
        // Constructor
        //================================================================================================================
        public Env(LispObject parent, LispObject symbols, LispObject values)
      {
         if (!((parent is Env) || parent == Nil))
            throw new ArgumentException("Parent must be an Env or Nil");

         if (!(symbols.IsProperList()))
            throw new ArgumentException("Symbols must be a proper list");

         if (!(values.IsProperList()))
            throw new ArgumentException("Values must be a proper list");

         Parent = parent;
         Symbols = symbols;
         Values = values;
      }

      //================================================================================================================
      // Instance methods
      //================================================================================================================
      public override string ToString() => $"{TypeName}({Parent}, {Symbols.Write()}, {Values.Write()})";
      public override string Write() => ToString();
      public bool IsRoot() => Parent == Nil;

      //================================================================================================================
      public (bool Found, LispObject Object) Lookup(LookupMode mode, Symbol symbol)
      {
         DebugWrite($"Looking up {symbol} in {this}...");

         if (symbol.IsKeyword() || symbol == Nil || symbol == True)
            return (true, symbol);

         Env currentEnv = this;

         // If mode is Global, move directly to the root environment
         if (mode == LookupMode.Global)
            while (!currentEnv.IsRoot())
               currentEnv = (Env)currentEnv.Parent;

         while (true)
         {
            LispObject symbols = currentEnv.Symbols;
            LispObject values = currentEnv.Values;

            while (symbols is Pair symPair && values is Pair valPair)
            {
               if (symbol == symPair.Car)
               {
                  DebugWrite($"Found {symbol} -> {valPair.Car}");
                  return (true, valPair.Car);
               }

               symbols = symPair.Cdr;
               values = valPair.Cdr;
            }

            // Check for the special case where the symbols list consists of a single symbol to which all values are bound.
            if (symbols == symbol)
               return (true, values);

            if (mode == LookupMode.Local || currentEnv.Parent == Nil)
               break;

            if (mode == LookupMode.Global || mode == LookupMode.Nearest)
               currentEnv = (Env)currentEnv.Parent;
         }

         DebugWrite("Did not find it!");

         return (false, Nil);
      }

      //================================================================================================================
      public void Add(Symbol symbol, LispObject value)
      {
         if (symbol.IsKeyword())
            throw new ArgumentException("Cannot add a keyword as a symbol.");

         Symbols = Cons(symbol, Symbols);
         Values = Cons(value, Values);

         DebugWrite($"Added {symbol} with value {value}.");
      }
      //================================================================================================================
   }
   //===================================================================================================================
}
