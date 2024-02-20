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

         if (symbols.IsImproperList)
            throw new ArgumentException("Symbols must be a proper list");

         if (values.IsImproperList)
            throw new ArgumentException("Values must be a proper list");

         Parent = parent;
         Symbols = symbols;
         Values = values;
      }

      //================================================================================================================
      public Env(LispObject parent)
      {
         if (!((parent is Env) || parent.IsNil))
            throw new ArgumentException("Parent must be an Env or Nil");

         Parent = parent;
         Symbols = Nil;
         Values = Nil;
      }

      //================================================================================================================
      // Instance methods
      //================================================================================================================
      public override string ToPrincString() => ToString();
      public bool IsRoot => Parent == Nil;

      //================================================================================================================
      public Env Spawn()
         => new Env(this, Nil, Nil);

      //================================================================================================================
      public Env Spawn(LispObject symbols, LispObject values)
         => new Env(this, symbols, values);
      
      //================================================================================================================
      protected override string? StringRepresentation
      {
         get
         {
            var parentStr = IsRoot ? $"{Parent.ToPrincString()}" : $"#{Parent.Id}";

            return $"{parentStr}, {Symbols.ToPrincString()}, {Values.ToPrincString()}";
         }
      }

      //================================================================================================================
      private Env GetRoot()
      {
         var current = this;

         while (!current.IsRoot)
            current = (Env)current.Parent;

         return current;
      }

      //================================================================================================================
      private void Add(Symbol symbol, LispObject value)
      {
         ThrowIfSymbolIsSelfEvaluating(symbol);

         Symbols = Cons(symbol, Symbols);
         Values = Cons(value, Values);

         DebugWrite($"Added {symbol} with value {value}.");
      }

      //================================================================================================================
      public (bool Found, LispObject PairOrNil) Lookup(LookupMode mode, LispObject symbol)
      {
         if (symbol is not Symbol)
            throw new ArgumentException($"{nameof(symbol)} must be a Symbol, got: {symbol}.");

         return Lookup(mode, (Symbol)symbol);
      }

      //================================================================================================================
      public (bool Found, LispObject PairOrNil) Lookup(LookupMode mode, Symbol symbol)
      {
         if (symbol.IsKeyword || symbol == Nil || symbol == True)
            return (true, symbol);

         (bool found, LispObject pairOrNil) = LookupPair(mode, symbol);

         if (!found)
            return (false, Nil);

         if (pairOrNil is not Pair && pairOrNil != Nil)
            throw new InvalidOperationException($"Expected a pair or nil, got ({found}, {pairOrNil}).");

         if (pairOrNil == Nil)
            return (found, Nil);

         return (true, ((Pair)pairOrNil).Car);
      }

      //================================================================================================================
      private (bool Found, LispObject PairOrNil) LookupPair(LookupMode mode, Symbol symbol)
      {
         DebugWrite($"\nLooking up {symbol} in {this}...");

         Env current = mode == LookupMode.Global ? GetRoot() : this;

         while (true)
         {
            LispObject symbols = current.Symbols;
            LispObject values = current.Values;

            while (symbols is Pair symPair && values is Pair valPair)
            {
               DebugWrite($"  symbs: {symPair}");
               DebugWrite($"   vals: {valPair}");

               if (symbol == symPair.Car)
               {
                  DebugWrite($"Found {symbol} -> {valPair.Car}");
                  return (true, valPair);
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
      private void ThrowIfSymbolIsSelfEvaluating(Symbol symbol)
      {
         if (symbol.IsSelfEvaluating)
            throw new ArgumentException($"Cannot set self-evaluating symbol {this}.");
      }


      //================================================================================================================
      public void Set(LookupMode mode, LispObject symbol, LispObject value)
      {
         if (symbol is not Symbol)
            throw new ArgumentException($"{nameof(symbol)} must be a Symbol, got: {symbol}.");

         Set(mode, (Symbol)symbol, value);
      }

      //================================================================================================================
      public void Set(LookupMode mode, Symbol symbol, LispObject value)
      {
         // Self-evaluating symbols (nil, t and keywords) cannot be set.
         ThrowIfSymbolIsSelfEvaluating(symbol);

         Env current = mode == LookupMode.Global ? GetRoot() : this;

         var (found, pairOrNil) = current.LookupPair(mode, symbol);

         if (found)
            ((Pair)pairOrNil).Car = value;
         else if (mode == LookupMode.Nearest)
            Add(symbol, value);
         else
            current.Add(symbol, value);
      }

      //================================================================================================================
   }
   //===================================================================================================================
}
