use crate::types::{agent::Agent, wallet::WalletState};

pub struct WalletStateManager;

impl WalletStateManager {
    pub fn set_connected(state: &mut WalletState, address: String, chain_id: u64) {
        state.set_connected(address, chain_id);
    }

    pub fn set_disconnected(state: &mut WalletState) {
        state.set_disconnected();
    }

    pub fn set_agents(state: &mut WalletState, agents: Vec<Agent>) {
        state.set_agents(agents);
    }

    pub fn update_balance(state: &mut WalletState, ata: f64, mnt: f64, wata: f64) {
        state.update_balance(ata, mnt, wata);
    }
}
