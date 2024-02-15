//======================================================================================================================
static partial class Ae
{
  public static bool EnableDebugWrite { get; set; } = false;
  
  public static void DebugWrite(string s) {
    if (EnableDebugWrite)
      Console.WriteLine(s);
  }
  
  public static void Die(int exitCode, string message)
    {
    Console.Error.WriteLine(message);
    Environment.Exit(exitCode);
  }

  public static string ReplaceNewlinesWithEscaped(this string input) => input.Replace("\n", "\\n");

  //====================================================================================================================
}
