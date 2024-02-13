using static System.Console;
using System.Text.RegularExpressions;
using System.Collections.Immutable;

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
    Integer,
    LParen,
    LineComment,
    LispStyleChar,
    MultilineComment,
    Newline,
    Nil,
    Quote,
    RParen,
    Rational,
    String,
    Symbol,
    Whitespace,
  };

  public record struct AeLispTokenizerState(int Line = 0, int Column = 0, bool InMultilineComment = false);

  public record struct PositionedToken<TTokenType>(TTokenType TokenType, string Text, int Line, int Column)
  {
    public override string ToString() => $"{TokenType} [{Text}] @ {Line},{Column}";
  }

  public class Tokenizer : StringTokenizer<TokenType, PositionedToken<TokenType>, AeLispTokenizerState>
  {
    //==================================================================================================================
    // Private constructor
    //==================================================================================================================
    private Tokenizer() : base(createToken: (tokenType, text) => new PositionedToken<TokenType>(tokenType, text, 0, 0),
                               createTokenizerStateFun: () => new AeLispTokenizerState()) // ,
                                                                                          // resetMode: StringTokenizer<TokenType, PositionedToken<TokenType>, AeLispTokenizerState>.StringTokenizerResetMode.Auto)
    {
      foreach (var (tokenType, discrete, process, active, pattern) in Tokens)
        Add(tokenType,
            discrete ? (pattern + FollowedByTokenBarrierOrEOF) : (pattern),
            process is null ? CountColumns : ((AeLispTokenizerState State, PositionedToken<TokenType> Token) tup) => process(CountColumns(tup)),
            active is null ? NotInMultilineComment : active);
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
                                  TokenDefinitionIsActiveFun? IsActive,
                                  string Pattern)> Tokens =
      new List<(TokenType Type,
                bool Discrete,
                ProcesTokenFun? Process,
                TokenDefinitionIsActiveFun? IsActive,
                string Pattern)>
      {
        (Type: TokenType.Newline,          Discrete: false, Process: CountLine,         IsActive: null,               Pattern: @"\r?\n"),
        (Type: TokenType.Whitespace,       Discrete: false, Process: null,              IsActive: null,               Pattern: @"[ \t\f\v]+"),
        (Type: TokenType.LParen,           Discrete: false, Process: null,              IsActive: null,               Pattern: @"\("),
        (Type: TokenType.RParen,           Discrete: true,  Process: null,              IsActive: null,               Pattern: @"\)"),
        (Type: TokenType.Nil,              Discrete: true,  Process: null,              IsActive: null,               Pattern: @"nil"),
        (Type: TokenType.Dot,              Discrete: true,  Process: null,              IsActive: null,               Pattern: @"\."),
        (Type: TokenType.CStyleChar,       Discrete: true,  Process: ProcStringLike,    IsActive: null,               Pattern: @"'[^']'"),
        (Type: TokenType.CStyleChar,       Discrete: true,  Process: ProcStringLike,    IsActive: null,               Pattern: @"'\\.'"),
        (Type: TokenType.Float,            Discrete: true,  Process: ProcFloat,         IsActive: null,               Pattern: Float),
        (Type: TokenType.Rational,         Discrete: true,  Process: ProcRational,      IsActive: null,               Pattern: Rational),
        (Type: TokenType.Integer,          Discrete: true,  Process: ProcNumber,        IsActive: null,               Pattern: MaybeSigned + DigitSeparatedInteger),
        (Type: TokenType.String,           Discrete: true,  Process: ProcStringLike,    IsActive: null,               Pattern: @"\""(\\\""|[^\""])*\"""),
        (Type: TokenType.Quote,            Discrete: false, Process: null,              IsActive: null,               Pattern: @"'"),
        (Type: TokenType.Backtick,         Discrete: false, Process: null,              IsActive: null,               Pattern: @"`"),
        (Type: TokenType.CommaAt,          Discrete: false, Process: null,              IsActive: null,               Pattern: @",@"),
        (Type: TokenType.Comma,            Discrete: false, Process: null,              IsActive: null,               Pattern: @","),
        (Type: TokenType.At,               Discrete: false, Process: null,              IsActive: null,               Pattern: @"@"),
        (Type: TokenType.Dollar,           Discrete: false, Process: null,              IsActive: null,               Pattern: @"\$"),
        (Type: TokenType.Symbol,           Discrete: true,  Process: null,              IsActive: null,               Pattern: Integer + @"?" + MathOp),
        (Type: TokenType.Symbol,           Discrete: true,  Process: null,              IsActive: null,               Pattern: MathOp + Integer),
        (Type: TokenType.Symbol,           Discrete: true,  Process: null,              IsActive: null,               Pattern: @"[\?]{3}"),
        (Type: TokenType.Symbol,           Discrete: true,  Process: null,              IsActive: null,               Pattern: SymBody),
        (Type: TokenType.Symbol,           Discrete: true,  Process: null,              IsActive: null,               Pattern: @"<"  + SymBody + @">"),
        (Type: TokenType.Symbol,           Discrete: true,  Process: null,              IsActive: null,               Pattern: @"\*" + SymBody + @"\*"),
        (Type: TokenType.Symbol,           Discrete: true,  Process: null,              IsActive: null,               Pattern: @"𝑎|𝑏|𝑐|𝑑|𝑒|𝑓|𝑚|𝑛|𝑜|𝑝|𝑞|𝑟|𝑠|𝑡|𝑢|𝑣|𝑤|𝑥|𝑦|𝑧"),
        (Type: TokenType.Symbol,           Discrete: true,  Process: null,              IsActive: null,               Pattern: @"(?:_)|(?:=)|(?:==)|(?:!=)|(?:>=?)|(?:<=?)"),
        (Type: TokenType.Symbol,           Discrete: true,  Process: null,              IsActive: null,               Pattern: @"¬|λ\??|∧|∨|⊤|⊥|≤|≥|×|÷|Ø|∈|∉|≠|!|∃|∄|∀|≔|\||&|~|\^|\?"),
        (Type: TokenType.LispStyleChar,    Discrete: true,  Process: ProcLispStyleChar, IsActive: null,               Pattern: @"\?\\."),
        (Type: TokenType.LispStyleChar,    Discrete: true,  Process: ProcLispStyleChar, IsActive: null,               Pattern: @"\?."),
        (Type: TokenType.LineComment,      Discrete: false, Process: TrimFirst,         IsActive: null,               Pattern: @";[^\n]*"),
        (Type: TokenType.MultilineComment, Discrete: false, Process: BeginMLC,          IsActive: null,               Pattern: @"#\|"),
        (Type: TokenType.MultilineComment, Discrete: false, Process: EndMLC,            IsActive: InMultilineComment, Pattern: @"\|#"),
        (Type: TokenType.MultilineComment, Discrete: false, Process: null,              IsActive: InMultilineComment, Pattern: @"\S+"),
        (Type: TokenType.MultilineComment, Discrete: false, Process: CountLine,         IsActive: InMultilineComment, Pattern: @"\r?\n"),
        (Type: TokenType.MultilineComment, Discrete: false, Process: null,              IsActive: InMultilineComment, Pattern: @"[ \t\f\v]+"),
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
    private static (AeLispTokenizerState, PositionedToken<TokenType>)
    UnescapeChars((AeLispTokenizerState State, PositionedToken<TokenType> Token) tup)
    {
      var str = tup.Token.Text;

      foreach (var (escaped, unescaped) in EscapedChars)
        str = str.Replace(escaped, unescaped);

      tup.Token.Text = str;

      return tup;
    }

    private static (AeLispTokenizerState, PositionedToken<TokenType>)
    ProcStringLike((AeLispTokenizerState State, PositionedToken<TokenType> Token) tup)
      => UnescapeChars((tup.State,
                         new PositionedToken<TokenType>(tup.Token.TokenType,
                                                        tup.Token.Text.Substring(1, tup.Token.Text.Length - 2),
                                                        tup.Token.Line,
                                                        tup.Token.Column)));

    private static (AeLispTokenizerState, PositionedToken<TokenType>)
    ProcLispStyleChar((AeLispTokenizerState State, PositionedToken<TokenType> Token) tup)
      => UnescapeChars(TrimFirst(tup));

    private static (AeLispTokenizerState, PositionedToken<TokenType>)
    TrimFirst((AeLispTokenizerState State, PositionedToken<TokenType> Token) tup)
     => (tup.State, new PositionedToken<TokenType>(tup.Token.TokenType, tup.Token.Text.Substring(1), tup.Token.Line, tup.Token.Column));

    private static (AeLispTokenizerState, PositionedToken<TokenType>)
    ProcNumber((AeLispTokenizerState State, PositionedToken<TokenType> Token) tup)
    {
      tup = StripCommas(tup);

      var pattern = @"^([+-]?)(?:0*)(\d*(?:\.\d+)?)$";
      var match = Regex.Match(tup.Token.Text, pattern);
      var sign = match.Groups[1].Value;
      var number = match.Groups[2].Value;

      tup.Token.Text = (string.IsNullOrEmpty(number) || number == "0")
        ? "0"
        : sign + number;

      return tup;
    }

    private static (AeLispTokenizerState, PositionedToken<TokenType>)
    ProcFloat((AeLispTokenizerState State, PositionedToken<TokenType> Token) tup)
    {
      tup = ProcNumber(tup);

      var pattern = @"^(.*?)(?:0*)$";
      var match = Regex.Match(tup.Token.Text, pattern);

      tup.Token.Text = match.Groups[1].Value;

      return tup;
    }

    private static (AeLispTokenizerState, PositionedToken<TokenType>)
    ProcRational((AeLispTokenizerState State, PositionedToken<TokenType> Token) tup)
    {
      tup = StripCommas(tup);

      var pattern = @"^([+-]?)(?:0*)(\d+)\/(?:0*)(\d+)$";
      var match = Regex.Match(tup.Token.Text, pattern);
      var sign = match.Groups[1].Value;
      var numer = match.Groups[2].Value;
      var denom = match.Groups[3].Value;

      numer = (string.IsNullOrEmpty(numer) || numer == "0")
        ? "0"
        : sign + numer;

      tup.Token.Text = numer + "/" + denom;

      return tup;
    }

    private static (AeLispTokenizerState, PositionedToken<TokenType>)
    StripCommas((AeLispTokenizerState State, PositionedToken<TokenType> Token) tup)
    {
      tup.Token.Text = tup.Token.Text.Replace(",", "");

      return tup;
    }

    private static (AeLispTokenizerState, PositionedToken<TokenType>)
    BeginMLC((AeLispTokenizerState State, PositionedToken<TokenType> Token) tup)
    {
      tup.State.InMultilineComment = true;

      return tup;
    }

    private static (AeLispTokenizerState, PositionedToken<TokenType>)
    EndMLC((AeLispTokenizerState State, PositionedToken<TokenType> Token) tup)
    {
      tup.State.InMultilineComment = false;

      return tup;
    }

    private static (AeLispTokenizerState, PositionedToken<TokenType>)
    CountLine((AeLispTokenizerState State, PositionedToken<TokenType> Token) tup)
    {
      tup.State.Line++;
      tup.State.Column = 0;

      return tup;
    }

    private static (AeLispTokenizerState, PositionedToken<TokenType>)
    CountColumns((AeLispTokenizerState State, PositionedToken<TokenType> Token) tup)
    {
      tup = SetTokenLinesAndColumns(tup);

      tup.State.Column += tup.Token.Text.Length;

      return tup;
    }

    private static (AeLispTokenizerState, PositionedToken<TokenType>)
    SetTokenLinesAndColumns((AeLispTokenizerState State, PositionedToken<TokenType> Token) tup)
    {
      tup.Token.Line = tup.State.Line;
      tup.Token.Column = tup.State.Column;

      return tup;
    }

    //==================================================================================================================
    // 'Is active?' callbacks
    //==================================================================================================================
    private static bool NotInMultilineComment(AeLispTokenizerState state)
    {
      return !InMultilineComment(state);
    }

    private static bool InMultilineComment(AeLispTokenizerState state)
    {
      return state.InMultilineComment;
    }

    private static bool Always(AeLispTokenizerState state)
    {
      return true;
    }
  }
}
