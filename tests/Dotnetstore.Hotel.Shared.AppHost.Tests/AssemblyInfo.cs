using Xunit;

// Every test class in this assembly boots a full AppHost (3 services + an ephemeral Postgres container).
// Running them in parallel (xUnit's default, by test class) overwhelms Docker Desktop on a single dev
// machine once there are more than a handful of these - causing HTTP-level timeouts unrelated to the
// code under test. Running them sequentially trades wall-clock time for reliability.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
