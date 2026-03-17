namespace Musicratic.Economy.Application.Services;

/// <summary>
/// ECON-014: Free trial lifecycle service per docs/06-monetization.md.
/// 30-day trial, prompts at day 20/28, deactivation at day 30, data retention 90 days.
/// </summary>
public interface IFreeTrialService
{
    Task<FreeTrialResult> StartTrial(
        Guid hubId, Guid tenantId, CancellationToken cancellationToken = default);

    Task<TrialStatusResult> GetTrialStatus(
        Guid hubId, CancellationToken cancellationToken = default);
}

public sealed record FreeTrialResult(
    bool Success,
    DateTime? TrialEndsAt,
    string? ErrorMessage = null);

public sealed record TrialStatusResult(
    bool IsTrialActive,
    int DaysRemaining,
    bool ShouldPromptConversion,
    bool ShouldWarnExpiry);
