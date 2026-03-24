use dioxus::prelude::*;

#[derive(Clone, Debug, PartialEq)]
pub struct VestingEntry {
    pub label: String,
    pub total_ata: f64,
    pub claimed_ata: f64,
    pub claimable_ata: f64,
    pub cliff_days: u32,
    pub vest_days: u32,
    pub start_date: String,
    pub end_date: String,
    pub revoked: bool,
}

#[derive(Props, Clone, PartialEq)]
pub struct VestingDashboardProps {
    pub entries: Vec<VestingEntry>,
    pub on_claim: EventHandler<usize>,
    pub on_claim_all: EventHandler<()>,
}

#[component]
pub fn VestingDashboard(props: VestingDashboardProps) -> Element {
    let total_claimable: f64 = props.entries.iter().map(|e| e.claimable_ata).sum();
    let total_claimed: f64   = props.entries.iter().map(|e| e.claimed_ata).sum();
    let total_alloc: f64     = props.entries.iter().map(|e| e.total_ata).sum();

    rsx! {
        div { class: "vesting-dashboard",
            h2 { "Token Vesting Schedule" }

            div { class: "vesting-summary",
                div { class: "summary-stat",
                    span { class: "label", "Total Allocated:" }
                    span { class: "value", "{total_alloc:.0} ATA" }
                }
                div { class: "summary-stat",
                    span { class: "label", "Claimed:" }
                    span { class: "value", "{total_claimed:.0} ATA" }
                }
                div { class: "summary-stat",
                    span { class: "label", "Claimable Now:" }
                    span { class: "value claimable", "{total_claimable:.0} ATA" }
                }
            }

            if total_claimable > 0.0 {
                button {
                    class: "btn-claim-all",
                    onclick: move |_| props.on_claim_all.call(()),
                    "Claim All ({total_claimable:.0} ATA)"
                }
            }

            div { class: "vesting-entries",
                for (idx, entry) in props.entries.iter().enumerate() {
                    div { class: "vesting-entry",
                        div { class: "entry-header",
                            span { class: "entry-label", "{entry.label}" }
                            if entry.revoked {
                                span { class: "revoked-badge", "Revoked" }
                            }
                        }
                        div { class: "entry-progress",
                            div { class: "progress-bar",
                                div {
                                    class: "progress-fill",
                                    style: "width: {(entry.claimed_ata / entry.total_ata * 100.0).min(100.0):.1}%;",
                                }
                            }
                            span { class: "progress-text",
                                "{entry.claimed_ata:.0} / {entry.total_ata:.0} ATA claimed"
                            }
                        }
                        div { class: "entry-details",
                            span { "Cliff: {entry.cliff_days}d" }
                            span { "Vest: {entry.vest_days}d linear" }
                            span { "Start: {entry.start_date}" }
                            span { "End: {entry.end_date}" }
                        }
                        if entry.claimable_ata > 0.0 && !entry.revoked {
                            button {
                                class: "btn-claim",
                                onclick: move |_| props.on_claim.call(idx),
                                "Claim {entry.claimable_ata:.0} ATA"
                            }
                        }
                    }
                }
            }
        }
    }
}
