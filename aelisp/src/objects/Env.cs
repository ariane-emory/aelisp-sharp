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
      public LispObject Symbols { get; private set; }
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
      public bool IsRoot => Parent == Nil;

      //================================================================================================================
      private Env GetRoot()
      {
         var current = this;

         while (!current.IsRoot)
            current = (Env)current.Parent;

         return current;
      }

      //================================================================================================================
      public (bool Found, LispObject Object) Lookup(LookupMode mode, Symbol symbol)
      {
         DebugWrite($"Looking up {symbol} in {this}...");

         if (symbol.IsKeyword || symbol == Nil || symbol == True)
            return (true, symbol);

         Env current = mode == LookupMode.Global ? GetRoot() : this;

         while (true)
         {
            LispObject symbols = current.Symbols;
            LispObject values = current.Values;

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

            if (mode == LookupMode.Local || current.IsRoot)
               break;

            if (mode == LookupMode.Global || mode == LookupMode.Nearest)
               current = (Env)current.Parent;
         }

         DebugWrite("Did not find it!");

         return (false, Nil);
      }

      //================================================================================================================
      public void Set(LookupMode mode, Symbol symbol, LispObject value)
      {
         if (symbol.IsKeyword)
            throw new ArgumentException("Cannot set a keyword.");

         // Start from the current environment or root depending on the mode.
         Env current = mode == LookupMode.Global ? GetRoot() : this;

         while (!(current is Nil)) // Assuming Nil is a singleton that represents the absence of a value
         {
            var symbolsList = current.Symbols;
            var valuesList = current.Values;

            while (symbolsList is Pair symbolsPair && valuesList is Pair valuesPair)
            {
               if (Equals(symbol, symbolsPair.Car))
               {
                  // Update existing symbol value
                  ((Pair)valuesList).Car = value;
                  DebugWrite($"Updated {symbol} with value {value}.");
                  return;
               }

               symbolsList = symbolsPair.Cdr;
               valuesList = valuesPair.Cdr;
            }

            // If not found and either in LOCAL mode or at the root, add new symbol-value pair
            if (mode == LookupMode.Local || current.Parent == Nil)
            {
               current.Add(symbol, value);
               return;
            }

            // Traverse to the parent environment
            if (current.Parent is Env parentEnv)
            {
               current = parentEnv;
            }
            else
            {
               // Assuming we hit Nil or an invalid parent type, we stop.
               break;
            }
         }
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
