// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

import "@openzeppelin/contracts/access/AccessControl.sol";
import "@openzeppelin/contracts/utils/Pausable.sol";
import "@openzeppelin/contracts/utils/ReentrancyGuard.sol";
import "@openzeppelin/contracts/token/ERC20/IERC20.sol";
import "@openzeppelin/contracts/token/ERC20/utils/SafeERC20.sol";
import "@openzeppelin/contracts/utils/cryptography/MerkleProof.sol";

interface IATAToken {
    function mint(address to, uint256 amount) external;
}

interface IChainlinkOracle {
    function latestRoundData()
        external view
        returns (uint80, int256, uint256, uint256, uint80);
}

contract NodeSaleICO is AccessControl, Pausable, ReentrancyGuard {
    using SafeERC20 for IERC20;

    bytes32 public constant SALE_ADMIN_ROLE = keccak256("SALE_ADMIN_ROLE");
    bytes32 public constant PAUSER_ROLE     = keccak256("PAUSER_ROLE");

    enum Phase { NOT_STARTED, FOUNDERS_PRESALE, TIER_1, TIER_2, TIER_3, TIER_4, FINALIZED }

    struct PhaseConfig {
        uint256 pricePerATA;
        uint256 allocation;
        uint256 sold;
    }

    Phase public currentPhase;
    mapping(Phase => PhaseConfig) public phaseConfig;

    bytes32 public foundersRoot;
    mapping(address => bool) public founderPurchased;

    uint256 public constant SOFT_CAP = 120_000_000 * 1e6;
    uint256 public constant HARD_CAP = 400_000_000 * 1e6;
    uint256 public totalRaisedUSDC;

    IERC20           public immutable usdc;
    IATAToken        public immutable ataToken;
    IChainlinkOracle public           oracle;
    address          public           treasury;

    mapping(address => uint256) public contributions;
    bool public refundEnabled;

    event PhaseAdvanced(Phase oldPhase, Phase newPhase);
    event Purchase(address indexed buyer, uint256 ataAmount, uint256 usdcPaid, Phase phase);
    event Refund(address indexed buyer, uint256 usdcRefunded);
    event SaleFinalized(bool softCapMet, uint256 totalRaised);
    event FoundersRootUpdated(bytes32 newRoot);

    constructor(address _usdc, address _ataToken, address _oracle, address _treasury) {
        require(_usdc     != address(0), "NodeSaleICO: zero usdc");
        require(_ataToken != address(0), "NodeSaleICO: zero ata");
        require(_oracle   != address(0), "NodeSaleICO: zero oracle");
        require(_treasury != address(0), "NodeSaleICO: zero treasury");

        usdc     = IERC20(_usdc);
        ataToken = IATAToken(_ataToken);
        oracle   = IChainlinkOracle(_oracle);
        treasury = _treasury;

        _grantRole(DEFAULT_ADMIN_ROLE, msg.sender);
        _grantRole(SALE_ADMIN_ROLE,    msg.sender);
        _grantRole(PAUSER_ROLE,        msg.sender);

        currentPhase = Phase.NOT_STARTED;

        phaseConfig[Phase.FOUNDERS_PRESALE] = PhaseConfig(3 * 1e6,   5_000_000 * 1e18, 0);
        phaseConfig[Phase.TIER_1]           = PhaseConfig(4 * 1e6,   5_000_000 * 1e18, 0);
        phaseConfig[Phase.TIER_2]           = PhaseConfig(5_500_000, 8_000_000 * 1e18, 0);
        phaseConfig[Phase.TIER_3]           = PhaseConfig(7 * 1e6,   8_000_000 * 1e18, 0);
        phaseConfig[Phase.TIER_4]           = PhaseConfig(10 * 1e6,  9_000_000 * 1e18, 0);
    }

    function advancePhase() external onlyRole(SALE_ADMIN_ROLE) {
        Phase old = currentPhase;
        require(old != Phase.FINALIZED, "NodeSaleICO: already finalized");

        if      (old == Phase.NOT_STARTED)       { currentPhase = Phase.FOUNDERS_PRESALE; }
        else if (old == Phase.FOUNDERS_PRESALE)  { currentPhase = Phase.TIER_1; }
        else if (old == Phase.TIER_1)            { currentPhase = Phase.TIER_2; }
        else if (old == Phase.TIER_2)            { currentPhase = Phase.TIER_3; }
        else if (old == Phase.TIER_3)            { currentPhase = Phase.TIER_4; }
        else if (old == Phase.TIER_4)            { _finalize(); return; }

        emit PhaseAdvanced(old, currentPhase);
    }

    function _finalize() internal {
        Phase old = currentPhase;
        currentPhase = Phase.FINALIZED;
        bool softCapMet = totalRaisedUSDC >= SOFT_CAP;

        if (!softCapMet) {
            refundEnabled = true;
        } else {
            uint256 bal = usdc.balanceOf(address(this));
            if (bal > 0) usdc.safeTransfer(treasury, bal);
        }

        emit SaleFinalized(softCapMet, totalRaisedUSDC);
        emit PhaseAdvanced(old, Phase.FINALIZED);
    }

    function buyFounders(uint256 ataAmount, bytes32[] calldata proof)
        external whenNotPaused nonReentrant
    {
        require(currentPhase == Phase.FOUNDERS_PRESALE, "NodeSaleICO: not founders phase");
        require(!founderPurchased[msg.sender],           "NodeSaleICO: already purchased");
        bytes32 leaf = keccak256(abi.encodePacked(msg.sender));
        require(MerkleProof.verify(proof, foundersRoot, leaf), "NodeSaleICO: not whitelisted");
        founderPurchased[msg.sender] = true;
        _executePurchase(Phase.FOUNDERS_PRESALE, ataAmount);
    }

    function buy(uint256 ataAmount) external whenNotPaused nonReentrant {
        Phase phase = currentPhase;
        require(
            phase == Phase.TIER_1 || phase == Phase.TIER_2 ||
            phase == Phase.TIER_3 || phase == Phase.TIER_4,
            "NodeSaleICO: not a public phase"
        );
        _executePurchase(phase, ataAmount);
    }

    function _executePurchase(Phase phase, uint256 ataAmount) internal {
        require(ataAmount > 0, "NodeSaleICO: zero amount");
        PhaseConfig storage cfg = phaseConfig[phase];
        require(cfg.sold + ataAmount <= cfg.allocation, "NodeSaleICO: exceeds allocation");
        uint256 usdcCost = (cfg.pricePerATA * ataAmount) / 1e18;
        require(usdcCost > 0, "NodeSaleICO: cost too small");
        require(totalRaisedUSDC + usdcCost <= HARD_CAP, "NodeSaleICO: hard cap reached");

        cfg.sold                  += ataAmount;
        totalRaisedUSDC           += usdcCost;
        contributions[msg.sender] += usdcCost;

        usdc.safeTransferFrom(msg.sender, address(this), usdcCost);
        ataToken.mint(msg.sender, ataAmount);
        emit Purchase(msg.sender, ataAmount, usdcCost, phase);
    }

    function claimRefund() external nonReentrant {
        require(refundEnabled,                 "NodeSaleICO: refund not active");
        require(contributions[msg.sender] > 0, "NodeSaleICO: no contribution");
        uint256 amount = contributions[msg.sender];
        contributions[msg.sender] = 0;
        usdc.safeTransfer(msg.sender, amount);
        emit Refund(msg.sender, amount);
    }

    function setFoundersRoot(bytes32 root) external onlyRole(SALE_ADMIN_ROLE) {
        foundersRoot = root;
        emit FoundersRootUpdated(root);
    }

    function setTreasury(address t) external onlyRole(DEFAULT_ADMIN_ROLE) {
        require(t != address(0), "NodeSaleICO: zero treasury");
        treasury = t;
    }

    function setOracle(address o) external onlyRole(DEFAULT_ADMIN_ROLE) {
        require(o != address(0), "NodeSaleICO: zero oracle");
        oracle = IChainlinkOracle(o);
    }

    function pause()   external onlyRole(PAUSER_ROLE) { _pause(); }
    function unpause() external onlyRole(PAUSER_ROLE) { _unpause(); }

    function remainingAllocation(Phase phase) external view returns (uint256) {
        return phaseConfig[phase].allocation - phaseConfig[phase].sold;
    }

    function costForAmount(Phase phase, uint256 ataAmount) external view returns (uint256) {
        return (phaseConfig[phase].pricePerATA * ataAmount) / 1e18;
    }
}
