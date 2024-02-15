using static System.Console;
using static Ae;
using static Utility;

//======================================================================================================================
class Program
{
  //====================================================================================================================
  static void Main()
  {
    var filename = "data/data.lisp";
    var fileText = File.ReadAllText(filename);
    var stream = new QueueingTokenStream(fileText, exclude: Ae.IsUninterestingToken);
    var take = 32;
    var ary = new Token[take];
    var read = stream.Read(ary);
    ary.Take(read).Print();
  }

  //====================================================================================================================
}

