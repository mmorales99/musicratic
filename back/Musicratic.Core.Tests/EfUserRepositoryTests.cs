using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Musicratic.Core.Data;
using Musicratic.Core.Models;

namespace Musicratic.Core.Tests;

public class EfUserRepositoryTests
{
    private static MusicraticDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<MusicraticDbContext>()
            .UseInMemoryDatabase($"EfUserRepo_{Guid.NewGuid()}")
            .Options);

    [Fact]
    public async Task AddAsync_PersistsUser()
    {
        using var ctx = CreateContext();
        var repo = new EfUserRepository(ctx);

        await repo.AddAsync(new User { Id = "u1" });

        var stored = await ctx.Users.FindAsync("u1");
        stored.Should().NotBeNull();
        stored!.Id.Should().Be("u1");
    }

    [Fact]
    public async Task AddAsync_DuplicateId_DoesNotDuplicate()
    {
        using var ctx = CreateContext();
        var repo = new EfUserRepository(ctx);

        await repo.AddAsync(new User { Id = "u1" });
        await repo.AddAsync(new User { Id = "u1" }); // second call is a no-op

        ctx.Users.Count().Should().Be(1);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingUser_ReturnsDomainObject()
    {
        using var ctx = CreateContext();
        var repo = new EfUserRepository(ctx);
        await repo.AddAsync(new User { Id = "u1" });

        var user = await repo.GetByIdAsync("u1");

        user.Should().NotBeNull();
        user!.Id.Should().Be("u1");
    }

    [Fact]
    public async Task GetByIdAsync_MissingUser_ReturnsNull()
    {
        using var ctx = CreateContext();
        var repo = new EfUserRepository(ctx);

        var user = await repo.GetByIdAsync("missing");

        user.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllUsers()
    {
        using var ctx = CreateContext();
        var repo = new EfUserRepository(ctx);
        await repo.AddAsync(new User { Id = "u1" });
        await repo.AddAsync(new User { Id = "u2" });

        var all = (await repo.GetAllAsync()).ToList();

        all.Should().HaveCount(2);
        all.Select(u => u.Id).Should().Contain(["u1", "u2"]);
    }

    [Fact]
    public async Task RemoveAsync_ExistingUser_RemovesFromDb()
    {
        using var ctx = CreateContext();
        var repo = new EfUserRepository(ctx);
        await repo.AddAsync(new User { Id = "u1" });

        await repo.RemoveAsync(new User { Id = "u1" });

        (await repo.GetByIdAsync("u1")).Should().BeNull();
    }
}
