using ArtheaEngine;
using ArtheaEngine.Model;
using ArtheaServer.Commands;

static void Equal<T>(T expected, T actual, string observation)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
        throw new InvalidOperationException(
            $"Legacy dispatch characterization failed: {observation}");
}

static void Check(
    string input,
    short playerLevel,
    bool expectedHandled,
    int expectedCalls,
    string expectedRemainder,
    string observation)
{
    ProbeCommand.Reset();
    var argument = new Argument(input);
    var handled = Command.Interpret(new Character { Level = playerLevel }, argument);
    Equal(expectedHandled, handled, observation + ": handled");
    Equal(expectedCalls, ProbeCommand.Calls, observation + ": calls");
    if (expectedCalls > 0)
        Equal(expectedRemainder, ProbeCommand.LastRemainder, observation + ": remainder");
}

// Only one test command is registered in the linked Command.cs assembly.
// Social and Ability handlers are disabled by the explicit test-only shims.
Check("inspect details", ProbeCommand.MinimumLevel, true, 1,
    "details", "exact command preserves argument remainder");
Check("INS details", ProbeCommand.MinimumLevel, true, 1,
    "details", "command prefix matches without requiring full name");
Check("ObS details", ProbeCommand.MinimumLevel, true, 1,
    "details", "case-insensitive alias prefix matches");
Check("inspect", ProbeCommand.MinimumLevel, true, 1,
    "", "command with no arguments is dispatched");
Check("unknown details", ProbeCommand.MinimumLevel, false, 0,
    "", "unknown command is not handled");
Check("inspect details", (short)(ProbeCommand.MinimumLevel - 1), false, 0,
    "", "unauthorized command is skipped");
Check("", ProbeCommand.MinimumLevel, true, 0,
    "", "empty line is handled without dispatch");

// Original Argument.Next() returns an empty token for a leading space.
// Original Command.Interpret() passes that token to StartsWith(), which
// matches ANY accessible command. This is an unsafe observed quirk, NOT
// a behavioral requirement for any modern command parser.
Check(" inspect details", ProbeCommand.MinimumLevel, true, 1,
    "inspect details", "leading space dispatches empty command prefix");
Check(" inspect details", (short)(ProbeCommand.MinimumLevel - 1), false, 0,
    "", "access check still rejects empty-prefix dispatch");

Console.WriteLine("Legacy original-source command dispatch characterization passed.");

// Only this synthetic subclass is discovered by the original command
// registration reflection; selection among multiple real commands is NOT tested.
public sealed class ProbeCommand : Command
{
    public const short MinimumLevel = 2;
    public const string CommandName = "inspect";
    public const string AliasName = "observe";

    public static int Calls { get; private set; }
    public static string LastRemainder { get; private set; } = "";

    public ProbeCommand() : base(CommandName, "Synthetic test command", MinimumLevel)
    {
        Aliases.Add(AliasName);
    }

    public static void Reset()
    {
        Calls = 0;
        LastRemainder = "";
    }

    public override void Execute(Character _, Argument argument)
    {
        Calls++;
        LastRemainder = argument.ToString();
    }
}
