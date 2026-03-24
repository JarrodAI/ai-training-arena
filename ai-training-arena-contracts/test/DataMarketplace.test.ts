import { expect } from "chai";
import { ethers } from "hardhat";
import { AgentNFT, DataMarketplace } from "../typechain-types";
import { SignerWithAddress } from "@nomicfoundation/hardhat-ethers/signers";

describe("DataMarketplace", function () {
  let agentNFT: AgentNFT;
  let marketplace: DataMarketplace;
  let deployer: SignerWithAddress;
  let treasury: SignerWithAddress;
  let seller: SignerWithAddress;
  let buyer: SignerWithAddress;
  let other: SignerWithAddress;

  let sellerNftId: bigint;
  const IPFS_HASH = "QmTest1234567890abcdefghijklmnop";
  const PRICE = ethers.parseEther("1");
  const CATEGORY_BATTLE_LOG = 0;
  const CATEGORY_MODEL_CHECKPOINT = 1;

  beforeEach(async function () {
    [deployer, treasury, seller, buyer, other] = await ethers.getSigners();

    const nftFactory = await ethers.getContractFactory("AgentNFT");
    agentNFT = await nftFactory.deploy();
    await agentNFT.waitForDeployment();

    const marketFactory = await ethers.getContractFactory("DataMarketplace");
    marketplace = await marketFactory.deploy(
      await agentNFT.getAddress(),
      treasury.address
    );
    await marketplace.waitForDeployment();

    // Grant minter role and mint an NFT for seller
    const MINTER_ROLE = await agentNFT.MINTER_ROLE();
    await agentNFT.grantRole(MINTER_ROLE, deployer.address);
    await agentNFT.mintAgent(seller.address, 0, "TestModel-7B");
    sellerNftId = 1n;
  });

  describe("Deployment", function () {
    it("should set correct references", async function () {
      expect(await marketplace.agentNFT()).to.equal(
        await agentNFT.getAddress()
      );
      expect(await marketplace.daoTreasury()).to.equal(treasury.address);
    });

    it("should set PLATFORM_FEE to 500 bps (5%)", async function () {
      expect(await marketplace.PLATFORM_FEE()).to.equal(500n);
    });

    it("should reject zero addresses in constructor", async function () {
      const factory = await ethers.getContractFactory("DataMarketplace");
      await expect(
        factory.deploy(ethers.ZeroAddress, treasury.address)
      ).to.be.revertedWith("zero NFT address");
      await expect(
        factory.deploy(await agentNFT.getAddress(), ethers.ZeroAddress)
      ).to.be.revertedWith("zero treasury address");
    });

    it("should grant DEFAULT_ADMIN_ROLE to deployer", async function () {
      const DEFAULT_ADMIN_ROLE = await marketplace.DEFAULT_ADMIN_ROLE();
      expect(
        await marketplace.hasRole(DEFAULT_ADMIN_ROLE, deployer.address)
      ).to.be.true;
    });
  });

  describe("Listing Data", function () {
    it("should create a listing successfully", async function () {
      await expect(
        marketplace
          .connect(seller)
          .listData(sellerNftId, IPFS_HASH, PRICE, CATEGORY_BATTLE_LOG)
      )
        .to.emit(marketplace, "DataListed")
        .withArgs(1n, seller.address, sellerNftId, PRICE, CATEGORY_BATTLE_LOG);

      const info = await marketplace.getListingInfo(1);
      expect(info.seller).to.equal(seller.address);
      expect(info.price).to.equal(PRICE);
      expect(info.active).to.be.true;
    });

    it("should return full listing details via listings()", async function () {
      await marketplace
        .connect(seller)
        .listData(sellerNftId, IPFS_HASH, PRICE, CATEGORY_BATTLE_LOG);

      const listing = await marketplace.listings(1);
      expect(listing.seller).to.equal(seller.address);
      expect(listing.nftId).to.equal(sellerNftId);
      expect(listing.ipfsHash).to.equal(IPFS_HASH);
      expect(listing.pricePerAccess).to.equal(PRICE);
      expect(listing.totalSales).to.equal(0n);
      expect(listing.active).to.be.true;
      expect(listing.category).to.equal(CATEGORY_BATTLE_LOG);
    });

    it("should increment listing IDs", async function () {
      await marketplace
        .connect(seller)
        .listData(sellerNftId, IPFS_HASH, PRICE, CATEGORY_BATTLE_LOG);
      await marketplace
        .connect(seller)
        .listData(
          sellerNftId,
          "QmSecondHash",
          ethers.parseEther("0.5"),
          CATEGORY_MODEL_CHECKPOINT
        );

      const info1 = await marketplace.listings(1);
      const info2 = await marketplace.listings(2);
      expect(info1.ipfsHash).to.equal(IPFS_HASH);
      expect(info2.ipfsHash).to.equal("QmSecondHash");
    });

    it("should track listings by NFT", async function () {
      await marketplace
        .connect(seller)
        .listData(sellerNftId, IPFS_HASH, PRICE, CATEGORY_BATTLE_LOG);
      await marketplace
        .connect(seller)
        .listData(
          sellerNftId,
          "QmSecondHash",
          ethers.parseEther("0.5"),
          CATEGORY_MODEL_CHECKPOINT
        );

      const listings = await marketplace.getListingsByNFT(sellerNftId);
      expect(listings.length).to.equal(2);
      expect(listings[0]).to.equal(1n);
      expect(listings[1]).to.equal(2n);
    });

    it("should revert if caller does not own the NFT", async function () {
      await expect(
        marketplace
          .connect(buyer)
          .listData(sellerNftId, IPFS_HASH, PRICE, CATEGORY_BATTLE_LOG)
      ).to.be.revertedWithCustomError(marketplace, "NotNFTOwner");
    });

    it("should revert with zero price", async function () {
      await expect(
        marketplace
          .connect(seller)
          .listData(sellerNftId, IPFS_HASH, 0, CATEGORY_BATTLE_LOG)
      ).to.be.revertedWithCustomError(marketplace, "InvalidPrice");
    });

    it("should revert when paused", async function () {
      await marketplace.pause();
      await expect(
        marketplace
          .connect(seller)
          .listData(sellerNftId, IPFS_HASH, PRICE, CATEGORY_BATTLE_LOG)
      ).to.be.revertedWithCustomError(marketplace, "EnforcedPause");
    });

    it("should support different categories", async function () {
      await marketplace
        .connect(seller)
        .listData(sellerNftId, IPFS_HASH, PRICE, 0); // BATTLE_LOG
      await marketplace
        .connect(seller)
        .listData(sellerNftId, "QmHash2", PRICE, 1); // MODEL_CHECKPOINT
      await marketplace
        .connect(seller)
        .listData(sellerNftId, "QmHash3", PRICE, 2); // QUESTION_CORPUS
      await marketplace
        .connect(seller)
        .listData(sellerNftId, "QmHash4", PRICE, 3); // TRAINING_SET

      expect((await marketplace.listings(1)).category).to.equal(0);
      expect((await marketplace.listings(2)).category).to.equal(1);
      expect((await marketplace.listings(3)).category).to.equal(2);
      expect((await marketplace.listings(4)).category).to.equal(3);
    });
  });

  describe("Purchasing Data Access", function () {
    beforeEach(async function () {
      await marketplace
        .connect(seller)
        .listData(sellerNftId, IPFS_HASH, PRICE, CATEGORY_BATTLE_LOG);
    });

    it("should grant access and distribute funds with 5% fee", async function () {
      const sellerBalBefore = await ethers.provider.getBalance(seller.address);
      const treasuryBalBefore = await ethers.provider.getBalance(
        treasury.address
      );

      await expect(
        marketplace.connect(buyer).purchaseDataAccess(1, { value: PRICE })
      )
        .to.emit(marketplace, "DataPurchased")
        .withArgs(1n, buyer.address, PRICE);

      expect(await marketplace.hasAccessTo(1, buyer.address)).to.be.true;

      const sellerBalAfter = await ethers.provider.getBalance(seller.address);
      const treasuryBalAfter = await ethers.provider.getBalance(
        treasury.address
      );

      const expectedFee = (PRICE * 500n) / 10000n;
      const expectedSellerAmount = PRICE - expectedFee;

      expect(sellerBalAfter - sellerBalBefore).to.equal(expectedSellerAmount);
      expect(treasuryBalAfter - treasuryBalBefore).to.equal(expectedFee);
    });

    it("should emit AccessGranted event", async function () {
      await expect(
        marketplace.connect(buyer).purchaseDataAccess(1, { value: PRICE })
      )
        .to.emit(marketplace, "AccessGranted")
        .withArgs(1n, buyer.address);
    });

    it("should increment totalSales on the listing", async function () {
      await marketplace
        .connect(buyer)
        .purchaseDataAccess(1, { value: PRICE });
      const listing = await marketplace.listings(1);
      expect(listing.totalSales).to.equal(1n);
    });

    it("should revert if listing is not active", async function () {
      await marketplace.connect(seller).delistData(1);
      await expect(
        marketplace.connect(buyer).purchaseDataAccess(1, { value: PRICE })
      ).to.be.revertedWithCustomError(marketplace, "ListingNotActive");
    });

    it("should revert if insufficient payment", async function () {
      await expect(
        marketplace
          .connect(buyer)
          .purchaseDataAccess(1, { value: PRICE / 2n })
      ).to.be.revertedWithCustomError(marketplace, "InsufficientPayment");
    });

    it("should revert if buyer already has access", async function () {
      await marketplace
        .connect(buyer)
        .purchaseDataAccess(1, { value: PRICE });
      await expect(
        marketplace.connect(buyer).purchaseDataAccess(1, { value: PRICE })
      ).to.be.revertedWithCustomError(marketplace, "AlreadyHasAccess");
    });

    it("should revert when paused", async function () {
      await marketplace.pause();
      await expect(
        marketplace.connect(buyer).purchaseDataAccess(1, { value: PRICE })
      ).to.be.revertedWithCustomError(marketplace, "EnforcedPause");
    });

    it("should allow multiple buyers for the same listing", async function () {
      await marketplace
        .connect(buyer)
        .purchaseDataAccess(1, { value: PRICE });
      await marketplace
        .connect(other)
        .purchaseDataAccess(1, { value: PRICE });

      expect(await marketplace.hasAccessTo(1, buyer.address)).to.be.true;
      expect(await marketplace.hasAccessTo(1, other.address)).to.be.true;

      const listing = await marketplace.listings(1);
      expect(listing.totalSales).to.equal(2n);
    });

    it("should accept overpayment", async function () {
      const overpay = PRICE * 2n;
      await expect(
        marketplace.connect(buyer).purchaseDataAccess(1, { value: overpay })
      ).to.not.be.reverted;

      expect(await marketplace.hasAccessTo(1, buyer.address)).to.be.true;
    });
  });

  describe("Delisting Data", function () {
    beforeEach(async function () {
      await marketplace
        .connect(seller)
        .listData(sellerNftId, IPFS_HASH, PRICE, CATEGORY_BATTLE_LOG);
    });

    it("should delist successfully", async function () {
      await expect(marketplace.connect(seller).delistData(1))
        .to.emit(marketplace, "DataDelisted")
        .withArgs(1n);

      const info = await marketplace.getListingInfo(1);
      expect(info.active).to.be.false;
    });

    it("should revert if not the seller", async function () {
      await expect(
        marketplace.connect(buyer).delistData(1)
      ).to.be.revertedWithCustomError(marketplace, "NotSeller");
    });

    it("should revert if already delisted", async function () {
      await marketplace.connect(seller).delistData(1);
      await expect(
        marketplace.connect(seller).delistData(1)
      ).to.be.revertedWithCustomError(marketplace, "ListingNotActive");
    });
  });

  describe("Update Price", function () {
    beforeEach(async function () {
      await marketplace
        .connect(seller)
        .listData(sellerNftId, IPFS_HASH, PRICE, CATEGORY_BATTLE_LOG);
    });

    it("should update price successfully", async function () {
      const newPrice = ethers.parseEther("2");
      await expect(marketplace.connect(seller).updatePrice(1, newPrice))
        .to.emit(marketplace, "PriceUpdated")
        .withArgs(1n, newPrice);

      const info = await marketplace.getListingInfo(1);
      expect(info.price).to.equal(newPrice);
    });

    it("should revert if not the seller", async function () {
      await expect(
        marketplace.connect(buyer).updatePrice(1, ethers.parseEther("2"))
      ).to.be.revertedWithCustomError(marketplace, "NotSeller");
    });

    it("should revert if listing is inactive", async function () {
      await marketplace.connect(seller).delistData(1);
      await expect(
        marketplace.connect(seller).updatePrice(1, ethers.parseEther("2"))
      ).to.be.revertedWithCustomError(marketplace, "ListingNotActive");
    });

    it("should revert with zero price", async function () {
      await expect(
        marketplace.connect(seller).updatePrice(1, 0)
      ).to.be.revertedWithCustomError(marketplace, "InvalidPrice");
    });
  });

  describe("Active Listings Query", function () {
    beforeEach(async function () {
      await marketplace
        .connect(seller)
        .listData(sellerNftId, "QmHash1", PRICE, 0);
      await marketplace
        .connect(seller)
        .listData(sellerNftId, "QmHash2", PRICE, 1);
      await marketplace
        .connect(seller)
        .listData(sellerNftId, "QmHash3", PRICE, 2);
    });

    it("should return all active listings", async function () {
      const active = await marketplace.getActiveListings(0, 10);
      expect(active.length).to.equal(3);
    });

    it("should exclude delisted entries", async function () {
      await marketplace.connect(seller).delistData(2);
      const active = await marketplace.getActiveListings(0, 10);
      expect(active.length).to.equal(2);
      expect(active[0]).to.equal(1n);
      expect(active[1]).to.equal(3n);
    });

    it("should handle offset and limit", async function () {
      const active = await marketplace.getActiveListings(1, 1);
      expect(active.length).to.equal(1);
      expect(active[0]).to.equal(2n);
    });

    it("should return empty array for out-of-range offset", async function () {
      const active = await marketplace.getActiveListings(100, 10);
      expect(active.length).to.equal(0);
    });
  });

  describe("Pause/Unpause", function () {
    it("should pause and unpause", async function () {
      await marketplace.pause();
      expect(await marketplace.paused()).to.be.true;

      await marketplace.unpause();
      expect(await marketplace.paused()).to.be.false;
    });

    it("should revert pause if unauthorized", async function () {
      await expect(marketplace.connect(buyer).pause()).to.be.reverted;
    });

    it("should revert unpause if unauthorized", async function () {
      await marketplace.pause();
      await expect(marketplace.connect(buyer).unpause()).to.be.reverted;
    });
  });

  describe("View Functions", function () {
    it("should return false for hasAccessTo when no purchase", async function () {
      expect(await marketplace.hasAccessTo(1, buyer.address)).to.be.false;
    });

    it("should return zero values for non-existent listing", async function () {
      const info = await marketplace.getListingInfo(999);
      expect(info.seller).to.equal(ethers.ZeroAddress);
      expect(info.price).to.equal(0n);
      expect(info.active).to.be.false;
    });

    it("should return empty array for NFT with no listings", async function () {
      const listings = await marketplace.getListingsByNFT(999);
      expect(listings.length).to.equal(0);
    });
  });
});
