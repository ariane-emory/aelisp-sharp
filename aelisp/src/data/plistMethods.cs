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
      
      if (plist is Pair plistHeadPair)
      {
         if (plistHeadPair.Length % 2 != 0)
         throw new ArgumentException($"plist must have an even number of elements, not {plistHeadPair.Length}!");
         
         LispObject current = plist;

         while (current is Pair currentPair)
         {

            LispObject currentKey = currentPair.Car;

            if (currentKey.Equals(key))
               return true; // Key found.
            
            current = (currentPair.Cdr is Pair cdrPair) ? cdrPair.Cdr : Nil;
         }
      }
      
      return false; // Key not found.
   }
   
}
//=====================================================================================================================

