using System.Collections.ObjectModel;

namespace Arthea.Core.World;

/// <summary>
/// The six cardinal/vertical exits present in the legacy Arthea model.
/// Numeric values are explicitly preserved for compatibility with old saves.
/// </summary>
public enum Direction
{
    North = 0,
    South = 1,
    East = 2,
    West = 3,
    Up = 4,
    Down = 5,
}

/// <summary>
/// A persistence- and transport-independent snapshot of one legacy-style room.
/// Ids are opaque: neither coordinates nor a spatial grid are implied.
/// </summary>
public sealed record RoomDefinition(
    string Id,
    string Name,
    IReadOnlyDictionary<Direction, string> Exits);

/// <summary>
/// Minimal read-only room-graph seam for incremental migration.
/// This is not yet a drop-in replacement for legacy Room/Exit/World.
/// </summary>
public sealed class RoomGraph
{
    private readonly ReadOnlyDictionary<string, RoomDefinition> _rooms;

    public RoomGraph(IEnumerable<RoomDefinition> rooms)
    {
        ArgumentNullException.ThrowIfNull(rooms);

        var index = new Dictionary<string, RoomDefinition>(StringComparer.Ordinal);
        foreach (var room in rooms)
        {
            ArgumentNullException.ThrowIfNull(room);
            if (string.IsNullOrWhiteSpace(room.Id) || string.IsNullOrWhiteSpace(room.Name))
                throw new ArgumentException("Rooms require a nonempty id and name.", nameof(rooms));
            ArgumentNullException.ThrowIfNull(room.Exits);

            var exits = new Dictionary<Direction, string>();
            foreach (var (direction, targetId) in room.Exits)
            {
                if (!Enum.IsDefined(direction) || string.IsNullOrWhiteSpace(targetId))
                    throw new ArgumentException("Exits require a supported direction and destination.", nameof(rooms));
                exits.Add(direction, targetId);
            }

            var detached = new RoomDefinition(
                room.Id,
                room.Name,
                new ReadOnlyDictionary<Direction, string>(exits));
            if (!index.TryAdd(detached.Id, detached))
                throw new ArgumentException("Duplicate room id.", nameof(rooms));
        }

        foreach (var room in index.Values)
        {
            foreach (var targetId in room.Exits.Values)
            {
                if (!index.ContainsKey(targetId))
                    throw new ArgumentException("An exit references a nonexistent room.", nameof(rooms));
            }
        }

        _rooms = new ReadOnlyDictionary<string, RoomDefinition>(index);
    }

    public int RoomCount => _rooms.Count;

    public IEnumerable<RoomDefinition> Rooms => _rooms.Values;

    public bool TryGetRoom(string id, out RoomDefinition? room) =>
        _rooms.TryGetValue(id, out room);

    /// <summary>
    /// Does not infer reverse exits: one-way and vertical room connections are valid.
    /// </summary>
    public bool TryMove(string fromId, Direction direction, out string? destinationId)
    {
        destinationId = null;
        return _rooms.TryGetValue(fromId, out var room) &&
               room.Exits.TryGetValue(direction, out destinationId);
    }
}
