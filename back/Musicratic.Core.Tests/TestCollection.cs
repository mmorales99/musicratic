namespace Musicratic.Core.Tests;

/// <summary>
/// All tests share one named collection so xUnit runs them sequentially —
/// required because they all share the static StateStore.
/// </summary>
[CollectionDefinition("MusicraticSuite")]
public class MusicraticSuiteCollection { }
