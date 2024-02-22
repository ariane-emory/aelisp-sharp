using static System.Console;
using System.Collections;

//======================================================================================================================
static partial class Ae
{
   //===================================================================================================================
   // plist related methods
   //===================================================================================================================
   
   //===================================================================================================================
   public static void ThrowUnlessPlist(LispObject plist)
   {
      if (!plist.IsList)
         throw new ArgumentException($"plist must be a list, not {plist}!");

      if (!(plist is Pair plistHeadPair)) // plist must be Nil.
         return;

      if (plistHeadPair.Length % 2 != 0)
         throw new ArgumentException($"plist must have an even number of elements, not {plistHeadPair.Length}!");
   }
   
   //===================================================================================================================
   public static Pair? PlistFindPair(LispObject plist, LispObject key)
   {
      ThrowUnlessPlist(plist);
      
      LispObject current = plist;
      
      while (current is Pair currentPair)
      {         
         if (currentPair.Car.Equals(key))
            return currentPair;

         // we already made sure the length was even, so this case should be safe:
         current = ((Pair)currentPair.Cdr).Cdr;
      }
      
      return null; 
   }
   
   //====================================================================================================================   
   public static bool PlistContains(LispObject plist, LispObject key)
   {
      Pair? found = PlistFindPair(plist, key);

      return found is not null;
   }
   
  //====================================================================================================================
   public static LispObject PlistGet(LispObject plist, LispObject key)
   { 
      Pair? found = PlistFindPair(plist, key);
      return found is null ? Nil : ((Pair)found.Cdr).Car;
   }

   //====================================================================================================================
   public static void PlistMutatingSet(LispObject plist, LispObject key, LispObject value)
   {
      if (plist.IsNil)
         throw new InvalidOperationException("mutating set is not permitted on Nil!");
      
      Pair? found = PlistFindPair(plist, key);

      if (found is not null)
      {
         ((Pair)found.Cdr).Car = value;
         return;
      }

      // if we got this far, then we already know that plist is a non-nil length with an even length.
      // we'll loop through to find the last pair, and then we'll tack a new key/value pair onto the end.
      var current = plist;

      while (current is Pair currentPair)
      {
         if (currentPair.Cdr.IsNil)
         {
            currentPair.Cdr = Cons(key, Cons(value, Nil));
            return;
         }

         current = currentPair.Cdr;
      }

      throw new ApplicationException("This should never happen!");
   }
      
   //====================================================================================================================
   public static LispObject PlistNonmutatingSet(LispObject plist, LispObject key, LispObject value)
   {
      if (plist.IsNil)
         return new Pair(key, new Pair(value, Nil));

      var stash = Nil;
      var current = plist;
      // first, we'll loop through the last adding all items before the key to stash (in reverse order, for now)

      return Nil;
   }
   
//=====================================================================================================================
}

