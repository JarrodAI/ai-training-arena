import { expect } from "chai";
import { ethers } from "hardhat";
import { ATAToken, AgentNFT, AITrainingArena } from "../typechain-types";
import { SignerWithAddress } from "@nomicfoundation/hardhat-ethers/signers";
import { time } from "@nomicfoundation/hardhat-toolbox/network-helpers";

describe("AITrainingArena", function () {
  let ataToken: ATAToken;
  let agentNFT: AgentNFT;
  let arena: AITrainingArena;
  let deployer: SignerWithAddress;
  let operator: SignerWithAddress;
  let daoExecutor: SignerWithAddress;
  let pauser: SignerWithAddress;
  let user1: SignerWithAddress;
  let user2: SignerWithAddress;

  let BATTLE_OPERATOR: string;
  let DAO_EXECUTOR: string;
  let PAUSER_ROLE: string;

  beforeEach(async function () {
    [deployer, operator, daoExecutor, pauser, user1, user2] =
      await ethers.getSigners();

    const tokenFactory = await ethers.getContractFactory("ATAToken");
    ataToken = await tokenFactory.deploy();
    await ataToken.waitForDeployment();

    const nftFactory = await ethers.getContractFactory("AgentNFT");
    agentNFT = await nftFactory.deploy();
    await agentNFT.waitForDeployment();

    const arenaFactory = await ethers.getContractFactory("AITrainingArena");
    arena = await arenaFactory.deploy(
      await ataToken.getAddress(),
      await agentNFT.getAddress()
    );
    await arena.waitForDeployment();

    BATTLE_OPERATOR = await arena.BATTLE_OPERATOR();
    DAO_EXECUTOR = await arena.DAO_EXECUTOR();
    PAUSER_ROLE = await arena.PAUSER_ROLE();

    // Grant arena roles on AgentNFT
    const MINTER_ROLE = await agentNFT.MINTER_ROLE();
    const ELO_UPDATER_ROLE = await agentNFT.ELO_UPDATER_ROLE();
    const BATTLE_OPERATOR_ROLE_NFT = await agentNFT.BATTLE_OPERATOR_ROLE();
    await agentNFT.grantRole(MINTER_ROLE, await arena.getAddress());
    await agentNFT.grantRole(ELO_UPDATER_ROLE, await arena.getAddress());
    await agentNFT.grantRole(BATTLE_OPERATOR_ROLE_NFT, await arena.getAddress());

    // Fund arena with ATA tokens for rewards
    const tokenMinterRole = await ataToken.MINTER_ROLE();
    await ataToken.grantRole(tokenMinterRole, deployer.address);
    await ataToken.connect(deployer).mint(
      await arena.getAddress(),
      ethers.parseEther("1000000")
    );

    // Grant roles on arena
    await arena.grantRole(BATTLE_OPERATOR, operator.address);
    await arena.grantRole(DAO_EXECUTOR, daoExecutor.address);
    await arena.grantRole(PAUSER_ROLE, pauser.address);
  });

  describe("Deployment", function () {
    it("should set ATA token and AgentNFT references", async function () {
      expect(await arena.ataToken()).to.equal(await ataToken.getAddress());
      expect(await arena.agentNFT()).to.equal(await agentNFT.getAddress());
    });

    it("should initialize class multipliers", async function () {
      expect(await arena.classMultiplier(0)).to.equal(100n);
      expect(await arena.classMultiplier(1)).to.equal(120n);
      expect(await arena.classMultiplier(2)).to.equal(150n);
      expect(await arena.classMultiplier(3)).to.equal(200n);
      expect(await arena.classMultiplier(4)).to.equal(300n);
    });

    it("should initialize mint prices", async function () {
      expect(await arena.mintPrice(0)).to.equal(ethers.parseEther("10"));
      expect(await arena.mintPrice(1)).to.equal(ethers.parseEther("50"));
      expect(await arena.mintPrice(2)).to.equal(ethers.parseEther("200"));
      expect(await arena.mintPrice(3)).to.equal(ethers.parseEther("800"));
      expect(await arena.mintPrice(4)).to.equal(ethers.parseEther("3000"));
    });

    it("should set daily reward pool", async function () {
      expect(await arena.dailyRewardPool()).to.equal(ethers.parseEther("8219"));
    });

    it("should revert with zero ATA address", async function () {
      const factory = await ethers.getContractFactory("AITrainingArena");
      await expect(
        factory.deploy(ethers.ZeroAddress, await agentNFT.getAddress())
      ).to.be.revertedWith("zero ATA address");
    });

    it("should revert with zero NFT address", async function () {
      const factory = await ethers.getContractFactory("AITrainingArena");
      await expect(
        factory.deploy(await ataToken.getAddress(), ethers.ZeroAddress)
      ).to.be.revertedWith("zero NFT address");
    });

    it("should grant DEFAULT_ADMIN_ROLE to deployer", async function () {
      const DEFAULT_ADMIN_ROLE = await arena.DEFAULT_ADMIN_ROLE();
      expect(await arena.hasRole(DEFAULT_ADMIN_ROLE, deployer.address)).to.be
        .true;
    });

    it("should grant BATTLE_OPERATOR to deployer", async function () {
      expect(await arena.hasRole(BATTLE_OPERATOR, deployer.address)).to.be
        .true;
    });
  });

  describe("mintAgent", function () {
    it("should mint class A agent with correct price", async function () {
      const price = ethers.parseEther("10");
      await expect(
        arena.connect(user1).mintAgent(0, "GPT-4", { value: price })
      )
        .to.emit(arena, "AgentPurchased")
        .withArgs(1n, user1.address, 0, price);

      expect(await agentNFT.ownerOf(1)).to.equal(user1.address);
      expect(await agentNFT.getAgentClass(1)).to.equal(0);
      expect(await agentNFT.getAgentElo(1)).to.equal(1500n);
    });

    it("should mint class B agent", async function () {
      const price = ethers.parseEther("50");
      await arena.connect(user1).mintAgent(1, "Claude-3", { value: price });
      expect(await agentNFT.getAgentClass(1)).to.equal(1);
    });

    it("should mint class E agent", async function () {
      const price = ethers.parseEther("3000");
      await arena.connect(user1).mintAgent(4, "Llama-405B", { value: price });
      expect(await agentNFT.getAgentClass(1)).to.equal(4);
    });

    it("should revert with incorrect price", async function () {
      await expect(
        arena.connect(user1).mintAgent(0, "GPT-4", {
          value: ethers.parseEther("5"),
        })
      ).to.be.revertedWithCustomError(arena, "InvalidPrice");
    });

    it("should revert with zero value", async function () {
      await expect(
        arena.connect(user1).mintAgent(0, "GPT-4", { value: 0 })
      ).to.be.revertedWithCustomError(arena, "InvalidPrice");
    });

    it("should revert when paused", async function () {
      await arena.connect(pauser).pause();
      await expect(
        arena.connect(user1).mintAgent(0, "GPT-4", {
          value: ethers.parseEther("10"),
        })
      ).to.be.revertedWithCustomError(arena, "EnforcedPause");
    });
  });

  describe("recordBattle", function () {
    beforeEach(async function () {
      const price = ethers.parseEther("10");
      await arena.connect(user1).mintAgent(0, "GPT-4", { value: price });
      await arena.connect(user2).mintAgent(0, "Claude-3", { value: price });
    });

    it("should record a battle and emit event", async function () {
      await expect(
        arena
          .connect(operator)
          .recordBattle(1, 2, user1.address, 85, 70, "QmTest123")
      )
        .to.emit(arena, "BattleCompleted")
        .withArgs(1n, 2n, user1.address, 85, 70, "QmTest123");
    });

    it("should update Elo ratings (equal start)", async function () {
      await arena
        .connect(operator)
        .recordBattle(1, 2, user1.address, 85, 70, "QmTest");

      // K=40 for new agents, equal Elo => expected=500/1000
      // gain = 40*(1000-500)/1000 = 20, loss = 40*500/1000 = 20
      expect(await agentNFT.getAgentElo(1)).to.equal(1520n);
      expect(await agentNFT.getAgentElo(2)).to.equal(1480n);
    });

    it("should increment battle counts", async function () {
      await arena
        .connect(operator)
        .recordBattle(1, 2, user1.address, 85, 70, "QmTest");

      expect(await agentNFT.getBattleCount(1)).to.equal(1n);
      expect(await agentNFT.getBattleCount(2)).to.equal(1n);
    });

    it("should distribute 90% winner / 10% loser rewards", async function () {
      await arena
        .connect(operator)
        .recordBattle(1, 2, user1.address, 85, 70, "QmTest");

      const winnerRewards = await arena.getPendingRewards(user1.address);
      const loserRewards = await arena.getPendingRewards(user2.address);

      expect(winnerRewards).to.be.gt(0n);
      expect(loserRewards).to.be.gt(0n);
      expect(winnerRewards).to.equal(loserRewards * 9n);
    });

    it("should burn 2% of rewards", async function () {
      const burnBefore = await arena.totalBurned();
      await arena
        .connect(operator)
        .recordBattle(1, 2, user1.address, 85, 70, "QmTest");
      const burnAfter = await arena.totalBurned();
      expect(burnAfter).to.be.gt(burnBefore);
    });

    it("should revert if same NFT on both sides", async function () {
      await expect(
        arena
          .connect(operator)
          .recordBattle(1, 1, user1.address, 85, 70, "QmTest")
      ).to.be.revertedWithCustomError(arena, "SameAgent");
    });

    it("should revert if agent is not active", async function () {
      await agentNFT.connect(user1).setActive(1, false);
      await expect(
        arena
          .connect(operator)
          .recordBattle(1, 2, user1.address, 85, 70, "QmTest")
      ).to.be.revertedWithCustomError(arena, "AgentNotActive");
    });

    it("should revert if winner is not a participant owner", async function () {
      await expect(
        arena
          .connect(operator)
          .recordBattle(1, 2, deployer.address, 85, 70, "QmTest")
      ).to.be.revertedWith("invalid winner");
    });

    it("should revert without BATTLE_OPERATOR role", async function () {
      await expect(
        arena
          .connect(user1)
          .recordBattle(1, 2, user1.address, 85, 70, "QmTest")
      ).to.be.revertedWithCustomError(
        arena,
        "AccessControlUnauthorizedAccount"
      );
    });

    it("should revert when paused", async function () {
      await arena.connect(pauser).pause();
      await expect(
        arena
          .connect(operator)
          .recordBattle(1, 2, user1.address, 85, 70, "QmTest")
      ).to.be.revertedWithCustomError(arena, "EnforcedPause");
    });

    it("should update class stats", async function () {
      await arena
        .connect(operator)
        .recordBattle(1, 2, user1.address, 85, 70, "QmTest");

      const [battles, rewards] = await arena.getClassStats(0);
      expect(battles).to.equal(1n);
      expect(rewards).to.be.gt(0n);
    });

    it("should increment totalBattles", async function () {
      await arena
        .connect(operator)
        .recordBattle(1, 2, user1.address, 85, 70, "QmTest");
      expect(await arena.totalBattles()).to.equal(1n);
    });
  });

  describe("Elo Calculation", function () {
    beforeEach(async function () {
      const price = ethers.parseEther("10");
      await arena.connect(user1).mintAgent(0, "Agent1", { value: price });
      await arena.connect(user2).mintAgent(0, "Agent2", { value: price });
    });

    it("should use K=40 for new agents", async function () {
      await arena
        .connect(operator)
        .recordBattle(1, 2, user1.address, 100, 50, "Qm1");
      expect(await agentNFT.getAgentElo(1)).to.equal(1520n);
      expect(await agentNFT.getAgentElo(2)).to.equal(1480n);
    });

    it("should handle unequal Elo", async function () {
      for (let i = 0; i < 5; i++) {
        await arena
          .connect(operator)
          .recordBattle(1, 2, user1.address, 100, 50, "Qm");
      }

      const elo1 = await agentNFT.getAgentElo(1);
      const elo2 = await agentNFT.getAgentElo(2);
      expect(elo1).to.be.gt(elo2);

      // Underdog wins
      await arena
        .connect(operator)
        .recordBattle(1, 2, user2.address, 50, 100, "QmUpset");

      const elo1After = await agentNFT.getAgentElo(1);
      const elo2After = await agentNFT.getAgentElo(2);
      expect(elo2After).to.be.gt(elo2);
      expect(elo1After).to.be.lt(elo1);
    });

    it("should cap eloDiff at +/- 400", async function () {
      const ELO_UPDATER_ROLE = await agentNFT.ELO_UPDATER_ROLE();
      await agentNFT.grantRole(ELO_UPDATER_ROLE, deployer.address);
      await agentNFT.updateElo(1, 2500);
      await agentNFT.updateElo(2, 1000);

      await arena
        .connect(operator)
        .recordBattle(1, 2, user1.address, 100, 50, "QmExtreme");

      // Favorite wins: gain = 0, loss = 0
      expect(await agentNFT.getAgentElo(1)).to.equal(2500n);
      expect(await agentNFT.getAgentElo(2)).to.equal(1000n);
    });
  });

  describe("Staking", function () {
    beforeEach(async function () {
      const price = ethers.parseEther("10");
      await arena.connect(user1).mintAgent(0, "GPT-4", { value: price });

      await ataToken
        .connect(deployer)
        .mint(user1.address, ethers.parseEther("10000"));
      await ataToken
        .connect(user1)
        .approve(await arena.getAddress(), ethers.parseEther("10000"));
    });

    it("should allow NFT owner to stake tokens", async function () {
      const amount = ethers.parseEther("100");
      await expect(arena.connect(user1).stakeTokens(1, amount))
        .to.emit(arena, "AgentStaked")
        .withArgs(1n, user1.address, amount);

      expect(await arena.stakedAmounts(1)).to.equal(amount);
    });

    it("should transfer ATA from user to arena", async function () {
      const amount = ethers.parseEther("100");
      const balBefore = await ataToken.balanceOf(user1.address);
      await arena.connect(user1).stakeTokens(1, amount);
      const balAfter = await ataToken.balanceOf(user1.address);
      expect(balBefore - balAfter).to.equal(amount);
    });

    it("should revert if not NFT owner", async function () {
      await expect(
        arena.connect(user2).stakeTokens(1, ethers.parseEther("100"))
      ).to.be.revertedWithCustomError(arena, "NotNFTOwner");
    });

    it("should revert with zero amount", async function () {
      await expect(
        arena.connect(user1).stakeTokens(1, 0)
      ).to.be.revertedWith("zero amount");
    });

    it("should accumulate staked amounts", async function () {
      await arena.connect(user1).stakeTokens(1, ethers.parseEther("100"));
      await arena.connect(user1).stakeTokens(1, ethers.parseEther("200"));
      expect(await arena.stakedAmounts(1)).to.equal(
        ethers.parseEther("300")
      );
    });

    it("should revert when paused", async function () {
      await arena.connect(pauser).pause();
      await expect(
        arena.connect(user1).stakeTokens(1, ethers.parseEther("100"))
      ).to.be.revertedWithCustomError(arena, "EnforcedPause");
    });
  });

  describe("Unstaking", function () {
    const stakeAmount = ethers.parseEther("500");

    beforeEach(async function () {
      const price = ethers.parseEther("10");
      await arena.connect(user1).mintAgent(0, "GPT-4", { value: price });

      await ataToken
        .connect(deployer)
        .mint(user1.address, ethers.parseEther("10000"));
      await ataToken
        .connect(user1)
        .approve(await arena.getAddress(), ethers.parseEther("10000"));
      await arena.connect(user1).stakeTokens(1, stakeAmount);
    });

    it("should initiate unstake with cooldown on first call", async function () {
      await arena.connect(user1).unstakeTokens(1, stakeAmount);
      expect(await arena.unstakeInitiatedAt(1)).to.be.gt(0n);
      expect(await arena.unstakeRequestedAmount(1)).to.equal(stakeAmount);
      expect(await arena.stakedAmounts(1)).to.equal(stakeAmount);
    });

    it("should revert if cooldown not passed", async function () {
      await arena.connect(user1).unstakeTokens(1, stakeAmount);
      await time.increase(3 * 24 * 60 * 60);
      await expect(
        arena.connect(user1).unstakeTokens(1, stakeAmount)
      ).to.be.revertedWithCustomError(arena, "UnstakeCooldownActive");
    });

    it("should complete unstake after 7-day cooldown", async function () {
      await arena.connect(user1).unstakeTokens(1, stakeAmount);
      await time.increase(7 * 24 * 60 * 60 + 1);

      const balBefore = await ataToken.balanceOf(user1.address);
      await expect(arena.connect(user1).unstakeTokens(1, stakeAmount))
        .to.emit(arena, "AgentUnstaked")
        .withArgs(1n, user1.address, stakeAmount);

      const balAfter = await ataToken.balanceOf(user1.address);
      expect(balAfter - balBefore).to.equal(stakeAmount);
      expect(await arena.stakedAmounts(1)).to.equal(0n);
      expect(await arena.unstakeInitiatedAt(1)).to.equal(0n);
    });

    it("should revert if not NFT owner", async function () {
      await expect(
        arena.connect(user2).unstakeTokens(1, stakeAmount)
      ).to.be.revertedWithCustomError(arena, "NotNFTOwner");
    });

    it("should revert if insufficient stake", async function () {
      await expect(
        arena.connect(user1).unstakeTokens(1, stakeAmount + 1n)
      ).to.be.revertedWithCustomError(arena, "InsufficientStake");
    });

    it("should revert with zero amount", async function () {
      await expect(
        arena.connect(user1).unstakeTokens(1, 0)
      ).to.be.revertedWith("zero amount");
    });
  });

  describe("claimRewards", function () {
    beforeEach(async function () {
      const price = ethers.parseEther("10");
      await arena.connect(user1).mintAgent(0, "GPT-4", { value: price });
      await arena.connect(user2).mintAgent(0, "Claude-3", { value: price });

      await arena
        .connect(operator)
        .recordBattle(1, 2, user1.address, 85, 70, "QmTest");
    });

    it("should allow claiming pending rewards", async function () {
      const pending = await arena.getPendingRewards(user1.address);
      expect(pending).to.be.gt(0n);

      const balBefore = await ataToken.balanceOf(user1.address);
      await expect(arena.claimRewards(user1.address))
        .to.emit(arena, "RewardsClaimed")
        .withArgs(user1.address, pending);

      const balAfter = await ataToken.balanceOf(user1.address);
      expect(balAfter - balBefore).to.equal(pending);
      expect(await arena.getPendingRewards(user1.address)).to.equal(0n);
    });

    it("should allow loser to claim their 10% share", async function () {
      const pending = await arena.getPendingRewards(user2.address);
      expect(pending).to.be.gt(0n);

      await arena.claimRewards(user2.address);
      expect(await arena.getPendingRewards(user2.address)).to.equal(0n);
    });

    it("should revert if no pending rewards", async function () {
      await expect(
        arena.claimRewards(deployer.address)
      ).to.be.revertedWithCustomError(arena, "NoPendingRewards");
    });
  });

  describe("executeBuyback", function () {
    it("should burn ATA tokens to dead address", async function () {
      const amount = ethers.parseEther("1000");
      const burnAddr = "0x000000000000000000000000000000000000dEaD";
      const burnBefore = await ataToken.balanceOf(burnAddr);

      await expect(arena.connect(daoExecutor).executeBuyback(amount))
        .to.emit(arena, "BuybackExecuted")
        .withArgs(amount);

      const burnAfter = await ataToken.balanceOf(burnAddr);
      expect(burnAfter - burnBefore).to.equal(amount);
    });

    it("should update totalBurned", async function () {
      const amount = ethers.parseEther("1000");
      const before = await arena.totalBurned();
      await arena.connect(daoExecutor).executeBuyback(amount);
      expect(await arena.totalBurned()).to.equal(before + amount);
    });

    it("should revert without DAO_EXECUTOR role", async function () {
      await expect(
        arena.connect(user1).executeBuyback(ethers.parseEther("100"))
      ).to.be.revertedWithCustomError(
        arena,
        "AccessControlUnauthorizedAccount"
      );
    });

    it("should revert with zero amount", async function () {
      await expect(
        arena.connect(daoExecutor).executeBuyback(0)
      ).to.be.revertedWith("zero amount");
    });
  });

  describe("setClassMultiplier", function () {
    it("should allow DAO_EXECUTOR to update", async function () {
      await expect(arena.connect(daoExecutor).setClassMultiplier(0, 110))
        .to.emit(arena, "ClassMultiplierUpdated")
        .withArgs(0, 110);

      expect(await arena.classMultiplier(0)).to.equal(110n);
    });

    it("should revert without DAO_EXECUTOR role", async function () {
      await expect(
        arena.connect(user1).setClassMultiplier(0, 110)
      ).to.be.revertedWithCustomError(
        arena,
        "AccessControlUnauthorizedAccount"
      );
    });

    it("should revert with zero multiplier", async function () {
      await expect(
        arena.connect(daoExecutor).setClassMultiplier(0, 0)
      ).to.be.revertedWith("zero multiplier");
    });

    it("should affect future battle rewards", async function () {
      const price = ethers.parseEther("10");
      await arena.connect(user1).mintAgent(0, "A1", { value: price });
      await arena.connect(user2).mintAgent(0, "A2", { value: price });

      await arena
        .connect(operator)
        .recordBattle(1, 2, user1.address, 100, 50, "Qm1");
      const rewards1 = await arena.getPendingRewards(user1.address);

      // Double the multiplier
      await arena.connect(daoExecutor).setClassMultiplier(0, 200);

      await arena
        .connect(operator)
        .recordBattle(1, 2, user1.address, 100, 50, "Qm2");

      const rewards2 = await arena.getPendingRewards(user1.address);
      expect(rewards2 - rewards1).to.be.gt(rewards1);
    });
  });

  describe("Pause / Unpause", function () {
    it("should allow PAUSER_ROLE to pause", async function () {
      await arena.connect(pauser).pause();
      expect(await arena.paused()).to.be.true;
    });

    it("should allow PAUSER_ROLE to unpause", async function () {
      await arena.connect(pauser).pause();
      await arena.connect(pauser).unpause();
      expect(await arena.paused()).to.be.false;
    });

    it("should revert if non-pauser tries to pause", async function () {
      await expect(
        arena.connect(user1).pause()
      ).to.be.revertedWithCustomError(
        arena,
        "AccessControlUnauthorizedAccount"
      );
    });

    it("should block mintAgent when paused", async function () {
      await arena.connect(pauser).pause();
      await expect(
        arena.connect(user1).mintAgent(0, "GPT-4", {
          value: ethers.parseEther("10"),
        })
      ).to.be.revertedWithCustomError(arena, "EnforcedPause");
    });

    it("should block recordBattle when paused", async function () {
      const price = ethers.parseEther("10");
      await arena.connect(user1).mintAgent(0, "A1", { value: price });
      await arena.connect(user2).mintAgent(0, "A2", { value: price });
      await arena.connect(pauser).pause();

      await expect(
        arena
          .connect(operator)
          .recordBattle(1, 2, user1.address, 100, 50, "Qm")
      ).to.be.revertedWithCustomError(arena, "EnforcedPause");
    });

    it("should block staking when paused", async function () {
      const price = ethers.parseEther("10");
      await arena.connect(user1).mintAgent(0, "A1", { value: price });
      await ataToken
        .connect(deployer)
        .mint(user1.address, ethers.parseEther("1000"));
      await ataToken
        .connect(user1)
        .approve(await arena.getAddress(), ethers.parseEther("1000"));

      await arena.connect(pauser).pause();
      await expect(
        arena.connect(user1).stakeTokens(1, ethers.parseEther("100"))
      ).to.be.revertedWithCustomError(arena, "EnforcedPause");
    });
  });

  describe("View Functions", function () {
    beforeEach(async function () {
      const price = ethers.parseEther("10");
      await arena.connect(user1).mintAgent(0, "GPT-4", { value: price });
    });

    it("getAgentInfo should return correct data", async function () {
      const [agentClass, elo, battles, staked, active] =
        await arena.getAgentInfo(1);
      expect(agentClass).to.equal(0);
      expect(elo).to.equal(1500n);
      expect(battles).to.equal(0n);
      expect(staked).to.equal(0n);
      expect(active).to.be.true;
    });

    it("getUserAgents should return user's NFT IDs", async function () {
      await arena.connect(user1).mintAgent(0, "Claude", {
        value: ethers.parseEther("10"),
      });
      const agents = await arena.getUserAgents(user1.address);
      expect(agents.length).to.equal(2);
      expect(agents[0]).to.equal(1n);
      expect(agents[1]).to.equal(2n);
    });

    it("getPendingRewards should return 0 for new user", async function () {
      expect(await arena.getPendingRewards(user1.address)).to.equal(0n);
    });

    it("getClassStats should return zeros initially", async function () {
      const [battles, rewards] = await arena.getClassStats(0);
      expect(battles).to.equal(0n);
      expect(rewards).to.equal(0n);
    });
  });

  describe("Access Control", function () {
    it("should allow admin to grant DAO_EXECUTOR", async function () {
      await arena.grantRole(DAO_EXECUTOR, user1.address);
      expect(await arena.hasRole(DAO_EXECUTOR, user1.address)).to.be.true;
    });

    it("should allow admin to revoke roles", async function () {
      await arena.revokeRole(BATTLE_OPERATOR, operator.address);
      expect(await arena.hasRole(BATTLE_OPERATOR, operator.address)).to.be
        .false;
    });
  });

  describe("Edge Cases", function () {
    it("should accept MNT via receive()", async function () {
      await deployer.sendTransaction({
        to: await arena.getAddress(),
        value: ethers.parseEther("1"),
      });
    });

    it("should handle multiple battles and accumulate rewards", async function () {
      const price = ethers.parseEther("10");
      await arena.connect(user1).mintAgent(0, "A1", { value: price });
      await arena.connect(user2).mintAgent(0, "A2", { value: price });

      await arena
        .connect(operator)
        .recordBattle(1, 2, user1.address, 100, 50, "Qm1");
      await arena
        .connect(operator)
        .recordBattle(1, 2, user2.address, 50, 100, "Qm2");

      expect(await arena.getPendingRewards(user1.address)).to.be.gt(0n);
      expect(await arena.getPendingRewards(user2.address)).to.be.gt(0n);
      expect(await arena.totalBattles()).to.equal(2n);
    });

    it("should record battle where solver wins", async function () {
      const price = ethers.parseEther("10");
      await arena.connect(user1).mintAgent(0, "A1", { value: price });
      await arena.connect(user2).mintAgent(0, "A2", { value: price });

      await arena
        .connect(operator)
        .recordBattle(1, 2, user2.address, 40, 90, "QmSolverWins");

      const winnerRewards = await arena.getPendingRewards(user2.address);
      const loserRewards = await arena.getPendingRewards(user1.address);
      expect(winnerRewards).to.equal(loserRewards * 9n);
    });
  });
});
