// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

import "@openzeppelin/contracts/access/AccessControl.sol";
import "@openzeppelin/contracts/utils/ReentrancyGuard.sol";
import "@openzeppelin/contracts/utils/Pausable.sol";
import "./interfaces/IATAToken.sol";
import "./AgentNFT.sol";

contract AITrainingArena is AccessControl, ReentrancyGuard, Pausable {
    bytes32 public constant BATTLE_OPERATOR = keccak256("BATTLE_OPERATOR");
    bytes32 public constant DAO_EXECUTOR = keccak256("DAO_EXECUTOR");
    bytes32 public constant ORACLE_ROLE = keccak256("ORACLE_ROLE");
    bytes32 public constant PAUSER_ROLE = keccak256("PAUSER_ROLE");

    IATAToken public immutable ataToken;
    AgentNFT public immutable agentNFT;

    uint256 public dailyRewardPool = 8219 ether;
    uint256 public lastRewardReset;

    mapping(AgentNFT.AgentClass => uint256) public classMultiplier;
    mapping(AgentNFT.AgentClass => uint256) public mintPrice;

    mapping(address => uint256) public pendingRewards;
    mapping(uint256 => uint256) public stakedAmounts;
    mapping(uint256 => uint256) public unstakeInitiatedAt;
    mapping(uint256 => uint256) public unstakeRequestedAmount;

    uint256 public constant UNSTAKE_COOLDOWN = 7 days;
    uint256 public constant BURN_RATE_BPS = 200;
    uint256 public constant BPS_DENOMINATOR = 10000;
    uint256 public constant ELO_K_NEW = 40;
    uint256 public constant ELO_K_VETERAN = 20;
    uint256 public constant VETERAN_THRESHOLD = 30;
    uint256 public constant ELO_SCALE = 1000;

    address public constant BURN_ADDRESS =
        0x000000000000000000000000000000000000dEaD;

    uint256 public totalBattles;
    uint256 public totalRewardsDistributed;
    uint256 public totalBurned;

    struct ClassStats {
        uint256 totalBattles;
        uint256 totalRewards;
    }
    mapping(AgentNFT.AgentClass => ClassStats) public classStats;

    event BattleCompleted(
        uint256 indexed proposerNFT,
        uint256 indexed solverNFT,
        address winner,
        uint256 proposerScore,
        uint256 solverScore,
        string ipfsHash
    );
    event AgentPurchased(
        uint256 indexed nftId,
        address indexed buyer,
        AgentNFT.AgentClass agentClass,
        uint256 price
    );
    event AgentStaked(
        uint256 indexed nftId,
        address indexed owner,
        uint256 amount
    );
    event AgentUnstaked(
        uint256 indexed nftId,
        address indexed owner,
        uint256 amount
    );
    event RewardsClaimed(address indexed user, uint256 amount);
    event BuybackExecuted(uint256 amount);
    event ClassMultiplierUpdated(
        AgentNFT.AgentClass indexed agentClass,
        uint256 newMultiplier
    );

    error SameAgent();
    error AgentNotActive(uint256 tokenId);
    error InvalidPrice();
    error NotNFTOwner();
    error InsufficientStake();
    error UnstakeCooldownActive();
    error NoUnstakePending();
    error NoPendingRewards();

    constructor(address _ataToken, address _agentNFT) {
        require(_ataToken != address(0), "zero ATA address");
        require(_agentNFT != address(0), "zero NFT address");

        ataToken = IATAToken(_ataToken);
        agentNFT = AgentNFT(_agentNFT);

        _grantRole(DEFAULT_ADMIN_ROLE, msg.sender);
        _grantRole(BATTLE_OPERATOR, msg.sender);
        _grantRole(PAUSER_ROLE, msg.sender);

        lastRewardReset = block.timestamp;

        _initClassMultipliers();
        _initMintPrices();
    }

    function mintAgent(
        AgentNFT.AgentClass agentClass,
        string calldata modelName
    ) external payable nonReentrant whenNotPaused {
        uint256 price = mintPrice[agentClass];
        if (msg.value != price) revert InvalidPrice();

        uint256 nftId = agentNFT.mintAgent(msg.sender, agentClass, modelName);
        emit AgentPurchased(nftId, msg.sender, agentClass, price);
    }

    function recordBattle(
        uint256 proposerNFT,
        uint256 solverNFT,
        address winner,
        uint256 proposerScore,
        uint256 solverScore,
        string calldata ipfsHash
    ) external onlyRole(BATTLE_OPERATOR) nonReentrant whenNotPaused {
        if (proposerNFT == solverNFT) revert SameAgent();
        if (!agentNFT.isActive(proposerNFT))
            revert AgentNotActive(proposerNFT);
        if (!agentNFT.isActive(solverNFT)) revert AgentNotActive(solverNFT);

        address proposerOwner = agentNFT.ownerOf(proposerNFT);
        address solverOwner = agentNFT.ownerOf(solverNFT);
        require(
            winner == proposerOwner || winner == solverOwner,
            "invalid winner"
        );

        _incrementBattleCounts(proposerNFT, solverNFT, winner, proposerOwner);
        _updateEloRatings(proposerNFT, solverNFT, winner == proposerOwner);
        _distributeRewards(
            proposerNFT,
            solverNFT,
            winner,
            proposerOwner,
            solverOwner
        );

        totalBattles++;
        AgentNFT.AgentClass agentClass = agentNFT.getAgentClass(proposerNFT);
        classStats[agentClass].totalBattles++;

        emit BattleCompleted(
            proposerNFT,
            solverNFT,
            winner,
            proposerScore,
            solverScore,
            ipfsHash
        );
    }

    function stakeTokens(
        uint256 nftId,
        uint256 amount
    ) external nonReentrant whenNotPaused {
        if (agentNFT.ownerOf(nftId) != msg.sender) revert NotNFTOwner();
        require(amount > 0, "zero amount");

        stakedAmounts[nftId] += amount;
        ataToken.transferFrom(msg.sender, address(this), amount);
        emit AgentStaked(nftId, msg.sender, amount);
    }

    function unstakeTokens(
        uint256 nftId,
        uint256 amount
    ) external nonReentrant {
        if (agentNFT.ownerOf(nftId) != msg.sender) revert NotNFTOwner();
        if (stakedAmounts[nftId] < amount) revert InsufficientStake();
        require(amount > 0, "zero amount");

        if (unstakeInitiatedAt[nftId] == 0) {
            unstakeInitiatedAt[nftId] = block.timestamp;
            unstakeRequestedAmount[nftId] = amount;
            return;
        }

        if (
            block.timestamp < unstakeInitiatedAt[nftId] + UNSTAKE_COOLDOWN
        ) revert UnstakeCooldownActive();

        uint256 unstakeAmount = unstakeRequestedAmount[nftId];
        if (unstakeAmount > amount) unstakeAmount = amount;

        stakedAmounts[nftId] -= unstakeAmount;
        unstakeInitiatedAt[nftId] = 0;
        unstakeRequestedAmount[nftId] = 0;

        ataToken.transfer(msg.sender, unstakeAmount);
        emit AgentUnstaked(nftId, msg.sender, unstakeAmount);
    }

    function claimRewards(address user) external nonReentrant {
        uint256 amount = pendingRewards[user];
        if (amount == 0) revert NoPendingRewards();

        pendingRewards[user] = 0;
        ataToken.transfer(user, amount);
        emit RewardsClaimed(user, amount);
    }

    function executeBuyback(
        uint256 ataAmount
    ) external onlyRole(DAO_EXECUTOR) nonReentrant {
        require(ataAmount > 0, "zero amount");
        ataToken.transfer(BURN_ADDRESS, ataAmount);
        totalBurned += ataAmount;
        emit BuybackExecuted(ataAmount);
    }

    function setClassMultiplier(
        AgentNFT.AgentClass agentClass,
        uint256 multiplier
    ) external onlyRole(DAO_EXECUTOR) {
        require(multiplier > 0, "zero multiplier");
        classMultiplier[agentClass] = multiplier;
        emit ClassMultiplierUpdated(agentClass, multiplier);
    }

    function pause() external onlyRole(PAUSER_ROLE) {
        _pause();
    }

    function unpause() external onlyRole(PAUSER_ROLE) {
        _unpause();
    }

    // --- View Functions ---

    function getAgentInfo(
        uint256 nftId
    )
        external
        view
        returns (
            AgentNFT.AgentClass agentClass,
            uint256 elo,
            uint256 battles,
            uint256 staked,
            bool active
        )
    {
        agentClass = agentNFT.getAgentClass(nftId);
        elo = agentNFT.getAgentElo(nftId);
        battles = agentNFT.getBattleCount(nftId);
        staked = stakedAmounts[nftId];
        active = agentNFT.isActive(nftId);
    }

    function getUserAgents(
        address user
    ) external view returns (uint256[] memory) {
        return agentNFT.getUserAgents(user);
    }

    function getPendingRewards(address user) external view returns (uint256) {
        return pendingRewards[user];
    }

    function getClassStats(
        AgentNFT.AgentClass agentClass
    ) external view returns (uint256 battles_, uint256 rewards_) {
        ClassStats memory stats = classStats[agentClass];
        return (stats.totalBattles, stats.totalRewards);
    }

    // --- Internal Helpers ---

    function _initClassMultipliers() internal {
        classMultiplier[AgentNFT.AgentClass.A] = 100;
        classMultiplier[AgentNFT.AgentClass.B] = 120;
        classMultiplier[AgentNFT.AgentClass.C] = 150;
        classMultiplier[AgentNFT.AgentClass.D] = 200;
        classMultiplier[AgentNFT.AgentClass.E] = 300;
    }

    function _initMintPrices() internal {
        mintPrice[AgentNFT.AgentClass.A] = 10 ether;
        mintPrice[AgentNFT.AgentClass.B] = 50 ether;
        mintPrice[AgentNFT.AgentClass.C] = 200 ether;
        mintPrice[AgentNFT.AgentClass.D] = 800 ether;
        mintPrice[AgentNFT.AgentClass.E] = 3000 ether;
    }

    function _incrementBattleCounts(
        uint256 proposerNFT,
        uint256 solverNFT,
        address winner,
        address proposerOwner
    ) internal {
        bool proposerWon = (winner == proposerOwner);
        agentNFT.incrementBattles(proposerNFT, proposerWon);
        agentNFT.incrementBattles(solverNFT, !proposerWon);
    }

    function _updateEloRatings(
        uint256 proposerNFT,
        uint256 solverNFT,
        bool proposerWon
    ) internal {
        (uint256 newEloA, uint256 newEloB) = calculateEloChange(
            proposerNFT,
            solverNFT,
            proposerWon
        );
        agentNFT.updateElo(proposerNFT, newEloA);
        agentNFT.updateElo(solverNFT, newEloB);
    }

    function calculateEloChange(
        uint256 nftA,
        uint256 nftB,
        bool aWon
    ) internal view returns (uint256 newEloA, uint256 newEloB) {
        uint256 eloA = agentNFT.getAgentElo(nftA);
        uint256 eloB = agentNFT.getAgentElo(nftB);

        uint256 kA = agentNFT.getBattleCount(nftA) < VETERAN_THRESHOLD
            ? ELO_K_NEW
            : ELO_K_VETERAN;
        uint256 kB = agentNFT.getBattleCount(nftB) < VETERAN_THRESHOLD
            ? ELO_K_NEW
            : ELO_K_VETERAN;

        // Linear approximation of expected score
        // E = 1/(1+10^((Rb-Ra)/400)), approximated as:
        // expected = 500 +/- (eloDiff * 500 / 400) out of 1000
        int256 eloDiff = int256(eloA) - int256(eloB);
        if (eloDiff > 400) eloDiff = 400;
        if (eloDiff < -400) eloDiff = -400;

        uint256 expectedA = uint256(
            int256(ELO_SCALE / 2) +
                (eloDiff * int256(ELO_SCALE / 2)) /
                400
        );
        uint256 expectedB = ELO_SCALE - expectedA;

        if (aWon) {
            uint256 gainA = (kA * (ELO_SCALE - expectedA)) / ELO_SCALE;
            uint256 lossB = (kB * expectedB) / ELO_SCALE;
            newEloA = eloA + gainA;
            newEloB = eloB > lossB ? eloB - lossB : 0;
        } else {
            uint256 gainB = (kB * (ELO_SCALE - expectedB)) / ELO_SCALE;
            uint256 lossA = (kA * expectedA) / ELO_SCALE;
            newEloB = eloB + gainB;
            newEloA = eloA > lossA ? eloA - lossA : 0;
        }
    }

    function _distributeRewards(
        uint256 proposerNFT,
        uint256, /* solverNFT */
        address winner,
        address proposerOwner,
        address solverOwner
    ) internal {
        AgentNFT.AgentClass agentClass = agentNFT.getAgentClass(proposerNFT);
        uint256 mult = classMultiplier[agentClass];

        // Base reward = dailyRewardPool / 8 rounds / ~10 battles per round
        // Simplified: use a fixed base scaled by multiplier
        uint256 baseReward = (dailyRewardPool * mult) / (100 * 8);

        uint256 burnAmount = (baseReward * BURN_RATE_BPS) / BPS_DENOMINATOR;
        uint256 netReward = baseReward - burnAmount;

        uint256 winnerReward = (netReward * 90) / 100;
        uint256 loserReward = netReward - winnerReward;

        address loser = winner == proposerOwner ? solverOwner : proposerOwner;
        pendingRewards[winner] += winnerReward;
        pendingRewards[loser] += loserReward;

        totalRewardsDistributed += netReward;
        totalBurned += burnAmount;
        classStats[agentClass].totalRewards += netReward;

        if (burnAmount > 0) {
            ataToken.transfer(BURN_ADDRESS, burnAmount);
        }
    }

    receive() external payable {}
}
