using System.Diagnostics;
using Arthea.Core.Social;
using Arthea.Core.World;

// Manual, non-gating characterization: results depend on host, JIT, scheduling,
// journal adapter and I/O. Never use elapsed time as a CI pass threshold.
var rooms = Enumerable.Range(0, 8).Select(i =>
    new RoomDefinition("room-" + i, "Room " + i,
        new Dictionary<Direction, string>())).ToArray();
var graph = new RoomGraph(rooms);
const int total = 8000;

static (TimeSpan Elapsed, int Completed) Measure(RoomGraph graph, int workers, int count)
{
    var runtime = new RoomChatRuntime(graph, retainedPerRoom: 100);
    for (var i = 0; i < workers; i++)
        runtime.Enter("writer-" + i, "room-" + (i % graph.RoomCount));

    var timer = Stopwatch.StartNew();
    Parallel.For(0, workers, worker =>
    {
        for (var j = 0; j < count / workers; j++)
            runtime.Say("writer-" + worker, "load");
    });
    timer.Stop();
    return (timer.Elapsed, count);
}

foreach (var workers in new[] { 1, 2, 4, 8 })
{
    var (elapsed, completed) = Measure(graph, workers, total);
    Console.WriteLine($"workers={workers} operations={completed} elapsed_ms={elapsed.TotalMilliseconds:F2} ops_per_sec={completed / elapsed.TotalSeconds:F0}");
}
