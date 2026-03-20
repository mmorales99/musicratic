// Disable parallel execution across test classes — all test classes share the static StateStore.
[assembly: Xunit.CollectionBehavior(DisableTestParallelization = true)]
