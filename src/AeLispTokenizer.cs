using static System.Console;

public static partial class Ae
{
  public enum TokenType
  {
    At,
    Backtick,
    CStyleChar,
    Comma,
    CommaAt,
    Dollar,
    Dot,
    Float,
    Garbage,
    Integer,
    LParen,
    LineComment,
    LispStyleChar,
    NewLine,
    Nil,
    Quote,
    RParen,
    Rational,
    String,
    Symbol,
    Whitespace,
  };

  public record struct AeLispTokenizerState(int Line = 0, int Column = 0);

  public record struct PositionedToken<TTokenType>(TTokenType TokenType, string Text, int Line, int Column) // : Token<TTokenType>(tokenType, Text)
  {
    public override string ToString() => $"{TokenType} [{Text}] @ {Line},{Column}";
  }

  public class Tokenizer : StringTokenizer<TokenType, PositionedToken<TokenType>, AeLispTokenizerState>
  {
    //==================================================================================================================
    // Private constructor
    //==================================================================================================================
    private Tokenizer() : base(createToken: (tokenType, text) => new PositionedToken<TokenType>(tokenType, text, 0, 0),
                               createTokenizerStateFun: () => new AeLispTokenizerState())
    {
      foreach (var (tokenType, discrete, fun, pattern) in Tokens)
        Add(tokenType,
            discrete ? (pattern + FollowedByTokenBarrierOrEOF) : (pattern),
            fun);
    }

    //==================================================================================================================
    // Get the instance
    //==================================================================================================================
    public static Tokenizer Get()
    {
      if (_instance is null)
        _instance = new Tokenizer();

      return _instance;
    }

    //==================================================================================================================
    // Private constants
    //==================================================================================================================
    private static readonly List<(TokenType Type,
                                  bool Discrete,
                                  ProcesTokenFun? Process,
                                  string Pattern)> Tokens =
      new List<(TokenType Type,
                bool Discrete,
                ProcesTokenFun? Process,
                string Pattern)>
      {
        (Type: TokenType.NewLine,       Discrete: false, Process: CountLine,            Pattern: @"\r?\n"),
        (Type: TokenType.Whitespace,    Discrete: false, Process: CountColumns,         Pattern: @"[ \t\f\v]+"),
        (Type: TokenType.LParen,        Discrete: false, Process: CountColumns,         Pattern: @"\("),
        (Type: TokenType.RParen,        Discrete: true,  Process: CountColumns,         Pattern: @"\)"),
        (Type: TokenType.Nil,           Discrete: true,  Process: CountColumns,         Pattern: @"nil"),
        (Type: TokenType.Dot,           Discrete: true,  Process: CountColumns,         Pattern: @"\."),
        (Type: TokenType.CStyleChar,    Discrete: true,  Process: TrimAndUnescape,      Pattern: @"'[^']'"),
        (Type: TokenType.CStyleChar,    Discrete: true,  Process: TrimAndUnescape,      Pattern: @"'\\.'"),
        (Type: TokenType.Float,         Discrete: true,  Process: CountColumns,         Pattern: Float),
        (Type: TokenType.Rational,      Discrete: true,  Process: CountColumns,         Pattern: Rational),
        (Type: TokenType.Integer,       Discrete: true,  Process: CountColumns,         Pattern: MaybeSigned + DigitSeparatedInteger),
        (Type: TokenType.String,        Discrete: true,  Process: TrimAndUnescape,      Pattern: @"\""(\\\""|[^\""])*\"""),
        (Type: TokenType.Quote,         Discrete: false, Process: CountColumns,         Pattern: @"'"),
        (Type: TokenType.Backtick,      Discrete: false, Process: CountColumns,         Pattern: @"`"),
        (Type: TokenType.CommaAt,       Discrete: false, Process: CountColumns,         Pattern: @",@"),
        (Type: TokenType.Comma,         Discrete: false, Process: CountColumns,         Pattern: @","),
        (Type: TokenType.At,            Discrete: false, Process: CountColumns,         Pattern: @"@"),
        (Type: TokenType.Dollar,        Discrete: false, Process: CountColumns,         Pattern: @"\$"),
        (Type: TokenType.Symbol,        Discrete: true,  Process: CountColumns,         Pattern: Integer + @"?" + MathOp),
        (Type: TokenType.Symbol,        Discrete: true,  Process: CountColumns,         Pattern: MathOp + Integer),
        (Type: TokenType.Symbol,        Discrete: true,  Process: CountColumns,         Pattern: @"[\?]{3}"),
        (Type: TokenType.Symbol,        Discrete: true,  Process: CountColumns,         Pattern: SymBody),
        (Type: TokenType.Symbol,        Discrete: true,  Process: CountColumns,         Pattern: @"<"  + SymBody + @">"),
        (Type: TokenType.Symbol,        Discrete: true,  Process: CountColumns,         Pattern: @"\*" + SymBody + @"\*"),
        (Type: TokenType.Symbol,        Discrete: true,  Process: CountColumns,         Pattern: @"𝑎|𝑏|𝑐|𝑑|𝑒|𝑓|𝑚|𝑛|𝑜|𝑝|𝑞|𝑟|𝑠|𝑡|𝑢|𝑣|𝑤|𝑥|𝑦|𝑧"),
        (Type: TokenType.Symbol,        Discrete: true,  Process: CountColumns,         Pattern: @"(?:_)|(?:=)|(?:==)|(?:!=)|(?:>=?)|(?:<=?)"),
        (Type: TokenType.Symbol,        Discrete: true,  Process: CountColumns,         Pattern: @"¬|λ\??|∧|∨|⊤|⊥|≤|≥|×|÷|Ø|∈|∉|≠|!|∃|∄|∀|≔|\||&|~|\^|\?"),
        (Type: TokenType.LispStyleChar, Discrete: true,  Process: TrimFirstAndUnescape, Pattern: @"\?\\."),
        (Type: TokenType.LispStyleChar, Discrete: true,  Process: TrimFirstAndUnescape, Pattern: @"\?."),
        (Type: TokenType.LineComment,   Discrete: false, Process: TrimFirst,            Pattern: @";.+"),
        (Type: TokenType.Garbage,       Discrete: false, Process: CountColumns,         Pattern: @".+"),
      };

    private const string DigitSeparatedInteger = @"(?:" + ZeroPaddedInteger + @"(?:," + ZeroPaddedInteger + @")*)";
    private const string Float = @"(?:" + MaybeSigned + DigitSeparatedInteger + @"?\.\d+)";
    private const string FollowedByTokenBarrierOrEOF = @"(?=\s|\)|$)";
    private const string Integer = @"(?:(?:[1-9]\d*)|0)";
    private const string MathOp = @"(?:[+\-%/\*\^]|" + ShiftLeft + @"|" + ShiftRight + @")";
    private const string MaybePunctuationSuffix = @"(?:[\?\!\*\+]|\!\!)?";
    private const string MaybeSigned = @"(?:" + Sign + @")?";
    private const string MaybeSymLeader = @"(?:[\-+:&!%#])?";
    private const string MaybeZeroPadding = @"(?:0+)?";
    private const string Rational = MaybeSigned + DigitSeparatedInteger + @"\/" + DigitSeparatedInteger;
    private const string ShiftLeft = @"(?:\<\<)";
    private const string ShiftRight = @"(?:\>\>)";
    private const string Sign = @"(?:[\-+])";
    private const string SymBody = @"(?:" + MaybeSymLeader + SymFirstWord + @"(?:" + SymSeparator + SymWord + @")*" + MaybePunctuationSuffix + @")";
    private const string SymFirstWord = @"(?:[a-zA-Z]" + SymWordChar + @"*)";
    private const string SymSeparator = @"(?:(?:\-+)|(?:_+)|(?:\:+)|(?:/+))";
    private const string SymWord = @"(?:" + SymWordChar + @"+)";
    private const string SymWordChar = @"[a-zA-Z0-9\'\.]";
    private const string ZeroPaddedInteger = @"(?:" + MaybeZeroPadding + Integer + @")";

    private static readonly List<(string, string)> EscapedChars = new List<(string, string)>
    {
      (@"\a",   "\a"),
      (@"\b",   "\b"),
      (@"\f",   "\f"),
      (@"\n",   "\n"),
      (@"\r",   "\r"),
      (@"\t",   "\t"),
      (@"\v",   "\v"),
      (@"\\",   "\\"),
      (@"\'",   "\'"),
      (@"\""",  "\""),
    };

    private static Tokenizer _instance = new Tokenizer();

    //==================================================================================================================
    // Token callbacks
    //==================================================================================================================
    private static (AeLispTokenizerState, PositionedToken<TokenType>) UnescapeChars((AeLispTokenizerState State, PositionedToken<TokenType> Token) tup)
    {
      var str = tup.Token.Text;

      foreach (var (escaped, unescaped) in EscapedChars)
        str = str.Replace(escaped, unescaped);

      return (tup.State, new PositionedToken<TokenType>(tup.Token.TokenType, str, tup.Token.Line, tup.Token.Column));
    }

    private static (AeLispTokenizerState, PositionedToken<TokenType>) TrimAndUnescape((AeLispTokenizerState State, PositionedToken<TokenType> Token) tup)
    {
      (tup.State, tup.Token) = CountColumns((tup.State, tup.Token));

      return UnescapeChars((tup.State,
                            new PositionedToken<TokenType>(tup.Token.TokenType,
                                                           tup.Token.Text.Substring(1, tup.Token.Text.Length - 2),
                                                           tup.Token.Line,
                                                           tup.Token.Column)));
    }

    private static (AeLispTokenizerState, PositionedToken<TokenType>) TrimFirstAndUnescape((AeLispTokenizerState State, PositionedToken<TokenType> Token) tup)
      => UnescapeChars(TrimFirst(CountColumns(tup)));


    private static (AeLispTokenizerState, PositionedToken<TokenType>) TrimFirst((AeLispTokenizerState State, PositionedToken<TokenType> Token) tup)
    {
      tup = CountColumns(tup);

      return (tup.State, new PositionedToken<TokenType>(tup.Token.TokenType, tup.Token.Text.Substring(1), tup.Token.Line, tup.Token.Column));
    }

    private static (AeLispTokenizerState, PositionedToken<TokenType>) CountLine((AeLispTokenizerState State, PositionedToken<TokenType> Token) tup)
    {
      tup = SetTokenLinesAndColumns(tup);

      tup.State.Line++;
      tup.State.Column = 0;

      return tup;
    }

    private static (AeLispTokenizerState, PositionedToken<TokenType>) CountColumns((AeLispTokenizerState State, PositionedToken<TokenType> Token) tup)
    {
      tup = SetTokenLinesAndColumns(tup);

      tup.State.Column += tup.Token.Text.Length;

      return tup;
    }

    private static (AeLispTokenizerState, PositionedToken<TokenType>) SetTokenLinesAndColumns((AeLispTokenizerState State, PositionedToken<TokenType> Token) tup)
    {
      tup.Token = new PositionedToken<TokenType>(tup.Token.TokenType, tup.Token.Text, tup.State.Line, tup.State.Column);

      return tup;
    }
  }
}
