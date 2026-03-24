import { run } from "hardhat";
import * as fs from "fs";
import * as path from "path";

async function main() {
  const network = process.env.HARDHAT_NETWORK || "mantle_testnet";
  const deploymentsPath = path.join(__dirname, "..", "deployments", `${network}.json`);

  if (!fs.existsSync(deploymentsPath)) {
    console.error(`No deployments found for network: ${network}`);
    process.exit(1);
  }

  const addresses = JSON.parse(fs.readFileSync(deploymentsPath, "utf-8"));

  for (const [name, address] of Object.entries(addresses)) {
    console.log(`Verifying ${name} at ${address}...`);
    try {
      await run("verify:verify", { address, constructorArguments: [] });
      console.log(`${name} verified.`);
    } catch (e: any) {
      if (e.message.includes("Already Verified")) {
        console.log(`${name} already verified.`);
      } else {
        console.error(`Failed to verify ${name}:`, e.message);
      }
    }
  }
}

main().catch((error) => {
  console.error(error);
  process.exitCode = 1;
});
