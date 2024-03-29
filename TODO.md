# Ælisp# To Do List:

## To Do - replicating features from the C implemtation of Ælisp:
- [ ] Core 'properties' methods.
- [ ] ImplApply for Macros.
- [ ] Implement REPL, maybe use ReadLine package?
- [ ] Find sensible/useful things for this implementation to put in object properties.
- [ ] OS interaction functions (data layer/core):
  - [ ] `load`/`require`/`provide`/`read`.
  - [ ] the rest of 'em.
- [ ] C#-level unit tests (XUnit?).

## To Do - practical:
- [ ] Might as well have built in min and max.
- [ ] If plist set methods find a (nil nil) sentinel, they could remove it.
- [ ] Make sure the main tokenizer call entry points are all using appropriate _exclude filters, whitespace and all comments are boring for now.
- [ ] Make that test of handling ParseErrors nicely more useful. Move it into LispTokenizer and use in Next()/ReadAll() probably?
- [ ] Refactor the stdlib and tests into it's own repo and try running it.
- [ ] Find an excuse to use T4 somewhere (just to learn how to work with it).
- [ ] Organize/tidy up building the root and make Main sensible, it's a construction zone ATM.
- [ ] Sensible/useful command line arguments, probably similar to Ælisp-C's - ~~maybe find a getopt-like package?~~ Probably use Mono.Options.
- [ ] Rearrange/tidy APIs:
  - [ ] Let's make Envs the main entry point for user-level interaction.
  - [ ] Eliminate mutable globals.
  - [ ] Possibility of multiple, independant 'root' environments.

## To Do - language improvements/'Ælisp 2.0'-ish features:
- [ ] Binary/hex number tokens?
- [ ] A radically different (whitespace sensitive?) non-Lispy syntax option that might just evolve into a 2nd language.

## To Do - implementation-neutral:
- [ ] Extend the stdlib's structs into an object system (which could turn out to look a lot like [tinyclos](https://github.com/kstephens/tinyclos), possibly just ammounting to successfully running a forked version of [tinyclos](https://github.com/kstephens/tinyclos)).

## Completed:
- [x] plist handling:
  - [x] data layer.
  - [x] core layer.
- [x] Public op/cmp methods on Number might warrant friendlier names and operator implementations.
- [x] Math comparison functions.
- [x] DRY up the int-only math methods.
- [x] ~/bit-not.

## Completed with alterations:
- [x] Math operator functions (probably gen with T4): done without T4.
- [x] Make princ the default ToString(): solved at Core layer instead, raw is based on ToString while other core funs use ToPrincString / To PrintString.
