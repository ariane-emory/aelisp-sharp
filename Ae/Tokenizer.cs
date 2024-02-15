using static System.Console;
using System.Text.RegularExpressions;
using System.Collections.Immutable;

//====================================================================================================================================================
static partial class Ae
{
  //==================================================================================================================================================
  // Public types
  //==================================================================================================================================================
  public enum TokenType
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
    MultilineCommentBeginning,
    MultilineCommentEnd,
    MultilineCommentContent,
    MultilineStringBeginning,
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
  public record struct Token(TokenType TokenType, string Text, int Line, int Column)
  {
    public override string ToString() => $"{TokenType} [{Text}] @ {Line},{Column}";
  }

  //==================================================================================================================================================
  public record struct TokenizerState(int Line = 0, int Column = 0, TokenizerMode Mode = TokenizerMode.Normal);

  //==================================================================================================================================================
  // Ae's static field
  //==================================================================================================================================================
  public static readonly ImmutableArray<TokenType> ExcludedTokenTypes = ImmutableArray.Create(
    TokenType.Whitespace,
    TokenType.LineComment,
    TokenType.MultilineCommentBeginning,
    TokenType.MultilineCommentContent,
    TokenType.MultilineCommentEnd,
    TokenType.Comment,
    TokenType.Newline);

  //==================================================================================================================================================
  // Ae's static methods
  //==================================================================================================================================================
  public static bool IsExcludedTokenType(Ae.Token token) => ExcludedTokenTypes.Contains(token.TokenType);
  public static bool IsIncludedTokenType(Ae.Token token) => !IsExcludedTokenType(token);

  //==================================================================================================================================================
  // Tokenizer class
  //==================================================================================================================================================
  public class Tokenizer : StringTokenizer<TokenType, Token, TokenizerState>
  {
    //================================================================================================================================================
    // Private constructor
    //================================================================================================================================================
    private Tokenizer() : base(createToken: (tokenType, text) => new Token(tokenType, text, 0, 0),
                               createTokenizerStateFun: () => new TokenizerState())
    {
      foreach (var (tokenType, discrete, process, active, pattern) in Tokens)
        Add(tokenType,
            discrete ? (pattern + FollowedByTokenBarrierOrEOF) : (pattern),
            process is null
              ? CountColumns
              : ((TokenizerState State, Token Token) tup) => process(CountColumns(tup)),
            active is null ? Normal : active);
    }

    //================================================================================================================================================
    // Private constants
    //================================================================================================================================================
    private static readonly
      ImmutableArray<(TokenType Type,
                      bool Discrete,
                      ProcesTokenFun? Process,
                      TokenDefinitionIsActiveFun? IsActive,
                      string Pattern)> Tokens =
      ImmutableArray<(TokenType Type, bool Discrete, ProcesTokenFun? Process, TokenDefinitionIsActiveFun? IsActive, string Pattern)>.Empty
      .Add((Type: TokenType.Newline, Discrete: false, Process: ProcCountLine, IsActive: null, Pattern: @"\r?\n"))
      .Add((Type: TokenType.Whitespace, Discrete: false, Process: null, IsActive: null, Pattern: @"[ \t\f\v]+"))
      .Add((Type: TokenType.LParen, Discrete: false, Process: null, IsActive: null, Pattern: @"\("))
      .Add((Type: TokenType.RParen, Discrete: true, Process: null, IsActive: null, Pattern: @"\)"))
      .Add((Type: TokenType.Nil, Discrete: true, Process: null, IsActive: null, Pattern: @"nil"))
      .Add((Type: TokenType.Dot, Discrete: true, Process: null, IsActive: null, Pattern: @"\."))
      .Add((Type: TokenType.CStyleChar, Discrete: true, Process: ProcStringLike, IsActive: null, Pattern: @"'[^']'"))
      .Add((Type: TokenType.CStyleChar, Discrete: true, Process: ProcStringLike, IsActive: null, Pattern: @"'\\.'"))
      .Add((Type: TokenType.Float, Discrete: true, Process: ProcFloat, IsActive: null, Pattern: Float))
      .Add((Type: TokenType.Rational, Discrete: true, Process: ProcRational, IsActive: null, Pattern: Rational))
      .Add((Type: TokenType.Integer, Discrete: true, Process: ProcNumber, IsActive: null, Pattern: MaybeSigned + DigitSeparatedInteger))
      .Add((Type: TokenType.Quote, Discrete: false, Process: null, IsActive: null, Pattern: @"'"))
      .Add((Type: TokenType.Backtick, Discrete: false, Process: null, IsActive: null, Pattern: @"`"))
      .Add((Type: TokenType.CommaAt, Discrete: false, Process: null, IsActive: null, Pattern: @",@"))
      .Add((Type: TokenType.Comma, Discrete: false, Process: null, IsActive: null, Pattern: @","))
      .Add((Type: TokenType.At, Discrete: false, Process: null, IsActive: null, Pattern: @"@"))
      .Add((Type: TokenType.Dollar, Discrete: false, Process: null, IsActive: null, Pattern: @"\$"))
      .Add((Type: TokenType.Symbol, Discrete: true, Process: null, IsActive: null, Pattern: Integer + @"?" + MathOp))
      .Add((Type: TokenType.Symbol, Discrete: true, Process: null, IsActive: null, Pattern: MathOp + Integer))
      .Add((Type: TokenType.Symbol, Discrete: true, Process: null, IsActive: null, Pattern: @"[\?]{3}"))
      .Add((Type: TokenType.Symbol, Discrete: true, Process: null, IsActive: null, Pattern: SymBody))
      .Add((Type: TokenType.Symbol, Discrete: true, Process: null, IsActive: null, Pattern: @"<" + SymBody + @">"))
      .Add((Type: TokenType.Symbol, Discrete: true, Process: null, IsActive: null, Pattern: @"\*" + SymBody + @"\*"))
      .Add((Type: TokenType.Symbol, Discrete: true, Process: null, IsActive: null, Pattern: @"𝑎|𝑏|𝑐|𝑑|𝑒|𝑓|𝑚|𝑛|𝑜|𝑝|𝑞|𝑟|𝑠|𝑡|𝑢|𝑣|𝑤|𝑥|𝑦|𝑧"))
      .Add((Type: TokenType.Symbol, Discrete: true, Process: null, IsActive: null, Pattern: @"(?:_)|(?:=)|(?:==)|(?:!=)|(?:>=?)|(?:<=?)"))
      .Add((Type: TokenType.Symbol, Discrete: true, Process: null, IsActive: null, Pattern: @"¬|λ\??|∧|∨|⊤|⊥|≤|≥|×|÷|Ø|∈|∉|≠|!|∃|∄|∀|≔|\||&|~|\^|\?"))
      .Add((Type: TokenType.LispStyleChar, Discrete: true, Process: ProcLispStyleChar, IsActive: null, Pattern: @"\?\\?."))
      .Add((Type: TokenType.String, Discrete: true, Process: ProcStringLike, IsActive: null, Pattern: @"\""" + StringContent + @"\"""))
      .Add((Type: TokenType.MultilineStringBeginning, Discrete: false, Process: ProcBeginMLS, IsActive: null, Pattern: @"\""" + StringContent + @"\n"))
      .Add((Type: TokenType.MultilineStringEnd, Discrete: true, Process: ProcEndMLS, IsActive: InMultilineString, Pattern: StringContent + @"\"""))
      .Add((Type: TokenType.MultilineStringContent, Discrete: false, Process: ProcMLSContent, IsActive: InMultilineString, Pattern: StringContent + @"\n"))
      .Add((Type: TokenType.LineComment, Discrete: false, Process: ProcTrimFirst, IsActive: null, Pattern: @";[^\n]*"))
      .Add((Type: TokenType.Comment, Discrete: false, Process: null, IsActive: null, Pattern: @"#\|[^\n]*\|#"))
      .Add((Type: TokenType.MultilineCommentBeginning, Discrete: false, Process: ProcBeginMLC, IsActive: null, Pattern: @"#\|[^\n]*\n"))
      .Add((Type: TokenType.MultilineCommentEnd, Discrete: false, Process: ProcEndMLC, IsActive: InMultilineComment, Pattern: @"[\S \t\f\v]*\|#"))
      .Add((Type: TokenType.MultilineCommentContent, Discrete: false, Process: ProcCountLine, IsActive: InMultilineComment, Pattern: @"[^\n]*\n"));
      //.Add((Type: TokenType.Garbage,                   Discrete: false, Process: null,              IsActive: null,               Pattern: @".+"));

    //================================================================================================================================================
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
    public static Tokenizer Instance { get; } = new Tokenizer();

    //================================================================================================================================================
    // Token callbacks
    //================================================================================================================================================
    private static (TokenizerState, Token)
      UnescapeChars((TokenizerState State, Token Token) tup)
    {
      var str = tup.Token.Text;

      foreach (var (escaped, unescaped) in EscapedChars)
        str = str.Replace(escaped, unescaped);

      tup.Token.Text = str;

      return tup;
    }
    
    //================================================================================================================================================
    private static (TokenizerState, Token)
      ProcStringLike((TokenizerState State, Token Token) tup)
      => UnescapeChars((tup.State,
                        new Token(tup.Token.TokenType,
                                  tup.Token.Text.Substring(1, tup.Token.Text.Length - 2),
                                  tup.Token.Line,
                                  tup.Token.Column)));
    
    //================================================================================================================================================
    private static (TokenizerState, Token)
      ProcLispStyleChar((TokenizerState State, Token Token) tup)
      => UnescapeChars(ProcTrimFirst(tup));
    
    //================================================================================================================================================
    private static (TokenizerState, Token)
      ProcTrimFirst((TokenizerState State, Token Token) tup)
      => (tup.State,
          new Token(tup.Token.TokenType, tup.Token.Text.Substring(1), tup.Token.Line, tup.Token.Column));

    //================================================================================================================================================
    private static (TokenizerState, Token)
      TrimLast((TokenizerState State, Token Token) tup)
      => (tup.State,
          new Token(tup.Token.TokenType,
                    tup.Token.Text.Substring(0, tup.Token.Text.Length - 1), tup.Token.Line, tup.Token.Column));
    
    //================================================================================================================================================
    private static (TokenizerState, Token)
      ProcNumber((TokenizerState State, Token Token) tup)
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
    private static (TokenizerState, Token)
      ProcFloat((TokenizerState State, Token Token) tup)
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
    private static (TokenizerState, Token)
      ProcRational((TokenizerState State, Token Token) tup)
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
    private static (TokenizerState, Token)
      ProcStripCommas((TokenizerState State, Token Token) tup)
    {
      tup.Token.Text = tup.Token.Text.Replace(",", "");

      return tup;
    }

    //================================================================================================================================================
    // Token callbacks (multiline comments)
    //================================================================================================================================================
    private static (TokenizerState, Token)
      ProcBeginMLC((TokenizerState State, Token Token) tup)
    {
      tup.State.Mode = TokenizerMode.InMultilineComment;

      return ProcCountLine(tup);
    }

    //================================================================================================================================================
    private static (TokenizerState, Token)
      ProcEndMLC((TokenizerState State, Token Token) tup)
    {
      tup.State.Mode = TokenizerMode.Normal;

      return tup;
    }

    //================================================================================================================================================
    // Token callbacks (multiline strings)
    //================================================================================================================================================
    private static (TokenizerState, Token) ProcBeginMLS((TokenizerState State, Token Token) tup)
    {
      tup = ProcTrimFirst(UnescapeChars(ProcCountLine(tup)));
      tup.State.Mode = TokenizerMode.InMultilineString;

      return tup;
    }

    //================================================================================================================================================
    private static (TokenizerState, Token)
      ProcEndMLS((TokenizerState State, Token Token) tup)
    {
      tup = TrimLast(UnescapeChars(tup));
      tup.State.Mode = TokenizerMode.Normal;

      return tup;
    }

    //================================================================================================================================================
    private static (TokenizerState, Token)
      ProcMLSContent((TokenizerState State, Token Token) tup)
      => UnescapeChars(ProcCountLine(tup));

    //================================================================================================================================================
    private static (TokenizerState, Token)
      ProcCountLine((TokenizerState State, Token Token) tup)
    {
      tup.State.Line++;
      tup.State.Column = 0;

      return tup;
    }
    
    //================================================================================================================================================
    private static (TokenizerState, Token)
      CountColumns((TokenizerState State, Token Token) tup)
    {
      tup = SetTokenLinesAndColumns(tup);

      tup.State.Column += tup.Token.Text.Length;

      return tup;
    }

    //================================================================================================================================================
    private static (TokenizerState, Token)
      SetTokenLinesAndColumns((TokenizerState State, Token Token) tup)
    {
      tup.Token.Line = tup.State.Line;
      tup.Token.Column = tup.State.Column;

      return tup;
    }

    //================================================================================================================================================
    // 'Is active?' callbacks
    //================================================================================================================================================
    private static bool InMultilineComment(TokenizerState state) => state.Mode == TokenizerMode.InMultilineComment;
    private static bool InMultilineString(TokenizerState state) => state.Mode == TokenizerMode.InMultilineString;
    private static bool Normal(TokenizerState state) => state.Mode == TokenizerMode.Normal;

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
