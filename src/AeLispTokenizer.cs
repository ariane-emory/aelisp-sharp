public static partial class AE
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
    LispStyleChar,
    Nil,
    Quote,
    RParen,
    Rational,
    String,
    Symbol,
    Whitespace,
  };

  public class Tokenizer : StringTokenizer<TokenType>
  {
    // Constructor
    public Tokenizer()
    {
      foreach (var (tokenType, discrete, fun, pattern) in Tokens)
        Add(tokenType,
            discrete
              ? (pattern + FollowedByTokenBarrierOrEOF)
              : (pattern),
            fun);
    }

    // Private constants.
    private static readonly List<(TokenType type, bool discrete, Func<string, string>? fun, string pattern)> Tokens =
       new List<(TokenType type, bool discrete, Func<string, string>? fun, string pattern)>
       {
            (TokenType.Whitespace,    discrete: false, fun: null, pattern: @"\s+"),
            (TokenType.LParen,        discrete: false, fun: null, pattern: @"\("),
            (TokenType.RParen,        discrete: true,  fun: null, pattern: @"\)"),
            (TokenType.Nil,           discrete: true,  fun: null, pattern: @"nil"),
            (TokenType.Dot,           discrete: true,  fun: null, pattern: @"\."),
            (TokenType.CStyleChar,    discrete: true,  fun: null, pattern: @"'[^']'"),
            (TokenType.CStyleChar,    discrete: true,  fun: null, pattern: @"'\\.'"),
            (TokenType.Float,         discrete: true,  fun: null, pattern: Float),
            (TokenType.Rational,      discrete: true,  fun: null, pattern: MaybeSigned + DigitSeparatedInteger + @"\/" + DigitSeparatedInteger),
            (TokenType.Integer,       discrete: true,  fun: null, pattern: MaybeSigned + DigitSeparatedInteger),
            (TokenType.String,        discrete: true,  fun: null, pattern: @"\""(\\\""|[^\""])*\"""),
            (TokenType.Quote,         discrete: false, fun: null, pattern: @"'"),
            (TokenType.Backtick,      discrete: false, fun: null, pattern: @"`"),
            (TokenType.CommaAt,       discrete: false, fun: null, pattern: @",@"),
            (TokenType.Comma,         discrete: false, fun: null, pattern: @","),
            (TokenType.At,            discrete: false, fun: null, pattern: @"@"),
            (TokenType.Dollar,        discrete: false, fun: null, pattern: @"\$"),
            (TokenType.Symbol,        discrete: true,  fun: null, pattern: Integer + @"?" + MathOp),
            (TokenType.Symbol,        discrete: true,  fun: null, pattern: MathOp + Integer),
            (TokenType.Symbol,        discrete: true,  fun: null, pattern: @"[\?]{3}"),
            (TokenType.Symbol,        discrete: true,  fun: null, pattern: SymBody),
            (TokenType.Symbol,        discrete: true,  fun: null, pattern: @"<"  + SymBody + @">"),
            (TokenType.Symbol,        discrete: true,  fun: null, pattern: @"\*" + SymBody + @"\*"),
            (TokenType.Symbol,        discrete: true,  fun: null, pattern: @"𝑎|𝑏|𝑐|𝑑|𝑒|𝑓|𝑚|𝑛|𝑜|𝑝|𝑞|𝑟|𝑠|𝑡|𝑢|𝑣|𝑤|𝑥|𝑦|𝑧"),
            (TokenType.Symbol,        discrete: true,  fun: null, pattern: @"(?:_)|(?:=)|(?:==)|(?:!=)|(?:>=?)|(?:<=?)"),
            (TokenType.Symbol,        discrete: true,  fun: null, pattern: @"¬|λ\??|∧|∨|⊤|⊥|≤|≥|×|÷|Ø|∈|∉|≠|!|∃|∄|∀|≔|\||&|~|\^|\?"),
            (TokenType.LispStyleChar, discrete: true,  fun: null, pattern: @"\?\\."),
            (TokenType.LispStyleChar, discrete: true,  fun: null, pattern: @"\?."),
            (TokenType.Garbage,       discrete: false, fun: null, pattern: @".+$"),
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
    private const string ShiftLeft = @"(?:\<\<)";
    private const string ShiftRight = @"(?:\>\>)";
    private const string Sign = @"(?:[\-+])";
    private const string SymBody = @"(?:" + MaybeSymLeader + SymFirstWord + @"(?:" + SymSeparator + SymWord + @")*" + MaybePunctuationSuffix + @")";
    private const string SymFirstWord = @"(?:[a-zA-Z]" + SymWordChar + @"*)";
    private const string SymSeparator = @"(?:(?:\-+)|(?:_+)|(?:\:+)|(?:/+))";
    private const string SymWord = @"(?:" + SymWordChar + @"+)";
    private const string SymWordChar = @"[a-zA-Z0-9\'\.]";
    private const string ZeroPaddedInteger = @"(?:" + MaybeZeroPadding + Integer + @")";
  }
}
