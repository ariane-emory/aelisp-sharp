using System.Collections;
using System.Reflection;
using System.Text;

//======================================================================================================================
static partial class Ae
{
   //===================================================================================================================
   // User function class
   //===================================================================================================================
   public abstract class UserFunction : Function
   {
      //================================================================================================================
      // Public properties
      //================================================================================================================
      public LispObject Parameters { get; }
      public LispObject Body { get; }
      public LispObject Env { get; }

      //================================================================================================================
      // Constructor
      //================================================================================================================
      public UserFunction(LispObject parameters, LispObject body, LispObject env)
      {
         Parameters = parameters;
         Body = body;
         Env = env;
      }

      //================================================================================================================
      // Protected instance properties
      //================================================================================================================
      protected override string? StringRepresentation => $"{Parameters.Princ()} {Body.Princ()}";

      //================================================================================================================
      // Instance methods
      //================================================================================================================
      public override string Princ() => ToString();
   }

   //===================================================================================================================
   // Lambda class
   //===================================================================================================================
   public class Lambda : UserFunction
   {
      //================================================================================================================
      // Constructor
      //================================================================================================================
      public Lambda(LispObject parameters, LispObject body, LispObject env) : base(parameters, body, env) { }

      //================================================================================================================
      // Protected instance methods
      //================================================================================================================
      protected override LispObject ImplApply(Env env, LispObject argsList)
      {
      //    if ((Parameters is Pair pair) &&
      //        ((argsList.Length < Parameters.Length) ||
      //         (Parameters.IsProperList && argsList.Length > Paramseters.Length)))
      //    )
      //    {
      //       char* const msg_tmp = free_list_malloc(256);

      //       snprintf(msg_tmp,
      //                256,
      //                "%s:%d: user fun %s requires %s %d arg%s, but got %d",
      //                __FILE__,
      //                __LINE__,
      //                fun_name_part,
      //                !PROPERP(FUN_PARAMS(fun)) ? "at least" : "exactly",
      //                LENGTH(FUN_PARAMS(fun)),
      //                s_or_blank(LENGTH(FUN_PARAMS(fun))),
      //                LENGTH(args));

      //       char* const msg = free_list_malloc(strlen(msg_tmp) + 1);
      //       strcpy(msg, msg_tmp);
      //       free_list_free(msg_tmp);

      //       ae_obj_t* err_data = NIL;

      //       ae_obj_t * const err = NEW_ERROR(msg);

      //       PUT_PROP(env, "error-env", err);
      //       PUT_PROP(args, "error-args", err);
      //       PUT_PROP(fun, "error-fun", err);

      //       RETURN_IF_ERRORP(err);
      //    }

      //    if (!SPECIAL_FUNP(fun))
      //       args = RETURN_IF_ERRORP(EVAL_ARGS(env, args));

      //    if (log_eval)
      //       LOG(args, "applying user fun %s to %d %s arg%s:", fun_name_part, LENGTH(args), (SPECIAL_FUNP(fun) ? "unevaled" : "evaled"), s_or_blank(LENGTH(args)));

      //    env = NEW_ENV(FUN_ENV(fun), FUN_PARAMS(fun), args);

      //    /* if (log_eval) { */
      //    /*   LOG(env,           "new env for user fun:"); */
      //    /*   LOG(ENV_SYMS(env), "new env's syms:"); */
      //    /*   LOG(ENV_VALS(env), "new env's vals:"); */
      //    /* } */

      //    INDENT;

      //    ae_obj_t * const body = FUN_BODY(fun);

      //    if (log_eval)
      //    {
      //       // If FUN_PARAMS(fun) is a blob, we lie to get a plural length:
      //       LOG(FUN_PARAMS(fun), "as param%s", s_or_blank(CONSP(FUN_PARAMS(fun)) ? LENGTH(FUN_PARAMS(fun)) : 2));
      //       LOG(body, "with body");
      //    }

      //    PUT_PROP(fun, "fun", env);

      //    ret = EVAL_AND_RETURN_IF_ERRORP(env, body);

      //    OUTDENT;

      // end:

      //    if (log_eval)
      //    {
      //       if (HAS_PROP("last-bound-to", fun))
      //       {
      //          LOG(ret, "applying user fun '%s' returned %s :%s",
      //              SYM_VAL(GET_PROP("last-bound-to", fun)), a_or_an(GET_TYPE_STR(ret)), GET_TYPE_STR(ret));
      //       }
      //       else
      //       {
      //          char* const tmp = SWRITE(fun);
      //          LOG(ret, "applying user fun %s returned %s :%s", tmp, a_or_an(GET_TYPE_STR(ret)), GET_TYPE_STR(ret));
      //          free(tmp);
      //       }
      //    }

      //    free_list_free(fun_name_part);

      //    JUMP_RETURN_EXIT;

         throw new NotImplementedException("Implement this!");
      }

      //================================================================================================================
   }

   //===================================================================================================================
   // Macro class
   //===================================================================================================================
   public class Macro : UserFunction
   {
      //================================================================================================================
      // Constructor
      //================================================================================================================
      public Macro(LispObject parameters, LispObject body, LispObject env) : base(parameters, body, env) { }

      //================================================================================================================
      // Protected instance methods
      //================================================================================================================
      protected override LispObject ImplApply(Env env, LispObject args)
      {
         throw new NotImplementedException("Implement this!");
      }

      //================================================================================================================      
   }

   //===================================================================================================================
}
