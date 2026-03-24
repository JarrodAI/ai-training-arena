use serde::{Deserialize, Serialize};
use super::agent::AgentClass;

#[derive(Clone, Debug, Serialize, Deserialize, PartialEq, Default)]
pub enum BattleStatus {
    #[default]
    Matchmaking,
    Live,
    Completed,
}

#[derive(Clone, Debug, Serialize, Deserialize, PartialEq)]
pub struct BattleSlot {
    pub slot_index: usize,
    pub nft_id_proposer: Option<u64>,
    pub nft_id_solver: Option<u64>,
    pub model_proposer: String,
    pub model_solver: String,
    pub class: AgentClass,
    pub elo_proposer: u32,
    pub elo_solver: u32,
    pub status: BattleStatus,
    pub time_remaining_secs: u32,
    pub proposer_score: u32,
    pub solver_score: u32,
    pub battle_id: Option<String>,
}

impl Default for BattleSlot {
    fn default() -> Self {
        Self {
            slot_index: 0,
            nft_id_proposer: None,
            nft_id_solver: None,
            model_proposer: String::new(),
            model_solver: String::new(),
            class: AgentClass::A,
            elo_proposer: 1500,
            elo_solver: 1500,
            status: BattleStatus::Matchmaking,
            time_remaining_secs: 0,
            proposer_score: 0,
            solver_score: 0,
            battle_id: None,
        }
    }
}

#[derive(Clone, Debug, Serialize, Deserialize)]
pub struct BattleUpdate {
    pub slot_index: usize,
    pub battle_id: Option<String>,
    pub status: BattleStatus,
    pub nft_id_proposer: Option<u64>,
    pub nft_id_solver: Option<u64>,
    pub model_proposer: Option<String>,
    pub model_solver: Option<String>,
    pub class: Option<AgentClass>,
    pub elo_proposer: Option<u32>,
    pub elo_solver: Option<u32>,
    pub time_remaining_secs: Option<u32>,
    pub proposer_score: Option<u32>,
    pub solver_score: Option<u32>,
}

#[derive(Clone, Debug, Serialize, Deserialize)]
pub struct WsMessage {
    pub msg_type: String,
    pub payload: serde_json::Value,
}

#[derive(Clone, Debug, Serialize, Deserialize)]
pub struct BattleResult {
    pub battle_id: String,
    pub proposer_nft_id: u64,
    pub solver_nft_id: u64,
    pub winner_nft_id: u64,
    pub proposer_score: u32,
    pub solver_score: u32,
    pub proposer_reward: f64,
    pub solver_reward: f64,
    pub burned: f64,
    pub timestamp: u64,
    pub class: AgentClass,
}

#[derive(Clone, Debug, Serialize, Deserialize, Default)]
pub struct RoundInfo {
    pub round_number: u8,
    pub time_remaining_secs: u32,
    pub battles_in_round: u32,
}

impl RoundInfo {
    pub fn compute_from_timestamp(now_secs: u64) -> Self {
        let round_duration = 3 * 3600u64; // 3 hours
        let day_start = (now_secs / 86400) * 86400;
        let time_in_day = now_secs - day_start;
        let round_index = (time_in_day / round_duration) as u8;
        let time_in_round = time_in_day % round_duration;
        let time_remaining = round_duration.saturating_sub(time_in_round) as u32;
        Self {
            round_number: round_index + 1,
            time_remaining_secs: time_remaining,
            battles_in_round: 0,
        }
    }
}
