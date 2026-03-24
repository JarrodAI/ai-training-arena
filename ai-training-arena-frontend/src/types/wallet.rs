use serde::{Deserialize, Serialize};
use super::agent::Agent;

#[derive(Clone, Debug, Serialize, Deserialize, PartialEq)]
pub enum WalletError {
    NotInstalled,
    UserRejected,
    WrongNetwork,
    Unknown(String),
}

impl std::fmt::Display for WalletError {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        match self {
            WalletError::NotInstalled => write!(f, "MetaMask not installed"),
            WalletError::UserRejected => write!(f, "User rejected the request"),
            WalletError::WrongNetwork => write!(f, "Please switch to Mantle network"),
            WalletError::Unknown(e) => write!(f, "Wallet error: {}", e),
        }
    }
}

#[derive(Clone, Debug, Serialize, Deserialize, Default)]
pub struct WalletState {
    pub address: Option<String>,
    pub ata_balance: f64,
    pub mnt_balance: f64,
    pub wata_balance: f64,
    pub is_connected: bool,
    pub chain_id: Option<u64>,
    pub agents: Vec<Agent>,
    pub pending_rewards: f64,
}

impl WalletState {
    pub fn new() -> Self {
        Self::default()
    }

    pub fn is_connected(&self) -> bool {
        self.is_connected && self.address.is_some()
    }

    pub fn short_address(&self) -> String {
        match &self.address {
            Some(addr) if addr.len() >= 10 => {
                format!("{}...{}", &addr[..6], &addr[addr.len() - 4..])
            }
            Some(addr) => addr.clone(),
            None => String::from("Not Connected"),
        }
    }

    pub fn is_on_mantle(&self) -> bool {
        matches!(self.chain_id, Some(5000) | Some(5003))
    }

    pub fn set_connected(&mut self, address: String, chain_id: u64) {
        self.address = Some(address);
        self.chain_id = Some(chain_id);
        self.is_connected = true;
    }

    pub fn set_disconnected(&mut self) {
        self.address = None;
        self.chain_id = None;
        self.is_connected = false;
        self.agents = Vec::new();
        self.ata_balance = 0.0;
        self.mnt_balance = 0.0;
        self.wata_balance = 0.0;
        self.pending_rewards = 0.0;
    }

    pub fn set_agents(&mut self, agents: Vec<Agent>) {
        self.agents = agents;
    }

    pub fn update_balance(&mut self, ata: f64, mnt: f64, wata: f64) {
        self.ata_balance = ata;
        self.mnt_balance = mnt;
        self.wata_balance = wata;
    }
}
