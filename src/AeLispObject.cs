using System.Collections;
using System.Reflection;

public static partial class Ae
{
  //================================================================================================
  // Base object abstract class
  //================================================================================================

  public abstract class LispObject
  {
    public LispObject Properties { get; set; } = Nil;

    public abstract override string ToString();
  }

  //================================================================================================
  // Core function delegate
  //================================================================================================

  public delegate LispObject LispCoreFunc(LispObject arg1, LispObject arg2, int arg3);

  //================================================================================================
  // LispObjectWithStringValue abstract class
  //================================================================================================

  public abstract class LispObjectWithStringValue : LispObject
  {
    public string Value { get; }

    public LispObjectWithStringValue(string value) => Value = value;

    public override string ToString() => $"{GetType().Name}(\"{Value}\")";
  }

  //================================================================================================
  // Symbol class
  //================================================================================================

  public class LispSymbol : LispObjectWithStringValue
  {
    public LispSymbol(string value): base(value)
    {
      if (string.IsNullOrEmpty(Value))
        throw new ArgumentException("Value cannot be null or empty", nameof(Value));
    }
  }

  //================================================================================================
  // String class
  //================================================================================================

  public class LispString : LispObjectWithStringValue
  {
    public LispString(string value) : base(value) { }
  }

  //================================================================================================
  // String class
  //================================================================================================

  public class LispError : LispObjectWithStringValue
  {
    public LispError(string value) : base(value) { }
  }

  //================================================================================================
  // Char class
  //================================================================================================

  public class LispChar : LispObject
  {
    public char Value { get; }

    public LispChar(char value) => Value = value;

    public override string ToString() => $"{GetType().Name}(\"{Value}\")";
  }

  //================================================================================================
  // Integer class
  //================================================================================================

  public class LispInteger : LispObject
  {
    public int Value { get; }

    public LispInteger(int value) => Value = value;

    public override string ToString() => $"{GetType().Name}(\"{Value}\")";
  }

  //================================================================================================
  // Float class
  //================================================================================================

  public class LispFloat : LispObject
  {
    public double Value { get; }

    public LispFloat(double value) => Value = value;

    public override string ToString() => $"{GetType().Name}(\"{Value}\")";
  }

  //================================================================================================
  // Rational class
  //================================================================================================

  public class LispRational : LispObject
  {
    public int Numerator { get; }
    public int Denominator { get; }

    public LispRational(int numerator, int denominator)
    {
      Numerator = numerator;
      Denominator = denominator;
    }

    public override string ToString() => $"{GetType().Name}(\"{Numerator}/{Denominator}\")";
  }

  //================================================================================================
  // Environment frame class
  //================================================================================================

  public class LispEnv : LispObject
  {
    public LispObject Parent { get; }
    public LispCons Symbols { get; }
    public LispCons Values { get; }

    public LispEnv(LispObject parent, LispCons symbols, LispCons values)
    {
      Parent = parent;
      Symbols = symbols;
      Values = values;
    }

    public override string ToString() => $"{GetType().Name}(\"{Parent}\")";
  }

  //================================================================================================
  // Core function class
  //================================================================================================

  public class LispCoreFunction : LispObject
  {
    public string Name { get; }
    public bool Special { get; }
    public LispCoreFunc Function { get; }

    public LispCoreFunction(string name, bool special, LispCoreFunc fun)
    {
      Name = name;
      Special = special;
      Function = fun;
    }

    public override string ToString() => $"{GetType().Name}(\"{Name}\")";
  }

  //================================================================================================
  // User function class
  //================================================================================================

  public abstract class LispUserFunction : LispObject
  {
    public LispObject Parameters { get; }
    public LispObject Body { get; }
    public LispObject Env { get; }

    public LispUserFunction(LispObject parameters, LispObject body, LispObject env)
    {
      Parameters = parameters;
      Body = body;
      Env = env;
    }

    public override string ToString() => $"{GetType().Name}()";
  }

  //================================================================================================
  // Lambda class
  //================================================================================================

  public class LispLambda : LispUserFunction
  {
    public LispLambda(LispObject parameters, LispObject body, LispObject env) : base(parameters, body, env) { }
  }

  //================================================================================================
  // Macro class
  //================================================================================================

  public class LispMacro : LispUserFunction
  {
    public LispMacro(LispObject parameters, LispObject body, LispObject env) : base(parameters, body, env) { }
  }

  //================================================================================================
  // static variables
  //================================================================================================

  public static readonly LispSymbol Nil = new LispSymbol("nil");
  public static readonly LispSymbol True = new LispSymbol("t");
  public static LispObject SymbolsList = Nil;

  //================================================================================================
  // static constructor
  //================================================================================================

  static Ae()
  {
    Nil.Properties = Nil;
    True.Properties = Nil;
  }

  //================================================================================================
  // Cons class
  //================================================================================================

  public class LispCons : LispObject, IEnumerable<LispObject>
  {
    public LispObject Car { get; }
    public LispObject Cdr { get; }

    public LispCons(LispObject car, LispObject cdr)
    {
      Car = car;
      Cdr = cdr;
    }

    public override string ToString() => $"{GetType().Name}({Car}, {Cdr})";

    public IEnumerator<LispObject> GetEnumerator()
    {
      LispObject current = this;

      while (current != Ae.Nil)
        if (current is LispCons cons)
        {
          yield return cons.Car;

          current = cons.Cdr;
        }
        else
        {
          yield break;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
  }

  //================================================================================================
  // static methods
  //================================================================================================

  public static LispCons Cons(LispObject car, LispObject cdr) => new LispCons(car, cdr);

  public static bool ProperListP(LispObject obj)
  {
    LispObject current = obj;

    while (current is LispCons cons)
    {
      if (cons.Cdr == Ae.Nil)
        return true;

      current = cons.Cdr;
    }

    return current == Ae.Nil;
  }
}
