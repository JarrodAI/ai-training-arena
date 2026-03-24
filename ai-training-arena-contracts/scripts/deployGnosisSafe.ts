import { ethers } from "hardhat";
import * as dotenv from "dotenv";
import * as fs from "fs";
import * as path from "path";

dotenv.config();

const SAFE_SINGLETON   = "0xd9Db270c1B5E3Bd161E8c8503c55cEABeE709552";
const SAFE_FACTORY     = "0xa6B71E26C5e0845f74c812102Ca7114b6a896AB1";
const FALLBACK_HANDLER = "0xf48f2B2d2a534e402487b3ee7C18c33Aec0Fe5e4";

const SAFE_FACTORY_ABI = [
  "function createProxyWithNonce(address singleton, bytes memory initializer, uint256 saltNonce) returns (address proxy)",
  "event ProxyCreation(address proxy, address singleton)"
];

const SAFE_SINGLETON_ABI = [
  "function setup(address[] calldata _owners, uint256 _threshold, address to, bytes calldata data, address fallbackHandler, address paymentToken, uint256 payment, address payable paymentReceiver) external"
];

async function main() {
  const [deployer] = await ethers.getSigners();
  console.log("Deploying Gnosis Safe with account:", deployer.address);

  const signers: string[] = [];
  for (let i = 1; i <= 5; i++) {
    const addr = process.env[`SAFE_SIGNER_${i}`];
    if (!addr) throw new Error(`Missing env var SAFE_SIGNER_${i}`);
    if (!ethers.isAddress(addr)) throw new Error(`Invalid address SAFE_SIGNER_${i}: ${addr}`);
    signers.push(addr);
  }

  console.log("Signers:", signers);
  console.log("Threshold: 3 of 5");

  const factory  = new ethers.Contract(SAFE_FACTORY,   SAFE_FACTORY_ABI,   deployer);
  const singleton = new ethers.Contract(SAFE_SINGLETON, SAFE_SINGLETON_ABI, deployer);

  const setupData = singleton.interface.encodeFunctionData("setup", [
    signers,
    3,
    ethers.ZeroAddress,
    "0x",
    FALLBACK_HANDLER,
    ethers.ZeroAddress,
    0,
    ethers.ZeroAddress,
  ]);

  const saltNonce = Date.now();
  const tx = await factory.createProxyWithNonce(SAFE_SINGLETON, setupData, saltNonce);
  const receipt = await tx.wait();

  const iface = new ethers.Interface(["event ProxyCreation(address proxy, address singleton)"]);
  let safeAddress = "";
  for (const log of receipt.logs) {
    try {
      const parsed = iface.parseLog(log);
      if (parsed && parsed.name === "ProxyCreation") {
        safeAddress = parsed.args.proxy;
        break;
      }
    } catch {}
  }

  if (!safeAddress) throw new Error("Could not parse Safe address from receipt");
  console.log("Gnosis Safe deployed at:", safeAddress);

  const network = (await ethers.provider.getNetwork()).name;
  const deploymentsDir = path.join(__dirname, "..", "deployments");
  if (!fs.existsSync(deploymentsDir)) fs.mkdirSync(deploymentsDir, { recursive: true });

  const deploymentsFile = path.join(deploymentsDir, `${network}.json`);
  let deployments: Record<string, string> = {};
  if (fs.existsSync(deploymentsFile)) {
    deployments = JSON.parse(fs.readFileSync(deploymentsFile, "utf8"));
  }
  deployments["GnosisSafe"] = safeAddress;
  fs.writeFileSync(deploymentsFile, JSON.stringify(deployments, null, 2));
  console.log(`Saved to deployments/${network}.json`);
}

main().catch((error) => {
  console.error(error);
  process.exitCode = 1;
});
