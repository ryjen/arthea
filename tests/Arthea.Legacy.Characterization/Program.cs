using ArtheaEngine;

static void Equal<T>(T expected, T actual, string name)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
        throw new Exception($"Legacy characterization failed: {name}");
}

static void Tokens(string input, params string[] expected)
{
    var parser = new Argument(input);
    foreach (var token in expected)
        Equal(token, parser.Next(), $"token from {input.Length}-character input");
    Equal(true, parser.IsEmpty(), "all tokens consumed");
}

Tokens("north", "north");
Tokens("say \"hello there\" now", "say", "hello there", "now");
Tokens("say 'hello there' now", "say", "hello there", "now");
Tokens("say (hello there) now", "say", "hello there", "now");
Tokens("  look    north  ", "look", "north");

var peek = new Argument("look north");
Equal("look", peek.Peek(), "peek token");
Equal("look", peek.Next(), "peek does not consume input");
Equal("north", peek.Next(), "remaining token after peek");

var comma = new Argument("one,two");
Equal("one", comma.Next(','), "custom delimiter first");
Equal("two", comma.Next(','), "custom delimiter second");

var all = new Argument("tell \"two words\" now").All();
Equal(3, all.Length, "All token count");
Equal("two words", all[1], "All retains quoted phrase");

var missingQuote = new Argument("\"unfinished");
Equal("\"", missingQuote.Next(), "legacy unmatched-quote behavior");
Equal("unfinished", missingQuote.Next(), "legacy unmatched-quote remainder");

Equal(true, Argument.Empty.IsEmpty(), "empty sentinel");
Console.WriteLine("Legacy ArtheaEngine.Argument tokenization characterization passed.");
