using static System.Console;
using System.Collections;
using System.Reflection;
using System.Text;

//======================================================================================================================
static partial class Ae
{
   //===================================================================================================================
   // Pair class
   //===================================================================================================================
   public class Pair : LispObject, IEnumerable<LispObject>
   {
      //================================================================================================================
      // Public properties
      //================================================================================================================
      public LispObject Car { get; set; }
      public LispObject Cdr { get; set; }

      //================================================================================================================
      // Constructor
      //================================================================================================================
      public Pair(LispObject car, LispObject cdr)
      {
         Car = car;
         Cdr = cdr;
      }

      //================================================================================================================
      // Protected instance properties
      //================================================================================================================
      protected override string? StringRepresentation => $"{Car}, {Cdr}";

      //================================================================================================================
      // Public instance methods
      //================================================================================================================
      public override string Princ()
      {
         var sb = new StringBuilder();

         sb.Append("(");

         LispObject current = this;

         while (current is Pair currentCons)
         {
            sb.Append(currentCons.Car.Print());

            if (currentCons.Cdr is Pair)
            {
               sb.Append(" ");
               current = currentCons.Cdr;
            }
            else if (currentCons.Cdr != Nil)
            {

               sb.Append(" . ");
               sb.Append(currentCons.Cdr.Princ());

               break;
            }
            else
            {
               break;
            }
         }

         sb.Append(")");

         return sb.ToString();
      }

      //================================================================================================================
      public IEnumerator<LispObject> GetEnumerator()
      {
         LispObject current = this;

         while (current != Nil)
            if (current is Pair pair)
            {
               yield return pair.Car;

               current = pair.Cdr;
            }
            else
            {
               // yield return current;
               yield break;

               // throw new InvalidOperationException("Enumerated improper list!");
            }
      }

      //================================================================================================================
      IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

      //================================================================================================================
      public int PairListLength
      {
         get
         {
            // NOTE: Remember, the Listlength of both (1 2 3) and (1 2 3 . 4 ) is 3 in this language at present.

            int length = 0;
            LispObject current = this;

            while (current is Pair pair)
            {
               length++;
               current = pair.Cdr;

               if (!(current is Pair) && current != Nil)
                  break;
            }

            return length;
         }
      }

      //================================================================================================================
      public LispObject ElementAt(int index) => this[index];

      //================================================================================================================
      public LispObject this[int index]
      {
         get
         {
            int currentIndex = 0;
            LispObject current = this;

            while (current is Pair pair)
            {
               if (currentIndex == index)
                  return pair.Car;

               current = pair.Cdr;
               currentIndex++;
            }

            return currentIndex == index ? current : Nil;
         }
         set
         {
            int currentIndex = 0;
            LispObject current = this;
            LispObject previous = Nil;

            while (current is Pair pair)
            {
               if (currentIndex == index)
               {
                  pair.Car = value;
                  return;
               }

               previous = current;
               current = pair.Cdr;
               currentIndex++;
            }

            if (currentIndex == index && previous is Pair prevPair)
            {
               prevPair.Cdr = value;

               return;
            }
            else if (currentIndex <= index)
            {
               throw new IndexOutOfRangeException("Index is out of range.");
            }
         }
      }
      //================================================================================================================

      public override LispObject Eval(Env env)
      {
         JUMP_RETURN_ENTER;

         assert(env);
         assert(ENVP(env));
         assert(obj);
         assert(CONSP(obj));

         ae_obj_t * const head = CAR(obj);
         ae_obj_t * const args = CDR(obj);

         assert(args);
         assert(TAILP(args));

         if (log_eval)
         {
            char* const tmp = SWRITE(head);
            char* const msg = free_list_malloc(256);

            snprintf(msg, 256,
                     "evaluating list by applying '%s' to %d arg%s:",
                     tmp, LENGTH(args), s_or_blank(LENGTH(args)));

            LOG(LENGTH(args) == 1
                ? CAR(args)
                : args,
                msg);

            free(tmp);
            free_list_free(msg);
         }

         INDENT;

         if (log_eval)
         {
            LOG(env, "in env");

            if (!ROOTP(env))
            {
               LOG(ENV_SYMS(env), "with syms");
               LOG(ENV_VALS(env), "and  vals");
            }
         }

         ae_obj_t * const fun = EVAL_AND_RETURN_IF_ERRORP(env, head);

         if (!(COREP(fun) || LAMBDAP(fun) || MACROP(fun)))
         {
            NL;
            LOG(head, "Result of evaluating head: ");
            LOG(fun, "is inapplicable object: ");
            SLOGF("of type: %s", GET_TYPE_STR(fun));
            NL;

            ae_obj_t * const err = NEW_ERROR("inapplicable");

            PUT_PROP(env, "error-env", err);
            PUT_PROP(args, "error-args", err);
            PUT_PROP(fun, "error-fun", err);

            RETURN_IF_ERRORP(err);
         }

         long int begin = -1;

         if (MACROP(fun) && (log_eval || log_macro))
         {
            LOG(obj, "expanding:");

            begin = ae_sys_time_now_us();

            INDENT;
         }

         ret = COREP(fun)
           ? apply_core(env, fun, args)
           : apply_user(env, fun, args);

         RETURN_IF_ERRORP(ret);

         if (MACROP(fun))
         {
            if (log_eval || log_macro)
            {
               LOG(ret, "expansion:");
            }

            if (ATOMP(ret))
            {
               ret = CONS(SYM("progn"), CONS(ret, NIL));

               if (log_eval || log_macro)
                  LOG(ret, "decorated expansion:");
            }

            ret = EVAL(env, ret);

            long long int after = ae_sys_time_now_us();

            if (log_eval || log_macro)
               LOG(ret, "evaled expansion:");

            if (log_eval || log_macro) // inside if MACROP
               OUTDENT;

            if (log_eval || log_macro)
               LOG(obj, "expanding took %.2f ms:", ((double)(after - begin)) / 1000.0);

            // RETURN_IF_ERRORP(ret);

            // this line would cause 'in-place expansion' and is disabled until a way
            // to annotate which macros should be expanded in-place is implemented:
            // *obj = *ret; 
         }

         if (ERRORP(ret))
         {
            char* const fun_tmp = SWRITE(fun);
            char* const ret_tmp = SWRITE(ret);

            FSLOGF(stderr, "%s returned an error: %s", fun_tmp, ret_tmp);

            free(fun_tmp);
            free(ret_tmp);

            /* if (HAS_PROP("error-fun", ret)) // this is probably going to double the first fun in the list but I can't be bothered fixing it yet. */
            /*   PUT_PROP(CONS(fun, GET_PROP("error-fun", ret)), "error-fun", ret); */
            /* else */
            /*   PUT_PROP(CONS(fun, NIL), "error-fun", ret); */

            // RETURN_IF_ERRORP(ret);
         }

      end:

         OUTDENT;

         if (log_eval)
            LOG(ret, "evaluating list returned %s :%s", a_or_an(GET_TYPE_STR(ret)), GET_TYPE_STR(ret));

         snap_indent();

         JUMP_RETURN_EXIT;
      }

      //================================================================================================================
   }
   //===================================================================================================================
}

