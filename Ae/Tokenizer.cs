using static System.Console;
using System.Text.RegularExpressions;
using System.Collections.Immutable;

//====================================================================================================================================================
static partial class Ae
{
  //==================================================================================================================================================
  // Ae's static field
  //==================================================================================================================================================
  private static readonly ImmutableArray<(string, string)> EscapedChars = ImmutableArray.Create(
    (@"\a", "\a"),
    (@"\b", "\b"),
    (@"\f", "\f"),
    (@"\n", "\n"),
    (@"\r", "\r"),
    (@"\t", "\t"),
    (@"\v", "\v"),
    (@"\\", "\\"),
    // (@"\'", "\'"),
    (@"\""", "\"")
    );

  //================================================================================================================================================
  // Escaping helper methods
  //================================================================================================================================================
  private static string UnescapeChars(this string self)
  {
    foreach (var (escaped, unescaped) in EscapedChars)
      self = self.Replace(escaped, unescaped);

    return self;
  }

  private static string EscapeChars(this string self)
  {
    foreach (var (escaped, unescaped) in EscapedChars)
      self = self.Replace(unescaped, escaped);

    return self;
  }

  //==================================================================================================================================================
  // Ae's public types
  //==================================================================================================================================================
  public enum LispTokenType
  {
    UninitializedToken,
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
    MultilineCommentBegin,
    MultilineCommentEnd,
    MultilineCommentContent,
    MultilineStringBegin,
    MultilineStringEnd,
    MultilineStringContent,
    Comment,
    Newline,
    Nil,
    Quote,
    RParen,
    Rational,
    String,
    Symbol,
    Whitespace,
  };

  //==================================================================================================================================================
  public enum TokenizerMode
  {
    Normal,
    InMultilineComment,
    InMultilineString,
  };

  //==================================================================================================================================================
  public record struct LispTokenizerState(int Line = 0, int Column = 0, TokenizerMode Mode = TokenizerMode.Normal, int ParenDepth = 0);

  //==================================================================================================================================================
  public record struct Token(LispTokenType Type, string Text, int Line, int Column, int ParenDepth)
  {
    public override string ToString() => $"{Type} [{Text}] @ {Line},{Column},{ParenDepth}";
  }

  //==================================================================================================================================================
  // Ae's static field
  //==================================================================================================================================================
  private static readonly ImmutableArray<LispTokenType> UninterestingLispTokenTypes = ImmutableArray.Create(
  LispTokenType.Whitespace,
  // LispTokenType.LineComment,
  // LispTokenType.MultilineCommentBegin,
  // LispTokenType.MultilineCommentContent,
  // LispTokenType.MultilineCommentEnd,
  // LispTokenType.Comment,
  LispTokenType.Newline);

  //==================================================================================================================================================
  // Ae's static methods
  //==================================================================================================================================================
  public static bool IsUninterestingToken(Token token) => !IsInterestingToken(token);
  public static bool IsInterestingToken(Token token) => !UninterestingLispTokenTypes.Contains(token.Type);

  //==================================================================================================================================================
  // Ae's extension methods
  //==================================================================================================================================================
  public static IEnumerable<Token> Interesting(this IEnumerable<Token> self) => self.Where(t => IsInterestingToken(t));
  public static IEnumerable<Token> Uninteresting(this IEnumerable<Token> self) => self.Where(t => IsUninterestingToken(t));

  //==================================================================================================================================================
  public static void Print(this IEnumerable<Token> self, int countOffset = 0)
  {
    foreach (var (token, index) in self.Select((value, index) => (value, index + countOffset)))
      WriteLine($"#{index}: {token}");
  }

  //==================================================================================================================================================
  // PureLispTokenizer class
  //==================================================================================================================================================
  public class PureLispTokenizer : PureStringTokenizer<LispTokenType, Token, LispTokenizerState>
  {
    //================================================================================================================================================
    // Private constructor
    //================================================================================================================================================
    private PureLispTokenizer() : base(createToken: (tokenType, text) => new(tokenType, text, 0, 0, 0),
                                       createTokenizerStateFun: () => new())
    {
      foreach (var (tokenType, discrete, process, active, pattern) in Tokens)
        Add(tokenType,
            discrete ? (pattern + FollowedByTokenBarrierOrEOF) : (pattern),
            process is null
              ? CountColumns
              : ((LispTokenizerState State, Token Token) tup) => process(CountColumns(tup)),
            active is null ? Normal : active);
    }

    //================================================================================================================================================
    // Private constants
    //================================================================================================================================================
    private static readonly
      ImmutableArray<(LispTokenType Type,
                      bool Discrete,
                      ProcesTokenFun? Process,
                      TokenDefinitionIsActiveFun? IsActive,
                      string Pattern)> Tokens =
      ImmutableArray<(LispTokenType Type, bool Discrete, ProcesTokenFun? Process, TokenDefinitionIsActiveFun? IsActive, string Pattern)>.Empty
      .Add((Type: LispTokenType.Newline,                   Discrete: false, Process: ProcCountLine,     IsActive: null,               Pattern: @"\r?\n"))
      .Add((Type: LispTokenType.Whitespace,                Discrete: false, Process: null,              IsActive: null,               Pattern: @"[ \t\f\v]+"))
      .Add((Type: LispTokenType.LParen,                    Discrete: false, Process: ProcLParen,        IsActive: null,               Pattern: @"\("))
      .Add((Type: LispTokenType.RParen,                    Discrete: true,  Process: ProcRParen,        IsActive: null,               Pattern: @"\)"))
      .Add((Type: LispTokenType.Nil,                       Discrete: true,  Process: null,              IsActive: null,               Pattern: @"nil"))
      .Add((Type: LispTokenType.Dot,                       Discrete: true,  Process: null,              IsActive: null,               Pattern: @"\."))
      .Add((Type: LispTokenType.CStyleChar,                Discrete: true,  Process: ProcStringLike,    IsActive: null,               Pattern: @"'[^']'"))
      .Add((Type: LispTokenType.CStyleChar,                Discrete: true,  Process: ProcStringLike,    IsActive: null,               Pattern: @"'\\.'"))
      .Add((Type: LispTokenType.Float,                     Discrete: true,  Process: ProcFloat,         IsActive: null,               Pattern: Float))
      .Add((Type: LispTokenType.Rational,                  Discrete: true,  Process: ProcRational,      IsActive: null,               Pattern: Rational))
      .Add((Type: LispTokenType.Integer,                   Discrete: true,  Process: ProcNumber,        IsActive: null,               Pattern: MaybeSigned + DigitSeparatedInteger))
      .Add((Type: LispTokenType.Quote,                     Discrete: false, Process: null,              IsActive: null,               Pattern: @"'"))
      .Add((Type: LispTokenType.Backtick,                  Discrete: false, Process: null,              IsActive: null,               Pattern: @"`"))
      .Add((Type: LispTokenType.CommaAt,                   Discrete: false, Process: null,              IsActive: null,               Pattern: @",@"))
      .Add((Type: LispTokenType.Comma,                     Discrete: false, Process: null,              IsActive: null,               Pattern: @","))
      .Add((Type: LispTokenType.At,                        Discrete: false, Process: null,              IsActive: null,               Pattern: @"@"))
      .Add((Type: LispTokenType.Dollar,                    Discrete: false, Process: null,              IsActive: null,               Pattern: @"\$"))
      .Add((Type: LispTokenType.Symbol,                    Discrete: true,  Process: null,              IsActive: null,               Pattern: Integer + @"?" + MathOp))
      .Add((Type: LispTokenType.Symbol,                    Discrete: true,  Process: null,              IsActive: null,               Pattern: MathOp + Integer))
      .Add((Type: LispTokenType.Symbol,                    Discrete: true,  Process: null,              IsActive: null,               Pattern: @"[\?]{3}"))
      .Add((Type: LispTokenType.Symbol,                    Discrete: true,  Process: null,              IsActive: null,               Pattern: SymBody))
      .Add((Type: LispTokenType.Symbol,                    Discrete: true,  Process: null,              IsActive: null,               Pattern: @"<"  + SymBody + @">"))
      .Add((Type: LispTokenType.Symbol,                    Discrete: true,  Process: null,              IsActive: null,               Pattern: @"\*" + SymBody + @"\*"))
      .Add((Type: LispTokenType.Symbol,                    Discrete: true,  Process: null,              IsActive: null,               Pattern: @"𝑎|𝑏|𝑐|𝑑|𝑒|𝑓|𝑚|𝑛|𝑜|𝑝|𝑞|𝑟|𝑠|𝑡|𝑢|𝑣|𝑤|𝑥|𝑦|𝑧"))
      .Add((Type: LispTokenType.Symbol,                    Discrete: true,  Process: null,              IsActive: null,               Pattern: @"(?:_)|(?:=)|(?:==)|(?:!=)|(?:>=?)|(?:<=?)"))
      .Add((Type: LispTokenType.Symbol,                    Discrete: true,  Process: null,              IsActive: null,               Pattern: @"¬|λ\??|∧|∨|⊤|⊥|≤|≥|×|÷|Ø|∈|∉|≠|!|∃|∄|∀|≔|\||&|~|\^|\?"))
      .Add((Type: LispTokenType.LispStyleChar,             Discrete: true,  Process: ProcLispStyleChar, IsActive: null,               Pattern: @"\?\\?."))
      .Add((Type: LispTokenType.String,                    Discrete: true,  Process: ProcStringLike,    IsActive: null,               Pattern: @"\""" + StringContent+ @"\"""))
      .Add((Type: LispTokenType.MultilineStringBegin,      Discrete: false, Process: ProcBeginMLS,      IsActive: null,               Pattern: @"\""" + StringContent+ @"\n"))
      .Add((Type: LispTokenType.MultilineStringEnd,        Discrete: true,  Process: ProcEndMLS,        IsActive: InMultilineString,  Pattern: StringContent + @"\"""))
      .Add((Type: LispTokenType.MultilineStringContent,    Discrete: false, Process: ProcMLSContent,    IsActive: InMultilineString,  Pattern: StringContent + @"\n"))
      .Add((Type: LispTokenType.LineComment,               Discrete: false, Process: ProcTrimFirst,     IsActive: null,               Pattern: @";[^\n]*"))
      .Add((Type: LispTokenType.Comment,                   Discrete: false, Process: ProcComment,       IsActive: null,               Pattern: @"#\|[^\n]*\|#"))
      .Add((Type: LispTokenType.MultilineCommentBegin,     Discrete: false, Process: ProcBeginMLC,      IsActive: null,               Pattern: @"#\|[^\n]*\n"))
      .Add((Type: LispTokenType.MultilineCommentEnd,       Discrete: false, Process: ProcEndMLC,        IsActive: InMultilineComment, Pattern: @"[\S \t\f\v]*\|#"))
      .Add((Type: LispTokenType.MultilineCommentContent,   Discrete: false, Process: ProcCountLine,     IsActive: InMultilineComment, Pattern: @"[^\n]*\n"));
      //.Add((Type: LispTokenType.Garbage,                   Discrete: false, Process: null,              IsActive: null,               Pattern: @".+"));

    //================================================================================================================================================
    public static PureLispTokenizer Instance { get; } = new();

    //=========================================================== =====================================================================================
    // Token callbacks
    //================================================================================================================================================
    private static (LispTokenizerState, Token)
      ProcUnescapeChars((LispTokenizerState State, Token Token) tup)
    {
      tup.Token.Text = tup.Token.Text.UnescapeChars();
      return tup;
    }

    //================================================================================================================================================
    private static (LispTokenizerState, Token)
      ProcLParen((LispTokenizerState State, Token Token) tup)
    {
      tup.State.ParenDepth += 1;
      return tup;
    }

    //================================================================================================================================================
    private static (LispTokenizerState, Token)
      ProcRParen((LispTokenizerState State, Token Token) tup)
    {
      tup.State.ParenDepth -= 1;
      // tup.Token.ParenDepth -= 1; // Dunno if we should do this or not.
      return tup;
    }

    //================================================================================================================================================
    private static (LispTokenizerState, Token)
      ProcStringLike((LispTokenizerState State, Token Token) tup)
    {
      tup.Token.Text = UnescapeChars(tup.Token.Text.Substring(1, tup.Token.Text.Length - 2));
      return tup;
    }
        
    //================================================================================================================================================
    private static (LispTokenizerState, Token)
      ProcLispStyleChar((LispTokenizerState State, Token Token) tup)
      => ProcUnescapeChars(ProcTrimFirst(tup));

    //================================================================================================================================================
    private static (LispTokenizerState, Token)
      ProcTrimFirst((LispTokenizerState State, Token Token) tup)
    {
      tup.Token.Text = tup.Token.Text.Substring(1);
      return tup;
    }

    //================================================================================================================================================
    private static (LispTokenizerState, Token)
      TrimLast((LispTokenizerState State, Token Token) tup)
    {
      tup.Token.Text = tup.Token.Text.Substring(0, tup.Token.Text.Length - 1);
      return tup;
    }

    //================================================================================================================================================
    private static (LispTokenizerState, Token)
      ProcNumber((LispTokenizerState State, Token Token) tup)
    {
      tup = ProcStripCommas(tup);

      var pattern = @"^([+-]?)(?:0*)(\d*(?:\.\d+)?)$";
      var match = Regex.Match(tup.Token.Text, pattern);
      var sign = match.Groups[1].Value;
      var number = match.Groups[2].Value;

      tup.Token.Text = (string.IsNullOrEmpty(number) || number == "0")
        ? "0"
        : sign + number;

      return tup;
    }

    //================================================================================================================================================
    private static (LispTokenizerState, Token)
      ProcFloat((LispTokenizerState State, Token Token) tup)
    {
      tup = ProcNumber(tup);

      var pattern = @"^(.*?\.\d*?)0*?$";
      var replacement = "$1";

      tup.Token.Text = Regex.Replace(tup.Token.Text, pattern, replacement);

      if (tup.Token.Text.StartsWith("."))
        tup.Token.Text = "0" + tup.Token.Text;

      if (tup.Token.Text.EndsWith("."))
        tup.Token.Text += "0";

      return tup;
    }

    //================================================================================================================================================
    private static (LispTokenizerState, Token)
      ProcRational((LispTokenizerState State, Token Token) tup)
    {
      tup = ProcStripCommas(tup);

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

    //================================================================================================================================================
    private static (LispTokenizerState, Token)
      ProcStripCommas((LispTokenizerState State, Token Token) tup)
    {
      tup.Token.Text = tup.Token.Text.Replace(",", "");
      return tup;
    }

    //================================================================================================================================================
    private static (LispTokenizerState, Token)
      ProcComment((LispTokenizerState State, Token Token) tup)
    {
      tup.Token.Text = tup.Token.Text.Substring(2, tup.Token.Text.Length - 4);

      return ProcCountLine(tup);
    }

    //================================================================================================================================================
    // Token callbacks (multiline comments)
    //================================================================================================================================================
    private static (LispTokenizerState, Token)
      ProcBeginMLC((LispTokenizerState State, Token Token) tup)
    {
      tup.Token.Text = tup.Token.Text.Substring(2);
      tup.State.Mode = TokenizerMode.InMultilineComment;

      return ProcCountLine(tup);
    }

    //================================================================================================================================================
    private static (LispTokenizerState, Token)
      ProcEndMLC((LispTokenizerState State, Token Token) tup)
    {
      tup.Token.Text = tup.Token.Text.Substring(0, tup.Token.Text.Length - 2);
      tup.State.Mode = TokenizerMode.Normal;
      return tup;
    }

    //================================================================================================================================================
    // Token callbacks (multiline strings)
    //================================================================================================================================================
    private static (LispTokenizerState, Token) ProcBeginMLS((LispTokenizerState State, Token Token) tup)
    {
      tup = ProcTrimFirst(ProcUnescapeChars(ProcCountLine(tup)));
      tup.State.Mode = TokenizerMode.InMultilineString;
      return tup;
    }

    //================================================================================================================================================
    private static (LispTokenizerState, Token)
      ProcEndMLS((LispTokenizerState State, Token Token) tup)
    {
      tup = TrimLast(ProcUnescapeChars(tup));
      tup.State.Mode = TokenizerMode.Normal;
      return tup;
    }

    //================================================================================================================================================
    private static (LispTokenizerState, Token)
      ProcMLSContent((LispTokenizerState State, Token Token) tup)
      => ProcUnescapeChars(ProcCountLine(tup));

    //================================================================================================================================================
    private static (LispTokenizerState, Token)
      ProcNewline((LispTokenizerState State, Token Token) tup)
    {
      tup = ProcCountLine(tup);
      tup.Token.Text = "\\n";
      return tup;
    }

    //================================================================================================================================================
    private static (LispTokenizerState, Token)
      ProcCountLine((LispTokenizerState State, Token Token) tup)
    {
      tup.State.Line++;
      tup.State.Column = 0;
      return tup;
    }

    //================================================================================================================================================
    private static (LispTokenizerState, Token)
      CountColumns((LispTokenizerState State, Token Token) tup)
    {
      tup = SetTokenLinesAndColumns(tup);
      tup.State.Column += tup.Token.Text.Length;
      return tup;
    }

    //================================================================================================================================================
    private static (LispTokenizerState, Token)
      SetTokenLinesAndColumns((LispTokenizerState State, Token Token) tup)
    {
      tup.Token.Line = tup.State.Line;
      tup.Token.Column = tup.State.Column;
      tup.Token.ParenDepth = tup.State.ParenDepth;
      return tup;
    }

    //================================================================================================================================================
    // 'Is active?' callbacks
    //================================================================================================================================================
    private static bool InMultilineComment(LispTokenizerState state) => state.Mode == TokenizerMode.InMultilineComment;
    private static bool InMultilineString(LispTokenizerState state) => state.Mode == TokenizerMode.InMultilineString;
    private static bool Normal(LispTokenizerState state) => state.Mode == TokenizerMode.Normal;

    //================================================================================================================================================
    // Patterns are down here since they confuse csharp-mode's indentation logic:
    //================================================================================================================================================
    private const string DigitSeparatedInteger = @"(?:" + ZeroPaddedInteger + @"(?:," + ZeroPaddedInteger + @")*)";
    private const string Float = @"(?:" + MaybeSigned + DigitSeparatedInteger + @"?\.\d+)";
    private const string FollowedByTokenBarrierOrEOF = @"(?=\s|\)|$|(?:#\|))";
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
    private const string StringContent = @"(?:(?:\\[abfnrtv\\""])|[^\""\n\\])*"; // Keep this in sync with EscapedChars.

    //================================================================================================================================================
  }
}
