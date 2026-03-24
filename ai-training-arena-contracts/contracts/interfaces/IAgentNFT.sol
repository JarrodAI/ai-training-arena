// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

interface IAgentNFT {
    enum AgentClass { A, B, C, D, E }

    function ownerOf(uint256 tokenId) external view returns (address);
    function balanceOf(address owner) external view returns (uint256);
    function getAgentClass(uint256 tokenId) external view returns (AgentClass);
    function getAgentElo(uint256 tokenId) external view returns (uint256);
    function updateElo(uint256 tokenId, uint256 newElo) external;
    function mintAgent(address to, AgentClass class_, string calldata modelName) external returns (uint256);
    function isActive(uint256 tokenId) external view returns (bool);
}
