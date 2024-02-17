using System.Collections;
using System.Reflection;
using System.Text;

//======================================================================================================================
static partial class Ae
{
   //====================================================================================================================
   // Ae's static variables
   //====================================================================================================================
   public static readonly LispObject Nil = (LispObject)new Symbol("nil");
   public static LispObject SymbolsList = Nil;
   public static readonly LispObject True = Intern("t");

   //====================================================================================================================
   // Ae's static constructor
   //====================================================================================================================
   static Ae()
   {
      Nil.Properties = Nil;
      True.Properties = Nil;
   }

   //====================================================================================================================
   // Ae's static methods
   //====================================================================================================================
   public static LispObject Cons(LispObject car, LispObject cdr) => (LispObject)new Pair(car, cdr);

   //====================================================================================================================
   public static LispObject Intern(string symbol) => Intern(symbol, ref SymbolsList);

   //====================================================================================================================
   public static LispObject Intern(string symbolName, ref Ae.LispObject symbolsList)
   {
      if (!symbolsList.IsList)
         throw new InvalidOperationException($"{nameof(symbolsList)} is not a list");

      if (symbolName == "nil")
         return Nil;

      var current = symbolsList;

      while (current != Nil)
         if (current is Pair pair)
         {
            if (pair.Car is Symbol symbol)
            {
               if (symbol.Value == symbolName)
                  return symbol;
            }
            else
               throw new InvalidOperationException($"Found non-symbol {pair.Car} in symbols list.");

            current = pair.Cdr;
         }
         else
            throw new InvalidOperationException($"{nameof(symbolsList)} is not a proper list");

      var newSymbol = new Symbol(symbolName);

      symbolsList = Cons(newSymbol, symbolsList);

      return newSymbol;
   }

   //====================================================================================================================
}
