namespace Musicratic.Analytics.Application.DTOs;

/// <summary>
/// ANLT-007: Monthly report of top proposed tracks for a hub.
/// </summary>
public sealed record PopularProposalsReport(
    Guid HubId,
    DateTime GeneratedAt,
    IReadOnlyList<PopularProposalEntry> Proposals);
