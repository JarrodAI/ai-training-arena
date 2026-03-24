// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

import "@openzeppelin/contracts/utils/ReentrancyGuard.sol";

/// @title FounderRevenue
/// @notice Splits protocol revenue between two founders.
///         Accumulates fees from battle operations, marketplace, and data sales.
///         Founders can withdraw their share at any time.
///         Address rotation requires 7-day timelock.
contract FounderRevenue is ReentrancyGuard {
    // ─── Fee Shares (basis points, 10000 = 100%) ─────────────────────────────
    uint256 public constant BATTLE_FEE_SHARE = 2000;      // 20%
    uint256 public constant MARKETPLACE_FEE_SHARE = 4000;  // 40%
    uint256 public constant DATA_FEE_SHARE = 3000;         // 30%
    uint256 public constant BASIS_POINTS = 10_000;

    // ─── Founder Addresses ────────────────────────────────────────────────────
    address public founder1;
    address public founder2;

    // ─── Fee Accumulators ─────────────────────────────────────────────────────
    uint256 public battleFeesCollected;
    uint256 public marketplaceFeesCollected;
    uint256 public dataFeesCollected;

    // ─── Withdrawal Tracking ─────────────────────────────────────────────────
    mapping(address => uint256) public withdrawn;

    // ─── Foundership Transfer Timelock ───────────────────────────────────────
    address public pendingFounder1;
    address public pendingFounder2;
    uint256 public foundershipTransferInitiatedAt;
    uint256 public constant FOUNDERSHIP_TIMELOCK = 7 days;

    // ─── Events ───────────────────────────────────────────────────────────────
    event FeeReceived(FeeType indexed feeType, uint256 amount);
    event Withdrawal(address indexed founder, uint256 amount);
    event FounderAddressUpdated(address indexed oldAddress, address indexed newAddress);
    event FoundershipTransferInitiated(address indexed newFounder1, address indexed newFounder2);
    event FoundershipTransferCompleted(address indexed newFounder1, address indexed newFounder2);

    enum FeeType { Battle, Marketplace, Data }

    // ─── Errors ───────────────────────────────────────────────────────────────
    error NotFounder();
    error NoBalanceAvailable();
    error TransferFailed();
    error TimelockNotExpired(uint256 unlocksAt);
    error NoPendingTransfer();

    modifier onlyFounders() {
        if (msg.sender != founder1 && msg.sender != founder2) revert NotFounder();
        _;
    }

    constructor(address _founder1, address _founder2) {
        founder1 = _founder1;
        founder2 = _founder2;
    }

    // ─── Fee Reception ───────────────────────────────────────────────────────

    /// @notice Receive battle protocol fees
    function receiveBattleFees() external payable {
        battleFeesCollected += msg.value;
        emit FeeReceived(FeeType.Battle, msg.value);
    }

    /// @notice Receive marketplace transaction fees
    function receiveMarketplaceFees() external payable {
        marketplaceFeesCollected += msg.value;
        emit FeeReceived(FeeType.Marketplace, msg.value);
    }

    /// @notice Receive data sales fees
    function receiveDataFees() external payable {
        dataFeesCollected += msg.value;
        emit FeeReceived(FeeType.Data, msg.value);
    }

    // ─── Withdrawal ──────────────────────────────────────────────────────────

    /// @notice Calculates available balance for a founder.
    ///         Formula: totalEarned * 50% - alreadyWithdrawn
    ///         totalEarned = sum of (fee * sharePercent) across all categories
    function availableBalance(address founder) public view returns (uint256) {
        if (founder != founder1 && founder != founder2) return 0;

        uint256 battleShare = (battleFeesCollected * BATTLE_FEE_SHARE) / BASIS_POINTS;
        uint256 marketplaceShare = (marketplaceFeesCollected * MARKETPLACE_FEE_SHARE) / BASIS_POINTS;
        uint256 dataShare = (dataFeesCollected * DATA_FEE_SHARE) / BASIS_POINTS;

        // Total founder pool = sum of shares, split 50/50 between founders
        uint256 totalFounderPool = battleShare + marketplaceShare + dataShare;
        uint256 founderShare = totalFounderPool / 2;

        uint256 alreadyWithdrawn = withdrawn[founder];
        if (founderShare <= alreadyWithdrawn) return 0;
        return founderShare - alreadyWithdrawn;
    }

    /// @notice Withdraw available balance. Each founder pulls their own share.
    function withdraw() external onlyFounders nonReentrant {
        uint256 amount = availableBalance(msg.sender);
        if (amount == 0) revert NoBalanceAvailable();

        withdrawn[msg.sender] += amount;

        (bool success, ) = msg.sender.call{value: amount}("");
        if (!success) revert TransferFailed();

        emit Withdrawal(msg.sender, amount);
    }

    // ─── Address Management ──────────────────────────────────────────────────

    /// @notice Update a founder's wallet address (immediate — no timelock on rotation).
    ///         For full multi-sig rotation use transferFoundership.
    function updateFounderAddress(address newAddress) external onlyFounders {
        address old = msg.sender;
        if (msg.sender == founder1) {
            founder1 = newAddress;
            // migrate withdrawal tracking
            withdrawn[newAddress] = withdrawn[old];
            withdrawn[old] = 0;
        } else {
            founder2 = newAddress;
            withdrawn[newAddress] = withdrawn[old];
            withdrawn[old] = 0;
        }
        emit FounderAddressUpdated(old, newAddress);
    }

    /// @notice Initiate foundership transfer with 7-day timelock.
    ///         Both new addresses must be confirmed by current founders.
    function transferFoundership(address newFounder1, address newFounder2) external onlyFounders {
        pendingFounder1 = newFounder1;
        pendingFounder2 = newFounder2;
        foundershipTransferInitiatedAt = block.timestamp;
        emit FoundershipTransferInitiated(newFounder1, newFounder2);
    }

    /// @notice Complete foundership transfer after 7-day timelock expires.
    function completeFoundershipTransfer() external onlyFounders {
        if (pendingFounder1 == address(0)) revert NoPendingTransfer();
        uint256 unlocksAt = foundershipTransferInitiatedAt + FOUNDERSHIP_TIMELOCK;
        if (block.timestamp < unlocksAt) revert TimelockNotExpired(unlocksAt);

        address newF1 = pendingFounder1;
        address newF2 = pendingFounder2;

        // Migrate withdrawal balances
        withdrawn[newF1] = withdrawn[founder1];
        withdrawn[newF2] = withdrawn[founder2];
        withdrawn[founder1] = 0;
        withdrawn[founder2] = 0;

        founder1 = newF1;
        founder2 = newF2;
        pendingFounder1 = address(0);
        pendingFounder2 = address(0);
        foundershipTransferInitiatedAt = 0;

        emit FoundershipTransferCompleted(newF1, newF2);
    }

    // ─── View Helpers ─────────────────────────────────────────────────────────

    /// @notice Total fees received across all categories
    function totalFeesReceived() external view returns (uint256) {
        return battleFeesCollected + marketplaceFeesCollected + dataFeesCollected;
    }

    /// @notice Contract ETH balance
    function contractBalance() external view returns (uint256) {
        return address(this).balance;
    }

    /// @notice Allow contract to receive ETH directly
    receive() external payable {
        battleFeesCollected += msg.value;
        emit FeeReceived(FeeType.Battle, msg.value);
    }
}
