using Arthea.Core.Social;
using Microsoft.Data.Sqlite;

namespace Arthea.Persistence;

/// <summary>
/// Single-node SQLite implementation of room-local public chat. The host must
/// place the DB in an owner-private directory and manage disk encryption/backups.
/// Never store passwords, session tokens, private messages or client credentials.
/// </summary>
public sealed class SqliteRoomChatJournal : IRoomChatJournal
{
    private readonly string _connectionString;
    private readonly object _gate = new();

    public SqliteRoomChatJournal(string databasePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);
        if (!Path.IsPathFullyQualified(databasePath))
            throw new ArgumentException("Journal path must be absolute.", nameof(databasePath));
        var path = Path.GetFullPath(databasePath);
        var parent = Path.GetDirectoryName(path);
        if (parent is null || !Directory.Exists(parent))
            throw new ArgumentException("Journal directory must already exist.", nameof(databasePath));
        if (File.Exists(path) && (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
            throw new ArgumentException("Journal path must not be a symbolic link.", nameof(databasePath));
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = path,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Private,
            Pooling = false,
            DefaultTimeout = 5
        }.ToString();

        lock (_gate)
        {
            using var conn = Open();
            using var setup = conn.CreateCommand();
            setup.CommandText = """
                PRAGMA journal_mode=WAL;
                PRAGMA synchronous=FULL;
                CREATE TABLE IF NOT EXISTS room_streams (
                    room_id TEXT PRIMARY KEY NOT NULL,
                    last_sequence INTEGER NOT NULL CHECK (last_sequence >= 0)
                );
                CREATE TABLE IF NOT EXISTS room_messages (
                    room_id TEXT NOT NULL,
                    sequence INTEGER NOT NULL CHECK(sequence > 0),
                    sender_id TEXT NOT NULL,
                    body TEXT NOT NULL,
                    PRIMARY KEY (room_id, sequence)
                );
                """;
            setup.ExecuteNonQuery();

            using var check = conn.CreateCommand();
            check.CommandText = "PRAGMA user_version;";
            var schema = Convert.ToInt32(check.ExecuteScalar());
            if (schema is not (0 or 1))
                throw new InvalidDataException("Unsupported journal schema version.");
            if (schema == 0)
            {
                using var setVersion = conn.CreateCommand();
                setVersion.CommandText = "PRAGMA user_version=1;";
                setVersion.ExecuteNonQuery();
            }
            // Permission hardening is defense-in-depth; host must also protect
            // the directory, backup files, SQLite WAL and snapshot artifacts.
            if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
                File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite);
        }
    }

    public long LastSequence(string roomId)
    {
        ValidateRoom(roomId);
        lock (_gate)
        {
            using var conn = Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT last_sequence FROM room_streams WHERE room_id = $r;";
            cmd.Parameters.AddWithValue("$r", roomId);
            return cmd.ExecuteScalar() is { } value ? Convert.ToInt64(value) : 0;
        }
    }

    public RoomChatMessage Append(string roomId, string senderId, string text, int retainedPerRoom)
    {
        ValidateRoom(roomId);
        ArgumentException.ThrowIfNullOrWhiteSpace(senderId);
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        if (retainedPerRoom is < 1 or > 1000)
            throw new ArgumentOutOfRangeException(nameof(retainedPerRoom));

        lock (_gate)
        {
            using var conn = Open();
            using var tx = conn.BeginTransaction(deferred: false);
            using var next = conn.CreateCommand();
            next.Transaction = tx;
            next.CommandText = """
                INSERT INTO room_streams (room_id, last_sequence)
                VALUES ($r, 1)
                ON CONFLICT(room_id)
                DO UPDATE SET last_sequence = last_sequence + 1
                RETURNING last_sequence;
                """;
            next.Parameters.AddWithValue("$r", roomId);
            var seq = Convert.ToInt64(next.ExecuteScalar());
            using var insert = conn.CreateCommand();
            insert.Transaction = tx;
            insert.CommandText = """
                INSERT INTO room_messages (room_id, sequence, sender_id, body)
                VALUES ($r, $seq, $sender, $text);
                """;
            insert.Parameters.AddWithValue("$r", roomId);
            insert.Parameters.AddWithValue("$seq", seq);
            insert.Parameters.AddWithValue("$sender", senderId);
            insert.Parameters.AddWithValue("$text", text);
            insert.ExecuteNonQuery();
            using var trim = conn.CreateCommand();
            trim.Transaction = tx;
            trim.CommandText = """
                DELETE FROM room_messages
                WHERE room_id = $r AND sequence <= $cutoff;
                """;
            trim.Parameters.AddWithValue("$r", roomId);
            trim.Parameters.AddWithValue("$cutoff", seq - retainedPerRoom);
            trim.ExecuteNonQuery();
            tx.Commit();
            return new RoomChatMessage(seq, roomId, senderId, text);
        }
    }

    public RoomReplay Replay(string roomId, long afterSequence, int maxCount)
    {
        ValidateRoom(roomId);
        if (afterSequence < 0) throw new ArgumentOutOfRangeException(nameof(afterSequence));
        if (maxCount is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(maxCount));
        lock (_gate)
        {
            using var conn = Open();
            using var tx = conn.BeginTransaction(deferred: true);
            using var oldest = conn.CreateCommand();
            oldest.Transaction = tx;
            oldest.CommandText = "SELECT MIN(sequence) FROM room_messages WHERE room_id = $r;";
            oldest.Parameters.AddWithValue("$r", roomId);
            var min = oldest.ExecuteScalar();
            var gap = min is not (null or DBNull) && Convert.ToInt64(min) - 1 > afterSequence;
            using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = """
                SELECT sequence, sender_id, body
                FROM room_messages
                WHERE room_id = $r AND sequence > $cursor
                ORDER BY sequence ASC
                LIMIT $limit;
                """;
            cmd.Parameters.AddWithValue("$r", roomId);
            cmd.Parameters.AddWithValue("$cursor", afterSequence);
            cmd.Parameters.AddWithValue("$limit", maxCount);
            var messages = new List<RoomChatMessage>(maxCount);
            using (var reader = cmd.ExecuteReader())
                while (reader.Read())
                    messages.Add(new RoomChatMessage(reader.GetInt64(0), roomId,
                        reader.GetString(1), reader.GetString(2)));
            tx.Commit();
            return new RoomReplay(gap, Array.AsReadOnly(messages.ToArray()));
        }
    }

    private SqliteConnection Open()
    {
        var conn = new SqliteConnection(_connectionString);
        try { conn.Open(); return conn; }
        catch { conn.Dispose(); throw; }
    }

    private static void ValidateRoom(string roomId)
    {
        if (string.IsNullOrWhiteSpace(roomId) || roomId.Length > 256 ||
            roomId.Any(char.IsControl))
            throw new ArgumentException("Invalid room identity.", nameof(roomId));
    }
}
