﻿using static Utility;
using static Ae;
using static System.Console;

class Program
{
  static void PrintTokens(List<PositionedToken<TokenType>> tokens)
  {
    foreach (var (token, index) in tokens
     .Where(token => token.TokenType != TokenType.Whitespace)
     .Select((value, index) => (value, index)))
      WriteLine($"#{index}: {token}");
  }

  static bool CheckForGarbage(List<PositionedToken<TokenType>> tokens) =>
    (!tokens.Any()) || tokens[Math.Max(0, tokens.Count() - 1)].TokenType == TokenType.Garbage;

  record TokenizeResult(List<PositionedToken<TokenType>> Tokens, bool TokenizedAllInput);

  static TokenizeResult TokenizeLines(IEnumerable<string> lines)
  {
    var tokens = new List<PositionedToken<TokenType>>();

    var lineNumber = 0;

    foreach (var line in lines)
    {
      var result = TokenizeLine(line);

      tokens.AddRange(result.Tokens.Select(t => new PositionedToken<TokenType>(t.TokenType, t.Text, lineNumber, t.Column)).ToList());

      lineNumber++;
    }

    return new TokenizeResult(tokens, true);
  }

  static TokenizeResult TokenizeLine(string line)
  {
    var tokens = Tokenizer.Get().Tokenize(line, false).ToList();

    PrintTokens(tokens);

    if (CheckForGarbage(tokens))
      return new TokenizeResult(tokens, false);

    return new TokenizeResult(tokens, true);
  }

  enum Mode
  {
    LineByLine,
    EntireFileAtOnce,
  };

  static void Main()
  {
    var filename = "data.lisp";

    foreach (var mode in new[] { Mode.LineByLine, Mode.EntireFileAtOnce })
    {
      var tokenizeResult = mode switch
      {
        Mode.LineByLine => TokenizeLines(File.ReadAllLines(filename).Select(s => s + "\n")),
        Mode.EntireFileAtOnce => TokenizeLine(File.ReadAllText(filename)),
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
      };

      WriteLine($"Token count: {tokenizeResult.Tokens.Count}.\n\n");
    }
  }

  // Die(0, $"Tokenized all input, {totalTokens} tokens.");

  // Pair properList = Cons(new Symbol("one"), Cons(new Symbol("two"), Cons(new Symbol("three"), Nil)));
  // Pair improperList = Cons(new Symbol("one"), Cons(new Integer(37), Cons(new Rational(3, 4), new Symbol("four"))));

  // WriteLine(properList);
  // WriteLine(Write(properList));

  // foreach (var obj in properList)
  //   WriteLine(obj);

  // WriteLine(improperList);
  // WriteLine(Write(improperList));

  // foreach (var obj in improperList)
  //   WriteLine(obj);

  // WriteLine(Write(new Lambda(Nil, Nil, Nil)));
  // WriteLine(Write(new Lambda(Nil, Nil, Nil)));
  // WriteLine(Write(new Lambda(Nil, Nil, Nil)));

  // WriteLine("Done.");

  // Ae.Object symbolsList = Cons(new Symbol("one"), Cons(new Symbol("two"), Cons(new Symbol("three"), Nil)));

  // Intern(ref symbolsList, "two");
  // WriteLine(symbolsList);
  // Intern(ref symbolsList, "four");
  // WriteLine(symbolsList);
  // Intern(ref symbolsList, "two");
  // WriteLine(symbolsList);
  // Intern(ref symbolsList, "four");
  // WriteLine(symbolsList);
  // }
}

