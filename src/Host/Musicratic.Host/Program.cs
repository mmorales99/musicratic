using Musicratic.Modules.Identity.Api;
using Musicratic.Modules.MusicSessions.Api;
using Musicratic.Host.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
	.AddInteractiveServerComponents();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.UseStaticFiles();
app.UseAntiforgery();

app.MapIdentityModule();
app.MapMusicSessionsModule();

app.MapRazorComponents<App>()
	.AddInteractiveServerRenderMode();

await app.RunAsync();
