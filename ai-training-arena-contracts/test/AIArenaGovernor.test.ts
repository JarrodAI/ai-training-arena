import { expect } from "chai";
import { ethers } from "hardhat";
import { AIArenaGovernor, ATAToken, AgentNFT } from "../typechain-types";
import { SignerWithAddress } from "@nomicfoundation/hardhat-ethers/signers";
import { time } from "@nomicfoundation/hardhat-network-helpers";

describe("AIArenaGovernor", function () {
  let governor: AIArenaGovernor;
  let ataToken: ATAToken;
  let agentNFT: AgentNFT;
  let deployer: SignerWithAddress;
  let member1: SignerWithAddress;
  let member2: SignerWithAddress;
  let member3: SignerWithAddress;
  let member4: SignerWithAddress;
  let member5: SignerWithAddress;
  let voter1: SignerWithAddress;
  let voter2: SignerWithAddress;
  let stakingStub: string; // address of a mock staking contract

  beforeEach(async function () {
    [deployer, member1, member2, member3, member4, member5, voter1, voter2] =
      await ethers.getSigners();

    // Deploy ATAToken (used as governance token — needs ERC20Votes)
    // ATAToken inherits ERC20 but not ERC20Votes — use a simple ERC20Votes mock
    // For testing purposes we deploy ATAToken and use its address
    const ataFactory = await ethers.getContractFactory("ATAToken");
    ataToken = await ataFactory.deploy();
    await ataToken.waitForDeployment();

    const agentNFTFactory = await ethers.getContractFactory("AgentNFT");
    agentNFT = await agentNFTFactory.deploy();
    await agentNFTFactory.deploy();
    agentNFT = await agentNFTFactory.deploy();
    await agentNFT.waitForDeployment();

    // Deploy a TimelockController with 0 delay for testing
    const timelockFactory = await ethers.getContractFactory(
      "@openzeppelin/contracts/governance/TimelockController.sol:TimelockController"
    );
    const timelock = await timelockFactory.deploy(
      0, // minDelay = 0 for tests
      [deployer.address], // proposers
      [deployer.address], // executors
      deployer.address    // admin
    );
    await timelock.waitForDeployment();

    // Use deployer address as stub staking contract (stakedBalance will return 0 via staticcall)
    stakingStub = deployer.address;

    const multiSigMembers: [string, string, string, string, string] = [
      member1.address,
      member2.address,
      member3.address,
      member4.address,
      member5.address,
    ];

    const govFactory = await ethers.getContractFactory("AIArenaGovernor");
    governor = await govFactory.deploy(
      await ataToken.getAddress(),
      await timelock.getAddress(),
      await agentNFT.getAddress(),
      stakingStub,
      multiSigMembers
    );
    await governor.waitForDeployment();
  });

  describe("Deployment", function () {
    it("should deploy with correct name", async function () {
      expect(await governor.name()).to.equal("AIArenaGovernor");
    });

    it("should have correct quorum", async function () {
      expect(await governor.quorum(0)).to.equal(ethers.parseEther("10000000"));
    });

    it("should have correct proposal threshold", async function () {
      expect(await governor.proposalThreshold()).to.equal(ethers.parseEther("100000"));
    });

    it("should have correct voting delay", async function () {
      expect(await governor.votingDelay()).to.equal(43200n);
    });

    it("should have correct voting period", async function () {
      expect(await governor.votingPeriod()).to.equal(302400n);
    });
  });

  describe("Multi-sig members", function () {
    it("should register all 5 multi-sig members", async function () {
      expect(await governor.isMultiSigMember(member1.address)).to.be.true;
      expect(await governor.isMultiSigMember(member2.address)).to.be.true;
      expect(await governor.isMultiSigMember(member3.address)).to.be.true;
      expect(await governor.isMultiSigMember(member4.address)).to.be.true;
      expect(await governor.isMultiSigMember(member5.address)).to.be.true;
    });

    it("should not register non-members", async function () {
      expect(await governor.isMultiSigMember(voter1.address)).to.be.false;
    });
  });

  describe("confirmProposal", function () {
    it("should allow multi-sig member to confirm a proposal", async function () {
      const proposalId = 12345n;
      await expect(governor.connect(member1).confirmProposal(proposalId))
        .to.emit(governor, "ProposalConfirmed")
        .withArgs(proposalId, member1.address, 1n);
      expect(await governor.confirmationCount(proposalId)).to.equal(1n);
    });

    it("should accumulate confirmations from different members", async function () {
      const proposalId = 99999n;
      await governor.connect(member1).confirmProposal(proposalId);
      await governor.connect(member2).confirmProposal(proposalId);
      await governor.connect(member3).confirmProposal(proposalId);
      expect(await governor.confirmationCount(proposalId)).to.equal(3n);
    });

    it("should revert double confirmation", async function () {
      const proposalId = 11111n;
      await governor.connect(member1).confirmProposal(proposalId);
      await expect(
        governor.connect(member1).confirmProposal(proposalId)
      ).to.be.revertedWithCustomError(governor, "AlreadyConfirmed");
    });

    it("should revert from non-member", async function () {
      await expect(
        governor.connect(voter1).confirmProposal(1n)
      ).to.be.revertedWithCustomError(governor, "NotMultiSigMember");
    });
  });

  describe("emergencyPause", function () {
    it("should allow multi-sig member to emit emergency pause", async function () {
      const target = voter1.address;
      await expect(governor.connect(member1).emergencyPause(target))
        .to.emit(governor, "EmergencyPaused")
        .withArgs(target, member1.address);
    });

    it("should revert from non-member", async function () {
      await expect(
        governor.connect(voter1).emergencyPause(voter2.address)
      ).to.be.revertedWithCustomError(governor, "NotMultiSigMember");
    });
  });

  describe("Constants", function () {
    it("should have correct MULTISIG_CONFIRMATIONS_REQUIRED", async function () {
      expect(await governor.MULTISIG_CONFIRMATIONS_REQUIRED()).to.equal(3n);
    });

    it("should have correct QUORUM_ATA", async function () {
      expect(await governor.QUORUM_ATA()).to.equal(ethers.parseEther("10000000"));
    });

    it("should have correct PROPOSAL_THRESHOLD_ATA", async function () {
      expect(await governor.PROPOSAL_THRESHOLD_ATA()).to.equal(ethers.parseEther("100000"));
    });
  });
});
