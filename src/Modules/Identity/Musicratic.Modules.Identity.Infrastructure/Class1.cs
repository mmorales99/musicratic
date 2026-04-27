namespace Musicratic.Modules.Identity.Infrastructure;

using Musicratic.Modules.Identity.Domain;

public static class InMemoryJoinSessionService
{
	public static VisitorSession StartJoin(Guid visitorId, bool isAuthenticated, DateTimeOffset now)
	{
		return VisitorSession.Start(visitorId, isAuthenticated, now);
	}

	public static VisitorSession BeginJoin(VisitorSession session)
	{
		return session.BeginJoin();
	}

	public static VisitorSession AcceptAuthentication(VisitorSession session)
	{
		return session.AcceptAuthentication();
	}

	public static VisitorSession CompleteJoin(VisitorSession session)
	{
		return session.CompleteJoin();
	}

	public static VisitorSession Expire(VisitorSession session)
	{
		return session.Expire();
	}
}
