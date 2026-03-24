import { ethers } from 'hardhat';
import { expect } from 'chai';
import { loadFixture, time } from '@nomicfoundation/hardhat-toolbox/network-helpers';

describe('VestingSchedule', function () {
  async function deployFixture() {
    const [owner, founder, nftHolder] = await ethers.getSigners();
    const ATAToken = await ethers.getContractFactory('ATAToken');
    const ataToken = await ATAToken.deploy();
    const VestingSchedule = await ethers.getContractFactory('VestingSchedule');
    const vesting = await VestingSchedule.deploy(await ataToken.getAddress());
    const minterRole = await ataToken.MINTER_ROLE();
    await ataToken.grantRole(minterRole, owner.address);
    return { owner, founder, nftHolder, ataToken, vesting };
  }
  async function fund(ataToken, owner, vesting, amount) {
    await ataToken.mint(owner.address, amount);
    await ataToken.approve(await vesting.getAddress(), amount);
  }
  it('FOUNDER schedule: 180d cliff, 540d vest', async function () {
    const { owner, founder, ataToken, vesting } = await loadFixture(deployFixture);
    const amount = ethers.parseUnits('1000000', 18);
    await fund(ataToken, owner, vesting, amount);
    const start = BigInt(await time.latest()) + 1000n;
    await vesting.createSchedule(founder.address, amount, start, 0);
    const s = await vesting.schedules(founder.address, 0);
    expect(s.totalAmount).to.equal(amount);
    expect(s.cliffDuration).to.equal(180n * 86400n);
    expect(s.vestDuration).to.equal(540n * 86400n);
  });
  it('NFT_BONUS schedule: 90d cliff, 360d vest', async function () {
    const { owner, nftHolder, ataToken, vesting } = await loadFixture(deployFixture);
    const amount = ethers.parseUnits('10000', 18);
    await fund(ataToken, owner, vesting, amount);
    const start = BigInt(await time.latest()) + 1000n;
    await vesting.createSchedule(nftHolder.address, amount, start, 1);
    const s = await vesting.schedules(nftHolder.address, 0);
    expect(s.cliffDuration).to.equal(90n * 86400n);
    expect(s.vestDuration).to.equal(360n * 86400n);
  });
  it('reverts zero address beneficiary', async function () {
    const { owner, ataToken, vesting } = await loadFixture(deployFixture);
    const amount = ethers.parseUnits('100', 18);
    await fund(ataToken, owner, vesting, amount);
    const start = BigInt(await time.latest()) + 1000n;
    await expect(vesting.createSchedule(ethers.ZeroAddress, amount, start, 0))
      .to.be.revertedWith('VestingSchedule: zero beneficiary');
  });
  it('reverts zero amount', async function () {
    const { owner, founder, vesting } = await loadFixture(deployFixture);
    const start = BigInt(await time.latest()) + 1000n;
    await expect(vesting.createSchedule(founder.address, 0n, start, 0))
      .to.be.revertedWith('VestingSchedule: zero amount');
  });
  it('FOUNDER: 0 claimable before 180d cliff', async function () {
    const { owner, founder, ataToken, vesting } = await loadFixture(deployFixture);
    const amount = ethers.parseUnits('1000000', 18);
    await fund(ataToken, owner, vesting, amount);
    const start = BigInt(await time.latest()) + 1000n;
    await vesting.createSchedule(founder.address, amount, start, 0);
    await time.increase(90 * 86400);
    expect(await vesting.claimableAmount(founder.address, 0)).to.equal(0n);
  });
  it('FOUNDER: reverts claim before cliff', async function () {
    const { owner, founder, ataToken, vesting } = await loadFixture(deployFixture);
    const amount = ethers.parseUnits('500000', 18);
    await fund(ataToken, owner, vesting, amount);
    const start = BigInt(await time.latest()) + 1000n;
    await vesting.createSchedule(founder.address, amount, start, 0);
    await time.increase(90 * 86400);
    await expect(vesting.connect(founder).claim(0))
      .to.be.revertedWith('VestingSchedule: nothing to claim');
  });
  it('FOUNDER: full claim after 540d', async function () {
    const { owner, founder, ataToken, vesting } = await loadFixture(deployFixture);
    const amount = ethers.parseUnits('1000000', 18);
    await fund(ataToken, owner, vesting, amount);
    const start = BigInt(await time.latest()) + 1000n;
    await vesting.createSchedule(founder.address, amount, start, 0);
    await time.increase(540 * 86400 + 2000);
    await vesting.connect(founder).claim(0);
    expect(await ataToken.balanceOf(founder.address)).to.equal(amount);
  });
  it('NFT_BONUS: full claim after 360d', async function () {
    const { owner, nftHolder, ataToken, vesting } = await loadFixture(deployFixture);
    const amount = ethers.parseUnits('50000', 18);
    await fund(ataToken, owner, vesting, amount);
    const start = BigInt(await time.latest()) + 1000n;
    await vesting.createSchedule(nftHolder.address, amount, start, 1);
    await time.increase(360 * 86400 + 2000);
    await vesting.connect(nftHolder).claim(0);
    expect(await ataToken.balanceOf(nftHolder.address)).to.equal(amount);
  });
  it('claimAll(): multiple schedules for one beneficiary', async function () {
    const { owner, founder, ataToken, vesting } = await loadFixture(deployFixture);
    const amount = ethers.parseUnits('500000', 18);
    await fund(ataToken, owner, vesting, amount * 2n);
    const start = BigInt(await time.latest()) + 1000n;
    await vesting.createSchedule(founder.address, amount, start, 0);
    await vesting.createSchedule(founder.address, amount, start, 0);
    await time.increase(540 * 86400 + 2000);
    await vesting.connect(founder).claimAll();
    expect(await ataToken.balanceOf(founder.address)).to.equal(amount * 2n);
  });
  it('multiple beneficiaries tracked independently', async function () {
    const { owner, founder, nftHolder, ataToken, vesting } = await loadFixture(deployFixture);
    const amt1 = ethers.parseUnits('300000', 18);
    const amt2 = ethers.parseUnits('100000', 18);
    await fund(ataToken, owner, vesting, amt1 + amt2);
    const start = BigInt(await time.latest()) + 1000n;
    await vesting.createSchedule(founder.address, amt1, start, 0);
    await vesting.createSchedule(nftHolder.address, amt2, start, 1);
    await time.increase(540 * 86400 + 2000);
    await vesting.connect(founder).claim(0);
    await vesting.connect(nftHolder).claim(0);
    expect(await ataToken.balanceOf(founder.address)).to.equal(amt1);
    expect(await ataToken.balanceOf(nftHolder.address)).to.equal(amt2);
  });
  it('revoke(): returns unvested to admin before cliff', async function () {
    const { owner, founder, ataToken, vesting } = await loadFixture(deployFixture);
    const amount = ethers.parseUnits('1000000', 18);
    await fund(ataToken, owner, vesting, amount);
    const start = BigInt(await time.latest()) + 1000n;
    await vesting.createSchedule(founder.address, amount, start, 0);
    const before = await ataToken.balanceOf(owner.address);
    await vesting.revoke(founder.address, 0);
    expect(await ataToken.balanceOf(owner.address) - before).to.equal(amount);
  });
  it('revoke(): reverts double revoke', async function () {
    const { owner, founder, ataToken, vesting } = await loadFixture(deployFixture);
    const amount = ethers.parseUnits('1000', 18);
    await fund(ataToken, owner, vesting, amount);
    const start = BigInt(await time.latest()) + 1000n;
    await vesting.createSchedule(founder.address, amount, start, 0);
    await vesting.revoke(founder.address, 0);
    await expect(vesting.revoke(founder.address, 0))
      .to.be.revertedWith('VestingSchedule: already revoked');
  });
});