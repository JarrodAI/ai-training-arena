// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

import "@openzeppelin/contracts/access/AccessControl.sol";
import "@openzeppelin/contracts/utils/ReentrancyGuard.sol";
import "@openzeppelin/contracts/utils/Pausable.sol";
import "./AgentNFT.sol";

contract DataMarketplace is AccessControl, ReentrancyGuard, Pausable {
    enum DataCategory {
        BATTLE_LOG,
        MODEL_CHECKPOINT,
        QUESTION_CORPUS,
        TRAINING_SET
    }

    struct DataListing {
        address seller;
        uint256 nftId;
        string ipfsHash;
        uint256 pricePerAccess;
        uint256 totalSales;
        uint256 createdAt;
        bool active;
        DataCategory category;
    }

    uint256 public constant PLATFORM_FEE = 500;
    uint256 private constant BASIS_POINTS = 10000;

    AgentNFT public immutable agentNFT;
    address public immutable daoTreasury;

    mapping(uint256 => DataListing) private _listings;
    mapping(uint256 => mapping(address => bool)) private _accessGranted;
    mapping(uint256 => uint256[]) private _listingsByNFT;

    uint256 private _nextListingId;
    uint256[] private _allListingIds;

    event DataListed(
        uint256 indexed listingId,
        address indexed seller,
        uint256 indexed nftId,
        uint256 pricePerAccess,
        DataCategory category
    );
    event DataPurchased(
        uint256 indexed listingId,
        address indexed buyer,
        uint256 price
    );
    event DataDelisted(uint256 indexed listingId);
    event PriceUpdated(uint256 indexed listingId, uint256 newPrice);
    event AccessGranted(uint256 indexed listingId, address indexed buyer);

    error NotNFTOwner();
    error ListingNotActive();
    error InsufficientPayment();
    error NotSeller();
    error AlreadyHasAccess();
    error InvalidPrice();

    constructor(address _agentNFT, address _daoTreasury) {
        require(_agentNFT != address(0), "zero NFT address");
        require(_daoTreasury != address(0), "zero treasury address");

        agentNFT = AgentNFT(_agentNFT);
        daoTreasury = _daoTreasury;
        _nextListingId = 1;

        _grantRole(DEFAULT_ADMIN_ROLE, msg.sender);
    }

    function listData(
        uint256 nftId,
        string calldata ipfsHash,
        uint256 pricePerAccess,
        DataCategory category
    ) external whenNotPaused returns (uint256) {
        if (agentNFT.ownerOf(nftId) != msg.sender) revert NotNFTOwner();
        if (pricePerAccess == 0) revert InvalidPrice();

        uint256 listingId = _nextListingId++;

        _listings[listingId] = DataListing({
            seller: msg.sender,
            nftId: nftId,
            ipfsHash: ipfsHash,
            pricePerAccess: pricePerAccess,
            totalSales: 0,
            createdAt: block.timestamp,
            active: true,
            category: category
        });

        _listingsByNFT[nftId].push(listingId);
        _allListingIds.push(listingId);

        emit DataListed(
            listingId, msg.sender, nftId, pricePerAccess, category
        );
        return listingId;
    }

    function purchaseDataAccess(
        uint256 listingId
    ) external payable nonReentrant whenNotPaused {
        DataListing storage listing = _listings[listingId];
        if (!listing.active) revert ListingNotActive();
        if (msg.value < listing.pricePerAccess) revert InsufficientPayment();
        if (_accessGranted[listingId][msg.sender]) revert AlreadyHasAccess();

        _accessGranted[listingId][msg.sender] = true;
        listing.totalSales++;

        uint256 fee = (msg.value * PLATFORM_FEE) / BASIS_POINTS;
        uint256 sellerAmount = msg.value - fee;

        (bool sentSeller, ) = payable(listing.seller).call{
            value: sellerAmount
        }("");
        require(sentSeller, "seller payment failed");

        (bool sentTreasury, ) = payable(daoTreasury).call{value: fee}("");
        require(sentTreasury, "treasury payment failed");

        emit DataPurchased(listingId, msg.sender, msg.value);
        emit AccessGranted(listingId, msg.sender);
    }

    function delistData(uint256 listingId) external {
        DataListing storage listing = _listings[listingId];
        if (listing.seller != msg.sender) revert NotSeller();
        if (!listing.active) revert ListingNotActive();

        listing.active = false;
        emit DataDelisted(listingId);
    }

    function updatePrice(uint256 listingId, uint256 newPrice) external {
        DataListing storage listing = _listings[listingId];
        if (listing.seller != msg.sender) revert NotSeller();
        if (!listing.active) revert ListingNotActive();
        if (newPrice == 0) revert InvalidPrice();

        listing.pricePerAccess = newPrice;
        emit PriceUpdated(listingId, newPrice);
    }

    function hasAccessTo(
        uint256 listingId,
        address buyer
    ) external view returns (bool) {
        return _accessGranted[listingId][buyer];
    }

    function getListingsByNFT(
        uint256 nftId
    ) external view returns (uint256[] memory) {
        return _listingsByNFT[nftId];
    }

    function getActiveListings(
        uint256 offset,
        uint256 limit
    ) external view returns (uint256[] memory) {
        uint256 total = _allListingIds.length;
        if (offset >= total) {
            return new uint256[](0);
        }

        uint256 count = 0;
        uint256 end = offset + limit;
        if (end > total) end = total;
        uint256[] memory temp = new uint256[](end - offset);

        for (uint256 i = offset; i < end; i++) {
            uint256 lid = _allListingIds[i];
            if (_listings[lid].active) {
                temp[count++] = lid;
            }
        }

        uint256[] memory result = new uint256[](count);
        for (uint256 i = 0; i < count; i++) {
            result[i] = temp[i];
        }
        return result;
    }

    function getListingInfo(
        uint256 listingId
    ) external view returns (address seller, uint256 price, bool active) {
        DataListing storage listing = _listings[listingId];
        return (listing.seller, listing.pricePerAccess, listing.active);
    }

    function listings(
        uint256 listingId
    ) external view returns (
        address seller,
        uint256 nftId,
        string memory ipfsHash,
        uint256 pricePerAccess,
        uint256 totalSales,
        uint256 createdAt,
        bool active,
        DataCategory category
    ) {
        DataListing storage l = _listings[listingId];
        return (
            l.seller, l.nftId, l.ipfsHash, l.pricePerAccess,
            l.totalSales, l.createdAt, l.active, l.category
        );
    }

    function pause() external onlyRole(DEFAULT_ADMIN_ROLE) {
        _pause();
    }

    function unpause() external onlyRole(DEFAULT_ADMIN_ROLE) {
        _unpause();
    }
}
