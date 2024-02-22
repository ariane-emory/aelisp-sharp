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
      
      if (!(plist is Pair plistHeadPair)) // plist must be Nil.
          return false;
          
      if (plistHeadPair.Length % 2 != 0)
         throw new ArgumentException($"plist must have an even number of elements, not {plistHeadPair.Length}!");
      
      LispObject current = plist;
      
      while (current is Pair currentPair)
      {         
         if (currentPair.Car.Equals(key))
            return true;

         // we already made sure the length was even, so this case should be safe:
         current = ((Pair)currentPair.Cdr).Cdr;
      }
      
      return false; 
   }
   
}
//=====================================================================================================================

