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
      private static LispObject LetInternal(Env env, LispObject argsList, bool lookupInNewEnv)
      {
         ThrowUnlessIsProperList("argsList", argsList);

         var arg0 = ((Pair)argsList).Car;
         var body = ((Pair)argsList).Cdr;

         ValidateLetArguments(arg0, body);

         var varlist = (Pair)arg0;
         var newEnv = env.Spawn(Nil, Nil);

         BindVarlistInEnv(varlist, lookupInNewEnv ? newEnv : env, newEnv);

         return Core.Progn(newEnv, body);
      }

      //===================================================================================================================
      private static void ValidateLetArguments(LispObject arg0, LispObject body)
      {
         if (!(arg0 is Pair varlist))
            throw new ArgumentException($"varlist must be a non-empty list, not {arg0.ToPrincString()}!");

         ThrowUnlessIsProperList("varlist", varlist);

         LispObject current = varlist;

         while (current is Pair currentPair)
         {
            if (currentPair.Car is Pair currentVarPair)
            {
               if (currentVarPair.Length != 2 || currentVarPair.IsImproperList)
                  throw new ArgumentException($"varlist items must be proper lists with two elements, not {currentVarPair.ToPrincString()}!");

               if (currentVarPair.Car is Symbol currentVarSym && !currentVarSym.IsLetBindable)
                  throw new ArgumentException($"let forms cannot bind {currentVarSym.ToPrincString()}!");
            }
            else if (currentPair.Car is Symbol currentVarSym)
            {
               if (!currentVarSym.IsLetBindable)
                  throw new ArgumentException($"let forms cannot bind {currentVarSym.ToPrincString()}!");
            }
            else
            {
               throw new ArgumentException($"varlist items must be symbols or proper lists with two elements " +
                                           $"pairs whose first element is a let-bindable symbol, " +
                                           $"not {current.ToPrincString()}!");
            }

            current = currentPair.Cdr;
         }

         ThrowUnlessIsProperList("body", body);

         if (body.IsNil)
            throw new ArgumentException("body cannot be empty!");
      }

      //================================================================================================================
      private static void BindVarlistInEnv(LispObject varlist, Env lookupEnv, Env bindEnv)
      {
         WriteLine($"lookupEnv: {lookupEnv.ToPrincString()}");
         WriteLine($"bindEnv: {bindEnv.ToPrincString()}");

         if (lookupEnv == bindEnv)
            WriteLine("(same env)");

         ThrowUnlessIsProperList("varlist", varlist);

         var current = varlist;
         
         while (current is Pair currentPair)
         {
            WriteLine($"\nthis item = {currentPair.Car.ToPrincString()}");
            
            var sym = Nil;
            var val = Nil;
            var evaled_val = Nil;
            
            if (currentPair.Car is Pair varlistVarPair)
            {
               sym = varlistVarPair.Car;
               val = ((Pair)varlistVarPair.Cdr).Car;

               WriteLine($"Let-bind {sym.ToPrincString()} to {val.ToPrincString()}...");

               evaled_val = val.Eval(lookupEnv);
               
               WriteLine($"Evaled: {evaled_val.ToPrincString()}...");

            }
            else if (currentPair.Car  is Symbol varlistVarSym)
            {
               sym = varlistVarSym;
               evaled_val = val = Nil;
            }
            
            bindEnv.Set(Env.LookupMode.Local, sym, val);
            WriteLine($"bindEnv after: {bindEnv.ToPrincString()}");
            
            current = currentPair.Cdr;
         }
      }

      //================================================================================================================
   }
   //===================================================================================================================
}
