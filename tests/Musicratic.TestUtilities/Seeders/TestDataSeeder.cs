using Microsoft.EntityFrameworkCore;
using Musicratic.Auth.Domain.Entities;
using Musicratic.Hub.Domain.Enums;
using Musicratic.Playback.Domain.Entities;
using Musicratic.TestUtilities.Builders;
using HubEntity = Musicratic.Hub.Domain.Entities.Hub;

namespace Musicratic.TestUtilities.Seeders;

public static class TestDataSeeder
{
    public static async Task<User> SeedUser(
        DbContext context, Action<UserBuilder>? configure = null)
    {
        var builder = new UserBuilder();
        configure?.Invoke(builder);

        var user = builder.Build();
        context.Add(user);
        await context.SaveChangesAsync();
        user.ClearDomainEvents();

        return user;
    }

    public static async Task<HubEntity> SeedHub(
        DbContext context, Action<HubBuilder>? configure = null)
    {
        var builder = new HubBuilder();
        configure?.Invoke(builder);

        var hub = builder.Build();
        context.Add(hub);
        await context.SaveChangesAsync();
        hub.ClearDomainEvents();

        return hub;
    }

    public static async Task<HubEntity> SeedHubWithMembers(
        DbContext context, Action<HubBuilder>? configure = null)
    {
        var builder = new HubBuilder();
        configure?.Invoke(builder);

        var hub = builder.Build();

        hub.AddMember(hub.OwnerId, HubMemberRole.SuperOwner, null);
        hub.AddMember(Guid.NewGuid(), HubMemberRole.SubHubManager, hub.OwnerId);
        hub.AddMember(Guid.NewGuid(), HubMemberRole.Visitor, hub.OwnerId);

        context.Add(hub);
        await context.SaveChangesAsync();
        hub.ClearDomainEvents();

        return hub;
    }

    public static async Task<Track> SeedTrack(
        DbContext context, Action<TrackBuilder>? configure = null)
    {
        var builder = new TrackBuilder();
        configure?.Invoke(builder);

        var track = builder.Build();
        context.Add(track);
        await context.SaveChangesAsync();
        track.ClearDomainEvents();

        return track;
    }

    public static async Task<IReadOnlyList<QueueEntry>> SeedQueueWithTracks(
        DbContext context, Guid hubId, int count = 3)
    {
        var entries = new List<QueueEntry>();

        for (var i = 0; i < count; i++)
        {
            var track = new TrackBuilder()
                .WithTitle($"Queue Track {i + 1}")
                .WithExternalId($"spotify-queue-{Guid.NewGuid():N}")
                .Build();
            context.Add(track);

            var entry = new QueueEntryBuilder()
                .WithHubId(hubId)
                .WithTenantId(hubId)
                .WithTrackId(track.Id)
                .WithPosition(i)
                .Build();
            context.Add(entry);
            entries.Add(entry);
        }

        await context.SaveChangesAsync();
        return entries.AsReadOnly();
    }
}
