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

  public record PositionedToken<TTokenType>(TTokenType tokenType, string Text, int Line, int Column) : Token<TTokenType>(tokenType, Text)
  {
    public override string ToString() => $"{tokenType} [{Text}] @ {Line},{Column}";
  }

  public class Tokenizer : StringTokenizer<TokenType, PositionedToken<TokenType>, AeLispTokenizerState>
  {
    //==================================================================================================================
    // Token callbacks
    //==================================================================================================================
    private static PositionedToken<TokenType> UnescapeChars(PositionedToken<TokenType> token)
    {
      var str = token.Text;

      foreach (var (escaped, unescaped) in EscapedChars)
        str = str.Replace(escaped, unescaped);
      
      return new PositionedToken<TokenType>(token.TokenType, str, token.Line, token.Column);
    }

    private static PositionedToken<TokenType> TrimAndUnescape(PositionedToken<TokenType> token)
    {
      token = CountColumns(token);

      return UnescapeChars(new PositionedToken<TokenType>(token.TokenType, token.Text.Substring(1, token.Text.Length - 2), token.Line, token.Column));
    }

    private static PositionedToken<TokenType> TrimFirstAndUnescape(PositionedToken<TokenType> token)
    {
      token = CountColumns(token);

      return UnescapeChars(TrimFirst(token));
    }

    private static PositionedToken<TokenType> TrimFirst(PositionedToken<TokenType> token)
    {
      token = CountColumns(token);

      return new PositionedToken<TokenType>(token.TokenType, token.Text.Substring(1), token.Line, token.Column);
    }

    private static PositionedToken<TokenType> CountLines(PositionedToken<TokenType> token)
    {
      token = new PositionedToken<TokenType>(token.TokenType, token.Text, Get().Line, Get().Column);

      Get().Line++;
      Get().Column = 0;

      return token;
    }

    private static PositionedToken<TokenType> CountColumns(PositionedToken<TokenType> token)
    {
      // WriteLine($"Begin CountColumns with {Get().Line},{Get().Column} at \"{token.Text}\".");

      token = new PositionedToken<TokenType>(token.TokenType, token.Text, Get().Line, Get().Column);

      Get().Column += token.Text.Length;

      return token;
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

    private static readonly List<(TokenType type, bool discrete, Func<PositionedToken<TokenType>, PositionedToken<TokenType>>? fun, string pattern)> Tokens =
      new List<(TokenType type, bool discrete, Func<PositionedToken<TokenType>, PositionedToken<TokenType>>? fun, string pattern)>
      {
        (TokenType.NewLine,       discrete: false, fun: CountLines,           pattern: @"\r?\n"),
        (TokenType.Whitespace,    discrete: false, fun: CountColumns,         pattern: @"[ \t\f\v]+"),
        (TokenType.LParen,        discrete: false, fun: CountColumns,         pattern: @"\("),
        (TokenType.RParen,        discrete: true,  fun: CountColumns,         pattern: @"\)"),
        (TokenType.Nil,           discrete: true,  fun: CountColumns,         pattern: @"nil"),
        (TokenType.Dot,           discrete: true,  fun: CountColumns,         pattern: @"\."),
        (TokenType.CStyleChar,    discrete: true,  fun: TrimAndUnescape,      pattern: @"'[^']'"),
        (TokenType.CStyleChar,    discrete: true,  fun: TrimAndUnescape,      pattern: @"'\\.'"),
        (TokenType.Float,         discrete: true,  fun: CountColumns,         pattern: Float),
        (TokenType.Rational,      discrete: true,  fun: CountColumns,         pattern: Rational),
        (TokenType.Integer,       discrete: true,  fun: CountColumns,         pattern: MaybeSigned + DigitSeparatedInteger),
        (TokenType.String,        discrete: true,  fun: TrimAndUnescape,      pattern: @"\""(\\\""|[^\""])*\"""),
        (TokenType.Quote,         discrete: false, fun: CountColumns,         pattern: @"'"),
        (TokenType.Backtick,      discrete: false, fun: CountColumns,         pattern: @"`"),
        (TokenType.CommaAt,       discrete: false, fun: CountColumns,         pattern: @",@"),
        (TokenType.Comma,         discrete: false, fun: CountColumns,         pattern: @","),
        (TokenType.At,            discrete: false, fun: CountColumns,         pattern: @"@"),
        (TokenType.Dollar,        discrete: false, fun: CountColumns,         pattern: @"\$"),
        (TokenType.Symbol,        discrete: true,  fun: CountColumns,         pattern: Integer + @"?" + MathOp),
        (TokenType.Symbol,        discrete: true,  fun: CountColumns,         pattern: MathOp + Integer),
        (TokenType.Symbol,        discrete: true,  fun: CountColumns,         pattern: @"[\?]{3}"),
        (TokenType.Symbol,        discrete: true,  fun: CountColumns,         pattern: SymBody),
        (TokenType.Symbol,        discrete: true,  fun: CountColumns,         pattern: @"<"  + SymBody + @">"),
        (TokenType.Symbol,        discrete: true,  fun: CountColumns,         pattern: @"\*" + SymBody + @"\*"),
        (TokenType.Symbol,        discrete: true,  fun: CountColumns,         pattern: @"𝑎|𝑏|𝑐|𝑑|𝑒|𝑓|𝑚|𝑛|𝑜|𝑝|𝑞|𝑟|𝑠|𝑡|𝑢|𝑣|𝑤|𝑥|𝑦|𝑧"),
        (TokenType.Symbol,        discrete: true,  fun: CountColumns,         pattern: @"(?:_)|(?:=)|(?:==)|(?:!=)|(?:>=?)|(?:<=?)"),
        (TokenType.Symbol,        discrete: true,  fun: CountColumns,         pattern: @"¬|λ\??|∧|∨|⊤|⊥|≤|≥|×|÷|Ø|∈|∉|≠|!|∃|∄|∀|≔|\||&|~|\^|\?"),
        (TokenType.LispStyleChar, discrete: true,  fun: TrimFirstAndUnescape, pattern: @"\?\\."),
        (TokenType.LispStyleChar, discrete: true,  fun: TrimFirstAndUnescape, pattern: @"\?."),
        (TokenType.LineComment,   discrete: false, fun: TrimFirst,            pattern: @";.+"),
        (TokenType.Garbage,       discrete: false, fun: CountColumns,         pattern: @".+"),
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
  private Tokenizer() : base((tokenType, text) => new PositionedToken<TokenType>(tokenType, text, 0, 0),
                             new AeLispTokenizerState())
    {
      foreach (var (tokenType, discrete, fun, pattern) in Tokens)
        Add(tokenType,
            discrete
              ? (pattern + FollowedByTokenBarrierOrEOF)
              : (pattern),
            fun);
        }
  }
}
