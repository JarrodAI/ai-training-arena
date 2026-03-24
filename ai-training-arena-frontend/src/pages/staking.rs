use dioxus::prelude::*;
use crate::components::{staking_panel::StakingPanel, wata_license_panel::WataLicensePanel};
use crate::types::wallet::WalletState;

#[component]
pub fn StakingPage() -> Element {
    let wallet = use_context::<Signal<WalletState>>();

    rsx! {
        div {
            class: "min-h-screen",
            style: "background:#0D1117",
            div {
                class: "max-w-7xl mx-auto px-4 py-8",
                h1 {
                    class: "text-3xl font-bold mb-8",
                    style: "color:#FFFFFF",
                    "Staking & Licenses"
                }
                if wallet.read().is_connected() {
                    div {
                        class: "grid grid-cols-1 gap-8 lg:grid-cols-2",
                        StakingPanel {
                            agents: wallet.read().agents.clone(),
                        }
                        WataLicensePanel {
                            wallet_address: wallet.read().address.clone(),
                            wata_balance: wallet.read().wata_balance,
                        }
                    }
                } else {
                    div {
                        class: "text-center py-20",
                        p {
                            style: "color:#8B949E",
                            class: "text-lg mb-4",
                            "Connect your wallet to access staking"
                        }
                    }
                }
            }
        }
    }
}
