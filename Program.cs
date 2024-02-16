﻿using System.Collections.Immutable;
using System.Text.RegularExpressions;
using static System.Console;
using static Ae;
using Pidgin;
using static Pidgin.Parser;
using static Pidgin.Parser<Ae.Token>;
// using TokenParser = Pidgin.Parser<Ae.Token>;

//======================================================================================================================
class Program
{
  //====================================================================================================================
  static Parser<Token, Token> TokenWithTokenType(TokenType tokenType) => Parser<Token>.Token(t => t.TokenType == tokenType);

  //====================================================================================================================
  static readonly ImmutableArray<TokenType> MultilineCommentTokenTypes =
    ImmutableArray.Create(TokenType.MultilineCommentBegin, TokenType.MultilineCommentContent, TokenType.MultilineCommentEnd);
  static readonly ImmutableArray<TokenType> MultilineStringTokenTypes =
    ImmutableArray.Create(TokenType.MultilineStringBegin, TokenType.MultilineStringContent, TokenType.MultilineStringEnd);

  static readonly Parser<Token, Token> MultilineCommentBegin = TokenWithTokenType(TokenType.MultilineCommentBegin);
  static readonly Parser<Token, Token> MultilineCommentContent = TokenWithTokenType(TokenType.MultilineCommentContent);
  static readonly Parser<Token, Token> MultilineCommentEnd = TokenWithTokenType(TokenType.MultilineCommentEnd);
  static readonly Parser<Token, Token> MultilineStringBegin = TokenWithTokenType(TokenType.MultilineStringBegin);
  static readonly Parser<Token, Token> MultilineStringContent = TokenWithTokenType(TokenType.MultilineStringContent);
  static readonly Parser<Token, Token> MultilineStringEnd = TokenWithTokenType(TokenType.MultilineStringEnd);
  static readonly Parser<Token, Token> SomeOtherToken = Token(t => (!MultilineCommentTokenTypes.Contains(t.TokenType))
                                                              && (!MultilineStringTokenTypes.Contains(t.TokenType)));

  static readonly Parser<Token, Token> MultilineComment =
    from begin in MultilineCommentBegin
    from contents in MultilineCommentContent.Many()
    from end in MultilineCommentEnd
    select new Token(
      TokenType.Comment,
      string.Join("", new[] { begin }.Concat(contents).Append(end).Select(t => t.Text)),
      begin.Line,
      begin.Column
      );

  static readonly Parser<Token, Token> MultilineString =
    from begin in MultilineStringBegin
    from contents in MultilineStringContent.Many()
    from end in MultilineStringEnd
    select new Token(
      TokenType.String,
      string.Join("", new[] { begin }.Concat(contents).Append(end).Select(t => t.Text)),
      begin.Line,
      begin.Column
      );

  static readonly Parser<Token, IEnumerable<Token>> ConsolidateMultilineTokens = OneOf(
    SomeOtherToken,
    MultilineComment,
    MultilineString
    ).Many();
  
  //====================================================================================================================
  static void Main()
  {
    //==================================================================================================================

    var filename = "data/data.lisp";
    var fileText = File.ReadAllText(filename);
    var stream = new TokenStream(fileText, exclude: IsUninterestingToken);
    var tokens = stream.ReadAll();

    WriteLine("\nBefore parse: ");
    tokens.Print(); // tokens are printed correctly..

    var result = ConsolidateMultilineTokens.ParseOrThrow(tokens);

    WriteLine("\nResult of parse: ");
    result.Print();
  }

  //====================================================================================================================
}
