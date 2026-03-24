use crate::types::battle::{BattleSlot, BattleUpdate};

#[derive(Clone, Debug)]
pub struct BattleState {
    pub slots: Vec<BattleSlot>,
}

impl Default for BattleState {
    fn default() -> Self {
        let mut slots = Vec::with_capacity(25);
        for i in 0..25 {
            let mut slot = BattleSlot::default();
            slot.slot_index = i;
            slots.push(slot);
        }
        Self { slots }
    }
}

impl BattleState {
    pub fn new() -> Self {
        Self::default()
    }

    pub fn update_slot(&mut self, update: BattleUpdate) {
        if update.slot_index < self.slots.len() {
            let slot = &mut self.slots[update.slot_index];
            slot.status = update.status;
            if let Some(id) = update.battle_id {
                slot.battle_id = Some(id);
            }
            if let Some(v) = update.nft_id_proposer {
                slot.nft_id_proposer = Some(v);
            }
            if let Some(v) = update.nft_id_solver {
                slot.nft_id_solver = Some(v);
            }
            if let Some(v) = update.model_proposer {
                slot.model_proposer = v;
            }
            if let Some(v) = update.model_solver {
                slot.model_solver = v;
            }
            if let Some(v) = update.class {
                slot.class = v;
            }
            if let Some(v) = update.elo_proposer {
                slot.elo_proposer = v;
            }
            if let Some(v) = update.elo_solver {
                slot.elo_solver = v;
            }
            if let Some(v) = update.time_remaining_secs {
                slot.time_remaining_secs = v;
            }
            if let Some(v) = update.proposer_score {
                slot.proposer_score = v;
            }
            if let Some(v) = update.solver_score {
                slot.solver_score = v;
            }
        }
    }

    pub fn get_filtered_slots(&self, filter: &Option<crate::types::agent::AgentClass>) -> Vec<BattleSlot> {
        match filter {
            None => self.slots.clone(),
            Some(class) => self
                .slots
                .iter()
                .filter(|s| &s.class == class)
                .cloned()
                .collect(),
        }
    }
}
