using static System.Console;
using static Ae;

//======================================================================================================================
static partial class Ae
{
   //====================================================================================================================
   public static partial class Core
   {
      //=================================================================================================================
      public static readonly CoreFun.FuncT Let = (env, argsList) =>
         LetInternal(env, argsList, false);

      //=================================================================================================================
      public static readonly CoreFun.FuncT LetStar = (env, argsList) =>
         LetInternal(env, argsList, true);

      //================================================================================================================
      public static readonly CoreFun.FuncT Letrec = (env, argsList) =>
      {
         var arg0 = ((Pair)argsList).Car;
         var body = ((Pair)argsList).Cdr;

         ValidateLetArguments(arg0, body);

         var varlist = (Pair)arg0;
         var newEnv = env.Spawn(Nil, Nil);
         LispObject dummy = Intern(":dummy");

         foreach (var varlistElem in varlist)
            newEnv.Set(Env.LookupMode.Local, ((Pair)varlistElem).Car, dummy);

         BindVarlistInEnv(varlist, newEnv, newEnv);

         return Core.Progn(newEnv, body);
      };

      //===================================================================================================================
      private static LispObject LetInternal(Env env, LispObject argsList, bool bindInNewEnv)
      {
         ThrowUnlessIsProperList("argsList", argsList);

         var arg0 = ((Pair)argsList).Car;
         var body = ((Pair)argsList).Cdr;

         ValidateLetArguments(arg0, body);

         var varlist = (Pair)arg0;
         var newEnv = env.Spawn(Nil, Nil);

         BindVarlistInEnv(varlist, bindInNewEnv ? newEnv : env, newEnv);

         return Core.Progn(newEnv, body);
      }

      //===================================================================================================================
      private static void ValidateLetArguments(LispObject arg0, LispObject body)
      {
         if (!(arg0 is Pair varlist))
            throw new ArgumentException($"varlist must be a non-empty list, not {arg0.ToPrincString()}!");

         ThrowUnlessIsProperList("varlist", varlist);

         LispObject current = varlist;

         while (!current.IsNil)
            if (current is Pair currentVarPair)
            {
               if (currentVarPair.Length != 2 || currentVarPair.IsImproperList)
                  throw new ArgumentException($"varlist items must be proper lists with two elements, not {currentVarPair.ToPrincString()}!");

               if (currentVarPair.Car is Symbol currentVarSym && !currentVarSym.IsLetBindable)
                  throw new ArgumentException($"let forms cannot bind {currentVarSym.ToPrincString()}!");
            }
            else if (current is Symbol currentVarSym)
            {
               if (!currentVarSym.IsLetBindable)
                  throw new ArgumentException($"let forms cannot bind {currentVarSym.ToPrincString()}!");
            }
            else
            {
               throw new ArgumentException($"varlist items must be symbols or proper lists with two elements " +
                                           "pairs whose first element is a " +
                                           $"let-bindable symbol, not {current.ToPrincString()}!");
            }

         ThrowUnlessIsProperList("body", body);

         if (body.IsNil)
            throw new ArgumentException("body cannot be empty!");
      }

      //================================================================================================================
      private static void BindVarlistInEnv(LispObject varlist, Env lookupEnv, Env bindEnv)
      {
         ThrowUnlessIsProperList("varlist", varlist);

         while (!varlist.IsNil)
         {
            var sym = Nil;
            var val = Nil;

            if (varlist is Pair varlistVarPair)
            {
               sym = varlistVarPair.Car;
               val = ((Pair)varlistVarPair.Cdr).Car.Eval(lookupEnv);
            }
            else if (varlist is Symbol varlistVarSym)
            {
               sym = varlistVarSym;
            }

            bindEnv.Set(Env.LookupMode.Local, sym, val);

            varlist = ((Pair)varlist).Cdr;
         }
      }

      //================================================================================================================
   }
   //===================================================================================================================
}
