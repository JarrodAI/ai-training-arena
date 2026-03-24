import { expect } from "chai";
import { ethers } from "hardhat";
import { loadFixture, time } from "@nomicfoundation/hardhat-toolbox/network-helpers";
import { ATAToken, AgentNFT, WrappedATA } from "../typechain-types";
import { HardhatEthersSigner } from "@nomicfoundation/hardhat-ethers/signers";

describe("WrappedATA", function () {
  let ataToken: ATAToken;
  let agentNFT: AgentNFT;
  let wrappedATA: WrappedATA;
  let deployer: HardhatEthersSigner;
  let user1: HardhatEthersSigner;
  let user2: HardhatEthersSigner;

  const CLASS_A = 0;
  const CLASS_B = 1;
  const CLASS_C = 2;
  const CLASS_D = 3;
  const CLASS_E = 4;

  const MIN_STAKE_A = ethers.parseEther("100");
  const MIN_STAKE_B = ethers.parseEther("500");
  const MIN_STAKE_C = ethers.parseEther("2000");
  const MIN_STAKE_D = ethers.parseEther("8000");
  const MIN_STAKE_E = ethers.parseEther("30000");

  const UNSTAKE_COOLDOWN = 7 * 24 * 60 * 60; // 7 days in seconds

  async function deployFixture() {
    const [dep, u1, u2] = await ethers.getSigners();

    const ATATokenFactory = await ethers.getContractFactory("ATAToken");
    const ata = await ATATokenFactory.deploy();

    const AgentNFTFactory = await ethers.getContractFactory("AgentNFT");
    const nft = await AgentNFTFactory.deploy();

    const WrappedATAFactory = await ethers.getContractFactory("WrappedATA");
    const wata = await WrappedATAFactory.deploy(
      await ata.getAddress(),
      await nft.getAddress()
    );

    // Grant minter role on ATAToken to deployer
    const MINTER_ROLE = await ata.MINTER_ROLE();
    await ata.grantRole(MINTER_ROLE, dep.address);

    // Grant minter role on AgentNFT to deployer
    const NFT_MINTER_ROLE = await nft.MINTER_ROLE();
    await nft.grantRole(NFT_MINTER_ROLE, dep.address);

    // Mint ATA to users
    await ata.mint(u1.address, ethers.parseEther("100000"));
    await ata.mint(u2.address, ethers.parseEther("100000"));

    // Mint agent NFTs
    await nft.mintAgent(u1.address, CLASS_A, "ModelA");
    await nft.mintAgent(u1.address, CLASS_B, "ModelB");
    await nft.mintAgent(u2.address, CLASS_C, "ModelC");
    await nft.mintAgent(u2.address, CLASS_E, "ModelE");

    // Approve wATA contract to spend user tokens
    await ata.connect(u1).approve(await wata.getAddress(), ethers.MaxUint256);
    await ata.connect(u2).approve(await wata.getAddress(), ethers.MaxUint256);

    return { ata, nft, wata, dep, u1, u2 };
  }

  beforeEach(async function () {
    const f = await loadFixture(deployFixture);
    ataToken = f.ata;
    agentNFT = f.nft;
    wrappedATA = f.wata;
    deployer = f.dep;
    user1 = f.u1;
    user2 = f.u2;
  });

  describe("Deployment", function () {
    it("should set correct token name and symbol", async function () {
      expect(await wrappedATA.name()).to.equal("Wrapped ATA");
      expect(await wrappedATA.symbol()).to.equal("wATA");
    });

    it("should set correct immutable references", async function () {
      expect(await wrappedATA.ataToken()).to.equal(await ataToken.getAddress());
      expect(await wrappedATA.agentNFT()).to.equal(await agentNFT.getAddress());
    });

    it("should set class minimum stakes", async function () {
      expect(await wrappedATA.classMinimumStake(CLASS_A)).to.equal(MIN_STAKE_A);
      expect(await wrappedATA.classMinimumStake(CLASS_B)).to.equal(MIN_STAKE_B);
      expect(await wrappedATA.classMinimumStake(CLASS_C)).to.equal(MIN_STAKE_C);
      expect(await wrappedATA.classMinimumStake(CLASS_D)).to.equal(MIN_STAKE_D);
      expect(await wrappedATA.classMinimumStake(CLASS_E)).to.equal(MIN_STAKE_E);
    });

    it("should revert with zero ATA address", async function () {
      const Factory = await ethers.getContractFactory("WrappedATA");
      await expect(
        Factory.deploy(ethers.ZeroAddress, await agentNFT.getAddress())
      ).to.be.revertedWith("zero ATA address");
    });

    it("should revert with zero NFT address", async function () {
      const Factory = await ethers.getContractFactory("WrappedATA");
      await expect(
        Factory.deploy(await ataToken.getAddress(), ethers.ZeroAddress)
      ).to.be.revertedWith("zero NFT address");
    });

    it("should grant admin and pauser roles to deployer", async function () {
      const ADMIN = await wrappedATA.DEFAULT_ADMIN_ROLE();
      const PAUSER = await wrappedATA.PAUSER_ROLE();
      expect(await wrappedATA.hasRole(ADMIN, deployer.address)).to.be.true;
      expect(await wrappedATA.hasRole(PAUSER, deployer.address)).to.be.true;
    });
  });

  describe("Staking", function () {
    it("should stake ATA and mint wATA 1:1", async function () {
      await wrappedATA.connect(user1).stake(1, MIN_STAKE_A);

      expect(await wrappedATA.balanceOf(user1.address)).to.equal(MIN_STAKE_A);
      expect(await wrappedATA.stakedPerNFT(1)).to.equal(MIN_STAKE_A);
      expect(await wrappedATA.totalStaked()).to.equal(MIN_STAKE_A);
      expect(await wrappedATA.nftStaker(1)).to.equal(user1.address);
    });

    it("should emit Staked event", async function () {
      await expect(wrappedATA.connect(user1).stake(1, MIN_STAKE_A))
        .to.emit(wrappedATA, "Staked")
        .withArgs(1, user1.address, MIN_STAKE_A);
    });

    it("should allow staking more on the same NFT", async function () {
      await wrappedATA.connect(user1).stake(1, MIN_STAKE_A);
      const extra = ethers.parseEther("50");
      await wrappedATA.connect(user1).stake(1, extra);

      expect(await wrappedATA.stakedPerNFT(1)).to.equal(MIN_STAKE_A + extra);
      expect(await wrappedATA.balanceOf(user1.address)).to.equal(MIN_STAKE_A + extra);
    });

    it("should transfer ATA from staker to contract", async function () {
      const balBefore = await ataToken.balanceOf(user1.address);
      await wrappedATA.connect(user1).stake(1, MIN_STAKE_A);
      const balAfter = await ataToken.balanceOf(user1.address);

      expect(balBefore - balAfter).to.equal(MIN_STAKE_A);
      expect(await ataToken.balanceOf(await wrappedATA.getAddress())).to.equal(MIN_STAKE_A);
    });

    it("should revert if not NFT owner", async function () {
      await expect(
        wrappedATA.connect(user2).stake(1, MIN_STAKE_A)
      ).to.be.revertedWithCustomError(wrappedATA, "NotNFTOwner");
    });

    it("should revert with zero amount", async function () {
      await expect(
        wrappedATA.connect(user1).stake(1, 0)
      ).to.be.revertedWithCustomError(wrappedATA, "ZeroAmount");
    });

    it("should revert if below class minimum", async function () {
      const tooLow = ethers.parseEther("50");
      await expect(
        wrappedATA.connect(user1).stake(1, tooLow)
      ).to.be.revertedWithCustomError(wrappedATA, "BelowClassMinimum");
    });

    it("should revert if another user already staked on NFT", async function () {
      // user1 stakes on NFT 1
      await wrappedATA.connect(user1).stake(1, MIN_STAKE_A);

      // Transfer NFT to user2
      await agentNFT.connect(user1).transferFrom(user1.address, user2.address, 1);

      // user2 tries to stake on NFT 1 which already has user1 as staker
      await expect(
        wrappedATA.connect(user2).stake(1, MIN_STAKE_A)
      ).to.be.revertedWithCustomError(wrappedATA, "AlreadyStakedByOther");
    });

    it("should enforce class B minimum", async function () {
      await expect(
        wrappedATA.connect(user1).stake(2, ethers.parseEther("100"))
      ).to.be.revertedWithCustomError(wrappedATA, "BelowClassMinimum");

      await wrappedATA.connect(user1).stake(2, MIN_STAKE_B);
      expect(await wrappedATA.stakedPerNFT(2)).to.equal(MIN_STAKE_B);
    });

    it("should revert when paused", async function () {
      await wrappedATA.pause();
      await expect(
        wrappedATA.connect(user1).stake(1, MIN_STAKE_A)
      ).to.be.revertedWithCustomError(wrappedATA, "EnforcedPause");
    });
  });

  describe("Unstake Request", function () {
    beforeEach(async function () {
      await wrappedATA.connect(user1).stake(1, MIN_STAKE_A);
    });

    it("should request unstake and emit event", async function () {
      await expect(wrappedATA.connect(user1).requestUnstake(1, MIN_STAKE_A))
        .to.emit(wrappedATA, "UnstakeRequested")
        .withArgs(1, user1.address, MIN_STAKE_A);

      expect(await wrappedATA.unstakeRequestedAt(1)).to.be.gt(0);
      expect(await wrappedATA.unstakeRequestedAmount(1)).to.equal(MIN_STAKE_A);
    });

    it("should revert if not the staker", async function () {
      await expect(
        wrappedATA.connect(user2).requestUnstake(1, MIN_STAKE_A)
      ).to.be.revertedWithCustomError(wrappedATA, "NotNFTOwner");
    });

    it("should revert with zero amount", async function () {
      await expect(
        wrappedATA.connect(user1).requestUnstake(1, 0)
      ).to.be.revertedWithCustomError(wrappedATA, "ZeroAmount");
    });

    it("should revert if requesting more than staked", async function () {
      await expect(
        wrappedATA.connect(user1).requestUnstake(1, ethers.parseEther("200"))
      ).to.be.revertedWithCustomError(wrappedATA, "InsufficientStake");
    });

    it("should revert if unstake already pending", async function () {
      await wrappedATA.connect(user1).requestUnstake(1, ethers.parseEther("50"));
      await expect(
        wrappedATA.connect(user1).requestUnstake(1, ethers.parseEther("50"))
      ).to.be.revertedWithCustomError(wrappedATA, "UnstakeAlreadyPending");
    });
  });

  describe("Complete Unstake", function () {
    beforeEach(async function () {
      await wrappedATA.connect(user1).stake(1, MIN_STAKE_A);
      await wrappedATA.connect(user1).requestUnstake(1, MIN_STAKE_A);
    });

    it("should complete unstake after cooldown", async function () {
      await time.increase(UNSTAKE_COOLDOWN);

      const ataBefore = await ataToken.balanceOf(user1.address);
      await wrappedATA.connect(user1).completeUnstake(1);
      const ataAfter = await ataToken.balanceOf(user1.address);

      expect(ataAfter - ataBefore).to.equal(MIN_STAKE_A);
      expect(await wrappedATA.balanceOf(user1.address)).to.equal(0);
      expect(await wrappedATA.stakedPerNFT(1)).to.equal(0);
      expect(await wrappedATA.totalStaked()).to.equal(0);
      expect(await wrappedATA.nftStaker(1)).to.equal(ethers.ZeroAddress);
    });

    it("should emit Unstaked event", async function () {
      await time.increase(UNSTAKE_COOLDOWN);
      await expect(wrappedATA.connect(user1).completeUnstake(1))
        .to.emit(wrappedATA, "Unstaked")
        .withArgs(1, user1.address, MIN_STAKE_A);
    });

    it("should revert before cooldown expires", async function () {
      await time.increase(UNSTAKE_COOLDOWN - 10);
      await expect(
        wrappedATA.connect(user1).completeUnstake(1)
      ).to.be.revertedWithCustomError(wrappedATA, "CooldownNotElapsed");
    });

    it("should revert if not the staker", async function () {
      await time.increase(UNSTAKE_COOLDOWN);
      await expect(
        wrappedATA.connect(user2).completeUnstake(1)
      ).to.be.revertedWithCustomError(wrappedATA, "NotNFTOwner");
    });

    it("should revert if no unstake pending", async function () {
      await time.increase(UNSTAKE_COOLDOWN);
      await wrappedATA.connect(user1).completeUnstake(1);

      // Try again with no pending
      await expect(
        wrappedATA.connect(user1).completeUnstake(1)
      ).to.be.revertedWithCustomError(wrappedATA, "NotNFTOwner");
    });

    it("should clear unstake request state after completion", async function () {
      await time.increase(UNSTAKE_COOLDOWN);
      await wrappedATA.connect(user1).completeUnstake(1);

      expect(await wrappedATA.unstakeRequestedAt(1)).to.equal(0);
      expect(await wrappedATA.unstakeRequestedAmount(1)).to.equal(0);
    });

    it("should partial unstake keeping staker if remaining > 0", async function () {
      // Stake more first so we have 200 ATA staked
      await wrappedATA.connect(user1).cancelUnstake(1);
      const extra = ethers.parseEther("100");
      await wrappedATA.connect(user1).stake(1, extra);

      // Request unstake for only 100
      await wrappedATA.connect(user1).requestUnstake(1, MIN_STAKE_A);
      await time.increase(UNSTAKE_COOLDOWN);
      await wrappedATA.connect(user1).completeUnstake(1);

      expect(await wrappedATA.stakedPerNFT(1)).to.equal(extra);
      expect(await wrappedATA.nftStaker(1)).to.equal(user1.address);
    });
  });

  describe("Cancel Unstake", function () {
    beforeEach(async function () {
      await wrappedATA.connect(user1).stake(1, MIN_STAKE_A);
      await wrappedATA.connect(user1).requestUnstake(1, MIN_STAKE_A);
    });

    it("should cancel unstake and emit event", async function () {
      await expect(wrappedATA.connect(user1).cancelUnstake(1))
        .to.emit(wrappedATA, "UnstakeCancelled")
        .withArgs(1, user1.address);

      expect(await wrappedATA.unstakeRequestedAt(1)).to.equal(0);
      expect(await wrappedATA.unstakeRequestedAmount(1)).to.equal(0);
    });

    it("should revert if not the staker", async function () {
      await expect(
        wrappedATA.connect(user2).cancelUnstake(1)
      ).to.be.revertedWithCustomError(wrappedATA, "NotNFTOwner");
    });

    it("should revert if no unstake pending", async function () {
      await wrappedATA.connect(user1).cancelUnstake(1);
      await expect(
        wrappedATA.connect(user1).cancelUnstake(1)
      ).to.be.revertedWithCustomError(wrappedATA, "NoUnstakePending");
    });
  });

  describe("Soulbound Transfer Restriction", function () {
    it("should prevent wATA transfers between users", async function () {
      await wrappedATA.connect(user1).stake(1, MIN_STAKE_A);
      await expect(
        wrappedATA.connect(user1).transfer(user2.address, MIN_STAKE_A)
      ).to.be.revertedWithCustomError(wrappedATA, "TransferDisabled");
    });

    it("should prevent transferFrom between users", async function () {
      await wrappedATA.connect(user1).stake(1, MIN_STAKE_A);
      await wrappedATA.connect(user1).approve(user2.address, MIN_STAKE_A);
      await expect(
        wrappedATA.connect(user2).transferFrom(user1.address, user2.address, MIN_STAKE_A)
      ).to.be.revertedWithCustomError(wrappedATA, "TransferDisabled");
    });
  });

  describe("View Functions", function () {
    it("should return correct stake info", async function () {
      await wrappedATA.connect(user1).stake(1, MIN_STAKE_A);

      const info = await wrappedATA.getStakeInfo(1);
      expect(info.staker).to.equal(user1.address);
      expect(info.amount).to.equal(MIN_STAKE_A);
      expect(info.pendingUnstake).to.be.false;
      expect(info.unstakeTime).to.equal(0);
    });

    it("should return pending unstake info", async function () {
      await wrappedATA.connect(user1).stake(1, MIN_STAKE_A);
      await wrappedATA.connect(user1).requestUnstake(1, MIN_STAKE_A);

      const info = await wrappedATA.getStakeInfo(1);
      expect(info.pendingUnstake).to.be.true;
      expect(info.unstakeTime).to.be.gt(0);
    });

    it("should check meetsClassMinimum correctly", async function () {
      // Before staking
      // NFT 1 doesn't exist in context yet for meetsClassMinimum, but it was minted
      expect(await wrappedATA.meetsClassMinimum(1)).to.be.false;

      await wrappedATA.connect(user1).stake(1, MIN_STAKE_A);
      expect(await wrappedATA.meetsClassMinimum(1)).to.be.true;
    });
  });

  describe("Pause", function () {
    it("should allow pauser to pause and unpause", async function () {
      await wrappedATA.pause();
      expect(await wrappedATA.paused()).to.be.true;

      await wrappedATA.unpause();
      expect(await wrappedATA.paused()).to.be.false;
    });

    it("should revert pause from non-pauser", async function () {
      await expect(
        wrappedATA.connect(user1).pause()
      ).to.be.revertedWithCustomError(wrappedATA, "AccessControlUnauthorizedAccount");
    });
  });
});
