﻿using System.Collections.Immutable;
using static System.Console;
using static Ae;
using static Utility;
using Pidgin;
using static Pidgin.Parser;
using static Pidgin.Parser<Ae.PositionedToken<Ae.TokenType>>; // no error
using AeToken = Ae.PositionedToken<Ae.TokenType>;

class Program
{
  static readonly ImmutableArray<TokenType> ExcludedTokenTypes = ImmutableArray.Create(
    TokenType.Whitespace,
    TokenType.LineComment,
    TokenType.MultilineCommentBeginning,
    TokenType.MultilineCommentContent,
    TokenType.MultilineCommentEnd,
    TokenType.Comment,
    TokenType.Newline);

  //====================================================================================================================
  static bool IsExcludedTokenType(AeToken token) => ExcludedTokenTypes.Contains(token.TokenType);
  static bool IsIncludedTokenType(AeToken token) => !IsExcludedTokenType(token);

  //====================================================================================================================
  static void PrintTokens(IEnumerable<AeToken> tokens)
  {
    foreach (var (token, index) in tokens
             //.Where(IsIncludedTokenType)
             .Select((value, index) => (value, index)))
      WriteLine($"#{index}: {token}");
  }

  //====================================================================================================================
  static (string? Input, AeLispTokenizerState State, List<AeToken> Tokens)
  TokenizeAndPrintLine(string? input, AeLispTokenizerState? state = null)
  {
    var tokens = new List<AeToken>();

    while (!string.IsNullOrEmpty(input)) // infinite loop risk, fix!
    {
      var (newInput, newState, newToken) = Tokenizer.Get().NextToken(input, state);

      if (newToken is null)
        throw new ApplicationException($"No token at \"{newInput}\"!");

      (input, state) = (newInput, newState);

      tokens.Add(newToken.Value);
    }

    PrintTokens(tokens);

    return (input, state!.Value, tokens);
  }

  //====================================================================================================================
  static (string? Input, AeLispTokenizerState State, List<AeToken> Tokens)
  TokenizeAndPrintLines(IEnumerable<string> lines, AeLispTokenizerState? state = null)
  {
    var tokens = new List<AeToken>();

    foreach (var line in lines)
    {
      var (leftoverInput, newState, newTokens) = TokenizeAndPrintLine(line, state);
      state = newState;

      if (!string.IsNullOrEmpty(leftoverInput))
        throw new ApplicationException($"leftover input \"{leftoverInput}\".");

      tokens.AddRange(newTokens);
    }

    return (null, state!.Value, tokens);
  }

  //====================================================================================================================
  enum Mode { LineByLine, EntireFileAtOnce, };

  //====================================================================================================================
  static void DoTokenizeAndPrintLinesTests(string filename)
  {
    foreach (var mode in new[] {
        Mode.LineByLine,
        Mode.EntireFileAtOnce
      })
    {
      var tokenizeResult = mode switch
      {
        Mode.LineByLine => TokenizeAndPrintLines(File.ReadAllLines(filename).Select(s => s + "\n"), null),
        Mode.EntireFileAtOnce => TokenizeAndPrintLine(File.ReadAllText(filename), null),
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
      };

      WriteLine($"Token count: {tokenizeResult.Tokens.Count}");
      WriteLine($"\n\n---\n\n");
    }
  }

  //====================================================================================================================
  public class AeLispTokenizerTokenStream : Pidgin.ITokenStream<AeToken>
  {
    public void Return(ReadOnlySpan<AeToken> leftovers) { }
    public int ChunkSizeHint => 16;

    private string? _input;
    private readonly Func<AeToken, bool>? _exclude;
    private AeLispTokenizerState? _state;
    private readonly Queue<AeToken> _queued;

    public AeLispTokenizerTokenStream(string input, Func<AeToken, bool>? exclude = null)
    {
      _input = input;
      _exclude = exclude;
      _queued = new Queue<AeToken>();
    }

    public int Read(Span<AeToken> buffer)
    {
      var ix = 0;

      for (; ix < buffer.Length; ix++)
      {
        var token = Next();

        if (token is null)
          break;

        buffer[ix] = token.Value;
      }

      return ix;
    }

    private AeToken? Next()
    {
      Next:
      var (newInput, newState, newToken) = Tokenizer.Get().NextToken(_input, _state);
      (_state, _input) = (newState, newInput);

      if (newToken is not null && _exclude is not null && _exclude(newToken.Value))
        goto Next;

      return newToken;
    }

    // private AeToken? Next()
    // {
    //   while (true)
    //   {
    //     var (newInput, newState, newToken) = Tokenizer.Get().NextToken(_input, _state);
    //     (_state, _input) = (newState, newInput);

    //     if (newToken is null || (_exclude is not null && !_exclude(newToken.Value)))
    //       return newToken;
    //   }
    // }
  }

  //====================================================================================================================
  static void Main()
  {
    var filename = "data.lisp";
    var fileText = File.ReadAllText(filename);
    var stream = new AeLispTokenizerTokenStream(fileText, IsExcludedTokenType);
    var take = 32;
    var ary = new AeToken[take];
    var read = stream.Read(ary);
    PrintTokens(ary.Take(read));
  }

  //====================================================================================================================
}

