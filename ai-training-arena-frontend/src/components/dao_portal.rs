use dioxus::prelude::*;
use crate::components::{proposal_card::ProposalCard, vote_modal::VoteModal};
use crate::types::{marketplace::{Proposal, ProposalStatus}, wallet::WalletState};
use crate::utils::formatting::format_large_number;
use crate::utils::web3::eth_send_transaction;

const GOVERNOR_ADDRESS: &str = "0x0000000000000000000000000000000000000000";

#[component]
pub fn DaoPortal() -> Element {
    let wallet = use_context::<Signal<WalletState>>();
    let mut proposals: Signal<Vec<Proposal>> = use_signal(Vec::new);
    let mut vote_modal_pid: Signal<Option<u64>> = use_signal(|| None);
    let mut show_create = use_signal(|| false);
    let mut create_title = use_signal(|| String::new());
    let mut create_desc = use_signal(|| String::new());
    let mut tx_status = use_signal(|| String::new());

    use_effect(move || { proposals.set(vec![]); });

    let w = wallet.read();
    let voting_power = w.ata_balance + w.wata_balance * 2.0;

    let on_create_proposal = move |_| {
        let addr = wallet.read().address.clone();
        if let Some(from) = addr {
            let title = create_title.read().clone();
            let mut status = tx_status.clone();
            if title.is_empty() { status.set("Title required".to_string()); return; }
            let data = format!("0xb58d3a61{:0>64x}", proposals.read().len() + 1);
            wasm_bindgen_futures::spawn_local(async move {
                status.set("Submitting proposal...".to_string());
                match eth_send_transaction(&from, GOVERNOR_ADDRESS, &data, None).await {
                    Ok(tx) => { status.set(format!("Proposal submitted! Tx: {}", tx)); }
                    Err(e) => status.set(format!("Error: {}", e)),
                }
            });
        }
    };

    let active_proposals: Vec<Proposal> = proposals.read().iter()
        .filter(|p| matches!(p.status, ProposalStatus::Active))
        .cloned().collect();
    let past_proposals: Vec<Proposal> = proposals.read().iter()
        .filter(|p| !matches!(p.status, ProposalStatus::Active))
        .cloned().collect();

    rsx! {
        div {
            class: "min-h-screen",
            style: "background:#0D1117",
            div {
                class: "max-w-7xl mx-auto px-4 py-8",
                h1 { class: "text-3xl font-bold mb-6", style: "color:#FFFFFF", "DAO Governance" }
                if w.is_connected() {
                    button {
                        class: "px-4 py-2 rounded-lg font-semibold mb-6",
                        style: "background:#9945FF;color:#FFFFFF",
                        onclick: move |_| show_create.set(!show_create()),
                        if *show_create.read() { "Cancel" } else { "Create Proposal" }
                    }
                }
                if *show_create.read() && w.is_connected() {
                    div {
                        class: "rounded-xl p-6 mb-8",
                        style: "background:#161B22;border:1px solid #9945FF",
                        h2 { class: "text-lg font-semibold mb-4", style: "color:#FFFFFF", "New Proposal" }
                        div { class: "space-y-4",
                            input {
                                class: "w-full px-3 py-2 rounded-lg text-sm",
                                style: "background:#0D1117;border:1px solid #30363D;color:#FFFFFF",
                                placeholder: "Title...",
                                value: "{create_title}",
                                oninput: move |e| create_title.set(e.value()),
                            }
                            textarea {
                                class: "w-full px-3 py-2 rounded-lg text-sm",
                                style: "background:#0D1117;border:1px solid #30363D;color:#FFFFFF;resize:none",
                                rows: "4",
                                placeholder: "Description...",
                                value: "{create_desc}",
                                oninput: move |e| create_desc.set(e.value()),
                            }
                            button {
                                class: "w-full py-3 rounded-xl font-bold",
                                style: "background:#9945FF;color:#FFFFFF",
                                onclick: on_create_proposal,
                                "Submit Proposal"
                            }
                            if !tx_status.read().is_empty() {
                                div { class: "text-xs", style: "color:#8B949E", "{tx_status}" }
                            }
                        }
                    }
                }
                h2 { class: "text-xl font-semibold mb-4", style: "color:#FFFFFF", "Active Proposals" }
                if active_proposals.is_empty() {
                    div { class: "text-center py-8", p { style: "color:#8B949E", "No active proposals" } }
                } else {
                    div { class: "space-y-4 mb-8",
                        for proposal in active_proposals {
                            {
                                let pid = proposal.proposal_id;
                                rsx! {
                                    ProposalCard {
                                        key: "{pid}",
                                        proposal: proposal,
                                        on_vote: move |id| vote_modal_pid.set(Some(id)),
                                    }
                                }
                            }
                        }
                    }
                }
                h2 { class: "text-xl font-semibold mb-4", style: "color:#FFFFFF", "Past Proposals" }
                div { class: "space-y-4",
                    for proposal in past_proposals.into_iter().take(10) {
                        {
                            let pid = proposal.proposal_id;
                            rsx! {
                                ProposalCard {
                                    key: "{pid}",
                                    proposal: proposal,
                                    on_vote: move |id| vote_modal_pid.set(Some(id)),
                                }
                            }
                        }
                    }
                }
            }
        }
        if let Some(pid) = *vote_modal_pid.read() {
            VoteModal {
                proposal_id: pid,
                show: true,
                on_close: move |_| vote_modal_pid.set(None),
            }
        }
    }
}
