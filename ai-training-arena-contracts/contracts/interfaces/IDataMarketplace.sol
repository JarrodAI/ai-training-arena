// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

interface IDataMarketplace {
    enum DataCategory { BATTLE_LOG, MODEL_CHECKPOINT, QUESTION_CORPUS, TRAINING_SET }

    function listData(uint256 nftId, string calldata ipfsHash, uint256 pricePerAccess, DataCategory category) external returns (uint256 listingId);
    function purchaseDataAccess(uint256 listingId) external payable;
    function delistData(uint256 listingId) external;
    function updatePrice(uint256 listingId, uint256 newPrice) external;
    function hasAccessTo(uint256 listingId, address buyer) external view returns (bool);
    function getListingsByNFT(uint256 nftId) external view returns (uint256[] memory);
    function getActiveListings(uint256 offset, uint256 limit) external view returns (uint256[] memory);
    function getListingInfo(uint256 listingId) external view returns (address seller, uint256 price, bool active);
}
