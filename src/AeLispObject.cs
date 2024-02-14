using System.Collections;
using System.Reflection;
using System.Text;

public static partial class Ae
{
  //================================================================================================
  // Base object abstract class
  //================================================================================================
  public abstract class Object
  {
    public Object Properties { get; set; } = Nil;
    public abstract override string ToString();
    public abstract string Write();
    protected string TypeName => GetType().Name;
  }

  //================================================================================================
  // Core function delegate
  //================================================================================================
  public delegate Object CoreFunc(Object arg1, Object arg2, int arg3); // ???

  //================================================================================================
  // ObjectWithStringValue abstract class
  //================================================================================================
  public abstract class ObjectWithStringValue : Object
  {
    public string Value { get; }
    public ObjectWithStringValue(string value) => Value = value;
    public override string ToString() => $"{TypeName}(\"{Value}\")";
  }

  //================================================================================================
  // Symbol class
  //================================================================================================
  public class Symbol : ObjectWithStringValue
  {
    public Symbol(string value) : base(value)
    {
      if (string.IsNullOrEmpty(Value))
        throw new ArgumentException("Value cannot be null or empty", nameof(Value));
    }

    public override string Write() => Value;
  }

  //================================================================================================
  // String class
  //================================================================================================
  public class String : ObjectWithStringValue
  {
    public String(string value) : base(value) { }
    public override string Write() => $"\"{Value}\"";
  }

  //================================================================================================
  // Error class
  //================================================================================================

  public class Error : ObjectWithStringValue
  {
    public Error(string value) : base(value) { }
    public override string Write() => ToString();
  }

  //================================================================================================
  // Char class
  //================================================================================================

  public class Char : Object
  {
    public char Value { get; }
    public Char(char value) => Value = value;
    public override string ToString() => $"{TypeName}(\'{Value}\')";
    public override string Write() => $"{Value}";
  }

  //================================================================================================
  // Integer class
  //================================================================================================
  public class Integer : Object
  {
    public int Value { get; }
    public Integer(int value) => Value = value;
    public override string ToString() => $"{TypeName}({Value})";
    public override string Write() => $"{Value}";
  }

  //================================================================================================
  // Float class
  //================================================================================================
  public class Float : Object
  {
    public double Value { get; }
    public Float(double value) => Value = value;
    public override string ToString() => $"{TypeName}({Value})";
    public override string Write() => $"{Value}";
  }

  //================================================================================================
  // Rational class
  //================================================================================================
  public class Rational : Object
  {
    public int Numerator { get; }
    public int Denominator { get; }

    public Rational(int numerator, int denominator)
    {
      Numerator = numerator;
      Denominator = denominator;
    }

    public override string ToString() => $"{TypeName}({Write()})";
    public override string Write() => $"{Numerator}/{Denominator}";
  }

  //================================================================================================
  // Environment frame class
  //================================================================================================
  public class Env : Object
  {
    public Object Parent { get; }
    public Pair Symbols { get; }
    public Pair Values { get; }

    public Env(Object parent, Pair symbols, Pair values)
    {
      Parent = parent;
      Symbols = symbols;
      Values = values;
    }

    public override string ToString() => $"{TypeName}({Parent})";
    public override string Write() => ToString();
  }

  //================================================================================================
  // Core function class
  //================================================================================================
  public class CoreFunction : Object
  {
    public string Name { get; }
    public bool Special { get; }
    public CoreFunc Function { get; }

    public CoreFunction(string name, bool special, CoreFunc fun)
    {
      Name = name;
      Special = special;
      Function = fun;
    }

    public override string ToString() => $"{TypeName}(\"{Name}\")";
    public override string Write() => ToString();
  }

  //================================================================================================
  // User function class
  //================================================================================================
  public abstract class UserFunction : Object
  {
    public Object Parameters { get; }
    public Object Body { get; }
    public Object Env { get; }

    private int Id { get; }
    private static int NextId { get; set; }

    public UserFunction(Object parameters, Object body, Object env)
    {
      Parameters = parameters;
      Body = body;
      Env = env;
      Id = NextId;
      NextId++;
    }

    public override string ToString() => $"{TypeName}({Id})";
    public override string Write() => ToString();
  }

  //================================================================================================
  // Lambda class
  //================================================================================================
  public class Lambda : UserFunction
  {
    public Lambda(Object parameters, Object body, Object env) : base(parameters, body, env) { }
  }

  //================================================================================================
  // Macro class
  //================================================================================================
  public class Macro : UserFunction
  {
    public Macro(Object parameters, Object body, Object env) : base(parameters, body, env) { }
  }

  //================================================================================================
  // static variables
  //================================================================================================
  public static readonly Symbol Nil = new Symbol("nil");
  public static readonly Symbol True = new Symbol("t");
  public static Object SymbolsList = Nil;

  //================================================================================================
  // static constructor
  //================================================================================================
  static Ae()
  {
    Nil.Properties = Nil;
    True.Properties = Nil;
  }

  //================================================================================================
  // Pair class
  //================================================================================================

  public class Pair : Object, IEnumerable<Object>
  {
    public Object Car { get; }
    public Object Cdr { get; }

    public Pair(Object car, Object cdr)
    {
      Car = car;
      Cdr = cdr;
    }

    public override string ToString() => $"{TypeName}({Car}, {Cdr})";

    public override string Write()
    {
      var sb = new StringBuilder();

      sb.Append("(");

      Object current = this;

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

    public IEnumerator<Object> GetEnumerator()
    {
      Object current = this;

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

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
  }

  //================================================================================================
  // static methods
  //================================================================================================
  public static string Write(Object obj) => obj.Write();
  public static Pair Cons(Object car, Object cdr) => new Pair(car, cdr);

  public static bool ProperListP(Object obj)
  {
    Object current = obj;

    while (current is Pair cons)
    {
      if (cons.Cdr == Ae.Nil)
        return true;

      current = cons.Cdr;
    }

    return current == Ae.Nil;
  }

  public static bool ListP(Object obj) => obj == Nil || obj is Pair;
  public static bool AtomP(Object obj) => !ListP(obj);

  public static Object Intern(ref Ae.Object symbolsList, string key)
  {
    if (!ListP(symbolsList))
      throw new InvalidOperationException($"{nameof(symbolsList)} is not a list");

    var current = symbolsList;

    while (current != Nil)
      if (current is Pair pair)
      {
        if (pair.Car is Symbol symbol)
        {
          if (symbol.Value == key)
            return symbol;
        }
        else
          throw new InvalidOperationException($"Found non-symbol {pair.Car} in symbols list.");

        current = pair.Cdr;
      }
      else
        throw new InvalidOperationException($"{nameof(symbolsList)} is not a proper list");

    var newSymbol = new Symbol(key);

    symbolsList = Cons(newSymbol, symbolsList);

    return newSymbol;
  }
}
