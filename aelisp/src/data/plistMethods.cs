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
   private static Pair? PlistFindPair(LispObject plist, LispObject key)
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
   public static LispObject MutatingPlistRemove(LispObject plist, LispObject key, LispObject value)
   {
      ThrowUnlessPlist(plist);

      if (plist.IsNil)
         throw new InvalidOperationException("mutating removeis not permitted on Nil!");

      return Nil;
      
   }

   //====================================================================================================================
   public static LispObject MutatingPlistSet(LispObject plist, LispObject key, LispObject value)
   {
      ThrowUnlessPlist(plist);

      if (plist.IsNil)
         throw new InvalidOperationException("mutating set is not permitted on Nil!");
      
      Pair? found = PlistFindPair(plist, key);

      if (found is not null)
      {
         ((Pair)found.Cdr).Car = value;

         return plist;
      }

      // if we got this far, then we already know that plist is a non-nil length with an even length.
      // we'll loop through to find the last pair, and then we'll tack a new key/value pair onto the end.
      var current = plist;

      while (current is Pair currentPair)
      {
         if (currentPair.Cdr.IsNil)
         {
            currentPair.Cdr = Cons(key, Cons(value, Nil));
            break;
         }

         current = currentPair.Cdr;
      }

      return plist;
   }
      
   //====================================================================================================================
   public static LispObject UnsafePlistSet(LispObject plist, LispObject key, LispObject value)
   {
      ThrowUnlessPlist(plist);
      
      if (plist.IsNil)
         return new Pair(key, new Pair(value, Nil));

      MutatingPlistSet(plist, key, value);
      
      return plist;
   }    
      
   //====================================================================================================================
   private static LispObject PurePlistTransform(LispObject plist, LispObject key,
                                               Func<LispObject> ifNilFunc,
                                               Func<LispObject, LispObject>? insertFunc)
   {
      ThrowUnlessPlist(plist);
      
      if (plist.IsNil)
         return ifNilFunc();

      var reversedNewList = Nil;
      var current = plist;

      // we're going to build a whole new plist, sharing no structure with the original plist. 
      
      // first, we'll loop through the last adding all items before the key to reversedNewList (in reverse order, for now):
      while (current is Pair currentPair)
      {
         if (currentPair.Car.Equals(key))
            break;

         reversedNewList = Cons(currentPair.Car, reversedNewList);

         current = currentPair.Cdr;
      }

      // then, we might add a new value onto reversedNewList:
      if (insertFunc is not null)
         reversedNewList = insertFunc(reversedNewList);

      // by now, we should have found either the end of the list or the key/value pair. if we found the key value pair,
      // we'll advance current past it and then loop through the rest of the list, adding all the items to onto reveredNewList:
      if (!current.IsNil)
      {
         current = ((Pair)((Pair)current).Cdr).Cdr;
         
         while (current is Pair currentPair)
         {
            reversedNewList = Cons(currentPair.Car, reversedNewList);
            current = currentPair.Cdr;
         }
      }
      
      var result = Nil;

      // finally, we'll reverse reversedNewList to get the final result:
      while (reversedNewList is Pair reversedNewListPair)
      {
         result = Cons(reversedNewListPair.Car, result);
         reversedNewList = reversedNewListPair.Cdr;
      }

      return result;
   }
   
   //====================================================================================================================
   public static LispObject PurePlistSet(LispObject plist, LispObject key, LispObject value) => 
      PurePlistTransform(plist, key,
                         ifNilFunc: () => new Pair(key, new Pair(value, Nil)),
                         insertFunc: reversedNewList => Cons(value, Cons(key, reversedNewList)));
   
   //====================================================================================================================
   public static LispObject PurePlistRemove(LispObject plist, LispObject key) => 
      PurePlistTransform(plist, key,
                         ifNilFunc: () => Nil,
                         insertFunc: null);
   
   
//=====================================================================================================================
}

