using Musicratic.Analytics.Domain.Enums;

namespace Musicratic.Analytics.Application.Services;

/// <summary>
/// ANLT-008: Calculates hotness tier for a hub based on activity.
/// Used by Economy module for coin pricing multiplier.
/// See docs/06-monetization.md.
/// </summary>
public interface IHotnessService
{
    Task<HotnessTier> CalculateHotness(
        Guid hubId, CancellationToken cancellationToken = default);
}
