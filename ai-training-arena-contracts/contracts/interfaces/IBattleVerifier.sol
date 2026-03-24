// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

interface IBattleVerifier {
    function submitProof(uint256 proposerNFT, uint256 solverNFT, bytes32 merkleRoot, bytes calldata zkProof) external;
    function verifyBattle(uint256 battleId) external view returns (bool verified, address winner);
    function challengeBattle(uint256 battleId) external;
    function resolveDispute(uint256 battleId, address winner) external;
}
