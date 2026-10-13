using System.Buffers;
using System.Globalization;
using System.Text;

namespace Arthea.Core;

/// <summary>
/// Shared scalar-level check for text displayed in rooms and line-oriented clients.
/// Caller-specific bounds and whitespace policies remain at each trust boundary.
/// </summary>
internal static class VisibleText
{
    internal static bool IsSafe(string value)
    {
        var remaining = value.AsSpan();
        while (!remaining.IsEmpty)
        {
            if (Rune.DecodeFromUtf16(remaining, out var rune, out var consumed) != OperationStatus.Done)
                return false;
            var category = Rune.GetUnicodeCategory(rune);
            if (Rune.IsControl(rune) || category is UnicodeCategory.Format or
                UnicodeCategory.LineSeparator or UnicodeCategory.ParagraphSeparator)
                return false;
            remaining = remaining[consumed..];
        }
        return true;
    }
}
