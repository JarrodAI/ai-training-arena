import { ethers } from 'hardhat';
import { expect } from 'chai';
import { loadFixture } from '@nomicfoundation/hardhat-toolbox/network-helpers';
import { MerkleTree } from 'merkletreejs';
import keccak256 from 'keccak256';

describe('NodeSaleICO', function () {
  async function deployFixture() {
    const [owner, buyer1, buyer2, founder1, founder2, treasury] = await ethers.getSigners();
    const ATAToken = await ethers.getContractFactory('ATAToken');
    const ataToken = await ATAToken.deploy();
    const MockUsdc = await ethers.getContractFactory('MockERC20');
    const usdc = await MockUsdc.deploy('USD Coin', 'USDC');
    const MockOracle = await ethers.getContractFactory('MockChainlinkOracle');
    const oracle = await MockOracle.deploy(2000n * BigInt(1e8));
    const NodeSaleICO = await ethers.getContractFactory('NodeSaleICO');
    const ico = await NodeSaleICO.deploy(
      await usdc.getAddress(), await ataToken.getAddress(),
      await oracle.getAddress(), treasury.address
    );
    const minterRole = await ataToken.MINTER_ROLE();
    await ataToken.grantRole(minterRole, await ico.getAddress());
    // MockERC20 has open mint
    const bigAmt = ethers.parseUnits('50000000', 18);
    for (const s of [buyer1, buyer2, founder1, founder2]) {
      await usdc.mint(s.address, bigAmt);
    }
    const founders = [founder1.address, founder2.address];
    const leaves = founders.map((a) => keccak256(a));
    const tree = new MerkleTree(leaves, keccak256, { sortPairs: true });
    const root = tree.getHexRoot();
    await ico.setFoundersRoot(root);
    return { owner, buyer1, buyer2, founder1, founder2, treasury, ico, ataToken, usdc, oracle, tree };
  }
  it('starts in NOT_STARTED phase (0)', async function () {
    const { ico } = await loadFixture(deployFixture);
    expect(await ico.currentPhase()).to.equal(0);
  });
  it('advances through all 6 phases to FINALIZED', async function () {
    const { ico } = await loadFixture(deployFixture);
    for (let i = 1; i <= 6; i++) {
      await ico.advancePhase();
      expect(await ico.currentPhase()).to.equal(i);
    }
  });
  it('reverts advance past FINALIZED', async function () {
    const { ico } = await loadFixture(deployFixture);
    for (let i = 0; i < 6; i++) await ico.advancePhase();
    await expect(ico.advancePhase()).to.be.revertedWith('NodeSaleICO: already finalized');
  });
  it('reverts non-admin advance', async function () {
    const { ico, buyer1 } = await loadFixture(deployFixture);
    await expect(ico.connect(buyer1).advancePhase()).to.be.reverted;
  });
  it('whitelisted founder can buy in FOUNDERS_PRESALE', async function () {
    const { ico, founder1, usdc, tree, ataToken } = await loadFixture(deployFixture);
    await ico.advancePhase();
    const proof = tree.getHexProof(keccak256(founder1.address));
    const ataAmt = ethers.parseUnits('100', 18);
    const cost = await ico.costForAmount(1, ataAmt);
    await usdc.connect(founder1).approve(await ico.getAddress(), cost);
    await ico.connect(founder1).buyFounders(ataAmt, proof);
    expect(await ataToken.balanceOf(founder1.address)).to.equal(ataAmt);
  });
  it('non-whitelisted address rejected', async function () {
    const { ico, buyer1, usdc, tree } = await loadFixture(deployFixture);
    await ico.advancePhase();
    const fakeProof = tree.getHexProof(keccak256(buyer1.address));
    const ataAmt = ethers.parseUnits('100', 18);
    const cost = await ico.costForAmount(1, ataAmt);
    await usdc.connect(buyer1).approve(await ico.getAddress(), cost);
    await expect(ico.connect(buyer1).buyFounders(ataAmt, fakeProof))
      .to.be.revertedWith('NodeSaleICO: not whitelisted');
  });
  it('founder cannot buy twice', async function () {
    const { ico, founder1, usdc, tree } = await loadFixture(deployFixture);
    await ico.advancePhase();
    const proof = tree.getHexProof(keccak256(founder1.address));
    const ataAmt = ethers.parseUnits('100', 18);
    const cost = await ico.costForAmount(1, ataAmt);
    await usdc.connect(founder1).approve(await ico.getAddress(), cost * 2n);
    await ico.connect(founder1).buyFounders(ataAmt, proof);
    await expect(ico.connect(founder1).buyFounders(ataAmt, proof))
      .to.be.revertedWith('NodeSaleICO: already purchased');
  });
  it('buy() works in TIER_1', async function () {
    const { ico, buyer1, usdc, ataToken } = await loadFixture(deployFixture);
    await ico.advancePhase();
    await ico.advancePhase();
    const ataAmt = ethers.parseUnits('1000', 18);
    const cost = await ico.costForAmount(2, ataAmt);
    await usdc.connect(buyer1).approve(await ico.getAddress(), cost);
    await ico.connect(buyer1).buy(ataAmt);
    expect(await ataToken.balanceOf(buyer1.address)).to.equal(ataAmt);
  });
  it('buy() reverts in FOUNDERS_PRESALE', async function () {
    const { ico, buyer1 } = await loadFixture(deployFixture);
    await ico.advancePhase();
    await expect(ico.connect(buyer1).buy(ethers.parseUnits('1', 18)))
      .to.be.revertedWith('NodeSaleICO: not a public phase');
  });
  it('buy() reverts exceeding allocation', async function () {
    const { ico, buyer1, usdc } = await loadFixture(deployFixture);
    await ico.advancePhase();
    await ico.advancePhase();
    const tooMuch = ethers.parseUnits('6000000', 18);
    const cost = await ico.costForAmount(2, tooMuch);
    await usdc.connect(buyer1).approve(await ico.getAddress(), cost);
    await expect(ico.connect(buyer1).buy(tooMuch))
      .to.be.revertedWith('NodeSaleICO: exceeds allocation');
  });
  it('SOFT_CAP = 120M USDC', async function () {
    const { ico } = await loadFixture(deployFixture);
    expect(await ico.SOFT_CAP()).to.equal(120_000_000n * 1_000_000n);
  });
  it('HARD_CAP = 400M USDC', async function () {
    const { ico } = await loadFixture(deployFixture);
    expect(await ico.HARD_CAP()).to.equal(400_000_000n * 1_000_000n);
  });
  it('refund enabled when soft cap missed on finalize', async function () {
    const { ico } = await loadFixture(deployFixture);
    for (let i = 0; i < 6; i++) await ico.advancePhase();
    expect(await ico.refundEnabled()).to.be.true;
  });
  it('buyer can claim refund after soft cap miss', async function () {
    const { ico, buyer1, usdc } = await loadFixture(deployFixture);
    await ico.advancePhase();
    await ico.advancePhase();
    const ataAmt = ethers.parseUnits('500', 18);
    const cost = await ico.costForAmount(2, ataAmt);
    await usdc.connect(buyer1).approve(await ico.getAddress(), cost);
    await ico.connect(buyer1).buy(ataAmt);
    for (let i = 0; i < 4; i++) await ico.advancePhase();
    const before = await usdc.balanceOf(buyer1.address);
    await ico.connect(buyer1).claimRefund();
    expect(await usdc.balanceOf(buyer1.address) - before).to.equal(cost);
  });
  it('reverts double refund', async function () {
    const { ico, buyer1, usdc } = await loadFixture(deployFixture);
    await ico.advancePhase();
    await ico.advancePhase();
    const ataAmt = ethers.parseUnits('100', 18);
    const cost = await ico.costForAmount(2, ataAmt);
    await usdc.connect(buyer1).approve(await ico.getAddress(), cost);
    await ico.connect(buyer1).buy(ataAmt);
    for (let i = 0; i < 4; i++) await ico.advancePhase();
    await ico.connect(buyer1).claimRefund();
    await expect(ico.connect(buyer1).claimRefund())
      .to.be.revertedWith('NodeSaleICO: no contribution');
  });
  it('refund reverts when not enabled', async function () {
    const { ico, buyer1 } = await loadFixture(deployFixture);
    await expect(ico.connect(buyer1).claimRefund())
      .to.be.revertedWith('NodeSaleICO: refund not active');
  });
  it('buy() reverts when paused', async function () {
    const { ico, buyer1 } = await loadFixture(deployFixture);
    await ico.advancePhase();
    await ico.advancePhase();
    await ico.pause();
    await expect(ico.connect(buyer1).buy(ethers.parseUnits('1', 18)))
      .to.be.revertedWithCustomError(ico, 'EnforcedPause');
  });
  it('Chainlink oracle address is set correctly', async function () {
    const { ico, oracle } = await loadFixture(deployFixture);
    expect(await ico.oracle()).to.equal(await oracle.getAddress());
  });
});