namespace Musicratic.Modules.Identity.Application;

using Musicratic.Modules.Identity.Domain;

public interface IJoinSessionService
{
	VisitorSession StartJoin(Guid visitorId, bool isAuthenticated, DateTimeOffset now);

	VisitorSession BeginJoin(VisitorSession session);

	VisitorSession AcceptAuthentication(VisitorSession session);

	VisitorSession CompleteJoin(VisitorSession session);

	VisitorSession Expire(VisitorSession session);
}
