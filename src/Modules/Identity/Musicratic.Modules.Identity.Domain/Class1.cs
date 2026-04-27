namespace Musicratic.Modules.Identity.Domain;

public enum JoinState
{
	UnverifiedGuest = 0,
	Guest = 1,
	PendingToJoin = 2,
	Joined = 3,
	Expired = 4
}

public sealed record VisitorJoinState(JoinState State, bool CanSuggestSongs, bool IsTerminal, string Description)
{
	public static VisitorJoinState UnverifiedGuest => new(
		JoinState.UnverifiedGuest,
		false,
		false,
		"Visitor is not authenticated and cannot suggest songs.");

	public static VisitorJoinState Guest => new(
		JoinState.Guest,
		true,
		false,
		"Visitor is authenticated and can suggest songs.");

	public static VisitorJoinState PendingToJoin => new(
		JoinState.PendingToJoin,
		false,
		false,
		"Server is collecting session details while the join response is pending.");

	public static VisitorJoinState Joined => new(
		JoinState.Joined,
		true,
		false,
		"Visitor finished the join response and is fully joined.");

	public static VisitorJoinState Expired => new(
		JoinState.Expired,
		false,
		true,
		"Visitor lost the join because of inactivity.");
}

public sealed record VisitorSession(Guid VisitorId, VisitorJoinState JoinState, DateTimeOffset UpdatedAt)
{
	public bool CanSuggestSongs => JoinState.CanSuggestSongs;

	public static VisitorSession Start(Guid visitorId, bool isAuthenticated, DateTimeOffset now)
	{
		return new VisitorSession(visitorId, isAuthenticated ? VisitorJoinState.Guest : VisitorJoinState.UnverifiedGuest, now);
	}

	public VisitorSession BeginJoin()
	{
		return this with { JoinState = VisitorJoinState.PendingToJoin };
	}

	public VisitorSession AcceptAuthentication()
	{
		return this with { JoinState = VisitorJoinState.Guest };
	}

	public VisitorSession CompleteJoin()
	{
		return this with { JoinState = VisitorJoinState.Joined };
	}

	public VisitorSession Expire()
	{
		return this with { JoinState = VisitorJoinState.Expired };
	}
}
