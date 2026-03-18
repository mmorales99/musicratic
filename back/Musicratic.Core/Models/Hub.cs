using System;

namespace Musicratic.Core.Models;

public class User
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;

    private User() { }

    public User(Guid id, string name)
    {
        Id = id;
        Name = name;
    }
}

public class Hub
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    // Code is unique identifier for the hub to be located easily by users. A locator code is made of 8 characters (A-Z and 0-9 only) to be easily shareable.
    public string Code { get; private set; } = null!;
    public User[] AttachedUsers { get; } = []; // Placeholder for attached users, to be implemented in the future when attachments are loaded
    public User Owner { get; private set; } = null!; // Placeholder for hub owner, to be implemented in the future when attachments are loaded

    public bool IsActive => true; 

    public void AttachUser(User user)
    {
        AttachedUsers = [.. AttachedUsers, user];
    }

    public void DetachUser(User user)
    {
        AttachedUsers = [.. AttachedUsers.Where(u => u != user)];
    }

    private Hub() { }

    public Hub(Guid id, string name, string code)
    {
        Id = id;
        Name = name;
        Code = code;
    }
}
