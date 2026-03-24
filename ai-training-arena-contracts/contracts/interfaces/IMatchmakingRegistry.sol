// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

import "./IAgentNFT.sol";

interface IMatchmakingRegistry {
    enum NodeStatus { Offline, Available, InBattle }

    function announceAvailability(uint256 nftId, uint256 eloRating, string calldata peerId) external;
    function updateStatus(uint256 nftId, NodeStatus status) external;
    function getAvailableOpponents(IAgentNFT.AgentClass agentClass, uint256 eloRating, uint256 eloRange) external view returns (uint256[] memory nftIds);
    function cleanStaleNodes() external;
}
