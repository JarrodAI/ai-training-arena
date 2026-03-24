import { ethers } from "hardhat";
import { time } from "@nomicfoundation/hardhat-toolbox/network-helpers";

export async function advanceTime(seconds: number) {
  await time.increase(seconds);
}

export async function advanceBlocks(blocks: number) {
  for (let i = 0; i < blocks; i++) {
    await ethers.provider.send("evm_mine", []);
  }
}

export function toWei(amount: number | string): bigint {
  return ethers.parseEther(amount.toString());
}

export function fromWei(amount: bigint): string {
  return ethers.formatEther(amount);
}
