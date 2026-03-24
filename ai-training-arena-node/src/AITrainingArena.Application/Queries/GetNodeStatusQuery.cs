using AITrainingArena.Application.DTOs;
using AITrainingArena.Domain.Enums;
using AITrainingArena.Domain.Interfaces;
using MediatR;

namespace AITrainingArena.Application.Queries;

public record GetNodeStatusQuery : IRequest<NodeStatusDto>;

public class GetNodeStatusQueryHandler : IRequestHandler<GetNodeStatusQuery, NodeStatusDto>
{
    private readonly INetworkManager _networkManager;

    public GetNodeStatusQueryHandler(INetworkManager networkManager)
        => _networkManager = networkManager;

    public async Task<NodeStatusDto> Handle(
        GetNodeStatusQuery request, CancellationToken cancellationToken)
    {
        var peers = await _networkManager.GetActivePeersAsync(cancellationToken);
        return new NodeStatusDto(
            PeerId: string.Empty,
            Status: NodeStatus.Available,
            ConnectedPeers: peers.Count,
            CompletedBattles: 0,
            EloRating: 1500,
            IsAutomatic: true);
    }
}
