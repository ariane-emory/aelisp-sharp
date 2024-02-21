# Ælisp#

A reimplementation of [ÆLisp](https://github.com/ariane-emory/aelisp) in C# using [pidgin](https://github.com/benjamin-hodgson/Pidgin) (alongside a custom tokenizer) to do its parsing. Largely seems to be working well under the hood, but doesn't have a user friendly REPL, sensible command line options or unit tests yet.

This implementation is meant to proceed conservatively: the first goal is to successfully re-implement ÆLisp-C faithfully enough to run the in-language test suite included in ÆLisp-C with identical results, prior to proceeding to improve/extend the language itself (though breaking compatibility with code that runs successfully in Ælisp-C should remain an anti-goal in the near term).
