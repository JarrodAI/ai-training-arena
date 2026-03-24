use serde::{Deserialize, Serialize};
use super::agent::AgentClass;

#[derive(Clone, Debug, Serialize, Deserialize, PartialEq)]
pub struct LeaderboardEntry {
    pub rank: usize,
    pub nft_id: u64,
    pub model_name: String,
    pub class: AgentClass,
    pub elo_rating: u32,
    pub total_battles: u32,
    pub wins: u32,
    pub win_rate: f64,
    pub total_rewards: f64,
    pub owner: String,
}

impl LeaderboardEntry {
    pub fn losses(&self) -> u32 {
        self.total_battles.saturating_sub(self.wins)
    }
}
