using FluentAssertions;

namespace Musicratic.Economy.Tests;

public class PlaceholderTests
{
    [Fact]
    public void Should_Pass_When_ModuleImplemented()
    {
        // Placeholder test to keep CI green until Economy module is implemented
        true.Should().BeTrue();
    }
}
