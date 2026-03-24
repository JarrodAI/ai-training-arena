import { expect } from "chai";
import { ethers } from "hardhat";
import { loadFixture, time } from "@nomicfoundation/hardhat-toolbox/network-helpers";
import { ATAToken, AgentNFT, AITrainingArena, BattleVerifier } from "../typechain-types";
import { HardhatEthersSigner } from "@nomicfoundation/hardhat-ethers/signers";

describe("BattleVerifier", function () {
  let ataToken: ATAToken;
  let agentNFT: AgentNFT;
  let arena: AITrainingArena;
  let verifier: BattleVerifier;
  let deployer: HardhatEthersSigner;
  let user1: HardhatEthersSigner;
  let user2: HardhatEthersSigner;
  let oracle: HardhatEthersSigner;

  const CHALLENGE_WINDOW = 3600;

  async function deployFixture() {
    const [dep, u1, u2, orc] = await ethers.getSigners();

    const ATATokenFactory = await ethers.getContractFactory("ATAToken");
    const ata = await ATATokenFactory.deploy();

    const AgentNFTFactory = await ethers.getContractFactory("AgentNFT");
    const nft = await AgentNFTFactory.deploy();

    const ArenaFactory = await ethers.getContractFactory("AITrainingArena");
    const ar = await ArenaFactory.deploy(
      await ata.getAddress(),
      await nft.getAddress()
    );

    const VerifierFactory = await ethers.getContractFactory("BattleVerifier");
    const ver = await VerifierFactory.deploy(
      await nft.getAddress(),
      await ar.getAddress()
    );

    const MINTER_ROLE = await ata.MINTER_ROLE();
    await ata.grantRole(MINTER_ROLE, dep.address);

    const NFT_MINTER_ROLE = await nft.MINTER_ROLE();
    await nft.grantRole(NFT_MINTER_ROLE, dep.address);

    const ORACLE_ROLE = await ver.ORACLE_ROLE();
    await ver.grantRole(ORACLE_ROLE, orc.address);

    await nft.mintAgent(u1.address, 0, "ModelA");
    await nft.mintAgent(u2.address, 1, "ModelB");

    return { ata, nft, ar, ver, dep, u1, u2, orc };
  }

  beforeEach(async function () {
    const f = await loadFixture(deployFixture);
    ataToken = f.ata;
    agentNFT = f.nft;
    arena = f.ar;
    verifier = f.ver;
    deployer = f.dep;
    user1 = f.u1;
    user2 = f.u2;
    oracle = f.orc;
  });

  describe("Deployment", function () {
    it("should set correct immutable references", async function () {
      expect(await verifier.agentNFT()).to.equal(await agentNFT.getAddress());
      expect(await verifier.arenaContract()).to.equal(await arena.getAddress());
    });

    it("should start with nextBattleId at 1", async function () {
      expect(await verifier.nextBattleId()).to.equal(1);
    });

    it("should revert with zero NFT address", async function () {
      const Factory = await ethers.getContractFactory("BattleVerifier");
      await expect(
        Factory.deploy(ethers.ZeroAddress, await arena.getAddress())
      ).to.be.revertedWith("zero NFT address");
    });

    it("should revert with zero arena address", async function () {
      const Factory = await ethers.getContractFactory("BattleVerifier");
      await expect(
        Factory.deploy(await agentNFT.getAddress(), ethers.ZeroAddress)
      ).to.be.revertedWith("zero arena address");
    });

    it("should grant admin role to deployer", async function () {
      const ADMIN = await verifier.DEFAULT_ADMIN_ROLE();
      expect(await verifier.hasRole(ADMIN, deployer.address)).to.be.true;
    });
  });

  describe("Proof Submission", function () {
    const merkleRoot = ethers.keccak256(ethers.toUtf8Bytes("battle-proof-1"));
    const zkProof = "0x";

    it("should allow proposer to submit proof", async function () {
      await expect(
        verifier.connect(user1).submitProof(1, 2, merkleRoot, zkProof)
      )
        .to.emit(verifier, "ProofSubmitted")
        .withArgs(1, 1, merkleRoot);

      expect(await verifier.nextBattleId()).to.equal(2);
    });

    it("should allow solver to submit proof", async function () {
      await verifier.connect(user1).submitProof(1, 2, merkleRoot, zkProof);

      const solverRoot = ethers.keccak256(ethers.toUtf8Bytes("solver-proof-1"));
      await expect(
        verifier.connect(user2).submitProof(1, 2, solverRoot, zkProof)
      )
        .to.emit(verifier, "ProofSubmitted")
        .withArgs(1, 2, solverRoot);
    });

    it("should revert if caller does not own either NFT", async function () {
      await expect(
        verifier.connect(deployer).submitProof(1, 2, merkleRoot, zkProof)
      ).to.be.revertedWithCustomError(verifier, "NotNFTParticipant");
    });

    it("should revert if same side submits twice", async function () {
      await verifier.connect(user1).submitProof(1, 2, merkleRoot, zkProof);
      await expect(
        verifier.connect(user1).submitProof(1, 2, merkleRoot, zkProof)
      ).to.be.revertedWithCustomError(verifier, "AlreadySubmitted");
    });
  });

  describe("Auto-Verification on Matching Roots", function () {
    const matchingRoot = ethers.keccak256(ethers.toUtf8Bytes("matching-root"));
    const zkProof = "0x";

    it("should verify battle when both sides submit matching roots", async function () {
      await verifier.connect(user1).submitProof(1, 2, matchingRoot, zkProof);

      await expect(
        verifier.connect(user2).submitProof(1, 2, matchingRoot, zkProof)
      ).to.emit(verifier, "BattleVerified").withArgs(1, 1, 2);

      const record = await verifier.getBattleRecord(1);
      expect(record.verified).to.be.true;
      expect(record.settled).to.be.true;
      expect(record.disputed).to.be.false;
    });

    it("should set winner to proposer owner on matching roots", async function () {
      await verifier.connect(user1).submitProof(1, 2, matchingRoot, zkProof);
      await verifier.connect(user2).submitProof(1, 2, matchingRoot, zkProof);

      const record = await verifier.getBattleRecord(1);
      expect(record.winner).to.equal(user1.address);
    });
  });

  describe("Dispute on Mismatched Roots", function () {
    const root1 = ethers.keccak256(ethers.toUtf8Bytes("root-proposer"));
    const root2 = ethers.keccak256(ethers.toUtf8Bytes("root-solver"));
    const zkProof = "0x";

    it("should open dispute when roots mismatch", async function () {
      await verifier.connect(user1).submitProof(1, 2, root1, zkProof);
      await expect(
        verifier.connect(user2).submitProof(1, 2, root2, zkProof)
      ).to.emit(verifier, "DisputeOpened").withArgs(1);

      const record = await verifier.getBattleRecord(1);
      expect(record.disputed).to.be.true;
      expect(record.verified).to.be.false;
      expect(record.settled).to.be.false;
    });

    it("should show as pending dispute", async function () {
      await verifier.connect(user1).submitProof(1, 2, root1, zkProof);
      await verifier.connect(user2).submitProof(1, 2, root2, zkProof);
      expect(await verifier.isPendingDispute(1)).to.be.true;
    });
  });

  describe("Challenge Battle", function () {
    const root = ethers.keccak256(ethers.toUtf8Bytes("root-1"));
    const zkProof = "0x";

    beforeEach(async function () {
      await verifier.connect(user1).submitProof(1, 2, root, zkProof);
    });

    it("should allow participant to challenge within window", async function () {
      await expect(verifier.connect(user1).challengeBattle(1))
        .to.emit(verifier, "DisputeOpened")
        .withArgs(1);
    });

    it("should emit OracleRequested on challenge", async function () {
      await expect(verifier.connect(user1).challengeBattle(1))
        .to.emit(verifier, "OracleRequested")
        .withArgs(1);
    });

    it("should revert challenge from non-participant", async function () {
      await expect(
        verifier.connect(deployer).challengeBattle(1)
      ).to.be.revertedWithCustomError(verifier, "NotNFTParticipant");
    });

    it("should revert challenge after window expires", async function () {
      await time.increase(CHALLENGE_WINDOW + 1);
      await expect(
        verifier.connect(user1).challengeBattle(1)
      ).to.be.revertedWithCustomError(verifier, "ChallengeWindowExpired");
    });

    it("should revert challenge on nonexistent battle", async function () {
      await expect(
        verifier.connect(user1).challengeBattle(99)
      ).to.be.revertedWithCustomError(verifier, "BattleNotFound");
    });
  });

  describe("Oracle Dispute Resolution", function () {
    const root1 = ethers.keccak256(ethers.toUtf8Bytes("root-A"));
    const root2 = ethers.keccak256(ethers.toUtf8Bytes("root-B"));
    const zkProof = "0x";

    beforeEach(async function () {
      await verifier.connect(user1).submitProof(1, 2, root1, zkProof);
      await verifier.connect(user2).submitProof(1, 2, root2, zkProof);
    });

    it("should allow oracle to resolve dispute", async function () {
      await expect(
        verifier.connect(oracle).resolveDispute(1, user2.address)
      )
        .to.emit(verifier, "DisputeResolved")
        .withArgs(1, user2.address);

      const record = await verifier.getBattleRecord(1);
      expect(record.winner).to.equal(user2.address);
      expect(record.settled).to.be.true;
      expect(record.verified).to.be.true;
    });

    it("should revert resolve from non-oracle", async function () {
      await expect(
        verifier.connect(user1).resolveDispute(1, user1.address)
      ).to.be.revertedWithCustomError(verifier, "AccessControlUnauthorizedAccount");
    });

    it("should revert resolve with invalid winner address", async function () {
      await expect(
        verifier.connect(oracle).resolveDispute(1, deployer.address)
      ).to.be.revertedWithCustomError(verifier, "InvalidWinner");
    });

    it("should revert resolve on already settled battle", async function () {
      await verifier.connect(oracle).resolveDispute(1, user1.address);
      await expect(
        verifier.connect(oracle).resolveDispute(1, user2.address)
      ).to.be.revertedWithCustomError(verifier, "BattleAlreadySettled");
    });

    it("should not show as pending dispute after resolution", async function () {
      await verifier.connect(oracle).resolveDispute(1, user1.address);
      expect(await verifier.isPendingDispute(1)).to.be.false;
    });
  });

  describe("View Functions", function () {
    const root = ethers.keccak256(ethers.toUtf8Bytes("view-root"));
    const zkProof = "0x";

    it("should return correct battle record", async function () {
      await verifier.connect(user1).submitProof(1, 2, root, zkProof);

      const record = await verifier.getBattleRecord(1);
      expect(record.proposerNFT).to.equal(1);
      expect(record.solverNFT).to.equal(2);
      expect(record.proposerMerkleRoot).to.equal(root);
      expect(record.submittedAt).to.be.gt(0);
    });

    it("should return false for isPendingDispute on non-disputed battle", async function () {
      await verifier.connect(user1).submitProof(1, 2, root, zkProof);
      expect(await verifier.isPendingDispute(1)).to.be.false;
    });
  });
});
