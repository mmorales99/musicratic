using Microsoft.EntityFrameworkCore;
using Musicratic.Hub.Domain.Enums;
using Musicratic.Hub.Infrastructure.Persistence;
using Musicratic.Shared.Contracts;

namespace Musicratic.Hub.Infrastructure.Services;

public sealed class HubDiscoveryProvider(HubDbContext dbContext) : IHubDiscoveryProvider
{
    public async Task<(IReadOnlyList<HubDiscoveryResult> Items, int TotalCount)> Search(
        HubSearchCriteria criteria,
        CancellationToken ct)
    {
        var query = dbContext.Hubs
            .Where(h => !h.IsDeleted)
            .Where(h => h.Visibility == HubVisibility.Public)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(criteria.SearchTerm))
            query = query.Where(h => h.Name.Contains(criteria.SearchTerm));

        if (criteria.IsActive.HasValue)
            query = query.Where(h => h.IsActive == criteria.IsActive.Value);

        query = criteria.SortBy?.ToLowerInvariant() switch
        {
            "trending" => query
                .Include(h => h.Members)
                .OrderByDescending(h => h.Members.Count),
            "name" => query.OrderBy(h => h.Name),
            _ => query.OrderByDescending(h => h.CreatedAt)
        };

        var totalCount = await query.CountAsync(ct);

        var hubs = await query
            .Include(h => h.Members)
            .Skip((criteria.Page - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToListAsync(ct);

        var results = hubs.Select(h => new HubDiscoveryResult(
            HubId: h.Id,
            Name: h.Name,
            Description: null,
            HubType: h.Type.ToString(),
            IsActive: h.IsActive,
            ActiveListenerCount: h.Members.Count,
            CurrentTrackTitle: null,
            CurrentTrackArtist: null
        )).ToList();

        return (results, totalCount);
    }
}
