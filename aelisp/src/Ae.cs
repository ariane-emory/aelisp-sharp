using static System.Console;
using System.Collections;
using System.Reflection;
using System.Text;
using static Ae.LispParsers;
using Pidgin;
using static Pidgin.Parser;
using static Pidgin.Parser<Ae.LispToken>;

//======================================================================================================================
static partial class Ae
{
   //====================================================================================================================
   // Static fields
   //====================================================================================================================
   public static readonly LispObject Nil = (LispObject)new Symbol("nil");
   private static LispObject SymbolsList = Nil;
   public static readonly LispObject True = Intern("t");

   //====================================================================================================================
   // Static properties
   //====================================================================================================================
   public static Env Root { get; } = new Env(Nil);

   //====================================================================================================================
   // Ae's static constructor
   //====================================================================================================================
   static Ae()
   {
      Nil.Properties = Nil;
      True.Properties = Nil;
   }

   //====================================================================================================================
   // Static methods 
   //====================================================================================================================
   public static List<LispToken> Tokenize(string input) => new LispTokenizer(input, IsWhitespaceOrCommentToken).ReadAll();
   public static LispObject Eval(Env env, string input, bool progn = true) => Read(input, progn).Eval(env);
   public static LispObject Read(string input, bool progn) => (progn ? ParseProgram : ParseSExp).ParseOrThrow(Tokenize(input));
   public static LispObject Cons(LispObject car, LispObject cdr) =>  (LispObject)new Pair(car, cdr);
   public static LispObject Truthiness(bool val) => val ? True : Nil;

   //====================================================================================================================
   public static LispObject Intern(string symbol) => Intern(symbol, ref SymbolsList);
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
