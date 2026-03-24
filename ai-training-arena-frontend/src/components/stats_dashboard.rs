use dioxus::prelude::*;
use crate::api::NodeApi;
use crate::components::AgentCard;
use crate::types::{agent::Agent, battle::BattleResult, wallet::WalletState};
use crate::utils::formatting::{format_ata, format_elo};

#[component]
pub fn StatsDashboard() -> Element {
    let wallet = use_context::<Signal<WalletState>>();
    let mut agents: Signal<Vec<Agent>> = use_signal(Vec::new);
    let mut battles: Signal<Vec<BattleResult>> = use_signal(Vec::new);
    let mut pending_rewards = use_signal(|| 0.0f64);
    let mut loading = use_signal(|| false);
    let mut claim_status = use_signal(|| String::new());

    use_effect(move || {
        if wallet.read().is_connected() {
            let mut a = agents.clone();
            let mut b = battles.clone();
            let mut r = pending_rewards.clone();
            let mut l = loading.clone();
            wasm_bindgen_futures::spawn_local(async move {
                l.set(true);
                if let Ok(data) = NodeApi::get_agents().await {
                    a.set(data);
                }
                if let Ok(data) = NodeApi::get_battles(20).await {
                    b.set(data);
                }
                if let Ok(data) = NodeApi::get_rewards().await {
                    r.set(data);
                }
                l.set(false);
            });
        }
    });

    let on_claim = move |_| {
        let mut status = claim_status.clone();
        wasm_bindgen_futures::spawn_local(async move {
            status.set("Claiming...".to_string());
            match NodeApi::claim_rewards().await {
                Ok(tx) => status.set(format!("Claimed! Tx: {}", tx.0)),
                Err(e) => status.set(format!("Error: {}", e)),
            }
        });
    };

    if !wallet.read().is_connected() {
        return rsx! {
            div {
                class: "min-h-screen flex items-center justify-center",
                style: "background:#0D1117",
                div {
                    class: "text-center",
                    p { style: "color:#8B949E;font-size:18px;margin-bottom:16px", "Connect your wallet to view your dashboard" }
                }
            }
        };
    }

    let total_earned: f64 = agents.read().iter().map(|a| a.total_rewards_earned).sum();
    let top_elo = agents.read().iter().map(|a| a.elo_rating).max().unwrap_or(0);
    let agent_count = agents.read().len();
    let active_count = agents.read().iter().filter(|a| a.is_active).count();

    rsx! {
        div {
            class: "min-h-screen",
            style: "background:#0D1117",
            div {
                class: "max-w-7xl mx-auto px-4 py-8",
                h1 {
                    class: "text-3xl font-bold mb-6",
                    style: "color:#FFFFFF",
                    "My Dashboard"
                }

                div {
                    class: "grid grid-cols-2 gap-4 mb-8 lg:grid-cols-4",
                    div {
                        class: "rounded-xl p-4",
                        style: "background:#161B22;border:1px solid #30363D",
                        div { class: "text-xs font-semibold mb-1", style: "color:#8B949E", "Total Agents" }
                        div { class: "text-2xl font-bold", style: "color:#9945FF", "{agent_count}" }
                    }
                    div {
                        class: "rounded-xl p-4",
                        style: "background:#161B22;border:1px solid #30363D",
                        div { class: "text-xs font-semibold mb-1", style: "color:#8B949E", "Active Battles" }
                        div { class: "text-2xl font-bold", style: "color:#14F195", "{active_count}" }
                    }
                    div {
                        class: "rounded-xl p-4",
                        style: "background:#161B22;border:1px solid #30363D",
                        div { class: "text-xs font-semibold mb-1", style: "color:#8B949E", "Total ATA Earned" }
                        div { class: "text-2xl font-bold", style: "color:#FFD166", "{format_ata(total_earned)}" }
                    }
                    div {
                        class: "rounded-xl p-4",
                        style: "background:#161B22;border:1px solid #30363D",
                        div { class: "text-xs font-semibold mb-1", style: "color:#8B949E", "Best ELO" }
                        div { class: "text-2xl font-bold", style: "color:#00C2FF", "{format_elo(top_elo)}" }
                    }
                }

                if *pending_rewards.read() > 0.0 {
                    div {
                        class: "rounded-xl p-4 mb-8 flex items-center justify-between",
                        style: "background:linear-gradient(135deg,#9945FF22,#14F19522);border:1px solid #14F195",
                        div {
                            div { class: "text-xs", style: "color:#8B949E", "PENDING REWARDS" }
                            div { class: "text-2xl font-bold", style: "color:#14F195", "{format_ata(*pending_rewards.read())}" }
                        }
                        div {
                            button {
                                class: "px-6 py-3 rounded-lg font-bold",
                                style: "background:#14F195;color:#0D1117",
                                onclick: on_claim,
                                "Claim Rewards"
                            }
                            if !claim_status.read().is_empty() {
                                div { class: "text-xs mt-2", style: "color:#8B949E", "{claim_status}" }
                            }
                        }
                    }
                }

                h2 {
                    class: "text-xl font-semibold mb-4",
                    style: "color:#FFFFFF",
                    "My Agents ({agent_count})"
                }
                if *loading.read() {
                    div {
                        class: "text-center py-12",
                        div {
                            class: "w-8 h-8 mx-auto border-2 border-t-transparent rounded-full animate-spin",
                            style: "border-color:#9945FF",
                        }
                    }
                } else {
                    div {
                        class: "grid grid-cols-1 gap-4 mb-8 md:grid-cols-2 lg:grid-cols-3",
                        for agent in agents.read().iter().cloned() {
                            { let k = agent.nft_id; rsx! { AgentCard { key: "{k}", agent: agent } } }
                        }
                    }
                }
            }
        }
    }
}
