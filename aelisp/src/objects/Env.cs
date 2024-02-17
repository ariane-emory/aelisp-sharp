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
      public enum LookupMode { Local, Nearest, Global };
      // public enum SetMode { Local, Nearest, Global, };

      //================================================================================================================
      // Public static properties
      //================================================================================================================
      public static bool EnableDebugWrite { get; set; } = true;

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
         DebugWrite($"\nLooking up {symbol} in {this}...");

         if (symbol.IsKeyword || symbol == Nil || symbol == True)
            return (true, symbol);

         Env current = mode == LookupMode.Global ? GetRoot() : this;

         while (true)
         {
            LispObject symbols = current.Symbols;
            LispObject values = current.Values;

            while (symbols is Pair symPair && values is Pair valPair)
            {
               DebugWrite($"  syms: {symPair}");
               DebugWrite($"  vals: {valPair}");

               if (symbol == symPair.Car)
               {
                  DebugWrite($"Found {symbol} -> {valPair.Car}");
                  return (true, valPair.Car);
               }

               symbols = symPair.Cdr;
               values = valPair.Cdr;
            }

            if (symbols != Nil || values != Nil)
               throw new InvalidOperationException(
                  "Symbols and values are not the same length, something has gone seriously wrong! "
                  + $"symbols: {symbols}, values: {values}");

            if (mode == LookupMode.Local || current.IsRoot)
               break;

            current = (Env)current.Parent;
         }

         DebugWrite("Did not find it!");

         return (false, Nil);
      }

      //================================================================================================================
      private void ThrowUnlessSymbolIsSettable(Symbol symbol)
      {
         if (symbol.IsKeyword)
            throw new ArgumentException($"Cannot set a {this}.");
      }

      //================================================================================================================
      public void Set(LookupMode mode, Symbol symbol, LispObject value)
      {
         ThrowUnlessSymbolIsSettable(symbol);

         Env current = mode == LookupMode.Global ? GetRoot() : this;

         while (true)
         {
            var symbols = current.Symbols;
            var values = current.Values;

            while (symbols is Pair symbolsPair && values is Pair valuesPair)
            {
               if (Equals(symbol, symbolsPair.Car))
               {
                  // Update existing symbol value
                  ((Pair)values).Car = value;
                  DebugWrite($"Updated {symbol} with value {value}.");
                  return;
               }

               symbols = symbolsPair.Cdr;
               values = valuesPair.Cdr;
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
      private void Add(Symbol symbol, LispObject value)
      {
         ThrowUnlessSymbolIsSettable(symbol);

         Symbols = Cons(symbol, Symbols);
         Values = Cons(value, Values);

         DebugWrite($"Added {symbol} with value {value}.");
      }

      //================================================================================================================
   }
   //===================================================================================================================
}
