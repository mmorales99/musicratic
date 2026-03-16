using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));

builder.Services.AddHealthChecks();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .SetIsOriginAllowed(_ => builder.Environment.IsDevelopment());
    });
});

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseCors();
app.MapHealthChecks("/health");

app.Run();

public partial class Program;
