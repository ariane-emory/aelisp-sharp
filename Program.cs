using static Utility;
using static Ae;
using static System.Console;

class Program
{
  static void Main()
   {
     var filename = "data.lisp";

     if (! File.Exists(filename))
       throw new ApplicationException($"No file '{filename}'.");

     var tokens = new List<PositionedToken<TokenType>>();
     var input = File.ReadAllLines(filename);

     foreach (var (line, lineNo) in input.Select((line, lineNo) => (line, lineNo)))
       tokens.AddRange(Tokenizer.Get().Tokenize(line).Select(t => new PositionedToken<TokenType>(t.TokenType, t.Text, lineNo, t.Column)));

      // Die if we tokenized nothing:
      if (! tokens.Any())
         Die(1, "No tokens!");

      // Print out the tokenized tokens:
      foreach (var (token, index) in tokens
               // .Where(token => token.TokenType != TokenType.Whitespace)
        .Select((value, index) => (value, index)))
        WriteLine($"#{index}: {token}");

      var lastToken = tokens.ToList()[tokens.Count()-1];

      // Die if we didn't tokenize everything:
      if (lastToken.TokenType == TokenType.Garbage)
         Die(1, $"Failed to tokenize the entire input, remaining text: \"{lastToken.Text}\"");

      Die(0, "Stop.");
                                            
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

