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
   // Ae's static fields
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
   public static List<LispToken> Tokenize(string input) => new LispTokenizer(input).ReadAll();
   public static LispObject Read(string input) => ParseSExp.ParseOrThrow(Tokenize(input));
   public static LispObject Eval(Env env, string input) => Read(input).Eval(env);

   //====================================================================================================================
   public static bool ConsDebugWrite { get; set; } = false;

   public static LispObject Cons(LispObject car, LispObject cdr)
   {
      if (ConsDebugWrite)
         WriteLine($"Cons({car}, {cdr})");
      
      return (LispObject)new Pair(car, cdr);
   }

   //====================================================================================================================
   public static LispObject Truthiness(bool val) => val ? True : Nil;

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
