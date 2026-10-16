using ArtheaEngine.Model;

namespace ArtheaEngine.Model
{
    // Model-only test stubs; the original BSON model is intentionally not linked.
    public enum Direction { North, South, East, West, Up, Down }

    public sealed class Exit
    {
        public Direction Direction { get; set; }
        public Room Room { get; set; }
        public Room ToRoom { get; set; }
    }

    public sealed class Room
    {
        private static readonly Dictionary<string, Room> Registered =
            new(StringComparer.Ordinal);
        private static readonly List<Room> Constructed = new();

        public Room() => Constructed.Add(this);
        public static IReadOnlyList<Room> CreatedRooms => Constructed;

        public string Id { get; set; } = "new-room";
        public string Name { get; set; } = "New room";
        public Dictionary<Direction, Exit> Exits { get; set; } = new();

        public static void Register(Room room) => Registered.Add(room.Id, room);
        public static Room Lookup(string id) =>
            Registered.TryGetValue(id, out var room) ? room : null;
        public static void Clear()
        {
            Registered.Clear();
            Constructed.Clear();
        }
    }
}

namespace ArtheaEngine
{
    public static class DbManager
    {
        public static T Find<T>(string name) where T : class =>
            typeof(T) == typeof(Room) ? Room.Lookup(name) as T : null;
    }

    public static class ExitEditorCompatibility
    {
        public static bool Is(this string value, string expected) =>
            value.Equals(expected, StringComparison.OrdinalIgnoreCase);

        // Mirrors ONLY the six cases from original ArtheaEngine/Extensions.cs.
        // This is test support, not a compiled copy of the full Extensions.cs.
        // The test suite also checks this mapping against the original source.
        public static Direction Reverse(this Direction direction) => direction switch
        {
            Direction.Down => Direction.Up,
            Direction.East => Direction.West,
            Direction.North => Direction.South,
            Direction.South => Direction.North,
            Direction.Up => Direction.Down,
            Direction.West => Direction.East,
            _ => throw new Exception("Unknown direction for reverse")
        };
    }
}

namespace ArtheaServer
{
    public sealed class TelnetConnection { }
}

namespace ArtheaServer.Creation
{
    public abstract class Editor
    {
        protected readonly TelnetConnection _connection;

        protected Editor(TelnetConnection connection) => _connection = connection;
        public abstract string Title { get; }
        public virtual bool Execute(ArtheaEngine.Argument _) => false;
        public virtual void Show() { }
        public virtual void Save() { }
        protected void Reset() { }
        protected void WriteLine(string _, params object[] __) { }
        protected void Forward(Editor _) { }
    }

    public sealed class ExitEditor : Editor
    {
        public ExitEditor(TelnetConnection connection, Exit _) : base(connection) { }
        public override string Title => "Synthetic exit editor";
    }
}
