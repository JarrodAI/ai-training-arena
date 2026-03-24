use dioxus::prelude::*;
use crate::types::marketplace::DataListing;
use crate::types::wallet::WalletState;
use crate::utils::formatting::format_ata;
use crate::utils::web3::eth_send_transaction;

const DATA_MARKETPLACE_ADDRESS: &str = "0x0000000000000000000000000000000000000000";

fn encode_purchase_access(listing_id: u64) -> String {
    // purchaseDataAccess(uint256) selector placeholder
    let selector = "b1c2d3e4";
    format!("0x{}{:0>64x}", selector, listing_id)
}

#[component]
pub fn DataListingCard(listing: DataListing) -> Element {
    let wallet = use_context::<Signal<WalletState>>();
    let mut tx_status = use_signal(|| String::new());
    let mut confirm_open = use_signal(|| false);

    let on_purchase = move |_| {
        let addr = wallet.read().address.clone();
        let listing_id = listing.listing_id;
        let price_ata = listing.price_ata;
        let mut status = tx_status.clone();
        let mut conf = confirm_open.clone();

        if let Some(from) = addr {
            // price in ATA (ERC-20 approval needed in production)
            let data = encode_purchase_access(listing_id);
            wasm_bindgen_futures::spawn_local(async move {
                status.set("Processing...".to_string());
                conf.set(false);
                match eth_send_transaction(&from, DATA_MARKETPLACE_ADDRESS, &data, None).await {
                    Ok(tx) => status.set(format!("Access granted! Tx: {}", tx)),
                    Err(e) => status.set(format!("Error: {}", e)),
                }
            });
        }
    };

    let color = listing.class_color();

    rsx! {
        div {
            class: "rounded-xl p-5",
            style: format!("background:#161B22;border:1px solid #30363D"),

            div {
                class: "flex items-center gap-2 mb-3",
                span {
                    class: "px-2 py-0.5 rounded text-xs font-bold",
                    style: "background:#9945FF22;color:#9945FF",
                    "{listing.category.label()}"
                }
                span {
                    class: "text-xs",
                    style: "color:#8B949E",
                    "NFT #{listing.seller_nft_id}"
                }
            }

            p {
                class: "text-sm mb-3",
                style: "color:#FFFFFF",
                "{listing.description}"
            }

            div {
                class: "flex items-center justify-between mb-4",
                div {
                    div { class: "text-xs", style: "color:#8B949E", "Price per access" }
                    div { class: "text-lg font-bold", style: "color:#14F195", "{format_ata(listing.price_ata)}" }
                }
                div {
                    class: "text-right",
                    div { class: "text-xs", style: "color:#8B949E", "Total sales" }
                    div { class: "text-sm font-bold", style: "color:#8B949E", "{listing.total_sales}" }
                }
            }

            if *confirm_open.read() {
                div {
                    class: "rounded-lg p-3 mb-3",
                    style: "background:#0D1117;border:1px solid #FFD166",
                    p { class: "text-sm mb-3", style: "color:#FFFFFF", "Purchase access for {format_ata(listing.price_ata)}?" }
                    div {
                        class: "flex gap-2",
                        button {
                            class: "flex-1 py-2 rounded-lg text-sm font-semibold",
                            style: "background:#14F195;color:#0D1117",
                            onclick: on_purchase,
                            "Confirm"
                        }
                        button {
                            class: "flex-1 py-2 rounded-lg text-sm font-semibold",
                            style: "background:#30363D;color:#8B949E",
                            onclick: move |_| confirm_open.set(false),
                            "Cancel"
                        }
                    }
                }
            } else {
                button {
                    class: "w-full py-2 rounded-lg text-sm font-semibold",
                    style: "background:#14F195;color:#0D1117",
                    onclick: move |_| confirm_open.set(true),
                    "Purchase Access"
                }
            }

            if !tx_status.read().is_empty() {
                div {
                    class: "mt-2 text-xs",
                    style: "color:#8B949E",
                    "{tx_status}"
                }
            }
        }
    }
}

// Helper to add class color to DataListing
trait DataListingExt {
    fn class_color(&self) -> &str;
}

impl DataListingExt for DataListing {
    fn class_color(&self) -> &str {
        "#9945FF"
    }
}
