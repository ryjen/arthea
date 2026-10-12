using System.Text;
using System.Text.Json;
using Arthea.Core.World;

namespace Arthea.Transport;

/// <summary>
/// Bounded incremental Telnet/GMCP framing for a single connection.
/// No TCP/TLS listener: the trusted transport feeds bytes and consumes explicit
/// responses. It does not authenticate, log, persist, or dispatch commands.
/// </summary>
public sealed class TelnetGmcpCodec
{
    public const byte Iac = 255, Sb = 250, Se = 240, Will = 251, Wont = 252,
        Do = 253, Dont = 254, Gmcp = 201;
    private enum State { Data, Iac, Option, SubOption, SubData, SubIac, Discard, DiscardIac }
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);
    private readonly List<byte> _text = new();
    private readonly List<byte> _sub = new();
    private readonly HashSet<string> _supports = new(StringComparer.OrdinalIgnoreCase);
    private State _state;
    private byte _verb;
    private byte _subOption;
    private bool _roomAdvertised;

    public bool Enabled { get; private set; }
    public bool SupportsRoomInfo => Enabled && _roomAdvertised;
    public static byte[] OfferGmcp() => [Iac, Will, Gmcp];

    /// <summary>
    /// Accept a bounded chunk and return complete plain-text lines and response
    /// negotiation frames. One connection owns one codec; never share across players.
    /// </summary>
    public TelnetGmcpBatch Feed(ReadOnlySpan<byte> chunk)
    {
        if (chunk.Length > 4096) throw new InvalidDataException("Telnet input chunk exceeds limit.");
        var lines = new List<string>();
        var replies = new List<byte[]>();
        foreach (var b in chunk)
        {
            switch (_state)
            {
                case State.Data:
                    if (b == Iac) _state = State.Iac;
                    else if (b == '\n') EmitLine(lines);
                    else AddText(b);
                    break;
                case State.Iac:
                    if (b is Do or Dont or Will or Wont)
                    {
                        _verb = b;
                        _state = State.Option;
                    }
                    else if (b == Sb) _state = State.SubOption;
                    else if (b == Iac) throw new InvalidDataException("Raw IAC is not permitted in text commands.");
                    else _state = State.Data;
                    break;
                case State.Option:
                    if (b == Gmcp)
                    {
                        if (_verb == Do) Enabled = true;
                        else if (_verb is Dont or Wont)
                        {
                            Enabled = false;
                            _roomAdvertised = false;
                            _supports.Clear();
                        }
                        else if (_verb == Will)
                            replies.Add([Iac, Dont, Gmcp]); // Only server -> client GMCP offer is supported.
                    }
                    else if (_verb == Do) replies.Add([Iac, Wont, b]);
                    else if (_verb == Will) replies.Add([Iac, Dont, b]);
                    _state = State.Data;
                    break;
                case State.SubOption:
                    _subOption = b;
                    _sub.Clear();
                    _state = b == Gmcp && Enabled ? State.SubData : State.Discard;
                    break;
                case State.SubData:
                    if (b == Iac) _state = State.SubIac;
                    else AddSub(b);
                    break;
                case State.SubIac:
                    if (b == Se)
                    {
                        ParseGmcp();
                        _state = State.Data;
                    }
                    else if (b == Iac)
                    {
                        AddSub(Iac); // Escaped IAC in a subnegotiation payload.
                        _state = State.SubData;
                    }
                    else throw new InvalidDataException("Malformed GMCP subnegotiation.");
                    break;
                case State.Discard:
                    if (b == Iac) _state = State.DiscardIac;
                    break;
                case State.DiscardIac:
                    if (b == Se) _state = State.Data;
                    else _state = State.Discard;
                    break;
            }
        }
        return new TelnetGmcpBatch(lines.AsReadOnly(), replies.AsReadOnly());
    }

    private void AddText(byte b)
    {
        if (b == 0 || _text.Count >= 1024)
            throw new InvalidDataException("Invalid or oversized Telnet command.");
        _text.Add(b);
    }

    private void EmitLine(List<string> lines)
    {
        if (_text.Count > 0 && _text[^1] == '\r') _text.RemoveAt(_text.Count - 1);
        string text;
        try { text = StrictUtf8.GetString(_text.ToArray()); }
        catch (DecoderFallbackException) { throw new InvalidDataException("Invalid UTF-8 command."); }
        _text.Clear();
        if (text.Any(char.IsControl)) throw new InvalidDataException("Control character in command.");
        lines.Add(text);
        if (lines.Count > 64) throw new InvalidDataException("Too many commands per chunk.");
    }

    private void AddSub(byte b)
    {
        if (_sub.Count >= 4096) throw new InvalidDataException("GMCP payload exceeds limit.");
        _sub.Add(b);
    }

    private void ParseGmcp()
    {
        string message;
        try { message = StrictUtf8.GetString(_sub.ToArray()); }
        catch (DecoderFallbackException) { throw new InvalidDataException("Invalid UTF-8 GMCP payload."); }
        _sub.Clear();
        var space = message.IndexOf(' ');
        var command = space < 0 ? message : message[..space];
        if (command.Length is < 1 or > 64 || !command.All(c => char.IsAsciiLetter(c) || c == '.'))
            throw new InvalidDataException("Invalid GMCP command name.");
        var body = space < 0 ? "" : message[(space + 1)..];
        if (command.Equals("Core.Hello", StringComparison.OrdinalIgnoreCase))
        {
            // No trust conveyed by untrusted client self-identification.
            if (body.Length == 0) return;
            try
            {
                using var json = JsonDocument.Parse(body, new JsonDocumentOptions { MaxDepth = 8 });
                if (json.RootElement.ValueKind != JsonValueKind.Object)
                    throw new InvalidDataException("Invalid Core.Hello.");
            }
            catch (JsonException) { throw new InvalidDataException("Invalid Core.Hello."); }
        }
        else if (command.Equals("Core.Supports.Set", StringComparison.OrdinalIgnoreCase) ||
                 command.Equals("Core.Supports.Add", StringComparison.OrdinalIgnoreCase) ||
                 command.Equals("Core.Supports.Remove", StringComparison.OrdinalIgnoreCase))
        {
            string[] modules;
            try
            {
                using var json = JsonDocument.Parse(body, new JsonDocumentOptions { MaxDepth = 8 });
                if (json.RootElement.ValueKind != JsonValueKind.Array ||
                    json.RootElement.GetArrayLength() > 32)
                    throw new InvalidDataException("Invalid Core.Supports.");
                modules = json.RootElement.EnumerateArray().Select(item =>
                    item.ValueKind == JsonValueKind.String ? item.GetString() ?? "" : "")
                    .ToArray();
            }
            catch (JsonException) { throw new InvalidDataException("Invalid Core.Supports."); }
            if (modules.Any(m => m.Length is < 1 or > 80))
                throw new InvalidDataException("Invalid Core.Supports module.");
            bool remove = command.Equals("Core.Supports.Remove", StringComparison.OrdinalIgnoreCase);
            bool reset = command.Equals("Core.Supports.Set", StringComparison.OrdinalIgnoreCase);
            var validated = new List<string>(modules.Length);
            foreach (var raw in modules)
            {
                var parts = raw.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (remove && parts.Length != 1 ||
                    !remove && (parts.Length != 2 ||
                      !int.TryParse(parts[1], out var version) || version is < 1 or > 1000))
                    throw new InvalidDataException("Invalid GMCP module declaration.");
                var name = parts[0];
                if (!name.All(c => char.IsAsciiLetter(c) || c == '.') || name.Length > 64)
                    throw new InvalidDataException("Invalid GMCP module name.");
                validated.Add(name);
            }
            // Do not partially mutate capability state when a declaration fails.
            if (reset) _supports.Clear();
            foreach (var name in validated)
            {
                if (remove) _supports.Remove(name);
                else _supports.Add(name);
            }
            _roomAdvertised = _supports.Contains("Room") || _supports.Contains("Room.Info");
        }
        // Unknown GMCP modules are ignored: they cannot mutate the game state.
    }

    /// <summary>
    /// Optional structured room metadata for a client that negotiated GMCP and
    /// announced Room support. Text-only clients receive no GMCP frames.
    /// </summary>
    public byte[]? EncodeRoomInfo(RoomGraph world, string roomId)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!SupportsRoomInfo) return null;
        if (!world.TryGetRoom(roomId, out var room) || room is null)
            throw new ArgumentException("Unknown room.", nameof(roomId));
        return EncodeRoomInfo(room);
    }

    public byte[]? EncodeRoomInfo(RoomDefinition room)
    {
        ArgumentNullException.ThrowIfNull(room);
        if (!SupportsRoomInfo) return null;
        var exits = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (direction, target) in room.Exits)
        {
            var key = direction switch
            {
                Direction.North => "n", Direction.South => "s",
                Direction.East => "e", Direction.West => "w",
                Direction.Up => "u", Direction.Down => "d",
                _ => throw new InvalidDataException("Unknown exit direction.")
            };
            exits.Add(key, target);
        }
        // `num` is the conventional mapper key (MudForge/Mudlet). Fresh-world
        // room IDs are opaque, so emit the same stable string as `id` instead
        // of inventing unstable numeric IDs. Optional zone/terrain are absent.
        var json = JsonSerializer.Serialize(new { num = room.Id, id = room.Id, name = room.Name, exits });
        var payload = StrictUtf8.GetBytes("Room.Info " + json);
        if (payload.Length > 4096)
            throw new InvalidDataException("Room metadata exceeds GMCP limit.");
        var frame = new List<byte>(payload.Length + 5) { Iac, Sb, Gmcp };
        foreach (var value in payload)
        {
            frame.Add(value);
            if (value == Iac) frame.Add(Iac);
        }
        frame.Add(Iac);
        frame.Add(Se);
        return frame.ToArray();
    }
}

public sealed record TelnetGmcpBatch(IReadOnlyList<string> Lines, IReadOnlyList<byte[]> Replies);
