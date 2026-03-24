// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

import "@openzeppelin/contracts/governance/Governor.sol";
import "@openzeppelin/contracts/governance/extensions/GovernorCountingSimple.sol";
import "@openzeppelin/contracts/governance/extensions/GovernorVotes.sol";
import "@openzeppelin/contracts/governance/extensions/GovernorTimelockControl.sol";
import "@openzeppelin/contracts/governance/extensions/GovernorSettings.sol";
import "./interfaces/IAgentNFT.sol";

/// @title AIArenaGovernor
/// @notice DAO governance contract for AI Training Arena.
///         Voting power: base ATA + staked bonus (2x) + NFT holder bonus (3x total).
///         Timelock: configured via TimelockController (48-hour delay set in deploy.ts).
///         Multi-sig: 3/5 confirmations required before _executeOperations runs.
contract AIArenaGovernor is
    Governor,
    GovernorSettings,
    GovernorCountingSimple,
    GovernorVotes,
    GovernorTimelockControl
{
    // ~1 day in uint48 blocks at 2s block time on Mantle
    uint48 public constant VOTING_DELAY_BLOCKS = 43200;
    // ~7 days in uint32 blocks at 2s block time on Mantle
    uint32 public constant VOTING_PERIOD_BLOCKS = 302400;
    uint256 public constant PROPOSAL_THRESHOLD_ATA = 100_000 ether;
    uint256 public constant QUORUM_ATA = 10_000_000 ether;

    IAgentNFT public immutable agentNFT;
    address public immutable stakingContract;

    mapping(uint256 => mapping(address => bool)) public multiSigConfirmations;
    mapping(uint256 => uint256) public confirmationCount;
    mapping(address => bool) public isMultiSigMember;

    uint256 public constant MULTISIG_CONFIRMATIONS_REQUIRED = 3;

    error InsufficientMultiSigConfirmations(uint256 required, uint256 actual);
    error AlreadyConfirmed();
    error NotMultiSigMember();

    event ProposalConfirmed(uint256 indexed proposalId, address indexed confirmer, uint256 total);
    event EmergencyPaused(address indexed target, address indexed initiator);

    constructor(
        IVotes _token,
        TimelockController _timelock,
        IAgentNFT _agentNFT,
        address _stakingContract,
        address[5] memory _multiSigMembers
    )
        Governor("AIArenaGovernor")
        GovernorSettings(VOTING_DELAY_BLOCKS, VOTING_PERIOD_BLOCKS, PROPOSAL_THRESHOLD_ATA)
        GovernorVotes(_token)
        GovernorTimelockControl(_timelock)
    {
        agentNFT = _agentNFT;
        stakingContract = _stakingContract;
        for (uint256 i = 0; i < 5; i++) {
            isMultiSigMember[_multiSigMembers[i]] = true;
        }
    }

    /// @notice Returns quorum: 10M ATA
    function quorum(uint256) public pure override returns (uint256) {
        return QUORUM_ATA;
    }

    /// @notice Internal vote weight with staking + NFT multipliers.
    ///         base = token.getPastVotes, staked 2x, staked+NFT 3x.
    function _getVotes(address account, uint256 timepoint, bytes memory params)
        internal view override(Governor, GovernorVotes)
        returns (uint256)
    {
        uint256 baseVotes = super._getVotes(account, timepoint, params);
        uint256 stakedBalance = _getStakedBalance(account);
        uint256 nftBonus = agentNFT.balanceOf(account) > 0 ? stakedBalance : 0;
        return baseVotes + stakedBalance + nftBonus;
    }

    /// @notice Confirm a passed proposal (multi-sig members only, 3/5 required before execute).
    function confirmProposal(uint256 proposalId) external {
        if (!isMultiSigMember[msg.sender]) revert NotMultiSigMember();
        if (multiSigConfirmations[proposalId][msg.sender]) revert AlreadyConfirmed();
        multiSigConfirmations[proposalId][msg.sender] = true;
        confirmationCount[proposalId]++;
        emit ProposalConfirmed(proposalId, msg.sender, confirmationCount[proposalId]);
    }

    /// @notice Emergency pause — no timelock, emits event for off-chain multi-sig processing.
    function emergencyPause(address target) external {
        if (!isMultiSigMember[msg.sender]) revert NotMultiSigMember();
        emit EmergencyPaused(target, msg.sender);
    }

    // ─── Required OpenZeppelin 5.x Overrides ─────────────────────────────────

    function state(uint256 proposalId)
        public view override(Governor, GovernorTimelockControl)
        returns (ProposalState)
    { return super.state(proposalId); }

    function proposalNeedsQueuing(uint256 proposalId)
        public view override(Governor, GovernorTimelockControl)
        returns (bool)
    { return super.proposalNeedsQueuing(proposalId); }

    function votingDelay()
        public view override(Governor, GovernorSettings)
        returns (uint256)
    { return super.votingDelay(); }

    function votingPeriod()
        public view override(Governor, GovernorSettings)
        returns (uint256)
    { return super.votingPeriod(); }

    function proposalThreshold()
        public view override(Governor, GovernorSettings)
        returns (uint256)
    { return super.proposalThreshold(); }

    function _queueOperations(
        uint256 proposalId,
        address[] memory targets,
        uint256[] memory values,
        bytes[] memory calldatas,
        bytes32 descriptionHash
    ) internal override(Governor, GovernorTimelockControl) returns (uint48) {
        return super._queueOperations(proposalId, targets, values, calldatas, descriptionHash);
    }

    function _executeOperations(
        uint256 proposalId,
        address[] memory targets,
        uint256[] memory values,
        bytes[] memory calldatas,
        bytes32 descriptionHash
    ) internal override(Governor, GovernorTimelockControl) {
        uint256 confs = confirmationCount[proposalId];
        if (confs < MULTISIG_CONFIRMATIONS_REQUIRED) {
            revert InsufficientMultiSigConfirmations(MULTISIG_CONFIRMATIONS_REQUIRED, confs);
        }
        super._executeOperations(proposalId, targets, values, calldatas, descriptionHash);
    }

    function _cancel(
        address[] memory targets,
        uint256[] memory values,
        bytes[] memory calldatas,
        bytes32 descriptionHash
    ) internal override(Governor, GovernorTimelockControl) returns (uint256) {
        return super._cancel(targets, values, calldatas, descriptionHash);
    }

    function _executor()
        internal view override(Governor, GovernorTimelockControl)
        returns (address)
    { return super._executor(); }

    // ─── Private ─────────────────────────────────────────────────────────────

    function _getStakedBalance(address account) private view returns (uint256) {
        (bool ok, bytes memory data) = stakingContract.staticcall(
            abi.encodeWithSignature("stakedBalance(address)", account)
        );
        if (!ok || data.length < 32) return 0;
        return abi.decode(data, (uint256));
    }
}
