use dioxus::prelude::*;
use crate::types::battle::BattleSlot;
use crate::utils::formatting::format_duration;

#[component]
pub fn BattleDetailModal(
    slot: BattleSlot,
    on_close: EventHandler<()>,
) -> Element {
    let color = slot.class.color().to_string();

    rsx! {
        div {
            class: "fixed inset-0 z-50 flex items-center justify-center p-4",
            style: "background:rgba(0,0,0,0.85)",
            onclick: move |_| on_close(()),

            div {
                class: "relative w-full max-w-2xl rounded-2xl p-6",
                style: format!("background:#161B22;border:2px solid {};max-height:90vh;overflow-y:auto", color),
                onclick: |e| e.stop_propagation(),

                // Close button
                button {
                    class: "absolute top-4 right-4 w-8 h-8 rounded-full flex items-center justify-center text-lg",
                    style: "background:#30363D;color:#FFFFFF",
                    onclick: move |_| on_close(()),
                    "x"
                }

                h2 {
                    class: "text-xl font-bold mb-6",
                    style: "color:#FFFFFF",
                    "Battle Detail"
                }

                // Both agents
                div {
                    class: "grid grid-cols-2 gap-4 mb-6",
                    // Proposer
                    div {
                        class: "rounded-xl p-4",
                        style: format!("background:#0D1117;border:1px solid {}", color),
                        div {
                            class: "text-xs font-semibold mb-1",
                            style: "color:#8B949E",
                            "PROPOSER"
                        }
                        div {
                            class: "font-medium text-sm",
                            style: "color:#FFFFFF",
                            "{slot.model_proposer}"
                        }
                        div {
                            class: "text-xs",
                            style: "color:#8B949E",
                            "NFT #{slot.nft_id_proposer.unwrap_or(0)}"
                        }
                        div {
                            class: "text-lg font-bold font-mono mt-2",
                            style: format!("color:{}", color),
                            "{slot.elo_proposer} ELO"
                        }
                    }
                    // Solver
                    div {
                        class: "rounded-xl p-4",
                        style: format!("background:#0D1117;border:1px solid {}", color),
                        div {
                            class: "text-xs font-semibold mb-1",
                            style: "color:#8B949E",
                            "SOLVER"
                        }
                        div {
                            class: "font-medium text-sm",
                            style: "color:#FFFFFF",
                            "{slot.model_solver}"
                        }
                        div {
                            class: "text-xs",
                            style: "color:#8B949E",
                            "NFT #{slot.nft_id_solver.unwrap_or(0)}"
                        }
                        div {
                            class: "text-lg font-bold font-mono mt-2",
                            style: format!("color:{}", color),
                            "{slot.elo_solver} ELO"
                        }
                    }
                }

                // Scores
                div {
                    class: "rounded-xl p-4 mb-4",
                    style: "background:#0D1117",
                    div {
                        class: "flex justify-around items-center",
                        div {
                            class: "text-center",
                            div { class: "text-3xl font-bold font-mono", style: "color:#FFFFFF", "{slot.proposer_score}" }
                            div { class: "text-xs", style: "color:#8B949E", "Proposer" }
                        }
                        div {
                            class: "text-center",
                            div { class: "text-xl font-bold", style: "color:#FFD166", "VS" }
                            div { class: "text-xs font-mono", style: "color:#14F195", "{format_duration(slot.time_remaining_secs)}" }
                        }
                        div {
                            class: "text-center",
                            div { class: "text-3xl font-bold font-mono", style: "color:#FFFFFF", "{slot.solver_score}" }
                            div { class: "text-xs", style: "color:#8B949E", "Solver" }
                        }
                    }
                }

                // Reward projection
                div {
                    class: "rounded-xl p-4",
                    style: "background:#0D1117",
                    div {
                        class: "text-xs font-semibold mb-2",
                        style: "color:#8B949E",
                        "REWARD PROJECTION"
                    }
                    div {
                        class: "flex justify-between text-sm",
                        span { style: "color:#8B949E", "Base Reward:" }
                        span { style: "color:#FFFFFF", "{slot.class.base_reward():.4} ATA" }
                    }
                    div {
                        class: "flex justify-between text-sm",
                        span { style: "color:#8B949E", "Class Multiplier:" }
                        span { style: "color:#FFFFFF", "{slot.class.reward_multiplier():.1}x" }
                    }
                    div {
                        class: "flex justify-between text-sm font-semibold mt-2 pt-2",
                        style: "border-top:1px solid #30363D",
                        span { style: "color:#8B949E", "Est. Winner Reward:" }
                        span { style: format!("color:{}", color), "{slot.class.base_reward() * slot.class.reward_multiplier():.4} ATA" }
                    }
                }
            }
        }
    }
}
