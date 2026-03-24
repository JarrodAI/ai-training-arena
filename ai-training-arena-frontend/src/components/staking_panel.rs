use dioxus::prelude::*;
use crate::api::NodeApi;
use crate::types::agent::Agent;
use crate::utils::formatting::format_ata;

#[component]
pub fn StakingPanel(agents: Vec<Agent>) -> Element {
    let mut stake_amounts: Signal<std::collections::HashMap<u64, String>> = use_signal(std::collections::HashMap::new);
    let mut status = use_signal(|| String::new());

    // APY = (365 * 8219) / totalStaked * 100
    let total_staked: f64 = agents.iter().map(|a| a.staked_amount).sum();
    let apy = if total_staked > 0.0 {
        (365.0 * 8219.0) / total_staked * 100.0
    } else {
        0.0
    };

    rsx! {
        div {
            class: "rounded-xl p-6",
            style: "background:#161B22;border:1px solid #30363D",
            h2 { class: "text-xl font-semibold mb-2", style: "color:#FFFFFF", "Staking" }
            div {
                class: "flex items-center gap-2 mb-4 px-3 py-2 rounded-lg",
                style: "background:#0D1117",
                span { style: "color:#8B949E;font-size:12px", "Est. APY:" }
                span { class: "text-lg font-bold", style: "color:#14F195", "{apy:.1}%" }
                span { style: "color:#8B949E;font-size:10px", "(365 x 8,219 ATA / Total Staked)" }
            }

            div { class: "space-y-4",
                for agent in agents.iter() {
                    {
                        let nft_id = agent.nft_id;
                        let color = agent.class.color().to_string();
                        let nft_id_clone = nft_id;
                        let mut sa = stake_amounts.clone();
                        let mut st = status.clone();

                        rsx! {
                            div {
                                key: "{nft_id}",
                                class: "rounded-xl p-4",
                                style: format!("background:#0D1117;border:1px solid {}", color),

                                div {
                                    class: "flex justify-between items-center mb-3",
                                    div {
                                        div { class: "text-sm font-medium", style: "color:#FFFFFF", "{agent.model_name}" }
                                        div { class: "text-xs", style: "color:#8B949E", "NFT #{nft_id} · {agent.class}" }
                                    }
                                    div {
                                        class: "text-right",
                                        div { class: "text-xs", style: "color:#8B949E", "Currently Staked" }
                                        div { class: "font-bold font-mono", style: format!("color:{}", color), "{format_ata(agent.staked_amount)}" }
                                    }
                                }

                                div {
                                    class: "flex gap-2",
                                    input {
                                        class: "flex-1 px-3 py-2 rounded-lg text-sm",
                                        style: "background:#161B22;border:1px solid #30363D;color:#FFFFFF",
                                        placeholder: "ATA amount",
                                        value: stake_amounts.read().get(&nft_id).cloned().unwrap_or_default(),
                                        oninput: move |e| {
                                            sa.write().insert(nft_id_clone, e.value());
                                        },
                                    }
                                    button {
                                        class: "px-4 py-2 rounded-lg text-sm font-semibold",
                                        style: format!("background:{};color:#0D1117", color),
                                        onclick: move |_| {
                                            let amount_str = stake_amounts.read().get(&nft_id_clone).cloned().unwrap_or_default();
                                            if let Ok(amount) = amount_str.parse::<f64>() {
                                                let mut s = st.clone();
                                                wasm_bindgen_futures::spawn_local(async move {
                                                    s.set("Staking...".to_string());
                                                    match NodeApi::stake(nft_id_clone, amount).await {
                                                        Ok(tx) => s.set(format!("Staked! Tx: {}", tx.0)),
                                                        Err(e) => s.set(format!("Error: {}", e)),
                                                    }
                                                });
                                            }
                                        },
                                        "Stake More"
                                    }
                                    button {
                                        class: "px-4 py-2 rounded-lg text-sm font-semibold",
                                        style: "background:#30363D;color:#FF6B6B",
                                        onclick: move |_| {
                                            let mut s = st.clone();
                                            if web_sys::window().unwrap().confirm_with_message("Unstaking has a 7-day cooldown. Continue?").unwrap_or(false) {
                                                wasm_bindgen_futures::spawn_local(async move {
                                                    s.set("Unstaking...".to_string());
                                                    match NodeApi::unstake(nft_id_clone).await {
                                                        Ok(tx) => s.set(format!("Unstake initiated! Tx: {}", tx.0)),
                                                        Err(e) => s.set(format!("Error: {}", e)),
                                                    }
                                                });
                                            }
                                        },
                                        "Unstake"
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if !status.read().is_empty() {
                div {
                    class: "mt-3 text-xs p-2 rounded",
                    style: "background:#0D1117;color:#8B949E",
                    "{status}"
                }
            }
        }
    }
}
