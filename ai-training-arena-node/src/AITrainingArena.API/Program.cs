using AITrainingArena.API;
using AITrainingArena.API.Hubs;
using AITrainingArena.API.Middleware;
using AITrainingArena.Application;
using AITrainingArena.BattleEngine;
using AITrainingArena.Blockchain;
using AITrainingArena.Infrastructure;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/arena-.log", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, svc, cfg) => cfg
        .ReadFrom.Configuration(ctx.Configuration)
        .ReadFrom.Services(svc)
        .WriteTo.Console()
        .WriteTo.File("logs/arena-.log", rollingInterval: RollingInterval.Day));

    builder.Services.Configure<NodeConfiguration>(
        builder.Configuration.GetSection(NodeConfiguration.SectionName));

    builder.Services.AddApplicationServices();
    builder.Services.AddInfrastructureServices(builder.Configuration);
    builder.Services.AddBattleEngineServices();
    builder.Services.AddBlockchainServices(builder.Configuration);

    builder.Services.AddSignalR();
    builder.Services.AddGrpc();
    builder.Services.AddControllers();
    builder.Services.AddHealthChecks();
    builder.Services.AddSingleton<WebSocketServer>();
    builder.Services.AddHostedService(sp => sp.GetRequiredService<WebSocketServer>());

    builder.Services.AddCors(opts =>
    {
        opts.AddDefaultPolicy(policy =>
            policy.WithOrigins("http://localhost:3000", "http://localhost:8080")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials());
    });

    var app = builder.Build();

    app.UseMiddleware<ErrorHandlingMiddleware>();
    app.UseCors();
    app.UseWebSockets();

    app.MapHealthChecks("/health/live");
    app.MapHealthChecks("/health/ready");
    app.MapControllers();
    app.MapHub<ArenaHub>("/hubs/arena");

    app.Map("/ws", async context =>
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            var ws = await context.WebSockets.AcceptWebSocketAsync();
            var server = context.RequestServices.GetRequiredService<WebSocketServer>();
            await server.HandleConnectionAsync(ws, context.RequestAborted);
        }
        else
        {
            context.Response.StatusCode = 400;
        }
    });

    Log.Information("AI Training Arena node starting...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "AI Training Arena node terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
