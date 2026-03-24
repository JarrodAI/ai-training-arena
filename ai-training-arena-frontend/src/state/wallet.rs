use serde::{Deserialize, Serialize};

#[derive(Clone, Debug, Default, PartialEq, Serialize, Deserialize)]
pub struct WalletState {
    pub connected: bool,
    pub address: Option<String>,
    pub chain_id: Option<u64>,
    pub balance_eth: Option<String>,
}

impl WalletState {
    pub fn new() -> Self {
        Self::default()
    }

    pub fn is_connected(&self) -> bool {
        self.connected && self.address.is_some()
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
}
