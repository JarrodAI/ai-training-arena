import { expect } from "chai";
import { ethers } from "hardhat";
import { FounderRevenue } from "../typechain-types";
import { SignerWithAddress } from "@nomicfoundation/hardhat-ethers/signers";
import { time } from "@nomicfoundation/hardhat-network-helpers";

describe("FounderRevenue", function () {
  let founderRevenue: FounderRevenue;
  let founder1: SignerWithAddress;
  let founder2: SignerWithAddress;
  let other: SignerWithAddress;
  let feeSource: SignerWithAddress;

  beforeEach(async function () {
    [founder1, founder2, other, feeSource] = await ethers.getSigners();
    const factory = await ethers.getContractFactory("FounderRevenue");
    founderRevenue = await factory.deploy(founder1.address, founder2.address);
    await founderRevenue.waitForDeployment();
  });

  describe("Fee reception", function () {
    it("should accumulate battle fees", async function () {
      const amount = ethers.parseEther("1");
      await founderRevenue.connect(feeSource).receiveBattleFees({ value: amount });
      expect(await founderRevenue.battleFeesCollected()).to.equal(amount);
    });

    it("should accumulate marketplace fees", async function () {
      const amount = ethers.parseEther("2");
      await founderRevenue.connect(feeSource).receiveMarketplaceFees({ value: amount });
      expect(await founderRevenue.marketplaceFeesCollected()).to.equal(amount);
    });

    it("should accumulate data fees", async function () {
      const amount = ethers.parseEther("0.5");
      await founderRevenue.connect(feeSource).receiveDataFees({ value: amount });
      expect(await founderRevenue.dataFeesCollected()).to.equal(amount);
    });

    it("should emit FeeReceived event", async function () {
      const amount = ethers.parseEther("1");
      await expect(
        founderRevenue.connect(feeSource).receiveBattleFees({ value: amount })
      ).to.emit(founderRevenue, "FeeReceived");
    });
  });

  describe("availableBalance", function () {
    it("should return 0 for non-founders", async function () {
      await founderRevenue.connect(feeSource).receiveBattleFees({ value: ethers.parseEther("10") });
      expect(await founderRevenue.availableBalance(other.address)).to.equal(0n);
    });

    it("should calculate correct available balance for each founder", async function () {
      // battleFeesCollected = 10 ETH
      // battleShare = 10 * 2000/10000 = 2 ETH
      // totalFounderPool = 2 ETH, founderShare = 1 ETH each
      const battleFees = ethers.parseEther("10");
      await founderRevenue.connect(feeSource).receiveBattleFees({ value: battleFees });

      const expectedShare = ethers.parseEther("1"); // 10 * 20% / 2
      expect(await founderRevenue.availableBalance(founder1.address)).to.equal(expectedShare);
      expect(await founderRevenue.availableBalance(founder2.address)).to.equal(expectedShare);
    });

    it("should subtract already withdrawn amount", async function () {
      await founderRevenue.connect(feeSource).receiveBattleFees({ value: ethers.parseEther("10") });
      await founderRevenue.connect(founder1).withdraw();
      expect(await founderRevenue.availableBalance(founder1.address)).to.equal(0n);
    });
  });

  describe("withdraw", function () {
    it("should allow founder1 to withdraw", async function () {
      await founderRevenue.connect(feeSource).receiveBattleFees({ value: ethers.parseEther("10") });
      const balanceBefore = await ethers.provider.getBalance(founder1.address);
      const tx = await founderRevenue.connect(founder1).withdraw();
      const receipt = await tx.wait();
      const gasUsed = receipt!.gasUsed * receipt!.gasPrice;
      const balanceAfter = await ethers.provider.getBalance(founder1.address);
      expect(balanceAfter - balanceBefore + gasUsed).to.equal(ethers.parseEther("1"));
    });

    it("should allow founder2 to withdraw independently", async function () {
      await founderRevenue.connect(feeSource).receiveBattleFees({ value: ethers.parseEther("10") });
      await founderRevenue.connect(founder1).withdraw();
      // founder2 can still withdraw their share
      const available = await founderRevenue.availableBalance(founder2.address);
      expect(available).to.equal(ethers.parseEther("1"));
    });

    it("should revert if no balance available", async function () {
      await expect(founderRevenue.connect(founder1).withdraw()).to.be.revertedWithCustomError(
        founderRevenue, "NoBalanceAvailable"
      );
    });

    it("should revert if called by non-founder", async function () {
      await founderRevenue.connect(feeSource).receiveBattleFees({ value: ethers.parseEther("10") });
      await expect(founderRevenue.connect(other).withdraw()).to.be.revertedWithCustomError(
        founderRevenue, "NotFounder"
      );
    });

    it("should emit Withdrawal event", async function () {
      await founderRevenue.connect(feeSource).receiveBattleFees({ value: ethers.parseEther("10") });
      await expect(founderRevenue.connect(founder1).withdraw())
        .to.emit(founderRevenue, "Withdrawal")
        .withArgs(founder1.address, ethers.parseEther("1"));
    });
  });

  describe("updateFounderAddress", function () {
    it("should allow founder to rotate address", async function () {
      await founderRevenue.connect(feeSource).receiveBattleFees({ value: ethers.parseEther("10") });
      const newFounder1 = other;
      await expect(founderRevenue.connect(founder1).updateFounderAddress(newFounder1.address))
        .to.emit(founderRevenue, "FounderAddressUpdated")
        .withArgs(founder1.address, newFounder1.address);
      expect(await founderRevenue.founder1()).to.equal(newFounder1.address);
    });

    it("should migrate withdrawn balance to new address", async function () {
      await founderRevenue.connect(feeSource).receiveBattleFees({ value: ethers.parseEther("10") });
      await founderRevenue.connect(founder1).withdraw();
      const withdrawnBefore = await founderRevenue.withdrawn(founder1.address);
      await founderRevenue.connect(founder1).updateFounderAddress(other.address);
      expect(await founderRevenue.withdrawn(other.address)).to.equal(withdrawnBefore);
      expect(await founderRevenue.withdrawn(founder1.address)).to.equal(0n);
    });

    it("should revert if called by non-founder", async function () {
      await expect(
        founderRevenue.connect(other).updateFounderAddress(other.address)
      ).to.be.revertedWithCustomError(founderRevenue, "NotFounder");
    });
  });

  describe("transferFoundership (7-day timelock)", function () {
    it("should initiate foundership transfer", async function () {
      await expect(
        founderRevenue.connect(founder1).transferFoundership(other.address, founder2.address)
      ).to.emit(founderRevenue, "FoundershipTransferInitiated");
    });

    it("should revert completion before timelock expires", async function () {
      await founderRevenue.connect(founder1).transferFoundership(other.address, founder2.address);
      await expect(
        founderRevenue.connect(founder1).completeFoundershipTransfer()
      ).to.be.revertedWithCustomError(founderRevenue, "TimelockNotExpired");
    });

    it("should allow completion after 7 days", async function () {
      await founderRevenue.connect(founder1).transferFoundership(other.address, founder2.address);
      await time.increase(7 * 24 * 60 * 60 + 1);
      await expect(founderRevenue.connect(founder1).completeFoundershipTransfer())
        .to.emit(founderRevenue, "FoundershipTransferCompleted")
        .withArgs(other.address, founder2.address);
      expect(await founderRevenue.founder1()).to.equal(other.address);
    });
  });

  describe("multiple fee types combined", function () {
    it("should correctly aggregate all fee types", async function () {
      // 10 ETH battle * 20% = 2 ETH total battle share
      // 5 ETH marketplace * 40% = 2 ETH total marketplace share
      // 10 ETH data * 30% = 3 ETH total data share
      // totalFounderPool = 7 ETH, founderShare = 3.5 ETH each
      await founderRevenue.connect(feeSource).receiveBattleFees({ value: ethers.parseEther("10") });
      await founderRevenue.connect(feeSource).receiveMarketplaceFees({ value: ethers.parseEther("5") });
      await founderRevenue.connect(feeSource).receiveDataFees({ value: ethers.parseEther("10") });

      expect(await founderRevenue.availableBalance(founder1.address)).to.equal(ethers.parseEther("3.5"));
      expect(await founderRevenue.availableBalance(founder2.address)).to.equal(ethers.parseEther("3.5"));
    });
  });
});
