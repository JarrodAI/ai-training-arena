use dioxus::prelude::*;
use crate::components::{WalletDashboard, StatsDashboard};
use crate::types::wallet::WalletState;

#[component]
pub fn DashboardPage() -> Element {
    let wallet = use_context::<Signal<WalletState>>();

    rsx! {
        div {
            class: "min-h-screen",
            style: "background:#0D1117",
            if wallet.read().is_connected() {
                WalletDashboard {}
            } else {
                StatsDashboard {}
            }
        }
    }
}
