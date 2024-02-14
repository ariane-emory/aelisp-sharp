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
    Garbage,
    Integer,
    LParen,
    LineComment,
    LispStyleChar,
    MultilineCommentBeginning,
    MultilineCommentEnd,
    MultilineCommentContent,
    MultilineStringBeginning,
    MultilineStringEnd,
    MultilineStringContent,
    InlineComment,
    Newline,
    Nil,
    Quote,
    RParen,
    Rational,
    String,
    Symbol,
    Whitespace,
  };

  public record struct PositionedToken<TTokenType>(TTokenType TokenType, string Text, int Line, int Column)
  {
    public override string ToString() => $"{TokenType} [{Text}] @ {Line},{Column}";
  }

  public enum AeLispTokenizerStateMode {
    Normal,
    InMultilineComment,
    InMultilineString,
  };
  
  public record struct AeLispTokenizerState(int Line = 0, int Column = 0, AeLispTokenizerStateMode Mode = AeLispTokenizerStateMode.Normal);

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
            process is null
              ? CountColumns
              : ((AeLispTokenizerState State, PositionedToken<TokenType> Token) tup) => process(CountColumns(tup)),
            active is null ? Normal : active);
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

    private static readonly ImmutableList<(TokenType Type,
                                           bool Discrete,
                                           ProcesTokenFun? Process,
                                           TokenDefinitionIsActiveFun? IsActive,
                                           string Pattern)> Tokens =
      ImmutableList<(TokenType Type, bool Discrete, ProcesTokenFun? Process, TokenDefinitionIsActiveFun? IsActive, string Pattern)>.Empty
      .Add((Type: TokenType.Newline,                    Discrete: false, Process: CountLine,         IsActive: null,                 Pattern: @"\r?\n"))
      .Add((Type: TokenType.Whitespace,                 Discrete: false, Process: null,              IsActive: null,                 Pattern: @"[ \t\f\v]+"))
      .Add((Type: TokenType.LParen,                     Discrete: false, Process: null,              IsActive: null,                 Pattern: @"\("))
      .Add((Type: TokenType.RParen,                     Discrete: true,  Process: null,              IsActive: null,                 Pattern: @"\)"))
      .Add((Type: TokenType.Nil,                        Discrete: true,  Process: null,              IsActive: null,                 Pattern: @"nil"))
      .Add((Type: TokenType.Dot,                        Discrete: true,  Process: null,              IsActive: null,                 Pattern: @"\."))
      .Add((Type: TokenType.CStyleChar,                 Discrete: true,  Process: ProcStringLike,    IsActive: null,                 Pattern: @"'[^']'"))
      .Add((Type: TokenType.CStyleChar,                 Discrete: true,  Process: ProcStringLike,    IsActive: null,                 Pattern: @"'\\.'"))
      .Add((Type: TokenType.Float,                      Discrete: true,  Process: ProcFloat,         IsActive: null,                 Pattern: Float))
      .Add((Type: TokenType.Rational,                   Discrete: true,  Process: ProcRational,      IsActive: null,                 Pattern: Rational))
      .Add((Type: TokenType.Integer,                    Discrete: true,  Process: ProcNumber,        IsActive: null,                 Pattern: MaybeSigned + DigitSeparatedInteger))
      .Add((Type: TokenType.Quote,                      Discrete: false, Process: null,              IsActive: null,                 Pattern: @"'"))
      .Add((Type: TokenType.Backtick,                   Discrete: false, Process: null,              IsActive: null,                 Pattern: @"`"))
      .Add((Type: TokenType.CommaAt,                    Discrete: false, Process: null,              IsActive: null,                 Pattern: @",@"))
      .Add((Type: TokenType.Comma,                      Discrete: false, Process: null,              IsActive: null,                 Pattern: @","))
      .Add((Type: TokenType.At,                         Discrete: false, Process: null,              IsActive: null,                 Pattern: @"@"))
      .Add((Type: TokenType.Dollar,                     Discrete: false, Process: null,              IsActive: null,                 Pattern: @"\$"))
      .Add((Type: TokenType.Symbol,                     Discrete: true,  Process: null,              IsActive: null,                 Pattern: Integer + @"?" + MathOp))
      .Add((Type: TokenType.Symbol,                     Discrete: true,  Process: null,              IsActive: null,                 Pattern: MathOp + Integer))
      .Add((Type: TokenType.Symbol,                     Discrete: true,  Process: null,              IsActive: null,                 Pattern: @"[\?]{3}"))
      .Add((Type: TokenType.Symbol,                     Discrete: true,  Process: null,              IsActive: null,                 Pattern: SymBody))
      .Add((Type: TokenType.Symbol,                     Discrete: true,  Process: null,              IsActive: null,                 Pattern: @"<"  + SymBody + @">"))
      .Add((Type: TokenType.Symbol,                     Discrete: true,  Process: null,              IsActive: null,                 Pattern: @"\*" + SymBody + @"\*"))
      .Add((Type: TokenType.Symbol,                     Discrete: true,  Process: null,              IsActive: null,                 Pattern: @"𝑎|𝑏|𝑐|𝑑|𝑒|𝑓|𝑚|𝑛|𝑜|𝑝|𝑞|𝑟|𝑠|𝑡|𝑢|𝑣|𝑤|𝑥|𝑦|𝑧"))
      .Add((Type: TokenType.Symbol,                     Discrete: true,  Process: null,              IsActive: null,                 Pattern: @"(?:_)|(?:=)|(?:==)|(?:!=)|(?:>=?)|(?:<=?)"))
      .Add((Type: TokenType.Symbol,                     Discrete: true,  Process: null,              IsActive: null,                 Pattern: @"¬|λ\??|∧|∨|⊤|⊥|≤|≥|×|÷|Ø|∈|∉|≠|!|∃|∄|∀|≔|\||&|~|\^|\?"))
      .Add((Type: TokenType.LispStyleChar,              Discrete: true,  Process: ProcLispStyleChar, IsActive: null,                 Pattern: @"\?\\?."))
      .Add((Type: TokenType.LineComment,                Discrete: false, Process: ProcTrimFirst,     IsActive: null,                 Pattern: @";[^\n]*"))
//      .Add((Type: TokenType.String,                     Discrete: true,  Process: ProcStringLike,    IsActive: null,                 Pattern: @"\""(\\\""|[^\""])*\"""))
      .Add((Type: TokenType.String,                     Discrete: true,  Process: null,              IsActive: NotInMultilineString, Pattern: @"\""" + StringContent+ @"\"""))
      .Add((Type: TokenType.MultilineStringBeginning,   Discrete: false, Process: BeginMLS,          IsActive: NotInMultilineString, Pattern: @"\""" + StringContent+ @"\n"))
      .Add((Type: TokenType.MultilineStringEnd,         Discrete: true,  Process: EndMLS,            IsActive: InMultilineString,    Pattern: StringContent + @"\"""))
      .Add((Type: TokenType.MultilineStringContent,     Discrete: false, Process: ProcMLSContent,    IsActive: InMultilineString,    Pattern: StringContent + @"\n"))
      .Add((Type: TokenType.InlineComment,              Discrete: false, Process: null,              IsActive: null,                 Pattern: @"#\|[^\n]*\|#"))
      .Add((Type: TokenType.MultilineCommentBeginning,  Discrete: false, Process: BeginMLC,          IsActive: null,                 Pattern: @"#\|[^\n]*\n"))
      .Add((Type: TokenType.MultilineCommentEnd,        Discrete: false, Process: EndMLC,            IsActive: InMultilineComment,   Pattern: @"[\S \t\f\v]*\|#"))
      .Add((Type: TokenType.MultilineCommentContent,    Discrete: false, Process: CountLine,         IsActive: InMultilineComment,   Pattern: @"[^\n]*\n"))
      .Add((Type: TokenType.Garbage,                    Discrete: false, Process: null,              IsActive: null,                 Pattern: @".+"));

    private static readonly ImmutableList<(string, string)> EscapedChars = ImmutableList.Create(
      (@"\a", "\a"),
      (@"\b", "\b"),
      (@"\f", "\f"),
      (@"\n", "\n"),
      (@"\r", "\r"),
      (@"\t", "\t"),
      (@"\v", "\v"),
      (@"\\", "\\"),
      (@"\'", "\'"),
      (@"\""", "\"")
      );

    private static Tokenizer _instance = new Tokenizer();

    //==================================================================================================================
    // Token callbacks
    //==================================================================================================================
    private static (AeLispTokenizerState, PositionedToken<TokenType>)
      ProcUnescapeChars((AeLispTokenizerState State, PositionedToken<TokenType> Token) tup)
    {
      var str = tup.Token.Text;
      
      foreach (var (escaped, unescaped) in EscapedChars)
        str = str.Replace(escaped, unescaped);
      
      tup.Token.Text = str;
      
      return tup;
    }
    
    private static (AeLispTokenizerState, PositionedToken<TokenType>)
      ProcStringLike((AeLispTokenizerState State, PositionedToken<TokenType> Token) tup)
      => ProcUnescapeChars((tup.State,
                            new PositionedToken<TokenType>(tup.Token.TokenType,
                                                           tup.Token.Text.Substring(1, tup.Token.Text.Length - 2),
                                                           tup.Token.Line,
                                                           tup.Token.Column)));
    
    private static (AeLispTokenizerState, PositionedToken<TokenType>)
      ProcLispStyleChar((AeLispTokenizerState State, PositionedToken<TokenType> Token) tup)
      => ProcUnescapeChars(ProcTrimFirst(tup));
    
    private static (AeLispTokenizerState, PositionedToken<TokenType>)
      ProcTrimFirst((AeLispTokenizerState State, PositionedToken<TokenType> Token) tup)
      => (tup.State, new PositionedToken<TokenType>(tup.Token.TokenType, tup.Token.Text.Substring(1), tup.Token.Line, tup.Token.Column));
    
    private static (AeLispTokenizerState, PositionedToken<TokenType>)
      ProcTrimLast((AeLispTokenizerState State, PositionedToken<TokenType> Token) tup)
      => (tup.State, new PositionedToken<TokenType>(tup.Token.TokenType, tup.Token.Text.Substring(0, tup.Token.Text.Length-2), tup.Token.Line, tup.Token.Column));
    
    private static (AeLispTokenizerState, PositionedToken<TokenType>)
      ProcNumber((AeLispTokenizerState State, PositionedToken<TokenType> Token) tup)
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
    
    private static (AeLispTokenizerState, PositionedToken<TokenType>)
      ProcFloat((AeLispTokenizerState State, PositionedToken<TokenType> Token) tup)
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
    
    private static (AeLispTokenizerState, PositionedToken<TokenType>)
      ProcRational((AeLispTokenizerState State, PositionedToken<TokenType> Token) tup)
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
    
    private static (AeLispTokenizerState, PositionedToken<TokenType>)
      ProcStripCommas((AeLispTokenizerState State, PositionedToken<TokenType> Token) tup)
    {
      tup.Token.Text = tup.Token.Text.Replace(",", "");
      
      return tup;
    }
    
    private static (AeLispTokenizerState, PositionedToken<TokenType>)
      BeginMLC((AeLispTokenizerState State, PositionedToken<TokenType> Token) tup)
    {
      tup.State.Mode = AeLispTokenizerStateMode.InMultilineComment;
      
      return CountLine(tup);
    }
    
    private static (AeLispTokenizerState, PositionedToken<TokenType>)
      EndMLC((AeLispTokenizerState State, PositionedToken<TokenType> Token) tup)
    {
      tup.State.Mode = AeLispTokenizerStateMode.Normal;
      
      return tup;
    }
    
    private static (AeLispTokenizerState, PositionedToken<TokenType>)
      BeginMLS((AeLispTokenizerState State, PositionedToken<TokenType> Token) tup)
    {
      tup = ProcUnescapeChars(CountLine(tup));
      tup.State.Mode = AeLispTokenizerStateMode.InMultilineString;
      
      return CountLine(tup);
    }
    
    private static (AeLispTokenizerState, PositionedToken<TokenType>)
      EndMLS((AeLispTokenizerState State, PositionedToken<TokenType> Token) tup)
    {
      tup = ProcUnescapeChars(CountLine(tup));
      tup.State.Mode = AeLispTokenizerStateMode.Normal;
      
      return tup;
    }
    
    private static (AeLispTokenizerState, PositionedToken<TokenType>)
      ProcMLSContent((AeLispTokenizerState State, PositionedToken<TokenType> Token) tup)
      => ProcUnescapeChars(CountLine(tup));
    
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
    private static bool InMultilineComment(AeLispTokenizerState state) => state.Mode == AeLispTokenizerStateMode.InMultilineComment;
    private static bool InMultilineString(AeLispTokenizerState state) => state.Mode == AeLispTokenizerStateMode.InMultilineString;
    private static bool Normal(AeLispTokenizerState state) => state.Mode == AeLispTokenizerStateMode.Normal;
    private static bool NotInMultilineString(AeLispTokenizerState state) => state.Mode == AeLispTokenizerStateMode.Normal;
    private static bool Always(AeLispTokenizerState state) => true;
    
    //==================================================================================================================
    // Patterns are down here since they confuse csharp-mode's indentation logic:
    //==================================================================================================================
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
    private const string StringContent = @"(?:\\\""|[^\""\n])*";                                                 
  }
}
