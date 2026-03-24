import { expect } from "chai";
import { ethers } from "hardhat";
import { loadFixture, time } from "@nomicfoundation/hardhat-toolbox/network-helpers";
import { MatchmakingRegistry, AgentNFT } from "../typechain-types";
import { HardhatEthersSigner } from "@nomicfoundation/hardhat-ethers/signers";

describe("MatchmakingRegistry", function () {
  let registry: MatchmakingRegistry;
  let agentNFT: AgentNFT;
  let deployer: HardhatEthersSigner;
  let user1: HardhatEthersSigner;
  let user2: HardhatEthersSigner;
  let user3: HardhatEthersSigner;

  const STALE_THRESHOLD = 10 * 60;
  const FRESHNESS_THRESHOLD = 5 * 60;

  async function deployFixture() {
    const [dep, u1, u2, u3] = await ethers.getSigners();

    const AgentNFTFactory = await ethers.getContractFactory("AgentNFT");
    const nft = await AgentNFTFactory.deploy();

    const RegistryFactory = await ethers.getContractFactory("MatchmakingRegistry");
    const reg = await RegistryFactory.deploy(await nft.getAddress());

    const MINTER_ROLE = await nft.MINTER_ROLE();
    await nft.grantRole(MINTER_ROLE, dep.address);

    await nft.mintAgent(u1.address, 0, "ModelA1");
    await nft.mintAgent(u2.address, 0, "ModelA2");
    await nft.mintAgent(u3.address, 1, "ModelB1");

    return { nft, reg, dep, u1, u2, u3 };
  }

  beforeEach(async function () {
    const f = await loadFixture(deployFixture);
    agentNFT = f.nft;
    registry = f.reg;
    deployer = f.dep;
    user1 = f.u1;
    user2 = f.u2;
    user3 = f.u3;
  });

  describe("Deployment", function () {
    it("should set correct agentNFT", async function () {
      expect(await registry.agentNFT()).to.equal(await agentNFT.getAddress());
    });

    it("should revert with zero NFT address", async function () {
      const Factory = await ethers.getContractFactory("MatchmakingRegistry");
      await expect(Factory.deploy(ethers.ZeroAddress)).to.be.revertedWith(
        "zero NFT address"
      );
    });

    it("should set STALE_THRESHOLD to 10 minutes", async function () {
      expect(await registry.STALE_THRESHOLD()).to.equal(STALE_THRESHOLD);
    });

    it("should set FRESHNESS_THRESHOLD to 5 minutes", async function () {
      expect(await registry.FRESHNESS_THRESHOLD()).to.equal(FRESHNESS_THRESHOLD);
    });
  });

  describe("announceAvailability", function () {
    it("should register a node and emit NodeAvailable", async function () {
      const tx = await registry
        .connect(user1)
        .announceAvailability(1, 1500, "peer-id-user1");

      await expect(tx)
        .to.emit(registry, "NodeAvailable")
        .withArgs(1, user1.address, 1500, "peer-id-user1");

      expect(await registry.isRegistered(1)).to.be.true;

      const node = await registry.nodes(1);
      expect(node.nftId_).to.equal(1);
      expect(node.owner).to.equal(user1.address);
      expect(node.eloRating).to.equal(1500);
      expect(node.status).to.equal(1);
      expect(node.peerId).to.equal("peer-id-user1");
    });

    it("should revert if caller is not NFT owner", async function () {
      await expect(
        registry.connect(user2).announceAvailability(1, 1500, "peer-id")
      ).to.be.revertedWithCustomError(registry, "NotNFTOwner");
    });

    it("should update existing node on re-announce", async function () {
      await registry.connect(user1).announceAvailability(1, 1500, "peer-id-v1");
      await registry.connect(user1).announceAvailability(1, 1600, "peer-id-v2");

      const node = await registry.nodes(1);
      expect(node.eloRating).to.equal(1600);
      expect(node.peerId).to.equal("peer-id-v2");
    });

    it("should add node to correct class array", async function () {
      await registry.connect(user1).announceAvailability(1, 1500, "peer1");
      expect(await registry.getClassNodeCount(0)).to.equal(1);
      expect(await registry.getClassNodeCount(1)).to.equal(0);
    });
  });

  describe("updateStatus", function () {
    beforeEach(async function () {
      await registry.connect(user1).announceAvailability(1, 1500, "peer1");
    });

    it("should update to InBattle and emit NodeBusy", async function () {
      const tx = await registry.connect(user1).updateStatus(1, 2);
      await expect(tx).to.emit(registry, "NodeBusy").withArgs(1);

      const node = await registry.nodes(1);
      expect(node.status).to.equal(2);
    });

    it("should update to Offline and emit NodeOffline", async function () {
      const tx = await registry.connect(user1).updateStatus(1, 0);
      await expect(tx).to.emit(registry, "NodeOffline").withArgs(1);
    });

    it("should update to Available and emit NodeAvailable", async function () {
      await registry.connect(user1).updateStatus(1, 2);
      const tx = await registry.connect(user1).updateStatus(1, 1);
      await expect(tx).to.emit(registry, "NodeAvailable");
    });

    it("should revert if not NFT owner", async function () {
      await expect(
        registry.connect(user2).updateStatus(1, 0)
      ).to.be.revertedWithCustomError(registry, "NotNFTOwner");
    });

    it("should revert if node not registered", async function () {
      await expect(
        registry.connect(user2).updateStatus(2, 0)
      ).to.be.revertedWithCustomError(registry, "NodeNotRegistered");
    });
  });

  describe("getAvailableOpponents", function () {
    beforeEach(async function () {
      await registry.connect(user1).announceAvailability(1, 1500, "peer1");
      await registry.connect(user2).announceAvailability(2, 1600, "peer2");
      await registry.connect(user3).announceAvailability(3, 1500, "peer3");
    });

    it("should return opponents within elo range of same class", async function () {
      const opponents = await registry.getAvailableOpponents(0, 1500, 200);
      expect(opponents.length).to.equal(2);
    });

    it("should filter by agent class", async function () {
      const opponents = await registry.getAvailableOpponents(1, 1500, 200);
      expect(opponents.length).to.equal(1);
      expect(opponents[0]).to.equal(3);
    });

    it("should filter by elo range", async function () {
      const opponents = await registry.getAvailableOpponents(0, 1500, 50);
      expect(opponents.length).to.equal(1);
      expect(opponents[0]).to.equal(1);
    });

    it("should exclude nodes not Available", async function () {
      await registry.connect(user1).updateStatus(1, 2);
      const opponents = await registry.getAvailableOpponents(0, 1500, 200);
      expect(opponents.length).to.equal(1);
      expect(opponents[0]).to.equal(2);
    });

    it("should exclude stale nodes (lastSeen > 5 min)", async function () {
      await time.increase(6 * 60);
      const opponents = await registry.getAvailableOpponents(0, 1500, 200);
      expect(opponents.length).to.equal(0);
    });

    it("should return empty for class with no nodes", async function () {
      const opponents = await registry.getAvailableOpponents(2, 1500, 200);
      expect(opponents.length).to.equal(0);
    });

    it("should return empty when no opponents in elo range", async function () {
      const opponents = await registry.getAvailableOpponents(0, 2000, 100);
      expect(opponents.length).to.equal(0);
    });
  });

  describe("cleanStaleNodes", function () {
    beforeEach(async function () {
      await registry.connect(user1).announceAvailability(1, 1500, "peer1");
      await registry.connect(user2).announceAvailability(2, 1600, "peer2");
    });

    it("should remove nodes older than 10 minutes", async function () {
      await time.increase(STALE_THRESHOLD + 1);

      const tx = await registry.cleanStaleNodes();
      await expect(tx).to.emit(registry, "StaleNodeRemoved").withArgs(1);
      await expect(tx).to.emit(registry, "StaleNodeRemoved").withArgs(2);

      expect(await registry.isRegistered(1)).to.be.false;
      expect(await registry.isRegistered(2)).to.be.false;
      expect(await registry.getClassNodeCount(0)).to.equal(0);
    });

    it("should not remove fresh nodes", async function () {
      await time.increase(5 * 60);

      await registry.cleanStaleNodes();
      expect(await registry.isRegistered(1)).to.be.true;
      expect(await registry.isRegistered(2)).to.be.true;
    });

    it("should selectively remove only stale nodes", async function () {
      await time.increase(STALE_THRESHOLD + 1);
      await registry.connect(user2).announceAvailability(2, 1600, "peer2-fresh");

      await registry.cleanStaleNodes();
      expect(await registry.isRegistered(1)).to.be.false;
      expect(await registry.isRegistered(2)).to.be.true;
    });

    it("should be callable by anyone", async function () {
      await time.increase(STALE_THRESHOLD + 1);
      await expect(registry.connect(user3).cleanStaleNodes()).to.not.be.reverted;
    });

    it("should handle no stale nodes gracefully", async function () {
      await expect(registry.cleanStaleNodes()).to.not.be.reverted;
      expect(await registry.getClassNodeCount(0)).to.equal(2);
    });
  });

  describe("nodes view", function () {
    it("should return node details via nodes()", async function () {
      await registry.connect(user1).announceAvailability(1, 1500, "peer1");
      const node = await registry.nodes(1);
      expect(node.nftId_).to.equal(1);
      expect(node.owner).to.equal(user1.address);
      expect(node.agentClass).to.equal(0);
      expect(node.eloRating).to.equal(1500);
      expect(node.status).to.equal(1);
      expect(node.peerId).to.equal("peer1");
    });

    it("should return defaults for unregistered node", async function () {
      const node = await registry.nodes(999);
      expect(node.owner).to.equal(ethers.ZeroAddress);
      expect(node.eloRating).to.equal(0);
    });
  });
});
