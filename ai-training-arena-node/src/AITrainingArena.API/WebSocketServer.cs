using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AITrainingArena.API;

/// <summary>
/// ASP.NET Core WebSocket endpoint at ws://localhost:8080/ws.
/// Broadcasts battle events to connected Dioxus frontend clients.
/// Message format: JSON with type + payload fields.
/// </summary>
public sealed class WebSocketServer : IHostedService
{
    private readonly ConcurrentDictionary<string, WebSocket> _clients = new();
    private readonly ILogger<WebSocketServer> _logger;

    public WebSocketServer(ILogger<WebSocketServer> logger) => _logger = logger;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("WebSocket server started");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("WebSocket server stopping. Clients: {Count}", _clients.Count);
        return Task.CompletedTask;
    }

    /// <summary>Handles a new WebSocket connection from the frontend.</summary>
    public async Task HandleConnectionAsync(WebSocket ws, CancellationToken ct)
    {
        var id = Guid.NewGuid().ToString("N");
        _clients.TryAdd(id, ws);
        _logger.LogInformation("WebSocket client connected: {Id}", id);
        try
        {
            await ReceiveMessagesAsync(ws, ct);
        }
        finally
        {
            _clients.TryRemove(id, out _);
            _logger.LogInformation("WebSocket client disconnected: {Id}", id);
        }
    }

    private async Task ReceiveMessagesAsync(WebSocket ws, CancellationToken ct)
    {
        var buf = new byte[4096];
        while (ws.State == WebSocketState.Open && !ct.IsCancellationRequested)
        {
            var result = await ws.ReceiveAsync(buf, ct);
            if (result.MessageType == WebSocketMessageType.Close) break;
            var msg = Encoding.UTF8.GetString(buf, 0, result.Count);
            _logger.LogDebug("WS received: {Msg}", msg);
        }
    }

    /// <summary>Broadcasts BattleStarted event to all connected clients.</summary>
    public Task BroadcastBattleStartedAsync(object battleInfo, CancellationToken ct = default)
        => BroadcastAsync("BattleStarted", battleInfo, ct);

    /// <summary>Broadcasts BattleUpdate event to all connected clients.</summary>
    public Task BroadcastBattleUpdateAsync(object update, CancellationToken ct = default)
        => BroadcastAsync("BattleUpdate", update, ct);

    /// <summary>Broadcasts BattleCompleted event to all connected clients.</summary>
    public Task BroadcastBattleCompletedAsync(object result, CancellationToken ct = default)
        => BroadcastAsync("BattleCompleted", result, ct);

    /// <summary>Broadcasts EloChanged event to all connected clients.</summary>
    public Task BroadcastEloChangedAsync(object eloChange, CancellationToken ct = default)
        => BroadcastAsync("EloChanged", eloChange, ct);

    /// <summary>Broadcasts RewardEarned event to all connected clients.</summary>
    public Task BroadcastRewardEarnedAsync(object reward, CancellationToken ct = default)
        => BroadcastAsync("RewardEarned", reward, ct);

    private async Task BroadcastAsync(string type, object payload, CancellationToken ct)
    {
        if (_clients.IsEmpty) return;
        var message = JsonSerializer.SerializeToUtf8Bytes(new { type, payload });
        var tasks = _clients.Values
            .Where(ws => ws.State == WebSocketState.Open)
            .Select(ws => SendAsync(ws, message, ct));
        await Task.WhenAll(tasks);
    }

    private async Task SendAsync(WebSocket ws, byte[] data, CancellationToken ct)
    {
        try
        {
            await ws.SendAsync(data, WebSocketMessageType.Text, true, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send WebSocket message");
        }
    }
}
