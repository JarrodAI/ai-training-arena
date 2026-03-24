// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

import "@openzeppelin/contracts/access/AccessControl.sol";
import "@openzeppelin/contracts/utils/ReentrancyGuard.sol";
import "./interfaces/IATAToken.sol";
import "./AgentNFT.sol";

interface IAITrainingArena {
    function recordBattle(
        uint256 proposerNFT,
        uint256 solverNFT,
        address winner,
        uint256 proposerScore,
        uint256 solverScore,
        string calldata ipfsHash
    ) external;
}

contract BattleVerifier is AccessControl, ReentrancyGuard {
    bytes32 public constant ORACLE_ROLE = keccak256("ORACLE_ROLE");

    uint256 public constant CHALLENGE_WINDOW = 1 hours;

    AgentNFT public immutable agentNFT;
    address public immutable arenaContract;

    struct BattleRecord {
        uint256 proposerNFT;
        uint256 solverNFT;
        bytes32 proposerMerkleRoot;
        bytes32 solverMerkleRoot;
        uint256 submittedAt;
        bool verified;
        bool disputed;
        address winner;
        bool settled;
    }

    mapping(uint256 => BattleRecord) public battles;
    uint256 public nextBattleId;

    // Track which side has submitted: battleId => side => bool
    mapping(uint256 => mapping(address => bool)) private _hasSubmitted;
    // Track battle lookup by NFT pair hash
    mapping(bytes32 => uint256) private _activeBattleByPair;

    event ProofSubmitted(
        uint256 indexed battleId,
        uint256 indexed nftId,
        bytes32 merkleRoot
    );
    event BattleVerified(
        uint256 indexed battleId,
        uint256 proposerNFT,
        uint256 solverNFT
    );
    event DisputeOpened(uint256 indexed battleId);
    event OracleRequested(uint256 indexed battleId);
    event DisputeResolved(
        uint256 indexed battleId,
        address indexed winner
    );

    error NotNFTParticipant();
    error BattleAlreadySettled();
    error AlreadySubmitted();
    error BattleNotDisputed();
    error ChallengeWindowExpired();
    error ChallengeWindowActive();
    error BattleNotFound();
    error InvalidWinner();

    constructor(address _agentNFT, address _arenaContract) {
        require(_agentNFT != address(0), "zero NFT address");
        require(_arenaContract != address(0), "zero arena address");

        agentNFT = AgentNFT(_agentNFT);
        arenaContract = _arenaContract;
        nextBattleId = 1;

        _grantRole(DEFAULT_ADMIN_ROLE, msg.sender);
    }

    function submitProof(
        uint256 proposerNFT,
        uint256 solverNFT,
        bytes32 merkleRoot,
        bytes calldata /* zkProof */
    ) external nonReentrant {
        address caller = msg.sender;
        address proposerOwner = agentNFT.ownerOf(proposerNFT);
        address solverOwner = agentNFT.ownerOf(solverNFT);

        bool isProposer = (caller == proposerOwner);
        bool isSolver = (caller == solverOwner);
        if (!isProposer && !isSolver) revert NotNFTParticipant();

        bytes32 pairHash = _pairHash(proposerNFT, solverNFT);
        uint256 battleId = _activeBattleByPair[pairHash];

        if (battleId == 0 || battles[battleId].settled) {
            battleId = nextBattleId++;
            battles[battleId].proposerNFT = proposerNFT;
            battles[battleId].solverNFT = solverNFT;
            battles[battleId].submittedAt = block.timestamp;
            _activeBattleByPair[pairHash] = battleId;
        }

        BattleRecord storage battle = battles[battleId];
        if (battle.settled) revert BattleAlreadySettled();
        if (_hasSubmitted[battleId][caller]) revert AlreadySubmitted();

        _hasSubmitted[battleId][caller] = true;

        if (isProposer) {
            battle.proposerMerkleRoot = merkleRoot;
        } else {
            battle.solverMerkleRoot = merkleRoot;
        }

        emit ProofSubmitted(battleId, isProposer ? proposerNFT : solverNFT, merkleRoot);

        _tryVerify(battleId);
    }

    function challengeBattle(uint256 battleId) external {
        BattleRecord storage battle = battles[battleId];
        if (battle.submittedAt == 0) revert BattleNotFound();
        if (battle.settled) revert BattleAlreadySettled();
        if (block.timestamp > battle.submittedAt + CHALLENGE_WINDOW) {
            revert ChallengeWindowExpired();
        }

        address caller = msg.sender;
        address proposerOwner = agentNFT.ownerOf(battle.proposerNFT);
        address solverOwner = agentNFT.ownerOf(battle.solverNFT);
        if (caller != proposerOwner && caller != solverOwner) {
            revert NotNFTParticipant();
        }

        battle.disputed = true;
        emit DisputeOpened(battleId);
        emit OracleRequested(battleId);
    }

    function resolveDispute(
        uint256 battleId,
        address winner
    ) external onlyRole(ORACLE_ROLE) nonReentrant {
        BattleRecord storage battle = battles[battleId];
        if (battle.submittedAt == 0) revert BattleNotFound();
        if (!battle.disputed) revert BattleNotDisputed();
        if (battle.settled) revert BattleAlreadySettled();

        address proposerOwner = agentNFT.ownerOf(battle.proposerNFT);
        address solverOwner = agentNFT.ownerOf(battle.solverNFT);
        if (winner != proposerOwner && winner != solverOwner) {
            revert InvalidWinner();
        }

        battle.winner = winner;
        battle.settled = true;
        battle.verified = true;

        bytes32 pairHash = _pairHash(battle.proposerNFT, battle.solverNFT);
        _activeBattleByPair[pairHash] = 0;

        emit DisputeResolved(battleId, winner);
    }

    function verifyMerkleProof(
        bytes32 root,
        bytes32 leaf,
        bytes32[] calldata proof
    ) internal pure returns (bool) {
        bytes32 computedHash = leaf;
        for (uint256 i = 0; i < proof.length; i++) {
            bytes32 proofElement = proof[i];
            if (computedHash <= proofElement) {
                computedHash = keccak256(
                    abi.encodePacked(computedHash, proofElement)
                );
            } else {
                computedHash = keccak256(
                    abi.encodePacked(proofElement, computedHash)
                );
            }
        }
        return computedHash == root;
    }

    // --- View Functions ---

    function getBattleRecord(
        uint256 battleId
    ) external view returns (BattleRecord memory) {
        return battles[battleId];
    }

    function isPendingDispute(uint256 battleId) external view returns (bool) {
        BattleRecord storage battle = battles[battleId];
        return battle.disputed && !battle.settled;
    }

    // --- Internal ---

    function _tryVerify(uint256 battleId) internal {
        BattleRecord storage battle = battles[battleId];

        bool proposerDone = battle.proposerMerkleRoot != bytes32(0);
        bool solverDone = battle.solverMerkleRoot != bytes32(0);
        if (!proposerDone || !solverDone) return;

        if (battle.proposerMerkleRoot == battle.solverMerkleRoot) {
            battle.verified = true;
            battle.settled = true;
            battle.winner = agentNFT.ownerOf(battle.proposerNFT);

            bytes32 pairHash = _pairHash(
                battle.proposerNFT,
                battle.solverNFT
            );
            _activeBattleByPair[pairHash] = 0;

            emit BattleVerified(
                battleId,
                battle.proposerNFT,
                battle.solverNFT
            );
        } else {
            battle.disputed = true;
            emit DisputeOpened(battleId);
        }
    }

    function _pairHash(
        uint256 a,
        uint256 b
    ) internal pure returns (bytes32) {
        return keccak256(abi.encodePacked(a, b));
    }
}
