using System.Text.RegularExpressions;

static class Utility
{
  public static void Die(int exitCode, string message)
  {
    Console.Error.WriteLine(message);
    Environment.Exit(exitCode);
  }

  public static string ReplaceTrailingNewlinesWithEscaped(this string input)
  {
    var match = Regex.Match(input, @"\n+$");

    if (!match.Success)
      return input; // No trailing newlines to replace

    int trailingNewlineCount = match.Value.Length;
    string result = input.TrimEnd('\n');

    for (int i = 0; i < trailingNewlineCount; i++)
      result += "\\n";

    return result;
  }

}
