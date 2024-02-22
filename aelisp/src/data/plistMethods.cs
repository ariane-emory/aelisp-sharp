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

      is (!(plist is Pair plistHeadPair)) // plist must be Nil.
         return false;

      if (!(plistHeadPair.Length % 2 == 0))
         throw new ArgumentException($"plist must have an even number of elements, not {plistHeadPair.Length}!");

      
      return false;
   }
   
}
//=====================================================================================================================

