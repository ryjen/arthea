using Arthea.Core.World;

namespace Arthea.LegacyBridge;

/// <summary>
/// An explicitly extracted, legacy-shaped exit. This is not a MongoDB BSON serializer.
/// In the original room model, the containing Room.Exits dictionary key specifies the
/// travel direction; Exit.Direction may be unset by the legacy "link" editor command.
/// </summary>
public sealed record LegacyExitSnapshot(string? ToRoomRef, string? StoredDirection);

/// <summary>
/// A persistence-neutral snapshot of the relevant fields in the old Room/Exit model.
/// Callers are responsible for reading source data within their trusted boundary.
/// </summary>
public sealed record LegacyRoomSnapshot(
    string Id,
    string Name,
    IReadOnlyDictionary<string, LegacyExitSnapshot> Exits);

public static class LegacyRoomProjection
{
    /// <summary>
    /// Converts extracted legacy data to a validated, detached room graph.
    /// There are no inferred reverse edges, repairs, database requests, or partial results.
    /// This is not a drop-in adapter for MongoDB's historic BSON format.
    /// </summary>
    public static RoomGraph Build(IEnumerable<LegacyRoomSnapshot> rooms)
    {
        ArgumentNullException.ThrowIfNull(rooms);

        var definitions = new List<RoomDefinition>();
        foreach (var room in rooms)
        {
            ArgumentNullException.ThrowIfNull(room);
            ArgumentNullException.ThrowIfNull(room.Exits);

            var exits = new Dictionary<Direction, string>();
            foreach (var (key, exit) in room.Exits)
            {
                // Parse only known legacy names, never numeric enum values or arbitrary keys.
                var direction = key switch
                {
                    _ when string.Equals(key, nameof(Direction.North), StringComparison.OrdinalIgnoreCase) => Direction.North,
                    _ when string.Equals(key, nameof(Direction.South), StringComparison.OrdinalIgnoreCase) => Direction.South,
                    _ when string.Equals(key, nameof(Direction.East), StringComparison.OrdinalIgnoreCase) => Direction.East,
                    _ when string.Equals(key, nameof(Direction.West), StringComparison.OrdinalIgnoreCase) => Direction.West,
                    _ when string.Equals(key, nameof(Direction.Up), StringComparison.OrdinalIgnoreCase) => Direction.Up,
                    _ when string.Equals(key, nameof(Direction.Down), StringComparison.OrdinalIgnoreCase) => Direction.Down,
                    _ => throw new ArgumentException("Unsupported legacy exit direction key.", nameof(rooms)),
                };
                if (exit is null || string.IsNullOrWhiteSpace(exit.ToRoomRef))
                    throw new ArgumentException("A legacy exit has no destination reference.", nameof(rooms));
                // Exit.StoredDirection is deliberately *not* authoritative.
                if (!exits.TryAdd(direction, exit.ToRoomRef))
                    throw new ArgumentException("Duplicate legacy exit direction.", nameof(rooms));
            }

            definitions.Add(new RoomDefinition(room.Id, room.Name, exits));
        }

        return new RoomGraph(definitions);
    }
}
