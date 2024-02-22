using static System.Console;
using System.Collections;

//======================================================================================================================
static partial class Ae
{
  //====================================================================================================================
  // plist related methods
  //====================================================================================================================
   public static bool PlistContains(LispObject plist, LispObject key)
   {
      if (!plist.IsList)
         throw new ArgumentException($"plist must be a list, not {plist}!");
      
      // Check if plist is a Pair and proceed if true, otherwise return false if it's not a Pair (e.g., if it's Nil).
      if (plist is Pair plistHeadPair)
      {
         if (plistHeadPair.Length % 2 != 0)
         throw new ArgumentException($"plist must have an even number of elements, not {plistHeadPair.Length}!");
         
         // Your logic to check if the plist contains the key.
         // Iterate over the plist assuming it's a properly structured property list.
         LispObject current = plist;
         while (current is Pair currentPair)
         {
            // Assuming plist structure is (key1, value1, key2, value2, ...)
            LispObject currentKey = currentPair.Car;
            if (currentKey.Equals(key))
               return true; // Key found.
            
            // Move to next key-value pair (skip current value and go to next key).
            current = (currentPair.Cdr is Pair cdrPair) ? cdrPair.Cdr : Nil;
         }
      }
      else
      {
         // plist is not a Pair, e.g., it's Nil.
         return false;
      }
      
      return false; // Key not found.
   }
   
}
//=====================================================================================================================

