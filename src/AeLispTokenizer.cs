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

  public class AeLispTokenizerState
  {
    public bool InsideMultilineComment { get; set; }
  }

  public record PositionedToken<TTokenType>(TTokenType TokenType, string Text, int Line, int Column) // : Token<TTokenType>(tokenType, Text)
  {
    public override string ToString() => $"{TokenType} [{Text}] @ {Line},{Column}";
  }

  public class Tokenizer : StringTokenizer<TokenType, PositionedToken<TokenType>, AeLispTokenizerState>
  {
    //==================================================================================================================
    // Token callbacks
    //==================================================================================================================
    // Func<AeLispTokenizerState, PositionedToken<TokenType>, (AeLispTokenizerState, PositionedToken<TokenType>)>? TransformToken,

    private static (AeLispTokenizerState state, PositionedToken<TokenType>) UnescapeChars((AeLispTokenizerState State, PositionedToken<TokenType> Token) tup)
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

    private static (AeLispTokenizerState, PositionedToken<TokenType>) CountLines((AeLispTokenizerState State, PositionedToken<TokenType> Token) tup)
    {
      tup.Token = new PositionedToken<TokenType>(tup.Token.TokenType, tup.Token.Text, Get().Line, Get().Column);

      Get().Line++;
      Get().Column = 0;

      return (tup.State, tup.Token);
    }

    private static (AeLispTokenizerState, PositionedToken<TokenType>) CountColumns((AeLispTokenizerState State, PositionedToken<TokenType> Token) tup)
    {
      // WriteLine($"Begin CountColumns with {Get().Line},{Get().Column} at \"{token.Text}\".");

      tup.Token = new PositionedToken<TokenType>(tup.Token.TokenType, tup.Token.Text, Get().Line, Get().Column);

      Get().Column += tup.Token.Text.Length;

      return (tup.State, tup.Token);
    }

    //==================================================================================================================
    // Private constants.
    //==================================================================================================================

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

    private static readonly List<(TokenType Type,
                                  bool Discrete,
                                  // Func<TTokenizerState, TToken, (TTokenizerState, TToken)>
                                  Func<(AeLispTokenizerState, PositionedToken<TokenType>), (AeLispTokenizerState, PositionedToken<TokenType>)>? TransformToken,
                                  string Pattern)> Tokens =
      new List<(TokenType Type,
                bool Discrete,
                Func<(AeLispTokenizerState, PositionedToken<TokenType>), (AeLispTokenizerState, PositionedToken<TokenType>)>? TransformToken,
                string Pattern)>
      {
        (TokenType.NewLine,       Discrete: false, TransformToken: CountLines,           Pattern: @"\r?\n"),
        (TokenType.Whitespace,    Discrete: false, TransformToken: CountColumns,         Pattern: @"[ \t\f\v]+"),
        (TokenType.LParen,        Discrete: false, TransformToken: CountColumns,         Pattern: @"\("),
        (TokenType.RParen,        Discrete: true,  TransformToken: CountColumns,         Pattern: @"\)"),
        (TokenType.Nil,           Discrete: true,  TransformToken: CountColumns,         Pattern: @"nil"),
        (TokenType.Dot,           Discrete: true,  TransformToken: CountColumns,         Pattern: @"\."),
        (TokenType.CStyleChar,    Discrete: true,  TransformToken: TrimAndUnescape,      Pattern: @"'[^']'"),
        (TokenType.CStyleChar,    Discrete: true,  TransformToken: TrimAndUnescape,      Pattern: @"'\\.'"),
        (TokenType.Float,         Discrete: true,  TransformToken: CountColumns,         Pattern: Float),
        (TokenType.Rational,      Discrete: true,  TransformToken: CountColumns,         Pattern: Rational),
        (TokenType.Integer,       Discrete: true,  TransformToken: CountColumns,         Pattern: MaybeSigned + DigitSeparatedInteger),
        (TokenType.String,        Discrete: true,  TransformToken: TrimAndUnescape,      Pattern: @"\""(\\\""|[^\""])*\"""),
        (TokenType.Quote,         Discrete: false, TransformToken: CountColumns,         Pattern: @"'"),
        (TokenType.Backtick,      Discrete: false, TransformToken: CountColumns,         Pattern: @"`"),
        (TokenType.CommaAt,       Discrete: false, TransformToken: CountColumns,         Pattern: @",@"),
        (TokenType.Comma,         Discrete: false, TransformToken: CountColumns,         Pattern: @","),
        (TokenType.At,            Discrete: false, TransformToken: CountColumns,         Pattern: @"@"),
        (TokenType.Dollar,        Discrete: false, TransformToken: CountColumns,         Pattern: @"\$"),
        (TokenType.Symbol,        Discrete: true,  TransformToken: CountColumns,         Pattern: Integer + @"?" + MathOp),
        (TokenType.Symbol,        Discrete: true,  TransformToken: CountColumns,         Pattern: MathOp + Integer),
        (TokenType.Symbol,        Discrete: true,  TransformToken: CountColumns,         Pattern: @"[\?]{3}"),
        (TokenType.Symbol,        Discrete: true,  TransformToken: CountColumns,         Pattern: SymBody),
        (TokenType.Symbol,        Discrete: true,  TransformToken: CountColumns,         Pattern: @"<"  + SymBody + @">"),
        (TokenType.Symbol,        Discrete: true,  TransformToken: CountColumns,         Pattern: @"\*" + SymBody + @"\*"),
        (TokenType.Symbol,        Discrete: true,  TransformToken: CountColumns,         Pattern: @"𝑎|𝑏|𝑐|𝑑|𝑒|𝑓|𝑚|𝑛|𝑜|𝑝|𝑞|𝑟|𝑠|𝑡|𝑢|𝑣|𝑤|𝑥|𝑦|𝑧"),
        (TokenType.Symbol,        Discrete: true,  TransformToken: CountColumns,         Pattern: @"(?:_)|(?:=)|(?:==)|(?:!=)|(?:>=?)|(?:<=?)"),
        (TokenType.Symbol,        Discrete: true,  TransformToken: CountColumns,         Pattern: @"¬|λ\??|∧|∨|⊤|⊥|≤|≥|×|÷|Ø|∈|∉|≠|!|∃|∄|∀|≔|\||&|~|\^|\?"),
        (TokenType.LispStyleChar, Discrete: true,  TransformToken: TrimFirstAndUnescape, Pattern: @"\?\\."),
        (TokenType.LispStyleChar, Discrete: true,  TransformToken: TrimFirstAndUnescape, Pattern: @"\?."),
        (TokenType.LineComment,   Discrete: false, TransformToken: TrimFirst,            Pattern: @";.+"),
        (TokenType.Garbage,       Discrete: false, TransformToken: CountColumns,         Pattern: @".+"),
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

    private static Tokenizer _tokenizer = new Tokenizer();

    //==================================================================================================================
    // Get the instance
    //==================================================================================================================
    public static Tokenizer Get()
    {
      if (_tokenizer is null)
        _tokenizer = new Tokenizer();

      return _tokenizer;
    }

    //==================================================================================================================
    // Properties
    //==================================================================================================================
    public int Line { get; private set; } = 0;
    public int Column { get; private set; } = 0;

    //==================================================================================================================
    // Private methods
    //==================================================================================================================
    protected override void Restart()
    {
      Line = 0;
      Column = 0;
    }

    //==================================================================================================================
    // Private constructor
    //==================================================================================================================
  private Tokenizer() : base(createToken: (tokenType, text) => new PositionedToken<TokenType>(tokenType, text, 0, 0),
                             state: new AeLispTokenizerState())
    {
      foreach (var (tokenType, discrete, fun, pattern) in Tokens)
        Add(tokenType,
            discrete ? (pattern + FollowedByTokenBarrierOrEOF) : (pattern),
            fun);
    }
  }
}
