import { ethers } from "hardhat";
import * as fs from "fs";
import * as path from "path";

async function main() {
  const [deployer] = await ethers.getSigners();
  console.log("Deploying contracts with account:", deployer.address);

  const network = (await ethers.provider.getNetwork()).name;
  const deploymentsDir = path.join(__dirname, "..", "deployments");
  if (!fs.existsSync(deploymentsDir)) fs.mkdirSync(deploymentsDir, { recursive: true });

  const addresses: Record<string, string> = {};

  // 1. ATAToken
  const ATAToken = await ethers.getContractFactory("ATAToken");
  const ataToken = await ATAToken.deploy();
  await ataToken.waitForDeployment();
  addresses["ATAToken"] = await ataToken.getAddress();
  console.log("ATAToken deployed:", addresses["ATAToken"]);

  // 2. VestingSchedule
  const VestingSchedule = await ethers.getContractFactory("VestingSchedule");
  const vestingSchedule = await VestingSchedule.deploy(addresses["ATAToken"]);
  await vestingSchedule.waitForDeployment();
  addresses["VestingSchedule"] = await vestingSchedule.getAddress();
  console.log("VestingSchedule deployed:", addresses["VestingSchedule"]);

  // External deps (USDC, oracle, treasury) from env or deployer as placeholder
  const usdcAddr    = process.env.USDC_ADDRESS     || deployer.address;
  const oracleAddr  = process.env.ORACLE_ADDRESS   || deployer.address;
  const treasuryAddr = process.env.TREASURY_ADDRESS || deployer.address;

  // 3. NodeSaleICO
  const NodeSaleICO = await ethers.getContractFactory("NodeSaleICO");
  const nodeSaleICO = await NodeSaleICO.deploy(usdcAddr, addresses["ATAToken"], oracleAddr, treasuryAddr);
  await nodeSaleICO.waitForDeployment();
  addresses["NodeSaleICO"] = await nodeSaleICO.getAddress();
  console.log("NodeSaleICO deployed:", addresses["NodeSaleICO"]);

  // 4. NodeTrainerNFT
  const NodeTrainerNFT = await ethers.getContractFactory("NodeTrainerNFT");
  const nodeTrainerNFT = await NodeTrainerNFT.deploy(usdcAddr, addresses["ATAToken"], oracleAddr, treasuryAddr);
  await nodeTrainerNFT.waitForDeployment();
  addresses["NodeTrainerNFT"] = await nodeTrainerNFT.getAddress();
  console.log("NodeTrainerNFT deployed:", addresses["NodeTrainerNFT"]);

  // 5. Grant MINTER_ROLE to ICO contracts
  const minterRole = await ataToken.MINTER_ROLE();
  await ataToken.grantRole(minterRole, addresses["NodeSaleICO"]);
  await ataToken.grantRole(minterRole, addresses["NodeTrainerNFT"]);
  console.log("MINTER_ROLE granted to NodeSaleICO and NodeTrainerNFT");

  // 6. Grant ALLOCATOR_ROLE to NodeSaleICO on VestingSchedule
  const allocatorRole = await vestingSchedule.ALLOCATOR_ROLE();
  await vestingSchedule.grantRole(allocatorRole, addresses["NodeSaleICO"]);
  console.log("ALLOCATOR_ROLE granted to NodeSaleICO on VestingSchedule");

  fs.writeFileSync(
    path.join(deploymentsDir, `${network}.json`),
    JSON.stringify(addresses, null, 2)
  );
  console.log(`Deployment addresses saved to deployments/${network}.json`);
}

main().catch((error) => {
  console.error(error);
  process.exitCode = 1;
});
