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
   // The original C version:
   // void ae_plist_remove_mutating(ae_obj_t * const plist, ae_obj_t * const key) {
   //   assert(plist);
   //   assert(key);
   //   assert(CONSP(plist));
   //   assert(!(LENGTH(plist) % 2));
   //
   //   ae_obj_t * current = plist;
   //   ae_obj_t * next    = CDR(plist);
   //
   //   if (EQL(CAR(current), key)) {
   //     ae_obj_t * const after_pair = CDR(next);
   //
   //     if (NILP(after_pair)) { // key matches the only pair in the list.
   //       CAR(current) = NIL;
   //       CDR(current) = CONS(NIL, NIL);
   //     }
   //     else {
   //       CAR(current) = CAR(after_pair);
   //       CDR(current) = CDR(after_pair);
   //     }
   //
   //     return;
   //   }
   //
   //   while (!NILP(next) && !NILP(CDR(next))) {
   //     if (EQL(CAR(next), key)) {
   //       CDR(current) = CDR(CDR(next));
   //
   //       return;
   //     }
   //     current = CDR(current);
   //     next = CDR(next);
   //   }
   // }
   public static LispObject MutatingPlistRemove(LispObject plist, LispObject key, LispObject value)
   {
      ThrowUnlessPlist(plist);

      if (plist.IsNil)
         throw new InvalidOperationException("mutating removeis not permitted on Nil!");

      var current = plist;
      var next = ((Pair)plist).Cdr;
      
      // first, we'll handle the case where the key is the first element of the plist:
      if (((Pair)current).Car.Equals(key))
      {
         var afterPair = ((Pair)next).Cdr;

         if (afterPair.IsNil) // key matches the only pair in the list.
         {
            // we can't actually remove the last pair, so we'll just replace it with an ugly subtitute:
            ((Pair)current).Car = Nil;
            ((Pair)current).Cdr = Cons(Nil, Nil);
         }
         else
         {
            ((Pair)current).Car = ((Pair)afterPair).Car;
            ((Pair)current).Cdr = ((Pair)afterPair).Cdr;
         }

         return plist;
      }

      while (! next.IsNil && ! ((Pair)next).Cdr.IsNil)
      {
         if (((Pair)next).Car.Eql(key))
         {
            ((Pair)current).Cdr = ((Pair)((Pair)next).Cdr).Cdr;
            
            return plist;
         }
      }

      return plist;
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

