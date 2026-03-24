use dioxus::prelude::*;
use crate::types::leaderboard::LeaderboardEntry;
use crate::utils::formatting::{format_ata, format_win_rate};

#[component]
pub fn LeaderboardRow(entry: LeaderboardEntry, rank: usize, is_mine: bool) -> Element {
    let rank_display = match rank {
        1 => "Gold".to_string(),
        2 => "Silver".to_string(),
        3 => "Bronze".to_string(),
        n => n.to_string(),
    };

    let rank_style = match rank {
        1 => "color:#FFD700;font-weight:bold",
        2 => "color:#C0C0C0;font-weight:bold",
        3 => "color:#CD7F32;font-weight:bold",
        _ => "color:#8B949E",
    };

    let row_bg = if is_mine {
        "background:rgba(153,69,255,0.1);border-left:3px solid #9945FF"
    } else if rank <= 3 {
        "background:rgba(20,241,149,0.05)"
    } else {
        "background:transparent"
    };

    let color = entry.class.color().to_string();

    rsx! {
        tr {
            style: "{row_bg}",
            td {
                class: "px-4 py-3 text-center text-sm font-mono",
                style: rank_style,
                "{rank_display}"
            }
            td {
                class: "px-4 py-3 font-mono text-sm",
                style: "color:#8B949E",
                "#{entry.nft_id}"
            }
            td {
                class: "px-4 py-3 text-sm font-medium",
                style: "color:#FFFFFF",
                "{entry.model_name}"
            }
            td {
                class: "px-4 py-3",
                span {
                    class: "px-2 py-0.5 rounded text-xs font-bold",
                    style: format!("background:{};color:#0D1117", color),
                    "{entry.class}"
                }
            }
            td {
                class: "px-4 py-3 font-mono font-bold",
                style: format!("color:{}", color),
                "{entry.elo_rating}"
            }
            td {
                class: "px-4 py-3 text-sm text-center",
                style: "color:#8B949E",
                "{entry.total_battles}"
            }
            td {
                class: "px-4 py-3",
                div {
                    class: "flex items-center gap-2",
                    div {
                        class: "flex-1 h-2 rounded-full",
                        style: "background:#30363D",
                        div {
                            class: "h-2 rounded-full",
                            style: format!(
                                "background:{};width:{}%",
                                color,
                                (entry.win_rate * 100.0).min(100.0)
                            ),
                        }
                    }
                    span {
                        class: "text-xs font-mono",
                        style: "color:#8B949E",
                        "{format_win_rate(entry.win_rate)}"
                    }
                }
            }
            td {
                class: "px-4 py-3 text-sm font-mono",
                style: "color:#14F195",
                "{format_ata(entry.total_rewards)}"
            }
        }
    }
}
