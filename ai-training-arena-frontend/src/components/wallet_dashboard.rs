use dioxus::prelude::*;
use crate::api::NodeApi;
use crate::types::wallet::WalletState;
use crate::utils::formatting::{format_ata, format_mnt, format_address_short};

#[component]
pub fn WalletDashboard() -> Element {
    let wallet = use_context::<Signal<WalletState>>();
    let mut claim_status = use_signal(|| String::new());

    let on_claim = move |_| {
        let mut status = claim_status.clone();
        wasm_bindgen_futures::spawn_local(async move {
            status.set("Claiming...".to_string());
            match NodeApi::claim_rewards().await {
                Ok(tx) => status.set(format!("Tx: {}", tx.0)),
                Err(e) => status.set(format!("Error: {}", e)),
            }
        });
    };

    let w = wallet.read();
    let addr = w.address.as_deref().unwrap_or("");

    rsx! {
        div {
            class: "max-w-7xl mx-auto px-4 py-8",
            h1 { class: "text-3xl font-bold mb-6", style: "color:#FFFFFF", "Wallet Dashboard" }

            div {
                class: "grid grid-cols-1 gap-8 lg:grid-cols-2",

                // Wallet summary
                div {
                    class: "rounded-xl p-6",
                    style: "background:#161B22;border:1px solid #30363D",
                    h2 { class: "text-lg font-semibold mb-4", style: "color:#FFFFFF", "Wallet" }
                    div {
                        class: "space-y-3",
                        div {
                            class: "flex justify-between",
                            span { style: "color:#8B949E", "Address" }
                            span { class: "font-mono text-sm", style: "color:#FFFFFF", "{format_address_short(addr)}" }
                        }
                        div {
                            class: "flex justify-between",
                            span { style: "color:#8B949E", "MNT Balance" }
                            span { class: "font-mono", style: "color:#FFFFFF", "{format_mnt(w.mnt_balance)}" }
                        }
                        div {
                            class: "flex justify-between",
                            span { style: "color:#8B949E", "ATA Balance" }
                            span { class: "font-mono", style: "color:#14F195", "{format_ata(w.ata_balance)}" }
                        }
                        div {
                            class: "flex justify-between",
                            span { style: "color:#8B949E", "wATA Balance" }
                            span { class: "font-mono", style: "color:#9945FF", "{format_ata(w.wata_balance)}" }
                        }
                        div {
                            class: "flex justify-between items-center pt-3",
                            style: "border-top:1px solid #30363D",
                            div {
                                span { style: "color:#8B949E", "Pending Rewards" }
                                div { class: "text-xl font-bold", style: "color:#14F195", "{format_ata(w.pending_rewards)}" }
                            }
                            button {
                                class: "px-4 py-2 rounded-lg font-semibold text-sm",
                                style: "background:#14F195;color:#0D1117",
                                onclick: on_claim,
                                "Claim"
                            }
                        }
                        if !claim_status.read().is_empty() {
                            div {
                                class: "text-xs",
                                style: "color:#8B949E",
                                "{claim_status}"
                            }
                        }
                    }
                }

                // Agent management
                div {
                    class: "rounded-xl p-6",
                    style: "background:#161B22;border:1px solid #30363D",
                    h2 { class: "text-lg font-semibold mb-4", style: "color:#FFFFFF", "My Agents" }
                    if w.agents.is_empty() {
                        p { style: "color:#8B949E", "No agents found. Mint one to get started." }
                    } else {
                        div {
                            class: "space-y-3",
                            for agent in w.agents.iter() {
                                div {
                                    key: "{agent.nft_id}",
                                    class: "flex items-center justify-between px-3 py-2 rounded-lg",
                                    style: "background:#0D1117",
                                    div {
                                        div { class: "text-sm font-medium", style: "color:#FFFFFF", "{agent.model_name}" }
                                        div { class: "text-xs", style: "color:#8B949E", "NFT #{agent.nft_id} · {agent.class} · {agent.elo_rating} ELO" }
                                    }
                                    div {
                                        class: "w-2 h-2 rounded-full",
                                        style: if agent.is_active { "background:#14F195" } else { "background:#8B949E" },
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
