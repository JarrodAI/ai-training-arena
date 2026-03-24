import { ethers } from 'hardhat';
import { expect } from 'chai';
import { loadFixture } from '@nomicfoundation/hardhat-toolbox/network-helpers';

describe('NodeTrainerNFT', function () {
  async function deployFixture() {
    const [owner, buyer, treasury, other] = await ethers.getSigners();
    const ATAToken = await ethers.getContractFactory('ATAToken');
    const ataToken = await ATAToken.deploy();
    const MockUsdc = await ethers.getContractFactory('MockERC20');
    const usdc = await MockUsdc.deploy('USD Coin', 'USDC');
    const MockOracle = await ethers.getContractFactory('MockChainlinkOracle');
    const oracle = await MockOracle.deploy(2000n * BigInt(1e8));
    const NodeTrainerNFT = await ethers.getContractFactory('NodeTrainerNFT');
    const nft = await NodeTrainerNFT.deploy(
      await usdc.getAddress(), await ataToken.getAddress(),
      await oracle.getAddress(), treasury.address
    );
    const minterRole = await ataToken.MINTER_ROLE();
    await ataToken.grantRole(minterRole, await nft.getAddress());
    // MockERC20 has open mint
    await usdc.mint(buyer.address, ethers.parseUnits('10000000', 18));
    return { owner, buyer, treasury, other, nft, ataToken, usdc, oracle };
  }
  it('Bronze: supply=15000 price=5000USDC bonus=10000ATA', async function () {
    const { nft } = await loadFixture(deployFixture);
    const cfg = await nft.tierConfig(0);
    expect(cfg.maxSupply).to.equal(15000n);
    expect(cfg.priceUSD).to.equal(5000n * 1_000_000n);
    expect(cfg.bonusATA).to.equal(ethers.parseUnits('10000', 18));
  });
  it('Silver: supply=6000 price=25000USDC bonus=75000ATA', async function () {
    const { nft } = await loadFixture(deployFixture);
    const cfg = await nft.tierConfig(1);
    expect(cfg.maxSupply).to.equal(6000n);
    expect(cfg.priceUSD).to.equal(25000n * 1_000_000n);
    expect(cfg.bonusATA).to.equal(ethers.parseUnits('75000', 18));
  });
  it('Gold: supply=2500 price=100000USDC', async function () {
    const { nft } = await loadFixture(deployFixture);
    const cfg = await nft.tierConfig(2);
    expect(cfg.maxSupply).to.equal(2500n);
    expect(cfg.priceUSD).to.equal(100000n * 1_000_000n);
  });
  it('Platinum: supply=1200 price=400000USDC', async function () {
    const { nft } = await loadFixture(deployFixture);
    const cfg = await nft.tierConfig(3);
    expect(cfg.maxSupply).to.equal(1200n);
    expect(cfg.priceUSD).to.equal(400000n * 1_000_000n);
  });
  it('Diamond: supply=300 price=1500000USDC bonus=10MATA', async function () {
    const { nft } = await loadFixture(deployFixture);
    const cfg = await nft.tierConfig(4);
    expect(cfg.maxSupply).to.equal(300n);
    expect(cfg.priceUSD).to.equal(1_500_000n * 1_000_000n);
    expect(cfg.bonusATA).to.equal(ethers.parseUnits('10000000', 18));
  });
  it('mint Bronze: sends USDC to treasury', async function () {
    const { buyer, treasury, nft, usdc } = await loadFixture(deployFixture);
    const price = 5000n * 1_000_000n;
    const before = await usdc.balanceOf(treasury.address);
    await usdc.connect(buyer).approve(await nft.getAddress(), price);
    await nft.connect(buyer).mint(0);
    expect(await nft.balanceOf(buyer.address)).to.equal(1n);
    expect(await usdc.balanceOf(treasury.address) - before).to.equal(price);
  });
  it('mint Bronze: grants 10000 ATA bonus', async function () {
    const { buyer, nft, usdc, ataToken } = await loadFixture(deployFixture);
    await usdc.connect(buyer).approve(await nft.getAddress(), 5000n * 1_000_000n);
    await nft.connect(buyer).mint(0);
    expect(await ataToken.balanceOf(buyer.address)).to.equal(ethers.parseUnits('10000', 18));
  });
  it('records tokenTier correctly', async function () {
    const { buyer, nft, usdc } = await loadFixture(deployFixture);
    await usdc.connect(buyer).approve(await nft.getAddress(), 5000n * 1_000_000n);
    await nft.connect(buyer).mint(0);
    expect(await nft.tokenTier(0)).to.equal(0);
  });
  it('remainingSupply decreases after mint', async function () {
    const { buyer, nft, usdc } = await loadFixture(deployFixture);
    await usdc.connect(buyer).approve(await nft.getAddress(), 5000n * 1_000_000n);
    await nft.connect(buyer).mint(0);
    expect(await nft.remainingSupply(0)).to.equal(14999n);
  });
  it('reverts when paused', async function () {
    const { buyer, nft, usdc } = await loadFixture(deployFixture);
    await nft.pause();
    await usdc.connect(buyer).approve(await nft.getAddress(), 5000n * 1_000_000n);
    await expect(nft.connect(buyer).mint(0)).to.be.revertedWithCustomError(nft, 'EnforcedPause');
  });
  it('works after unpause', async function () {
    const { buyer, nft, usdc } = await loadFixture(deployFixture);
    await nft.pause();
    await nft.unpause();
    await usdc.connect(buyer).approve(await nft.getAddress(), 5000n * 1_000_000n);
    await nft.connect(buyer).mint(0);
    expect(await nft.balanceOf(buyer.address)).to.equal(1n);
  });
  it('mint Silver: grants 75000 ATA bonus', async function () {
    const { buyer, nft, usdc, ataToken } = await loadFixture(deployFixture);
    const price = 25000n * 1_000_000n;
    await usdc.connect(buyer).approve(await nft.getAddress(), price);
    await nft.connect(buyer).mint(1);
    expect(await ataToken.balanceOf(buyer.address)).to.equal(ethers.parseUnits('75000', 18));
  });
  it('setTreasury: updates treasury address', async function () {
    const { nft, other } = await loadFixture(deployFixture);
    await nft.setTreasury(other.address);
    expect(await nft.treasury()).to.equal(other.address);
  });
  it('setTreasury: reverts zero address', async function () {
    const { nft } = await loadFixture(deployFixture);
    await expect(nft.setTreasury(ethers.ZeroAddress))
      .to.be.revertedWith('NodeTrainerNFT: zero treasury');
  });
  it('non-admin cannot setTreasury', async function () {
    const { nft, buyer, other } = await loadFixture(deployFixture);
    await expect(nft.connect(buyer).setTreasury(other.address)).to.be.reverted;
  });
  it('totalMinted() increments', async function () {
    const { buyer, nft, usdc } = await loadFixture(deployFixture);
    await usdc.connect(buyer).approve(await nft.getAddress(), 5000n * 1_000_000n * 3n);
    await nft.connect(buyer).mint(0);
    await nft.connect(buyer).mint(0);
    await nft.connect(buyer).mint(0);
    expect(await nft.totalMinted()).to.equal(3n);
  });
});