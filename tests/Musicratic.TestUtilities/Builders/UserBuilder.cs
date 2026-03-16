using Musicratic.Auth.Domain.Entities;

namespace Musicratic.TestUtilities.Builders;

public sealed class UserBuilder
{
    private string _authentikSub = "test-sub-" + Guid.NewGuid().ToString("N")[..8];
    private string _displayName = "Test User";
    private string _email = $"test-{Guid.NewGuid():N}@example.com";
    private string? _avatarUrl;

    public UserBuilder WithSub(string sub)
    {
        _authentikSub = sub;
        return this;
    }

    public UserBuilder WithName(string name)
    {
        _displayName = name;
        return this;
    }

    public UserBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public UserBuilder WithAvatar(string? url)
    {
        _avatarUrl = url;
        return this;
    }

    public User Build() => User.Create(_authentikSub, _displayName, _email, _avatarUrl);
}
