using Musicratic.Hub.Domain.Enums;

namespace Musicratic.Hub.Domain.Entities;

public sealed record HubSettings
{
    public bool AllowProposals { get; init; } = true;

    public double AutoSkipThreshold { get; init; } = 0.65;

    public int VotingWindowSeconds { get; init; } = 60;

    public int MaxQueueSize { get; init; } = 50;

    public List<MusicProvider> AllowedProviders { get; init; } = [MusicProvider.Spotify];

    public bool EnableLocalStorage { get; init; }

    public bool AdsEnabled { get; init; }
}
