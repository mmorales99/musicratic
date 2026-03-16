using Musicratic.Hub.Domain.Enums;
using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Commands.UpdateHubSettings;

public sealed record UpdateHubSettingsCommand(
    Guid HubId,
    bool? AllowProposals,
    double? AutoSkipThreshold,
    int? VotingWindowSeconds,
    int? MaxQueueSize,
    List<MusicProvider>? AllowedProviders,
    bool? EnableLocalStorage,
    bool? AdsEnabled) : ICommand;
