static class Utility
{
   public static void Die(int exitCode, string message)
   {
      Console.Error.WriteLine(message);
      Environment.Exit(exitCode);
   }
}
