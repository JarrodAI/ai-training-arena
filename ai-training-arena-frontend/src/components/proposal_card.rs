use dioxus::prelude::*;
use crate::types::marketplace::Proposal;
use crate::utils::formatting::format_large_number;

#[component]
pub fn ProposalCard(proposal: Proposal, on_vote: EventHandler<u64>) -> Element {
    let status_color = proposal.status.color().to_string();
    let status_label = proposal.status.label().to_string();
    let for_pct = proposal.for_pct();
    let against_pct = proposal.against_pct();
    let abstain_pct = proposal.abstain_pct();
    let quorum_pct = (proposal.total_votes() as f64 / 10_000_000.0 * 100.0).min(100.0);
    let is_active = matches!(proposal.status, crate::types::marketplace::ProposalStatus::Active);
    let pid = proposal.proposal_id;

    rsx! {
        div {
            class: "rounded-xl p-5",
            style: "background:#161B22;border:1px solid #30363D",

            div {
                class: "flex items-start justify-between mb-3",
                div {
                    h3 { class: "text-base font-semibold mb-1", style: "color:#FFFFFF", "{proposal.title}" }
                    div { class: "text-xs", style: "color:#8B949E", "by {&proposal.proposer[..10]}..." }
                }
                span {
                    class: "px-2 py-0.5 rounded text-xs font-bold ml-3",
                    style: format!("color:{};background:{}22", status_color, status_color),
                    "{status_label}"
                }
            }

            p { class: "text-sm mb-4", style: "color:#8B949E", "{&proposal.description[..200.min(proposal.description.len())]}..." }

            // Vote bars
            div { class: "space-y-2 mb-4",
                div {
                    div { class: "flex justify-between text-xs mb-1", style: "color:#8B949E",
                        span { "For" }
                        span { "{for_pct:.1}% ({format_large_number(proposal.votes_for as f64)} ATA)" }
                    }
                    div { class: "h-2 rounded-full", style: "background:#30363D",
                        div { class: "h-2 rounded-full", style: format!("background:#14F195;width:{}%", for_pct) }
                    }
                }
                div {
                    div { class: "flex justify-between text-xs mb-1", style: "color:#8B949E",
                        span { "Against" }
                        span { "{against_pct:.1}% ({format_large_number(proposal.votes_against as f64)} ATA)" }
                    }
                    div { class: "h-2 rounded-full", style: "background:#30363D",
                        div { class: "h-2 rounded-full", style: format!("background:#FF6B6B;width:{}%", against_pct) }
                    }
                }
                div {
                    div { class: "flex justify-between text-xs mb-1", style: "color:#8B949E",
                        span { "Abstain" }
                        span { "{abstain_pct:.1}%" }
                    }
                    div { class: "h-2 rounded-full", style: "background:#30363D",
                        div { class: "h-2 rounded-full", style: format!("background:#8B949E;width:{}%", abstain_pct) }
                    }
                }
            }

            // Quorum indicator
            div { class: "mb-4",
                div { class: "flex justify-between text-xs mb-1", style: "color:#8B949E",
                    span { "Quorum (10M ATA needed)" }
                    span {
                        style: if proposal.quorum_reached { "color:#14F195" } else { "color:#FFD166" },
                        if proposal.quorum_reached { "REACHED" } else { "{quorum_pct:.1}%" }
                    }
                }
                div { class: "h-1.5 rounded-full", style: "background:#30363D",
                    div { class: "h-1.5 rounded-full", style: format!("background:{};width:{}%",
                        if proposal.quorum_reached { "#14F195" } else { "#FFD166" }, quorum_pct) }
                }
            }

            if is_active {
                button {
                    class: "w-full py-2 rounded-lg font-semibold text-sm",
                    style: "background:#9945FF;color:#FFFFFF",
                    onclick: move |_| on_vote(pid),
                    "Vote"
                }
            }
        }
    }
}
