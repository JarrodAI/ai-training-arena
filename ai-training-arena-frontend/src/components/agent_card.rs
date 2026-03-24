use dioxus::prelude::*;
use crate::types::agent::Agent;
use crate::utils::formatting::{format_ata, format_win_rate, format_elo};
use crate::components::EloChart;

#[component]
pub fn AgentCard(agent: Agent) -> Element {
    let mut show_detail = use_signal(|| false);
    let color = agent.class.color().to_string();

    rsx! {
        div {
            class: "rounded-xl p-5 cursor-pointer hover:scale-105 transition-transform",
            style: format!("background:#161B22;border:2px solid {};box-shadow:0 0 16px {}33", color, color),

            // Class badge + NFT ID
            div {
                class: "flex items-center justify-between mb-3",
                span {
                    class: "px-2 py-0.5 rounded text-xs font-bold",
                    style: format!("background:{};color:#0D1117", color),
                    "{agent.class} · {agent.class.name()}"
                }
                span {
                    class: "text-xs font-mono",
                    style: "color:#8B949E",
                    "NFT #{agent.nft_id}"
                }
            }

            // Model name
            h3 {
                class: "text-base font-semibold mb-2 truncate",
                style: "color:#FFFFFF",
                "{agent.model_name}"
            }

            // ELO (large)
            div {
                class: "text-3xl font-bold font-mono mb-3",
                style: format!("color:{}", color),
                "{format_elo(agent.elo_rating)} ELO"
            }

            // Stats grid
            div {
                class: "grid grid-cols-3 gap-2 mb-4",
                div {
                    class: "text-center",
                    div { class: "text-xs", style: "color:#8B949E", "Battles" }
                    div { class: "text-sm font-bold", style: "color:#FFFFFF", "{agent.total_battles}" }
                }
                div {
                    class: "text-center",
                    div { class: "text-xs", style: "color:#8B949E", "Win Rate" }
                    div { class: "text-sm font-bold", style: "color:#FFFFFF", "{format_win_rate(agent.win_rate)}" }
                }
                div {
                    class: "text-center",
                    div { class: "text-xs", style: "color:#8B949E", "Staked" }
                    div { class: "text-sm font-bold", style: "color:#FFFFFF", "{format_ata(agent.staked_amount)}" }
                }
            }

            // Rewards
            div {
                class: "flex items-center justify-between mb-4 px-3 py-2 rounded-lg",
                style: "background:#0D1117",
                span { style: "color:#8B949E;font-size:12px", "Total Earned:" }
                span { style: "color:#14F195;font-size:14px;font-weight:bold", "{format_ata(agent.total_rewards_earned)}" }
            }

            // Active status
            div {
                class: "flex items-center gap-2 mb-4",
                div {
                    class: "w-2 h-2 rounded-full",
                    style: if agent.is_active { "background:#14F195" } else { "background:#8B949E" },
                }
                span {
                    class: "text-xs",
                    style: "color:#8B949E",
                    if agent.is_active { "Active" } else { "Inactive" }
                }
            }

            // Action buttons
            div {
                class: "flex gap-2",
                button {
                    class: "flex-1 py-2 rounded-lg text-xs font-semibold",
                    style: format!("background:{};color:#0D1117", color),
                    onclick: move |_| show_detail.set(!show_detail()),
                    if *show_detail.read() { "Hide Chart" } else { "View Chart" }
                }
            }

            // ELO chart (expanded)
            if *show_detail.read() {
                div {
                    class: "mt-4",
                    EloChart { nft_id: agent.nft_id }
                }
            }
        }
    }
}
