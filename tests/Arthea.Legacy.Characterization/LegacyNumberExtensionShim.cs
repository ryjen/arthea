using System;

namespace ArtheaEngine;

/// <summary>
/// Test-only shim for an unrelated method on legacy Extensions.cs, whose other
/// imports require MongoDB and world classes. This method is NOT characterized
/// here and deliberately fails if used; token parsing never calls it.
/// </summary>
internal static class LegacyNumberExtensionShim
{
    public static bool IsNumber(this string _) =>
        throw new NotSupportedException("Legacy Argument.IsNumber is outside this characterization slice.");
}
