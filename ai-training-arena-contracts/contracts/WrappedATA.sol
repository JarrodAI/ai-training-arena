// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

import "@openzeppelin/contracts/token/ERC20/ERC20.sol";
import "@openzeppelin/contracts/access/AccessControl.sol";
import "@openzeppelin/contracts/utils/ReentrancyGuard.sol";
import "@openzeppelin/contracts/utils/Pausable.sol";
import "./interfaces/IATAToken.sol";
import "./AgentNFT.sol";

contract WrappedATA is ERC20, AccessControl, ReentrancyGuard, Pausable {
    bytes32 public constant PAUSER_ROLE = keccak256("PAUSER_ROLE");

    IATAToken public immutable ataToken;
    AgentNFT public immutable agentNFT;

    uint256 public constant UNSTAKE_COOLDOWN = 7 days;

    mapping(AgentNFT.AgentClass => uint256) public classMinimumStake;
    mapping(uint256 => uint256) public stakedPerNFT;
    mapping(uint256 => address) public nftStaker;
    mapping(uint256 => uint256) public unstakeRequestedAt;
    mapping(uint256 => uint256) public unstakeRequestedAmount;
    uint256 public totalStaked;

    struct StakeInfo {
        address staker;
        uint256 amount;
        bool pendingUnstake;
        uint256 unstakeTime;
    }

    event Staked(uint256 indexed nftId, address indexed staker, uint256 amount);
    event UnstakeRequested(
        uint256 indexed nftId,
        address indexed staker,
        uint256 amount
    );
    event Unstaked(
        uint256 indexed nftId,
        address indexed staker,
        uint256 amount
    );
    event UnstakeCancelled(uint256 indexed nftId, address indexed staker);

    error NotNFTOwner();
    error ZeroAmount();
    error BelowClassMinimum();
    error AlreadyStakedByOther();
    error TransferDisabled();
    error CooldownNotElapsed();
    error NoUnstakePending();
    error UnstakeAlreadyPending();
    error InsufficientStake();

    constructor(
        address _ataToken,
        address _agentNFT
    ) ERC20("Wrapped ATA", "wATA") {
        require(_ataToken != address(0), "zero ATA address");
        require(_agentNFT != address(0), "zero NFT address");

        ataToken = IATAToken(_ataToken);
        agentNFT = AgentNFT(_agentNFT);

        _grantRole(DEFAULT_ADMIN_ROLE, msg.sender);
        _grantRole(PAUSER_ROLE, msg.sender);

        _initClassMinimumStakes();
    }

    function stake(
        uint256 nftId,
        uint256 amount
    ) external nonReentrant whenNotPaused {
        if (amount == 0) revert ZeroAmount();
        if (agentNFT.ownerOf(nftId) != msg.sender) revert NotNFTOwner();

        address existing = nftStaker[nftId];
        if (existing != address(0) && existing != msg.sender) {
            revert AlreadyStakedByOther();
        }

        AgentNFT.AgentClass agentClass = agentNFT.getAgentClass(nftId);
        uint256 minStake = classMinimumStake[agentClass];
        uint256 currentStake = stakedPerNFT[nftId];
        if (currentStake == 0 && amount < minStake) revert BelowClassMinimum();

        ataToken.transferFrom(msg.sender, address(this), amount);
        _mint(msg.sender, amount);

        stakedPerNFT[nftId] += amount;
        totalStaked += amount;
        nftStaker[nftId] = msg.sender;

        emit Staked(nftId, msg.sender, amount);
    }

    function requestUnstake(
        uint256 nftId,
        uint256 amount
    ) external nonReentrant {
        if (amount == 0) revert ZeroAmount();
        if (nftStaker[nftId] != msg.sender) revert NotNFTOwner();
        if (stakedPerNFT[nftId] < amount) revert InsufficientStake();
        if (unstakeRequestedAt[nftId] != 0) revert UnstakeAlreadyPending();

        unstakeRequestedAt[nftId] = block.timestamp;
        unstakeRequestedAmount[nftId] = amount;

        emit UnstakeRequested(nftId, msg.sender, amount);
    }

    function completeUnstake(uint256 nftId) external nonReentrant {
        if (nftStaker[nftId] != msg.sender) revert NotNFTOwner();
        if (unstakeRequestedAt[nftId] == 0) revert NoUnstakePending();
        if (block.timestamp < unstakeRequestedAt[nftId] + UNSTAKE_COOLDOWN) {
            revert CooldownNotElapsed();
        }

        uint256 amount = unstakeRequestedAmount[nftId];

        _burn(msg.sender, amount);
        stakedPerNFT[nftId] -= amount;
        totalStaked -= amount;

        unstakeRequestedAt[nftId] = 0;
        unstakeRequestedAmount[nftId] = 0;

        if (stakedPerNFT[nftId] == 0) {
            nftStaker[nftId] = address(0);
        }

        ataToken.transfer(msg.sender, amount);

        emit Unstaked(nftId, msg.sender, amount);
    }

    function cancelUnstake(uint256 nftId) external {
        if (nftStaker[nftId] != msg.sender) revert NotNFTOwner();
        if (unstakeRequestedAt[nftId] == 0) revert NoUnstakePending();

        unstakeRequestedAt[nftId] = 0;
        unstakeRequestedAmount[nftId] = 0;

        emit UnstakeCancelled(nftId, msg.sender);
    }

    function pause() external onlyRole(PAUSER_ROLE) {
        _pause();
    }

    function unpause() external onlyRole(PAUSER_ROLE) {
        _unpause();
    }

    // --- View Functions ---

    function getStakeInfo(
        uint256 nftId
    ) external view returns (StakeInfo memory) {
        return
            StakeInfo({
                staker: nftStaker[nftId],
                amount: stakedPerNFT[nftId],
                pendingUnstake: unstakeRequestedAt[nftId] != 0,
                unstakeTime: unstakeRequestedAt[nftId] != 0
                    ? unstakeRequestedAt[nftId] + UNSTAKE_COOLDOWN
                    : 0
            });
    }

    function meetsClassMinimum(uint256 nftId) external view returns (bool) {
        AgentNFT.AgentClass agentClass = agentNFT.getAgentClass(nftId);
        return stakedPerNFT[nftId] >= classMinimumStake[agentClass];
    }

    // --- Soulbound: disable transfers ---

    function _update(
        address from,
        address to,
        uint256 value
    ) internal override {
        if (from != address(0) && to != address(0)) {
            revert TransferDisabled();
        }
        super._update(from, to, value);
    }

    // --- Internal ---

    function _initClassMinimumStakes() internal {
        classMinimumStake[AgentNFT.AgentClass.A] = 100 ether;
        classMinimumStake[AgentNFT.AgentClass.B] = 500 ether;
        classMinimumStake[AgentNFT.AgentClass.C] = 2000 ether;
        classMinimumStake[AgentNFT.AgentClass.D] = 8000 ether;
        classMinimumStake[AgentNFT.AgentClass.E] = 30000 ether;
    }

    function supportsInterface(
        bytes4 interfaceId
    ) public view override(AccessControl) returns (bool) {
        return super.supportsInterface(interfaceId);
    }
}
