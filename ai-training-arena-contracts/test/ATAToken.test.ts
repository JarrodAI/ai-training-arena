import { ethers } from "hardhat";
import { expect } from "chai";
import { loadFixture } from "@nomicfoundation/hardhat-toolbox/network-helpers";

describe("ATAToken", function () {
  async function deployFixture() {
    const [owner, minter, user] = await ethers.getSigners();
    const ATAToken = await ethers.getContractFactory("ATAToken");
    const token = await ATAToken.deploy();
    const minterRole = await token.MINTER_ROLE();
    await token.grantRole(minterRole, minter.address);
    return { owner, minter, user, token };
  }

  it("has correct name and symbol", async function () {
    const { token } = await loadFixture(deployFixture);
    expect(await token.name()).to.equal("AI Training Arena");
    expect(await token.symbol()).to.equal("ATA");
  });

  it("has correct MAX_SUPPLY of 100M", async function () {
    const { token } = await loadFixture(deployFixture);
    const expected = ethers.parseUnits("100000000", 18);
    expect(await token.MAX_SUPPLY()).to.equal(expected);
  });

  it("mints up to MAX_SUPPLY", async function () {
    const { minter, user, token } = await loadFixture(deployFixture);
    const maxSupply = await token.MAX_SUPPLY();
    await token.connect(minter).mint(user.address, maxSupply);
    expect(await token.totalSupply()).to.equal(maxSupply);
  });

  it("reverts mint exceeding MAX_SUPPLY", async function () {
    const { minter, user, token } = await loadFixture(deployFixture);
    const maxSupply = await token.MAX_SUPPLY();
    await token.connect(minter).mint(user.address, maxSupply);
    await expect(token.connect(minter).mint(user.address, 1n))
      .to.be.revertedWith("ATAToken: max supply exceeded");
  });

  it("reverts partial mint that would exceed MAX_SUPPLY", async function () {
    const { minter, user, token } = await loadFixture(deployFixture);
    const maxSupply = await token.MAX_SUPPLY();
    await token.connect(minter).mint(user.address, maxSupply - 100n);
    await expect(token.connect(minter).mint(user.address, 101n))
      .to.be.revertedWith("ATAToken: max supply exceeded");
  });

  it("allows exactly MAX_SUPPLY in multiple calls", async function () {
    const { minter, user, token } = await loadFixture(deployFixture);
    const half = (await token.MAX_SUPPLY()) / 2n;
    await token.connect(minter).mint(user.address, half);
    await token.connect(minter).mint(user.address, half);
    expect(await token.totalSupply()).to.equal(await token.MAX_SUPPLY());
  });

  it("reverts mint from non-minter", async function () {
    const { user, token } = await loadFixture(deployFixture);
    await expect(token.connect(user).mint(user.address, 1n)).to.be.reverted;
  });

  it("pauses and unpauses transfers", async function () {
    const { owner, minter, user, token } = await loadFixture(deployFixture);
    const pauserRole = await token.PAUSER_ROLE();
    await token.grantRole(pauserRole, owner.address);
    await token.connect(minter).mint(user.address, ethers.parseEther("100"));
    await token.pause();
    await expect(token.connect(user).transfer(minter.address, 1n))
      .to.be.revertedWithCustomError(token, "EnforcedPause");
    await token.unpause();
    await token.connect(user).transfer(minter.address, 1n);
  });
});
