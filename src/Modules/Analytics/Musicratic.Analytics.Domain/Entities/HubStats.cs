using Musicratic.Shared.Domain;

namespace Musicratic.Analytics.Domain.Entities;

/// <summary>
/// ANLT-001: Aggregated hub-level statistics.
/// Tracks total plays, votes, unique listeners, and activity data.
/// </summary>
public sealed class HubStats : BaseEntity, ITenantScoped
{
    public Guid TenantId { get; private set; }

    public Guid HubId { get; private set; }

    public int TotalPlays { get; private set; }

    public int TotalVotes { get; private set; }

    public int UniqueListeners { get; private set; }

    public double ActiveHours { get; private set; }

    public int PeakConcurrentUsers { get; private set; }

    public DateTime? LastActivityAt { get; private set; }

    private HubStats() { }

    public static HubStats Create(Guid hubId, Guid tenantId)
    {
        if (hubId == Guid.Empty)
            throw new ArgumentException("Hub ID is required.", nameof(hubId));

        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant ID is required.", nameof(tenantId));

        return new HubStats
        {
            HubId = hubId,
            TenantId = tenantId,
            TotalPlays = 0,
            TotalVotes = 0,
            UniqueListeners = 0,
            ActiveHours = 0,
            PeakConcurrentUsers = 0
        };
    }

    public void IncrementPlays()
    {
        TotalPlays++;
        LastActivityAt = DateTime.UtcNow;
    }

    public void IncrementVotes()
    {
        TotalVotes++;
        LastActivityAt = DateTime.UtcNow;
    }

    public void UpdateConcurrentUsers(int concurrentUsers)
    {
        if (concurrentUsers > PeakConcurrentUsers)
            PeakConcurrentUsers = concurrentUsers;

        LastActivityAt = DateTime.UtcNow;
    }

    public void AddActiveHours(double hours)
    {
        ActiveHours += hours;
    }

    public void SetUniqueListeners(int count)
    {
        UniqueListeners = count;
    }
}
