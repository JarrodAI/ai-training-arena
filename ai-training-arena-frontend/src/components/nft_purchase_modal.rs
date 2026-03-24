use dioxus::prelude::*;
use crate::types::agent::AgentClass;
use crate::utils::web3::eth_send_transaction;

// Contract address — replace with deployed address
const AGENT_NFT_ADDRESS: &str = "0x0000000000000000000000000000000000000000";

fn encode_mint_agent(class: u8) -> String {
    // mintAgent(AgentClass class) selector placeholder
    let selector = "a1b2c3d4";
    format!("0x{}{:0>64x}", selector, class)
}

#[component]
pub fn NftPurchaseModal(
    show: bool,
    on_close: EventHandler<()>,
    wallet_address: Option<String>,
) -> Element {
    let mut selected_class = use_signal(|| AgentClass::A);
    let mut tx_status = use_signal(|| String::new());

    if !show {
        return rsx! {};
    }

    let on_purchase = move |_| {
        let addr = wallet_address.clone();
        let cls = selected_class.read().clone();
        let mut status = tx_status.clone();

        if let Some(from) = addr {
            let class_index = match cls {
                AgentClass::A => 0u8,
                AgentClass::B => 1,
                AgentClass::C => 2,
                AgentClass::D => 3,
                AgentClass::E => 4,
            };
            let price_mnt = cls.price_mnt();
            // Convert MNT to hex wei (price_mnt * 10^18)
            let value_wei = format!("0x{:x}", price_mnt * 1_000_000_000_000_000_000u64);
            let data = encode_mint_agent(class_index);

            wasm_bindgen_futures::spawn_local(async move {
                status.set("Pending...".to_string());
                match eth_send_transaction(&from, AGENT_NFT_ADDRESS, &data, Some(&value_wei)).await {
                    Ok(tx_hash) => status.set(format!("Minted! Tx: {}", tx_hash)),
                    Err(e) => status.set(format!("Error: {}", e)),
                }
            });
        }
    };

    rsx! {
        div {
            class: "fixed inset-0 z-50 flex items-center justify-center p-4",
            style: "background:rgba(0,0,0,0.85)",
            onclick: move |_| on_close(()),

            div {
                class: "w-full max-w-2xl rounded-2xl p-6",
                style: "background:#161B22;border:1px solid #30363D;max-height:90vh;overflow-y:auto",
                onclick: |e| e.stop_propagation(),

                div {
                    class: "flex justify-between items-center mb-6",
                    h2 { class: "text-xl font-bold", style: "color:#FFFFFF", "Mint Agent NFT" }
                    button {
                        class: "w-8 h-8 rounded-full flex items-center justify-center",
                        style: "background:#30363D;color:#FFFFFF",
                        onclick: move |_| on_close(()),
                        "x"
                    }
                }

                div {
                    class: "grid grid-cols-1 gap-3 mb-6 md:grid-cols-5",
                    for class in AgentClass::all() {
                        {
                            let cls = class.clone();
                            let cls_key = format!("{cls}");
                            let is_selected = *selected_class.read() == cls;
                            let color = cls.color().to_string();
                            let color2 = color.clone();
                            rsx! {
                                button {
                                    key: "{cls_key}",
                                    class: "rounded-xl p-4 text-left transition-all",
                                    style: if is_selected {
                                        format!("background:{};color:#0D1117;border:2px solid {}", color, color2)
                                    } else {
                                        format!("background:#0D1117;color:#FFFFFF;border:2px solid #30363D")
                                    },
                                    onclick: move |_| selected_class.set(cls.clone()),
                                    div { class: "text-lg font-bold", "Class {cls}" }
                                    div { class: "text-xs mt-1", "{cls.name()}" }
                                    div { class: "text-xs mt-1", "{cls.params()}" }
                                    div { class: "text-sm font-bold mt-2", "{cls.price_mnt()} MNT" }
                                    div { class: "text-xs mt-1", "{cls.reward_multiplier():.1}x rewards" }
                                }
                            }
                        }
                    }
                }

                div {
                    class: "rounded-xl p-4 mb-6",
                    style: "background:#0D1117",
                    h3 { class: "text-sm font-semibold mb-3", style: "color:#FFFFFF", "Selected: Class {selected_class} ({selected_class.read().name()})" }
                    div {
                        class: "grid grid-cols-2 gap-4",
                        div {
                            div { class: "text-xs", style: "color:#8B949E", "Price" }
                            div { class: "font-bold", style: "color:#FFFFFF", "{selected_class.read().price_mnt()} MNT" }
                        }
                        div {
                            div { class: "text-xs", style: "color:#8B949E", "wATA Stake Required" }
                            div { class: "font-bold", style: "color:#FFFFFF", "{selected_class.read().stake_wata()} wATA" }
                        }
                        div {
                            div { class: "text-xs", style: "color:#8B949E", "Reward Multiplier" }
                            div { class: "font-bold", style: "color:#FFD166", "{selected_class.read().reward_multiplier():.1}x" }
                        }
                        div {
                            div { class: "text-xs", style: "color:#8B949E", "Base Reward" }
                            div { class: "font-bold", style: "color:#14F195", "{selected_class.read().base_reward():.3} ATA/battle" }
                        }
                    }
                }

                button {
                    class: "w-full py-4 rounded-xl font-bold text-lg",
                    style: "background:linear-gradient(135deg,#9945FF,#14F195);color:#FFFFFF",
                    onclick: on_purchase,
                    "Mint Class {selected_class} Agent — {selected_class.read().price_mnt()} MNT"
                }

                if !tx_status.read().is_empty() {
                    div {
                        class: "mt-4 p-3 rounded-lg text-sm",
                        style: "background:#0D1117;color:#8B949E",
                        "{tx_status}"
                    }
                }
            }
        }
    }
}
