using Musicratic.Hub.Domain.Enums;

namespace Musicratic.Hub.Application.DTOs;

public sealed record HubSettingsDto(
    bool AllowProposals,
    double AutoSkipThreshold,
    int VotingWindowSeconds,
    int MaxQueueSize,
    List<MusicProvider> AllowedProviders,
    bool EnableLocalStorage,
    bool AdsEnabled);
