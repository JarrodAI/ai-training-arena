import { ethers } from "hardhat";

export async function getSigners() {
  const [deployer, founder1, founder2, user1, user2, user3, oracle] =
    await ethers.getSigners();
  return { deployer, founder1, founder2, user1, user2, user3, oracle };
}

export async function deployAllContracts() {
  const signers = await getSigners();

  // Contracts will be deployed here once implementations exist
  // Return typed contract instances in correct deployment order:
  // ATAToken → AgentNFT → AITrainingArena → WrappedATA →
  // BattleVerifier → DataMarketplace → AIArenaGovernor →
  // FounderRevenue → MatchmakingRegistry

  return {
    signers,
    // ataToken,
    // agentNFT,
    // arena,
    // wrappedATA,
    // battleVerifier,
    // dataMarketplace,
    // governor,
    // founderRevenue,
    // matchmakingRegistry,
  };
}
