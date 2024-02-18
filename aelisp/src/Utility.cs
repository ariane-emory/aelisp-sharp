using System.Collections.Immutable;

//======================================================================================================================
static partial class Ae
{
   //====================================================================================================================
   // Private static fields
   //====================================================================================================================
   private static readonly ImmutableArray<(string Escaped, string Unescaped)> EscapedChars = ImmutableArray.Create(
     (@"\a", "\a"),
     (@"\b", "\b"),
     (@"\f", "\f"),
     (@"\n", "\n"),
     (@"\r", "\r"),
     (@"\t", "\t"),
     (@"\v", "\v"),
     // (@"\'", "\'"),
     (@"\""", "\"")
     //(@"\\", "\\")
     );

   //====================================================================================================================
   // Public properties
   //====================================================================================================================
   public static bool EnableDebugWrite { get; set; } = false;

   //====================================================================================================================
   // Public static methods
   //====================================================================================================================
   private static void DebugWrite(string s)
   {
      if (EnableDebugWrite)
         Console.WriteLine(s);
   }

   //====================================================================================================================
   public static void Die(int exitCode, string message)
   {
      Console.Error.WriteLine(message);
      Environment.Exit(exitCode);
   }

   //====================================================================================================================
   public static int GCD(int a, int b)
   {
      while (b != 0)
      {
         int temp = b;
         b = a % b;
         a = temp;
      }
      return a;
   }

   //====================================================================================================================
   // String extension methods
   //====================================================================================================================
   private static string UnescapeChars(this string self)
   {
      foreach (var (escaped, unescaped) in EscapedChars)
         self = self.Replace(escaped, unescaped);

      return self;
   }

   private static string EscapeChars(this string self)
   {
      foreach (var (escaped, unescaped) in EscapedChars)
         self = self.Replace(unescaped, escaped);

      return self;
   }

   //====================================================================================================================
   private static string ReplaceNewlinesWithEscaped(this string input) => input.Replace("\n", "\\n");

   //====================================================================================================================
   private static string Trim(this string input, bool trim) => trim ? input.Trim() : input;

   //====================================================================================================================
   private static string FirstLine(this string input)
   {
      var newlineIndex = input.IndexOf('\n');

      if (newlineIndex == -1)
         return input;

      return input.Substring(0, newlineIndex);
   }

   //====================================================================================================================
   public static string ExpandTilde(this string path)
   {
      if (string.IsNullOrEmpty(path) || !path.StartsWith("~"))
         return path;

      var homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
      return Path.Combine(homeDirectory, path.TrimStart('~').TrimStart('/', '\\'));
   }

   //====================================================================================================================
}
