using FluentAssertions;
using Musicratic.Core.Models;

namespace Musicratic.Core.Tests;

[Collection("MusicraticSuite")]
public class HubConfigTests
{
    [Fact]
    public void FadeOutDuration_Default_Is10Seconds()
    {
        new HubConfig().FadeOutDuration.Should().Be(TimeSpan.FromSeconds(10));
    }

    [Fact]
    public void FadeOutDuration_BelowMinimum_ClampsTo5Seconds()
    {
        var config = new HubConfig { FadeOutDuration = TimeSpan.FromSeconds(2) };
        config.FadeOutDuration.Should().Be(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void FadeOutDuration_AboveMaximum_ClampsTo15Seconds()
    {
        var config = new HubConfig { FadeOutDuration = TimeSpan.FromSeconds(20) };
        config.FadeOutDuration.Should().Be(TimeSpan.FromSeconds(15));
    }

    [Theory]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(15)]
    public void FadeOutDuration_WithinRange_StoredAsIs(double seconds)
    {
        var config = new HubConfig { FadeOutDuration = TimeSpan.FromSeconds(seconds) };
        config.FadeOutDuration.Should().Be(TimeSpan.FromSeconds(seconds));
    }
}
