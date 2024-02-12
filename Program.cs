using static Utility;
using static Ae;
using static System.Console;

class Program
{
  static void Main()
   {
      var tokens = Tokenizer.Get().Tokenize(@"one two
                                            three");
//         @"one two
// three (123 457 *bingo* <boop> nil (789 nil)) 12 34 (1 . (2 3)) 'nil '123 "
//         + @"`(1 ,2 3) $(1 2 3) (1 2 ,@(4 5)) ?a ?\n ""he\""llo"" "
//         + @" ""one \""two\"" three"" -05 1,234 2,3,4 1/3 -3/5 -4/1,200 ??? "
//         + @"here--it/is! %another-symbol1-23//asd 1+ 10- ∨ ⊤ ⊥ ≤ ≥ × 12.004 "
//         + @"-.24 -0.43 *bingo* bang!! <Bob'sAge> 1,024.4 a.b_c.2
//  pi_is_3.14 "
//         + @">> <= 1- 2- -2 - %2 2% 02,345 1,234 -1,234 >>2 <<3 2>> 3<< << >> "
//         + @"a::b/c :keyword @
// &rest -a / !abc ! <!good> 'a' '\n' '\""' "
//         + @" ""hello\nworld"" "
//         + @" ?a ?\n ");
// //        + @"\#!2garbage");

      // Die if we tokenized nothing:
      if (! tokens.Any())
         Die(1, "No tokens!");

      // Print out the tokenized tokens:
      foreach (var (token, index) in tokens
        .Where(token => token.TokenType != TokenType.Whitespace)
        .Select((value, index) => (value, index)))
        WriteLine(token);

      var lastToken = tokens.ToList()[tokens.Count()-1];

      // Die if we didn't tokenize everything:
      // if (lastToken.TokenType == TokenType.Garbage)
      //    Die(1, $"Failed to tokenize the entire input, remaining text: \"{lastToken.Text}\"");

      Pair properList = Cons(new Symbol("one"), Cons(new Symbol("two"), Cons(new Symbol("three"), Nil)));
      Pair improperList = Cons(new Symbol("one"), Cons(new Integer(37), Cons(new Rational(3, 4), new Symbol("four"))));

      WriteLine(properList);
      WriteLine(Write(properList));

      foreach (var obj in properList)
        WriteLine(obj);

      WriteLine(improperList);
      WriteLine(Write(improperList));

      foreach (var obj in improperList)
        WriteLine(obj);

      WriteLine(Write(new Lambda(Nil, Nil, Nil)));
      WriteLine(Write(new Lambda(Nil, Nil, Nil)));
      WriteLine(Write(new Lambda(Nil, Nil, Nil)));
      
      WriteLine("Done.");

      Ae.Object symbolsList = Cons(new Symbol("one"), Cons(new Symbol("two"), Cons(new Symbol("three"), Nil)));

      Intern(ref symbolsList, "two");
      WriteLine(symbolsList);
      Intern(ref symbolsList, "four");
      WriteLine(symbolsList);
      Intern(ref symbolsList, "two");
      WriteLine(symbolsList);
      Intern(ref symbolsList, "four");
      WriteLine(symbolsList);
   }
}

