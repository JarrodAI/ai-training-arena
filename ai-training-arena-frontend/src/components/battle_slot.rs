use dioxus::prelude::*;
use crate::types::battle::{BattleSlot, BattleStatus};
use crate::utils::formatting::format_duration;

#[component]
pub fn BattleSlotCard(
    slot: BattleSlot,
    position: usize,
    on_click: EventHandler<BattleSlot>,
) -> Element {
    let color = slot.class.color().to_string();
    let slot_clone = slot.clone();

    let (border_style, status_content) = match &slot.status {
        BattleStatus::Matchmaking => (
            format!("border:2px solid {};animation:pulse 2s infinite;opacity:0.6", color),
            rsx! {
                div {
                    class: "text-center py-4",
                    div {
                        class: "text-xs font-semibold tracking-widest mb-2",
                        style: "color:#8B949E",
                        "FINDING OPPONENT..."
                    }
                    div {
                        class: "w-6 h-6 mx-auto border-2 border-t-transparent rounded-full animate-spin",
                        style: format!("border-color:{}", color),
                    }
                }
            },
        ),
        BattleStatus::Live => (
            format!("border:2px solid {};box-shadow:0 0 12px {}", color, color),
            rsx! {
                div {
                    class: "py-2",
                    div {
                        class: "flex justify-between items-center mb-2",
                        span {
                            class: "text-xs font-mono",
                            style: "color:#8B949E",
                            "#{slot.nft_id_proposer.unwrap_or(0)}"
                        }
                        span {
                            class: "text-sm font-bold",
                            style: "color:#FFFFFF",
                            "{slot.proposer_score} - {slot.solver_score}"
                        }
                        span {
                            class: "text-xs font-mono",
                            style: "color:#8B949E",
                            "#{slot.nft_id_solver.unwrap_or(0)}"
                        }
                    }
                    div {
                        class: "text-center text-xs font-mono",
                        style: "color:#FFD166",
                        "{format_duration(slot.time_remaining_secs)}"
                    }
                }
            },
        ),
        BattleStatus::Completed => (
            format!("border:2px solid {}", color),
            rsx! {
                div {
                    class: "text-center py-2",
                    div {
                        class: "text-xs font-semibold",
                        style: "color:#14F195",
                        "COMPLETED"
                    }
                }
            },
        ),
    };

    rsx! {
        div {
            class: "relative rounded-xl p-3 cursor-pointer hover:scale-105 transition-transform select-none",
            style: format!("background:#161B22;{}", border_style),
            onclick: move |_| on_click(slot_clone.clone()),

            // Position badge
            div {
                class: "absolute top-2 left-2 w-5 h-5 rounded-full flex items-center justify-center text-xs font-bold",
                style: format!("background:{};color:#0D1117", color),
                "{position + 1}"
            }

            // Class badge
            div {
                class: "absolute top-2 right-2 px-2 py-0.5 rounded text-xs font-bold",
                style: format!("background:{};color:#0D1117", color),
                "{slot.class}"
            }

            // Agent names
            div {
                class: "mt-5 space-y-1",
                div {
                    class: "text-xs font-medium truncate",
                    style: "color:#FFFFFF",
                    "{slot.model_proposer}"
                }
                div {
                    class: "text-center text-xs font-bold",
                    style: "color:#8B949E",
                    "VS"
                }
                div {
                    class: "text-xs font-medium truncate",
                    style: "color:#FFFFFF",
                    "{slot.model_solver}"
                }
            }

            // Status content
            {status_content}

            // Elo display
            div {
                class: "flex justify-between text-xs mt-1",
                style: "color:#8B949E",
                span { "{slot.elo_proposer}" }
                span { "ELO" }
                span { "{slot.elo_solver}" }
            }
        }
    }
}
