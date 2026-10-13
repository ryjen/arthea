# Legacy `Argument` tokenizer — characterization, not a replacement

Arthea is an old personal MUD project. These tests are a deliberately small **code archaeology** exercise; they are **not** an attempt to turn the old command parser into a supported product or re-create game content.

The `Arthea.Legacy.Characterization` executable compiles the **original** `ArtheaEngine/Argument.cs` directly with a narrowly scoped test-only compatibility shim. No production legacy source was changed. This helps avoid accidentally treating undesirable historical behavior as a design requirement.

## Observed historical behavior

- Space is the default separator; repeated *interior* spaces are consumed after a token, while leading spaces can yield an empty token. A TAB is not interpreted as a separator.
- `Next()` mutates the remaining argument; `Peek()` and `All()` do not mutate their source.
- Quote and parenthesis parsing is **not a full grammar**: a closing quote or `)` may remain at the end of returned quoted tokens because of the legacy substring length calculation. An unmatched opener is returned as a token, leaving the remainder for a later call.
- The `Argument == string` operator compares with `CurrentCultureIgnoreCase`, but `Argument.Equals(Argument)` compares the backing string case-sensitively. Do not assume their results or hash-based collection semantics match.
- No security guarantee is implied for terminal escapes, newlines, command injection or unbounded input. The legacy server is not authorized for network exposure.

## Experiment boundaries

A future optional command grammar should have an **independent** specification: explicit encoding and whitespace rules, max byte/character budget, deterministic case comparison, malformed quote handling, and tests derived from that specification. Do **not** mechanically preserve these old quirks in new security-sensitive code.

The tests are pure local .NET 10 characterization exercises. Original 2015 code remains untouched on `master`; modernization and tests live on `main`. This repository is currently **public**, but historic provenance/secret and redistribution verification remains outstanding in [#5](https://github.com/ryjen/arthea/issues/5).
