To Do - replicatinc features from C impelemtation of Ælisp:
- [ ] Rearrange APIs:
  - Let's make Envs the main entry point for user-level interaction.
  - Eliminate mutable globals.
- [ ] C#-level unit tests.
- [ ] Try running stdlib's tests package.
- [ ] Refactor the stdlib and tests into it's own repo and try running it.
- [ ] OS interaction functions (data layer/core).
- [ ] Organize building the root and make Main sensible, it's a construction zone ATM.
- [ ] Find sensible/useful things for this implementation to put in object properties.
- [ ] Implement REPL, maybe use ReadLine package?
- [ ] plist handling (data layer/core).
- [ ] Math comparison functions (probably gen with T4).
- [ ] Math operator functions (probably gen with T4).

Abandoned/changed:
- [x] Make princ the default ToString(): solved at Core layer instead, raw is based on ToString while others use ToPrincString / To PrintString.
