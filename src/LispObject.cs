namespace AeLisp
{
  public static class Object
  {
    public abstract class LispObject
    {
      public LispObject? Properties { get; set; } = null;
    }

    public class LispString : LispObject
    {
      public string Value { get; }

      public LispString(string value) => Value = value;
    }

    public class LispError : LispObject
    {
      public string Value { get; }

      public LispError(string value) => Value = value;
    }

    public class LispChar : LispObject
    {
      public char Value { get; }

      public LispChar(char value) => Value = value;
    }

    public class LispInteger : LispObject
    {
      public int Value { get; }

      public LispInteger(int value) => Value = value;
    }

    public class LispFloat : LispObject
    {
      public double Value { get; }

      public LispFloat(double value) => Value = value;
    }

    public class LispRational : LispObject
    {
      public int Numerator { get; }
      public int Denominator { get; }

      public LispRational(int numerator, int denominator)
      {
        Numerator = numerator;
        Denominator = denominator;
      }
    }

    public class LispCons : LispObject
    {
      public LispObject Car { get; }
      public LispObject Cdr { get; }

      public LispCons(LispObject car, LispObject cdr)
      {
        Car = car;
        Cdr = cdr;
      }
    }

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
    }

    public delegate LispObject LispCoreFunc(LispObject arg1, LispObject arg2, int arg3);

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
    }

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
    }

    public abstract class LispLambda : LispUserFunction
    {
      public LispLambda(LispObject parameters, LispObject body, LispObject env) : base(parameters, body, env) { }
    }

    public abstract class LispMacro : LispUserFunction
    {
      public LispMacro(LispObject parameters, LispObject body, LispObject env) : base(parameters, body, env) { }
    }

    public class LispSymbol : LispObject
    {
      public string Value { get; }

      public LispSymbol(string value) => Value = value;
    }

    public static readonly LispSymbol Nil = new LispSymbol("nil");
    public static LispObject SymbolsList = Nil;

  }
}
