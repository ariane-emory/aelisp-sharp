using System.Text.RegularExpressions;

//======================================================================================================================
static partial class Ae
{
  public static void Die(int exitCode, string message)
    {
    Console.Error.WriteLine(message);
    Environment.Exit(exitCode);
  }

  public static string ReplaceNewlinesWithEscaped(this string input) => input.Replace("\n", "\\n");
  
  //====================================================================================================================
}
