use std::collections::HashMap;
use crate::types::{
    agent::AgentClass,
    battle::BattleSlot,
    leaderboard::LeaderboardEntry,
    marketplace::NodeStatus,
    wallet::WalletState,
};

#[derive(Clone, Debug, Default)]
pub struct AppState {
    pub wallet: WalletState,
    pub battle_slots: Vec<BattleSlot>,
    pub leaderboard: HashMap<String, Vec<LeaderboardEntry>>,
    pub node_status: NodeStatus,
    pub selected_class_filter: Option<AgentClass>,
    pub is_loading: bool,
    pub error_message: Option<String>,
}

impl AppState {
    pub fn new() -> Self {
        let mut battle_slots = Vec::with_capacity(25);
        for i in 0..25 {
            let mut slot = BattleSlot::default();
            slot.slot_index = i;
            battle_slots.push(slot);
        }
        Self {
            battle_slots,
            ..Default::default()
        }
    }

    pub fn filtered_slots(&self) -> Vec<BattleSlot> {
        match &self.selected_class_filter {
            None => self.battle_slots.clone(),
            Some(class) => self
                .battle_slots
                .iter()
                .filter(|s| &s.class == class)
                .cloned()
                .collect(),
        }
    }
}
