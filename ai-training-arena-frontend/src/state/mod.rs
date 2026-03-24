pub mod app_state;
pub mod battle_state;
pub mod wallet_state;

pub use app_state::AppState;
pub use battle_state::BattleState;
pub use wallet_state::WalletStateManager;

// Keep backward compat
pub use crate::types::wallet::WalletState;
