using Arthea.Core.World;
using Arthea.LegacyBridge;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace Arthea.LegacyBson;

/// <summary>
/// Bounded, read-only BSON decoding for a legacy-shaped Arthea room subset.
/// Never instantiates the old Room model (its reference setters perform DB lookups).
/// No database connections, live credentials, or network activity.
/// </summary>
public static class LegacyBsonRoomReader
{
    public const int MaxRoomBytes = 256 * 1024;
    public const int MaxRoomCount = 10_000;
    public const int MaxTotalBytes = 64 * 1024 * 1024;

    public static RoomGraph ReadGraph(IEnumerable<byte[]> roomDocuments)
    {
        ArgumentNullException.ThrowIfNull(roomDocuments);
        var snapshots = new List<LegacyRoomSnapshot>();
        long total = 0;
        foreach (var bytes in roomDocuments)
        {
            if (snapshots.Count == MaxRoomCount)
                throw new InvalidDataException("Too many room documents.");
            if (bytes is null || bytes.Length == 0 || bytes.Length > MaxRoomBytes)
                throw new InvalidDataException("Invalid room document size.");
            total += bytes.Length;
            if (total > MaxTotalBytes)
                throw new InvalidDataException("Room import exceeds its byte budget.");
            snapshots.Add(ReadRoom(bytes));
        }
        return LegacyRoomProjection.Build(snapshots);
    }

    public static LegacyRoomSnapshot ReadRoom(byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        if (bytes.Length == 0 || bytes.Length > MaxRoomBytes)
            throw new InvalidDataException("Invalid room document size.");

        BsonDocument doc;
        using (var stream = new MemoryStream(bytes, writable: false))
        {
            try
            {
                doc = BsonSerializer.Deserialize<BsonDocument>(stream);
            }
            catch (Exception ex) when (ex is FormatException or EndOfStreamException
                                           or BsonSerializationException or InvalidOperationException)
            {
                // Do not embed raw BSON, credentials, or field values in error messages.
                throw new InvalidDataException("Invalid BSON room document.");
            }
            if (stream.Position != stream.Length)
                throw new InvalidDataException("Trailing data after a BSON room document.");
        }

        // Legacy Room.Id has [BsonId] on a string with StringObjectIdGenerator;
        // do not silently coerce ObjectId, numeric IDs or Extended JSON.
        var id = RequiredString(doc, "_id", 256);
        var name = RequiredString(doc, "Name", 1024);
        if (!doc.TryGetValue("Exits", out var exitsField) || exitsField is not BsonDocument exitsDocument)
            throw new InvalidDataException("Room must contain an Exits document.");
        if (exitsDocument.ElementCount > 6)
            throw new InvalidDataException("Room has too many directional exits.");

        var exits = new Dictionary<string, LegacyExitSnapshot>(StringComparer.Ordinal);
        foreach (var element in exitsDocument.Elements)
        {
            if (element.Value is not BsonDocument exitDocument)
                throw new InvalidDataException("Exit must be a BSON document.");
            var target = RequiredString(exitDocument, "ToRoomRef", 256);
            // In the historic "link" editor path Exit.Direction may be unset.
            // It is intentionally ignored: the containing dictionary key controls movement.
            if (!exits.TryAdd(element.Name, new LegacyExitSnapshot(target, null)))
                throw new InvalidDataException("Duplicate exit field.");
        }
        return new LegacyRoomSnapshot(id, name, exits);
    }

    private static string RequiredString(BsonDocument doc, string field, int maxLength)
    {
        if (!doc.TryGetValue(field, out var value) || value is not BsonString text ||
            string.IsNullOrWhiteSpace(text.Value) || text.Value.Length > maxLength)
            throw new InvalidDataException("Missing or invalid required BSON room field.");
        return text.Value;
    }
}
