using static Utility;

class Program
{
   static void Main()
   {
      var tokens = new Ae.Tokenizer().Tokenize(
        @"(123 457 *bingo* <boop> nil (789 nil)) 12 34 (1 . (2 3)) 'nil '123 "
        + @"`(1 ,2 3) $(1 2 3) (1 2 ,@(4 5)) ?a 'b' '\n' ?\n ""he\""llo"" "
        + @" ""one \""two\"" three"" -05 1,234 2,3,4 1/3 -3/5 -4/1,200 ??? "
        + @"here--it/is! %another-symbol1-23//asd 1+ 10- ∨ ⊤ ⊥ ≤ ≥ × 12.004 "
        + @"-.24 -0.43 *bingo* bang!! <Bob'sAge> 1,024.4 a.b_c.2 pi_is_3.14 "
        + @">> <= 1- 2- -2 - %2 2% 02,345 1,234 -1,234 >>2 <<3 2>> 3<< << >> "
        + @"a::b/c :keyword @ &rest -a / !abc ! <!good> \#!2garbage");

      // Die if we tokenized nothing:
      if (! tokens.Any())
         Die(1, "No tokens!");

      // Print out the tokenized tokens:
      foreach (var (token, index) in tokens
         .Where(token => token.TokenType != Ae.TokenType.Whitespace)
         .Select((value, index) => (value, index)))
         Console.WriteLine($"#{index}: {token.TokenType} [{token.Text}]");

      var lastToken = tokens.ToList()[tokens.Count()-1];

      // Die if we didn't tokenize everything:
      // if (lastToken.TokenType == Ae.TokenType.Garbage)
      //    Die(1, $"Failed to tokenize the entire input, remaining text: \"{lastToken.Text}\"");

      // Exit successfully.
      Console.WriteLine("Done!");

      Ae.LispCons properList = Ae.Cons(new Ae.LispSymbol("one"), Ae.Cons(new Ae.LispSymbol("two"), Ae.Cons(new Ae.LispSymbol("three"), Ae.Nil)));
      Ae.LispCons improperList = Ae.Cons(new Ae.LispSymbol("one"), Ae.Cons(new Ae.LispSymbol("two"), Ae.Cons(new Ae.LispSymbol("three"), new Ae.LispSymbol("four"))));        

      Console.WriteLine(properList);
      Console.WriteLine(Ae.Write(properList));
      foreach (var obj in properList)
        Console.WriteLine(obj);

      Console.WriteLine(improperList);
      Console.WriteLine(Ae.Write(improperList));
      foreach (var obj in improperList)
        Console.WriteLine(obj);

   }
}

