// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

import "@openzeppelin/contracts/access/AccessControl.sol";
import "./AgentNFT.sol";

contract MatchmakingRegistry is AccessControl {
    AgentNFT public immutable agentNFT;

    uint256 public constant MAX_RESULTS = 50;
    uint256 public constant STALE_THRESHOLD = 10 minutes;
    uint256 public constant FRESHNESS_THRESHOLD = 5 minutes;

    enum NodeStatus {
        Offline,
        Available,
        InBattle
    }

    struct NodeAdvertisement {
        uint256 nftId;
        address owner;
        AgentNFT.AgentClass agentClass;
        uint256 eloRating;
        NodeStatus status;
        uint256 lastSeen;
        string peerId;
    }

    mapping(uint256 => NodeAdvertisement) private _nodes;
    mapping(AgentNFT.AgentClass => uint256[]) private _classNodes;
    mapping(uint256 => uint256) private _classNodeIndex;
    mapping(uint256 => bool) private _registered;

    event NodeAvailable(
        uint256 indexed nftId,
        address indexed owner,
        uint256 eloRating,
        string peerId
    );
    event NodeBusy(uint256 indexed nftId);
    event NodeOffline(uint256 indexed nftId);
    event StaleNodeRemoved(uint256 indexed nftId);

    error NotNFTOwner();
    error NodeNotRegistered(uint256 nftId);

    constructor(address _agentNFT) {
        require(_agentNFT != address(0), "zero NFT address");
        agentNFT = AgentNFT(_agentNFT);
        _grantRole(DEFAULT_ADMIN_ROLE, msg.sender);
    }

    function announceAvailability(
        uint256 nftId,
        uint256 eloRating,
        string calldata peerId
    ) external {
        if (agentNFT.ownerOf(nftId) != msg.sender) revert NotNFTOwner();

        AgentNFT.AgentClass agentClass = agentNFT.getAgentClass(nftId);

        if (!_registered[nftId]) {
            _registered[nftId] = true;
            _classNodeIndex[nftId] = _classNodes[agentClass].length;
            _classNodes[agentClass].push(nftId);
        }

        _nodes[nftId] = NodeAdvertisement({
            nftId: nftId,
            owner: msg.sender,
            agentClass: agentClass,
            eloRating: eloRating,
            status: NodeStatus.Available,
            lastSeen: block.timestamp,
            peerId: peerId
        });

        emit NodeAvailable(nftId, msg.sender, eloRating, peerId);
    }

    function updateStatus(uint256 nftId, NodeStatus status) external {
        if (agentNFT.ownerOf(nftId) != msg.sender) revert NotNFTOwner();
        if (!_registered[nftId]) revert NodeNotRegistered(nftId);

        _nodes[nftId].status = status;
        _nodes[nftId].lastSeen = block.timestamp;

        if (status == NodeStatus.Available) {
            emit NodeAvailable(
                nftId, msg.sender, _nodes[nftId].eloRating, _nodes[nftId].peerId
            );
        } else if (status == NodeStatus.InBattle) {
            emit NodeBusy(nftId);
        } else {
            emit NodeOffline(nftId);
        }
    }

    function getAvailableOpponents(
        AgentNFT.AgentClass agentClass,
        uint256 eloRating,
        uint256 eloRange
    ) external view returns (uint256[] memory) {
        uint256[] storage classNodeIds = _classNodes[agentClass];
        uint256 count = 0;

        uint256 minElo = eloRating > eloRange ? eloRating - eloRange : 0;
        uint256 maxElo = eloRating + eloRange;
        uint256 freshCutoff = block.timestamp > FRESHNESS_THRESHOLD
            ? block.timestamp - FRESHNESS_THRESHOLD
            : 0;

        uint256 len = classNodeIds.length < MAX_RESULTS
            ? classNodeIds.length
            : MAX_RESULTS;
        uint256[] memory temp = new uint256[](len);

        for (
            uint256 i = 0;
            i < classNodeIds.length && count < MAX_RESULTS;
            i++
        ) {
            NodeAdvertisement storage node = _nodes[classNodeIds[i]];
            if (
                node.status == NodeStatus.Available &&
                node.eloRating >= minElo &&
                node.eloRating <= maxElo &&
                node.lastSeen >= freshCutoff
            ) {
                temp[count++] = classNodeIds[i];
            }
        }

        uint256[] memory result = new uint256[](count);
        for (uint256 i = 0; i < count; i++) {
            result[i] = temp[i];
        }
        return result;
    }

    function cleanStaleNodes() external {
        uint256 staleCutoff = block.timestamp > STALE_THRESHOLD
            ? block.timestamp - STALE_THRESHOLD
            : 0;

        for (uint256 c = 0; c < 5; c++) {
            AgentNFT.AgentClass agentClass = AgentNFT.AgentClass(c);
            uint256[] storage classNodeIds = _classNodes[agentClass];

            uint256 i = 0;
            while (i < classNodeIds.length) {
                uint256 nftId = classNodeIds[i];
                if (_nodes[nftId].lastSeen < staleCutoff) {
                    _removeFromClassArray(agentClass, nftId);
                    delete _nodes[nftId];
                    _registered[nftId] = false;
                    emit StaleNodeRemoved(nftId);
                } else {
                    i++;
                }
            }
        }
    }

    function nodes(
        uint256 nftId
    ) external view returns (
        uint256 nftId_,
        address owner,
        AgentNFT.AgentClass agentClass,
        uint256 eloRating,
        NodeStatus status,
        uint256 lastSeen,
        string memory peerId
    ) {
        NodeAdvertisement storage n = _nodes[nftId];
        return (
            n.nftId, n.owner, n.agentClass,
            n.eloRating, n.status, n.lastSeen, n.peerId
        );
    }

    function isRegistered(uint256 nftId) external view returns (bool) {
        return _registered[nftId];
    }

    function getClassNodeCount(
        AgentNFT.AgentClass agentClass
    ) external view returns (uint256) {
        return _classNodes[agentClass].length;
    }

    function _removeFromClassArray(
        AgentNFT.AgentClass agentClass,
        uint256 nftId
    ) internal {
        uint256 index = _classNodeIndex[nftId];
        uint256 lastIndex = _classNodes[agentClass].length - 1;

        if (index != lastIndex) {
            uint256 lastNftId = _classNodes[agentClass][lastIndex];
            _classNodes[agentClass][index] = lastNftId;
            _classNodeIndex[lastNftId] = index;
        }

        _classNodes[agentClass].pop();
        delete _classNodeIndex[nftId];
    }
}
