using ArtheaEngine;

namespace ArtheaEngine.Model;

// These are deliberately limited test doubles; no real player, social,
// ability, database, socket, or command implementations are instantiated.
public sealed class Character
{
    public short Level { get; set; }
}

public static class Social
{
    public static bool Interpret(Character _, Argument __) => false;
}

public static class Ability
{
    public static bool Interpret(Character _, Argument __) => false;
}
