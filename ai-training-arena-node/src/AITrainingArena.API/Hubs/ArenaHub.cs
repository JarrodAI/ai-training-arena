using Microsoft.AspNetCore.SignalR;

namespace AITrainingArena.API.Hubs;

public class ArenaHub : Hub
{
    private const string BattleGroup = "BattleSubscribers";

    public async Task SubscribeToBattles()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, BattleGroup);
    }

    public async Task UnsubscribeFromBattles()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, BattleGroup);
    }

    public static Task BattleStarted(IHubContext<ArenaHub> context, object battleInfo)
    {
        return context.Clients.Group(BattleGroup).SendAsync("BattleStarted", battleInfo);
    }

    public static Task BattleCompleted(IHubContext<ArenaHub> context, object battleResult)
    {
        return context.Clients.Group(BattleGroup).SendAsync("BattleCompleted", battleResult);
    }

    public static Task EloUpdated(IHubContext<ArenaHub> context, object eloUpdate)
    {
        return context.Clients.Group(BattleGroup).SendAsync("EloUpdated", eloUpdate);
    }

    public static Task NodeStatusChanged(IHubContext<ArenaHub> context, object statusInfo)
    {
        return context.Clients.All.SendAsync("NodeStatusChanged", statusInfo);
    }
}
