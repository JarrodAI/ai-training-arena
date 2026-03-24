// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

import "@openzeppelin/contracts/token/ERC721/ERC721.sol";
import "@openzeppelin/contracts/token/ERC721/extensions/ERC721Enumerable.sol";
import "@openzeppelin/contracts/access/AccessControl.sol";
import "@openzeppelin/contracts/utils/Pausable.sol";
import "@openzeppelin/contracts/utils/ReentrancyGuard.sol";
import "@openzeppelin/contracts/token/ERC20/IERC20.sol";
import "@openzeppelin/contracts/token/ERC20/utils/SafeERC20.sol";

interface IChainlinkOracle {
    function latestRoundData()
        external
        view
        returns (
            uint80 roundId,
            int256 answer,
            uint256 startedAt,
            uint256 updatedAt,
            uint80 answeredInRound
        );
}

interface IATAToken {
    function mint(address to, uint256 amount) external;
}

/**
 * @title NodeTrainerNFT
 * @notice ERC-721 NFT with 5 tiers: Bronze, Silver, Gold, Platinum, Diamond.
 *         Priced in USD (via Chainlink ETH/USD oracle), paid in USDC.
 *         Each tier grants bonus ATA tokens on mint.
 */
contract NodeTrainerNFT is ERC721Enumerable, AccessControl, Pausable, ReentrancyGuard {
    using SafeERC20 for IERC20;

    bytes32 public constant MINTER_ROLE   = keccak256("MINTER_ROLE");
    bytes32 public constant PAUSER_ROLE   = keccak256("PAUSER_ROLE");
    bytes32 public constant TREASURY_ROLE = keccak256("TREASURY_ROLE");

    enum Tier { Bronze, Silver, Gold, Platinum, Diamond }

    struct TierConfig {
        uint256 maxSupply;       // max NFTs in this tier
        uint256 priceUSD;        // price in USD (6 decimals, matching USDC)
        uint256 bonusATA;        // bonus ATA tokens (18 decimals) granted on mint
        uint256 minted;          // current minted count
    }

    mapping(Tier => TierConfig) public tierConfig;
    mapping(uint256 => Tier)    public tokenTier;

    uint256 private _nextTokenId;

    IERC20           public immutable usdc;
    IATAToken        public immutable ataToken;
    IChainlinkOracle public           oracle;        // ETH/USD price feed (8 decimals)
    address          public           treasury;      // Gnosis Safe

    string private _baseTokenURI;

    event NFTMinted(address indexed to, uint256 tokenId, Tier tier, uint256 paidUSDC, uint256 bonusATA);
    event TreasuryUpdated(address indexed oldTreasury, address indexed newTreasury);
    event OracleUpdated(address indexed oldOracle, address indexed newOracle);

    constructor(
        address _usdc,
        address _ataToken,
        address _oracle,
        address _treasury
    ) ERC721("AI Training Arena Node", "ATANFT") {
        require(_usdc     != address(0), "NodeTrainerNFT: zero usdc");
        require(_ataToken != address(0), "NodeTrainerNFT: zero ata");
        require(_oracle   != address(0), "NodeTrainerNFT: zero oracle");
        require(_treasury != address(0), "NodeTrainerNFT: zero treasury");

        usdc     = IERC20(_usdc);
        ataToken = IATAToken(_ataToken);
        oracle   = IChainlinkOracle(_oracle);
        treasury = _treasury;

        _grantRole(DEFAULT_ADMIN_ROLE, msg.sender);
        _grantRole(MINTER_ROLE,        msg.sender);
        _grantRole(PAUSER_ROLE,        msg.sender);
        _grantRole(TREASURY_ROLE,      msg.sender);

        // Tier configs: priceUSD in 6-decimal USDC units
        // Bronze:   15,000 supply @ $5,000     bonus 10,000 ATA
        // Silver:    6,000 supply @ $25,000    bonus 75,000 ATA
        // Gold:      2,500 supply @ $100,000   bonus 400,000 ATA
        // Platinum:  1,200 supply @ $400,000   bonus 2,000,000 ATA
        // Diamond:     300 supply @ $1,500,000 bonus 10,000,000 ATA
        tierConfig[Tier.Bronze]   = TierConfig(15000, 5_000 * 1e6,       10_000 * 1e18,   0);
        tierConfig[Tier.Silver]   = TierConfig(6000,  25_000 * 1e6,      75_000 * 1e18,   0);
        tierConfig[Tier.Gold]     = TierConfig(2500,  100_000 * 1e6,     400_000 * 1e18,  0);
        tierConfig[Tier.Platinum] = TierConfig(1200,  400_000 * 1e6,     2_000_000 * 1e18, 0);
        tierConfig[Tier.Diamond]  = TierConfig(300,   1_500_000 * 1e6,   10_000_000 * 1e18, 0);
    }

    // ─────────────────────────────────────────────────────────────
    //  MINT
    // ─────────────────────────────────────────────────────────────

    /**
     * @notice Mint an NFT of the given tier, paying in USDC.
     * @dev USDC allowance must be set by caller before calling.
     */
    function mint(Tier tier) external whenNotPaused nonReentrant {
        TierConfig storage cfg = tierConfig[tier];
        require(cfg.minted < cfg.maxSupply, "NodeTrainerNFT: tier sold out");

        // Pull USDC from buyer → treasury (Gnosis Safe)
        usdc.safeTransferFrom(msg.sender, treasury, cfg.priceUSD);

        // Mint NFT
        uint256 tokenId = _nextTokenId++;
        cfg.minted++;
        tokenTier[tokenId] = tier;
        _safeMint(msg.sender, tokenId);

        // Grant bonus ATA
        if (cfg.bonusATA > 0) {
            ataToken.mint(msg.sender, cfg.bonusATA);
        }

        emit NFTMinted(msg.sender, tokenId, tier, cfg.priceUSD, cfg.bonusATA);
    }

    // ─────────────────────────────────────────────────────────────
    //  VIEW HELPERS
    // ─────────────────────────────────────────────────────────────

    function remainingSupply(Tier tier) external view returns (uint256) {
        TierConfig storage cfg = tierConfig[tier];
        return cfg.maxSupply - cfg.minted;
    }

    function priceInUSDC(Tier tier) external view returns (uint256) {
        return tierConfig[tier].priceUSD;
    }

    function totalMinted() external view returns (uint256) {
        return _nextTokenId;
    }

    // ─────────────────────────────────────────────────────────────
    //  ADMIN
    // ─────────────────────────────────────────────────────────────

    function setTreasury(address newTreasury) external onlyRole(TREASURY_ROLE) {
        require(newTreasury != address(0), "NodeTrainerNFT: zero treasury");
        emit TreasuryUpdated(treasury, newTreasury);
        treasury = newTreasury;
    }

    function setOracle(address newOracle) external onlyRole(DEFAULT_ADMIN_ROLE) {
        require(newOracle != address(0), "NodeTrainerNFT: zero oracle");
        emit OracleUpdated(address(oracle), newOracle);
        oracle = IChainlinkOracle(newOracle);
    }

    function setBaseURI(string calldata baseURI) external onlyRole(DEFAULT_ADMIN_ROLE) {
        _baseTokenURI = baseURI;
    }

    function pause()   external onlyRole(PAUSER_ROLE) { _pause(); }
    function unpause() external onlyRole(PAUSER_ROLE) { _unpause(); }

    function _baseURI() internal view override returns (string memory) {
        return _baseTokenURI;
    }

    // ─────────────────────────────────────────────────────────────
    //  INTERFACE SUPPORT
    // ─────────────────────────────────────────────────────────────

    function supportsInterface(bytes4 interfaceId)
        public
        view
        override(ERC721Enumerable, AccessControl)
        returns (bool)
    {
        return super.supportsInterface(interfaceId);
    }
}
