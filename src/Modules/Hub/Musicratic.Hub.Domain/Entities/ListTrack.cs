using Musicratic.Hub.Domain.Enums;
using Musicratic.Shared.Domain;

namespace Musicratic.Hub.Domain.Entities;

public sealed class ListTrack : BaseEntity, ITenantScoped
{
    public Guid ListId { get; private set; }

    public List? List { get; private set; }

    public Guid TrackId { get; private set; }

    public Guid TenantId { get; private set; }

    public int Position { get; private set; }

    public DateTime AddedAt { get; private set; }

    public int TotalUpvotes { get; private set; }

    public int TotalDownvotes { get; private set; }

    public int TotalPlays { get; private set; }

    public double ShuffleWeight { get; private set; }

    private ListTrack() { }

    public static ListTrack Create(Guid listId, Guid tenantId, Guid trackId, int position)
    {
        return new ListTrack
        {
            ListId = listId,
            TenantId = tenantId,
            TrackId = trackId,
            Position = position,
            AddedAt = DateTime.UtcNow,
            TotalUpvotes = 0,
            TotalDownvotes = 0,
            TotalPlays = 0,
            ShuffleWeight = 1.0
        };
    }

    public void UpdatePosition(int newPosition)
    {
        Position = newPosition;
    }

    public void RecordUpvote()
    {
        TotalUpvotes++;
        RecalculateShuffleWeight();
    }

    public void RecordDownvote()
    {
        TotalDownvotes++;
        RecalculateShuffleWeight();
    }

    public void RecordPlay()
    {
        TotalPlays++;
    }

    private void RecalculateShuffleWeight()
    {
        var totalVotes = TotalUpvotes + TotalDownvotes;
        ShuffleWeight = totalVotes == 0 ? 1.0 : (double)TotalUpvotes / totalVotes;
    }
}
