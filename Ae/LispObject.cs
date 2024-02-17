using System.Collections;
using System.Reflection;
using System.Text;

//======================================================================================================================
static partial class Ae
{
  //====================================================================================================================
  // Base object abstract class
  //====================================================================================================================
  public abstract class LispObject
  {
    public LispObject Properties { get; set; } = Nil;
    public abstract override string ToString();
    public abstract string Write();
    protected string TypeName => GetType().Name;
  }

  //====================================================================================================================
  // LispObjectWithStringValue abstract class
  //====================================================================================================================
  public abstract class LispObjectWithStringValue : LispObject
  {
    public string Value { get; }
    public LispObjectWithStringValue(string value) => Value = value;
    public override string ToString() => $"{TypeName}(\"{Value.EscapeChars()}\")";
  }

  //====================================================================================================================
  // Symbol class
  //====================================================================================================================
  public class Symbol : LispObjectWithStringValue
  {
    //==================================================================================================================
    // Constructor
    //==================================================================================================================
    public Symbol(string value) : base(value)
    {
      if (string.IsNullOrEmpty(Value))
        throw new ArgumentException($"{nameof(Value)} cannot be null or empty", nameof(Value));
    }

    //==================================================================================================================
    // Public methods
    //==================================================================================================================
    public override string Write() => Value;
    public override string ToString() => $"{TypeName}({Value})";
  }

  //====================================================================================================================
  // String class
  //====================================================================================================================
  public class String : LispObjectWithStringValue
  {
    public String(string value) : base(value) { }
    public override string Write() => $"\"{Value}\"";
  }

  //====================================================================================================================
  // Error class
  //====================================================================================================================

  public class Error : LispObjectWithStringValue
  {
    public Error(string value) : base(value) { }
    public override string Write() => ToString();
  }

  //====================================================================================================================
  // Char class
  //====================================================================================================================
  public class Char : LispObject
  {
    public char Value { get; }
    public Char(char value) => Value = value;
    public override string ToString() => $"{TypeName}(\'{Value}\')";
    public override string Write() => $"{Value}";
  }

  //====================================================================================================================
  // Integer class
  //====================================================================================================================
  public class Integer : LispObject
  {
    public int Value { get; }
    public Integer(int value) => Value = value;
    public override string ToString() => $"{TypeName}({Value})";
    public override string Write() => $"{Value}";
  }

  //====================================================================================================================
  // Float class
  //====================================================================================================================
  public class Float : LispObject
  {
    public double Value { get; }
    public Float(double value) => Value = value;
    public override string ToString() => $"{TypeName}({Value})";
    public override string Write() => $"{Value}";
  }

  //====================================================================================================================
  // Rational class
  //====================================================================================================================
  public class Rational : LispObject
  {
    //==================================================================================================================
    // Public properties
    //==================================================================================================================
    public int Numerator { get; }
    public int Denominator { get; }

    //===================================================================================================================
    // Constructor
    //================================================================================================================== 
    public Rational(int numerator, int denominator)
    {
      Numerator = numerator;
      Denominator = denominator;
    }

    //==================================================================================================================
    // Instance methods
    //==================================================================================================================
    public override string ToString() => $"{TypeName}({Write()})";
    public override string Write() => $"{Numerator}/{Denominator}";
  }

  //====================================================================================================================
  // Environment frame class
  //====================================================================================================================
  public class Env : LispObject
  {
    //==================================================================================================================
    // Public properties
    //==================================================================================================================
    public LispObject Parent { get; }
    public Pair Symbols { get; }
    public Pair Values { get; }

    //==================================================================================================================
    // Constructor
    //==================================================================================================================
    public Env(LispObject parent, Pair symbols, Pair values)
    {
      Parent = parent;
      Symbols = symbols;
      Values = values;
    }

    //==================================================================================================================
    // Instance methods
    //==================================================================================================================
    public override string ToString() => $"{TypeName}({Parent})";
    public override string Write() => ToString();
  }

  //====================================================================================================================
  // Core function delegate
  //====================================================================================================================
  public delegate LispObject CoreFunc(LispObject arg1, LispObject arg2, int arg3); // ???

  //====================================================================================================================
  // Core function class
  //====================================================================================================================
  public class CoreFunction : LispObject
  {
    //==================================================================================================================
    // Public properties
    //==================================================================================================================
    public string Name { get; }
    public bool Special { get; }
    public CoreFunc Function { get; }

    //==================================================================================================================
    // Constructor
    //==================================================================================================================
    public CoreFunction(string name, bool special, CoreFunc fun)
    {
      if (string.IsNullOrEmpty(name))
        throw new ArgumentException($"{nameof(name)} cannot be null or empty", nameof(name));

      Name = name;
      Special = special;
      Function = fun;
    }

    //==================================================================================================================
    // Instance methods
    //==================================================================================================================
    public override string ToString() => $"{TypeName}(\"{Name}\")";
    public override string Write() => ToString();
  }

  //====================================================================================================================
  // User function class
  //====================================================================================================================
  public abstract class UserFunction : LispObject
  {
    //==================================================================================================================
    // Public properties
    //==================================================================================================================
    public LispObject Parameters { get; }
    public LispObject Body { get; }
    public LispObject Env { get; }

    //==================================================================================================================
    // Private properties
    //==================================================================================================================
    private int Id { get; }

    //==================================================================================================================
    // Private static properties
    //==================================================================================================================
    private static int NextId { get; set; } = 0;

    //==================================================================================================================
    // Constructor
    //==================================================================================================================
    public UserFunction(LispObject parameters, LispObject body, LispObject env)
    {
      Parameters = parameters;
      Body = body;
      Env = env;
      Id = NextId;
      NextId++;
    }

    //==================================================================================================================
    // Instance methods
    //==================================================================================================================
    public override string ToString() => $"{TypeName}({Id})";
    public override string Write() => ToString();
  }

  //====================================================================================================================
  // Lambda class
  //====================================================================================================================
  public class Lambda : UserFunction
  {
    public Lambda(LispObject parameters, LispObject body, LispObject env) : base(parameters, body, env) { }
  }

  //====================================================================================================================
  // Macro class
  //====================================================================================================================
  public class Macro : UserFunction
  {
    public Macro(LispObject parameters, LispObject body, LispObject env) : base(parameters, body, env) { }
  }

  //====================================================================================================================
  // Pair class
  //====================================================================================================================
  public class Pair : LispObject, IEnumerable<LispObject>
  {
    //==================================================================================================================
    // Public properties
    //==================================================================================================================
    public LispObject Car { get; }
    public LispObject Cdr { get; }

    //==================================================================================================================
    // Constructor
    //==================================================================================================================
    public Pair(LispObject car, LispObject cdr)
    {
      Car = car;
      Cdr = cdr;
    }

    //==================================================================================================================
    // Instance methods
    //==================================================================================================================
    public override string ToString() => $"{TypeName}({Car}, {Cdr})";

    //==================================================================================================================
    public override string Write()
    {
      var sb = new StringBuilder();

      sb.Append("(");

      LispObject current = this;

      while (current is Pair currentCons)
      {
        sb.Append(currentCons.Car.Write());

        if (currentCons.Cdr is Pair)
        {
          sb.Append(" ");
          current = currentCons.Cdr;
        }
        else if (currentCons.Cdr != Nil)
        {

          sb.Append(" . ");
          sb.Append(currentCons.Cdr.Write());

          break;
        }
        else
        {
          break;
        }
      }

      sb.Append(")");

      return sb.ToString();
    }

    //==================================================================================================================
    public IEnumerator<LispObject> GetEnumerator()
    {
      LispObject current = this;

      while (current != Nil)
        if (current is Pair pair)
        {
          yield return pair.Car;

          current = pair.Cdr;
        }
        else
        {
          yield return current;
          yield break;

          // throw new InvalidOperationException("Enumerated improper list!");
        }
    }

    //==================================================================================================================
    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
  }

  //====================================================================================================================
  // Ae's static variables
  //====================================================================================================================
  public static readonly Symbol Nil = new Symbol("nil");
  public static readonly Symbol True = new Symbol("t");
  public static LispObject SymbolsList = Nil;

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
  public static string Write(LispObject obj) => obj.Write();
  public static Pair Cons(LispObject car, LispObject cdr) => new Pair(car, cdr);

  //====================================================================================================================
  public static bool ProperListP(LispObject obj)
  {
    LispObject current = obj;

    while (current is Pair cons)
    {
      if (cons.Cdr == Ae.Nil)
        return true;

      current = cons.Cdr;
    }

    return current == Ae.Nil;
  }

  //====================================================================================================================
  public static bool ListP(LispObject obj) => obj == Nil || obj is Pair;
  public static bool AtomP(LispObject obj) => !ListP(obj);

  //====================================================================================================================
  public static LispObject Intern(string symbol) => Intern2(symbol, ref SymbolsList);
  //====================================================================================================================
  public static LispObject Intern2(string symbolName, ref Ae.LispObject symbolsList)
  {
    if (!ListP(symbolsList))
      throw new InvalidOperationException($"{nameof(symbolsList)} is not a list");

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
