using OpenTelemetry.Trace;
using Scalar.AspNetCore;
using Serilog;
using Musicratic.Shared.Application;
using Musicratic.Shared.Infrastructure;
using Musicratic.Auth.Domain;
using Musicratic.Auth.Application;
using Musicratic.Auth.Infrastructure;
using Musicratic.Auth.Api;
using Musicratic.Auth.Api.Endpoints;
using Musicratic.Hub.Domain;
using Musicratic.Hub.Application;
using Musicratic.Hub.Infrastructure;
using Musicratic.Hub.Api;
using Musicratic.Playback.Domain;
using Musicratic.Playback.Application;
using Musicratic.Playback.Infrastructure;
using Musicratic.Playback.Api;
using Musicratic.Voting.Domain;
using Musicratic.Voting.Application;
using Musicratic.Voting.Infrastructure;
using Musicratic.Voting.Api;
using Musicratic.Economy.Domain;
using Musicratic.Economy.Application;
using Musicratic.Economy.Infrastructure;
using Musicratic.Economy.Api;
using Musicratic.Economy.Api.Endpoints;
using Musicratic.Analytics.Domain;
using Musicratic.Analytics.Application;
using Musicratic.Analytics.Infrastructure;
using Musicratic.Analytics.Api;
using Musicratic.Analytics.Api.Endpoints;
using Musicratic.Social.Domain;
using Musicratic.Social.Application;
using Musicratic.Social.Infrastructure;
using Musicratic.Social.Api;
using Musicratic.Notification.Domain;
using Musicratic.Notification.Application;
using Musicratic.Notification.Infrastructure;
using Musicratic.Notification.Api;
using Musicratic.Notification.Api.Endpoints;
using Musicratic.Voting.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((ctx, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

// OpenTelemetry
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddSource("Musicratic")
        .AddConsoleExporter());

// Dapr
builder.Services.AddDaprClient();

// Health checks
builder.Services.AddHealthChecks();

// OpenAPI + Scalar
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Authorization
builder.Services.AddAuthorization();

// Shared
builder.Services.AddSharedApplication();
builder.Services.AddSharedInfrastructure();

// Auth module
builder.Services
    .AddAuthDomain()
    .AddAuthApplication()
    .AddAuthInfrastructure(builder.Configuration)
    .AddAuthApi();

// Hub module
builder.Services
    .AddHubDomain()
    .AddHubApplication()
    .AddHubInfrastructure(builder.Configuration)
    .AddHubApi();

// Playback module
builder.Services
    .AddPlaybackDomain()
    .AddPlaybackApplication()
    .AddPlaybackInfrastructure(builder.Configuration)
    .AddPlaybackApi();

// Voting module
builder.Services
    .AddVotingDomain()
    .AddVotingApplication()
    .AddVotingInfrastructure(builder.Configuration)
    .AddVotingApi();

// Economy module
builder.Services
    .AddEconomyDomain()
    .AddEconomyApplication()
    .AddEconomyInfrastructure(builder.Configuration)
    .AddEconomyApi();

// Analytics module
builder.Services
    .AddAnalyticsDomain()
    .AddAnalyticsApplication()
    .AddAnalyticsInfrastructure(builder.Configuration)
    .AddAnalyticsApi();

// Social module
builder.Services
    .AddSocialDomain()
    .AddSocialApplication()
    .AddSocialInfrastructure()
    .AddSocialApi();

// Notification module
builder.Services
    .AddNotificationDomain()
    .AddNotificationApplication()
    .AddNotificationInfrastructure(builder.Configuration)
    .AddNotificationApi();

var app = builder.Build();

app.UseSerilogRequestLogging();
app.MapHealthChecks("/health");
app.UseSwagger();
app.MapScalarApiReference();

app.UseWebSockets();
app.UseAuthentication();
app.UseAuthorization();

// Module endpoints
app.MapAuthEndpoints();
app.MapVotingEndpoints();
app.MapVoteWebSocket();
app.MapNotificationWebSocket();
app.MapNotificationEndpoints();
app.MapDeviceEndpoints();
app.MapEconomyEndpoints();
app.MapEconomyDaprEndpoints();
app.MapStripeWebhookEndpoints();
app.MapAnalyticsEndpoints();

app.Run();

public partial class Program;
