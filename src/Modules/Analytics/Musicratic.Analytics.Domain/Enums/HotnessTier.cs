namespace Musicratic.Analytics.Domain.Enums;

/// <summary>
/// ANLT-008: Hotness tier for hub activity level.
/// Used by Economy module for coin pricing multiplier.
/// See docs/06-monetization.md.
/// </summary>
public enum HotnessTier
{
    Low = 0,
    Medium = 1,
    High = 2
}
