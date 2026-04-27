namespace Musicratic.Modules.Identity.Api;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Musicratic.Shared.Contracts;

public static class IdentityModule
{
	public static IEndpointRouteBuilder MapIdentityModule(this IEndpointRouteBuilder endpoints)
	{
		var group = endpoints.MapGroup("/api/identity");
		var authGroup = group.MapGroup("/auth");

		group.MapGet("/join-state", () => Results.Ok(new JoinStateInfoDto(
			JoinState.UnverifiedGuest,
			"Unverified Guest",
			"Visitor is not authenticated and cannot suggest songs.",
			false)));

		group.MapGet("/join-states", () => Results.Ok(new[]
		{
			new JoinStateInfoDto(JoinState.UnverifiedGuest, "Unverified Guest", "Visitor is not authenticated and cannot suggest songs.", false),
			new JoinStateInfoDto(JoinState.Guest, "Guest", "Visitor is authenticated and can suggest songs.", true),
			new JoinStateInfoDto(JoinState.PendingToJoin, "Pending To Join", "Server is collecting session details while the join response is pending.", false),
			new JoinStateInfoDto(JoinState.Joined, "Joined", "Visitor finished the join response and is fully joined.", true),
			new JoinStateInfoDto(JoinState.Expired, "Expired", "Visitor lost the join because of inactivity.", false)
		}));

		authGroup.MapGet("/status", (HttpContext context) =>
		{
			var isAuthenticated = context.User?.Identity?.IsAuthenticated == true;
			var joinState = isAuthenticated ? "guest" : "unverified-guest";

			return Results.Ok(new AppAuthStatusDto(isAuthenticated, isAuthenticated, joinState));
		});

		authGroup.MapGet("/suggestion-permission", (HttpContext context) =>
		{
			var canSuggestSongs = context.User?.Identity?.IsAuthenticated == true;
			var joinState = canSuggestSongs ? JoinState.Guest : JoinState.UnverifiedGuest;

			return Results.Ok(new SuggestionPermissionDto(canSuggestSongs, joinState, canSuggestSongs));
		});

		return endpoints;
	}
}
