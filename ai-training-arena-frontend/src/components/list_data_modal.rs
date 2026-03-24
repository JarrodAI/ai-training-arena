use dioxus::prelude::*;
use crate::api::NodeApi;
use crate::types::agent::Agent;
use crate::types::marketplace::DataCategory;
use crate::utils::web3::eth_send_transaction;

const DATA_MARKETPLACE_ADDRESS: &str = "0x0000000000000000000000000000000000000000";

fn encode_list_data(_nft_id: u64, _ipfs_hash: &str, _price_ata_wei: &str) -> String {
    "0xdeadbeef00000000000000000000000000000000000000000000000000000001".to_string()
}

#[component]
pub fn ListDataModal(
    show: bool,
    on_close: EventHandler<()>,
    owned_agents: Vec<Agent>,
    wallet_address: Option<String>,
) -> Element {
    let mut selected_nft = use_signal(|| owned_agents.first().map(|a| a.nft_id).unwrap_or(0));
    let mut category = use_signal(|| DataCategory::BattleLog);
    let mut ipfs_hash = use_signal(|| String::new());
    let mut price = use_signal(|| String::new());
    let mut tx_status = use_signal(|| String::new());

    if !show {
        return rsx! {};
    }

    let on_pin = move |_| {
        let mut status = tx_status.clone();
        let data_str = format!("nft_id:{}", selected_nft());
        let mut hash = ipfs_hash.clone();
        wasm_bindgen_futures::spawn_local(async move {
            status.set("Pinning to IPFS...".to_string());
            match NodeApi::pin_to_ipfs(&data_str).await {
                Ok(cid) => { hash.set(cid.clone()); status.set(format!("Pinned: {}", cid)); }
                Err(e) => status.set(format!("Error: {}", e)),
            }
        });
    };

    let on_list = move |_| {
        let addr = wallet_address.clone();
        if let Some(from) = addr {
            let nft_id = *selected_nft.read();
            let hash = ipfs_hash.read().clone();
            let price_str = price.read().clone();
            let mut status = tx_status.clone();
            if hash.is_empty() || price_str.is_empty() {
                status.set("Please fill all fields".to_string());
                return;
            }
            if let Ok(price_val) = price_str.parse::<f64>() {
                let price_wei = format!("0x{:x}", (price_val * 1e18) as u64);
                let data = encode_list_data(nft_id, &hash, &price_wei);
                wasm_bindgen_futures::spawn_local(async move {
                    status.set("Submitting...".to_string());
                    match eth_send_transaction(&from, DATA_MARKETPLACE_ADDRESS, &data, None).await {
                        Ok(tx) => status.set(format!("Listed! Tx: {}", tx)),
                        Err(e) => status.set(format!("Error: {}", e)),
                    }
                });
            }
        }
    };

    rsx! {
        div {
            class: "fixed inset-0 z-50 flex items-center justify-center p-4",
            style: "background:rgba(0,0,0,0.85)",
            onclick: move |_| on_close(()),
            div {
                class: "w-full max-w-lg rounded-2xl p-6",
                style: "background:#161B22;border:1px solid #30363D",
                onclick: |e| e.stop_propagation(),
                div {
                    class: "flex justify-between items-center mb-6",
                    h2 { class: "text-xl font-bold", style: "color:#FFFFFF", "List My Data" }
                    button {
                        class: "w-8 h-8 rounded-full", style: "background:#30363D;color:#FFFFFF",
                        onclick: move |_| on_close(()),
                        "x"
                    }
                }
                div { class: "space-y-4",
                    div {
                        label { class: "block text-xs font-medium mb-1", style: "color:#8B949E", "Select NFT" }
                        select {
                            class: "w-full px-3 py-2 rounded-lg text-sm",
                            style: "background:#0D1117;border:1px solid #30363D;color:#FFFFFF",
                            onchange: move |e| { if let Ok(id) = e.value().parse::<u64>() { selected_nft.set(id); } },
                            for agent in owned_agents.iter() {
                                option {
                                    key: "{agent.nft_id}",
                                    value: "{agent.nft_id}",
                                    "NFT #{agent.nft_id} - {agent.model_name}"
                                }
                            }
                        }
                    }
                    div {
                        label { class: "block text-xs font-medium mb-1", style: "color:#8B949E", "Category" }
                        select {
                            class: "w-full px-3 py-2 rounded-lg text-sm",
                            style: "background:#0D1117;border:1px solid #30363D;color:#FFFFFF",
                            onchange: move |e| {
                                let cat = match e.value().as_str() {
                                    "BATTLE_LOG" => DataCategory::BattleLog,
                                    "MODEL_CHECKPOINT" => DataCategory::ModelCheckpoint,
                                    "QUESTION_CORPUS" => DataCategory::QuestionCorpus,
                                    "TRAINING_SET" => DataCategory::TrainingSet,
                                    _ => DataCategory::BattleLog,
                                };
                                category.set(cat);
                            },
                            for cat in DataCategory::all().into_iter().skip(1) {
                                option { key: "{cat.label()}", value: "{cat.label()}", "{cat.label()}" }
                            }
                        }
                    }
                    div {
                        label { class: "block text-xs font-medium mb-1", style: "color:#8B949E", "IPFS Hash (CID)" }
                        div { class: "flex gap-2",
                            input {
                                class: "flex-1 px-3 py-2 rounded-lg text-sm",
                                style: "background:#0D1117;border:1px solid #30363D;color:#FFFFFF",
                                placeholder: "Qm...",
                                value: "{ipfs_hash}",
                                oninput: move |e| ipfs_hash.set(e.value()),
                            }
                            button {
                                class: "px-3 py-2 rounded-lg text-xs font-semibold",
                                style: "background:#9945FF;color:#FFFFFF",
                                onclick: on_pin,
                                "Pin"
                            }
                        }
                    }
                    div {
                        label { class: "block text-xs font-medium mb-1", style: "color:#8B949E", "Price per Access (ATA)" }
                        input {
                            class: "w-full px-3 py-2 rounded-lg text-sm",
                            style: "background:#0D1117;border:1px solid #30363D;color:#FFFFFF",
                            placeholder: "10.0",
                            value: "{price}",
                            oninput: move |e| price.set(e.value()),
                        }
                    }
                    button {
                        class: "w-full py-3 rounded-xl font-bold",
                        style: "background:linear-gradient(135deg,#9945FF,#14F195);color:#FFFFFF",
                        onclick: on_list,
                        "List Data"
                    }
                    if !tx_status.read().is_empty() {
                        div { class: "text-xs p-2 rounded", style: "background:#0D1117;color:#8B949E", "{tx_status}" }
                    }
                }
            }
        }
    }
}
