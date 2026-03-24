use dioxus::prelude::*;
use crate::types::wallet::WalletState;
use crate::utils::web3::eth_send_transaction;
use crate::utils::formatting::format_large_number;

const GOVERNOR_ADDRESS: &str = "0x0000000000000000000000000000000000000000";

fn encode_cast_vote(proposal_id: u64, support: u8, reason: &str) -> String {
    // castVoteWithReason(uint256,uint8,string) selector placeholder
    let selector = "7b3c71d3";
    format!("0x{}{:0>64x}{:0>64x}", selector, proposal_id, support)
}

#[component]
pub fn VoteModal(
    proposal_id: u64,
    show: bool,
    on_close: EventHandler<()>,
) -> Element {
    let wallet = use_context::<Signal<WalletState>>();
    let mut vote_choice: Signal<Option<u8>> = use_signal(|| None);
    let mut reason = use_signal(|| String::new());
    let mut tx_status = use_signal(|| String::new());

    if !show {
        return rsx! {};
    }

    let w = wallet.read();
    let voting_power = w.ata_balance + w.wata_balance * 2.0;

    let on_vote = move |_| {
        let choice = *vote_choice.read();
        let addr = wallet.read().address.clone();
        if let (Some(support), Some(from)) = (choice, addr) {
            let reason_str = reason.read().clone();
            let mut status = tx_status.clone();
            let data = encode_cast_vote(proposal_id, support, &reason_str);
            wasm_bindgen_futures::spawn_local(async move {
                status.set("Submitting vote...".to_string());
                match eth_send_transaction(&from, GOVERNOR_ADDRESS, &data, None).await {
                    Ok(tx) => status.set(format!("Vote cast! Tx: {}", tx)),
                    Err(e) => status.set(format!("Error: {}", e)),
                }
            });
        } else {
            tx_status.set("Please select a vote option".to_string());
        }
    };

    rsx! {
        div {
            class: "fixed inset-0 z-50 flex items-center justify-center p-4",
            style: "background:rgba(0,0,0,0.85)",
            onclick: move |_| on_close(()),
            div {
                class: "w-full max-w-md rounded-2xl p-6",
                style: "background:#161B22;border:1px solid #30363D",
                onclick: |e| e.stop_propagation(),

                div {
                    class: "flex justify-between items-center mb-6",
                    h2 { class: "text-xl font-bold", style: "color:#FFFFFF", "Cast Vote" }
                    button {
                        class: "w-8 h-8 rounded-full", style: "background:#30363D;color:#FFFFFF",
                        onclick: move |_| on_close(()),
                        "x"
                    }
                }

                div {
                    class: "px-3 py-2 rounded-lg mb-4",
                    style: "background:#0D1117",
                    div { class: "text-xs", style: "color:#8B949E", "Your Voting Power" }
                    div { class: "text-xl font-bold", style: "color:#9945FF", "{format_large_number(voting_power)} votes" }
                }

                div { class: "space-y-3 mb-4",
                    for (label, color, value) in [
                        ("For", "#14F195", 1u8),
                        ("Against", "#FF6B6B", 0u8),
                        ("Abstain", "#8B949E", 2u8),
                    ] {
                        {
                            let is_selected = *vote_choice.read() == Some(value);
                            rsx! {
                                button {
                                    key: "{label}",
                                    class: "w-full py-3 rounded-xl font-semibold text-sm transition-all",
                                    style: if is_selected {
                                        format!("background:{};color:#0D1117;border:2px solid {}", color, color)
                                    } else {
                                        format!("background:#0D1117;color:{};border:2px solid {}44", color, color)
                                    },
                                    onclick: move |_| vote_choice.set(Some(value)),
                                    "{label}"
                                }
                            }
                        }
                    }
                }

                div { class: "mb-4",
                    label { class: "block text-xs font-medium mb-1", style: "color:#8B949E", "Reason (optional)" }
                    textarea {
                        class: "w-full px-3 py-2 rounded-lg text-sm",
                        style: "background:#0D1117;border:1px solid #30363D;color:#FFFFFF;resize:none",
                        rows: "3",
                        placeholder: "Why are you voting this way?",
                        value: "{reason}",
                        oninput: move |e| reason.set(e.value()),
                    }
                }

                button {
                    class: "w-full py-3 rounded-xl font-bold",
                    style: "background:#9945FF;color:#FFFFFF",
                    onclick: on_vote,
                    "Submit Vote"
                }

                if !tx_status.read().is_empty() {
                    div { class: "mt-3 text-xs p-2 rounded", style: "background:#0D1117;color:#8B949E", "{tx_status}" }
                }
            }
        }
    }
}
