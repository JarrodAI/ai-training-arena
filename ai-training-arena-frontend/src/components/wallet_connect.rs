use dioxus::prelude::*;
use crate::types::wallet::WalletState;
use crate::utils::web3::{request_accounts, get_chain_id, switch_to_mantle};
use crate::utils::formatting::format_address_short;

#[component]
pub fn WalletConnect() -> Element {
    let mut wallet = use_context::<Signal<WalletState>>();
    let mut error = use_signal(|| String::new());

    let on_connect = move |_| {
        let mut w = wallet.clone();
        let mut err = error.clone();
        wasm_bindgen_futures::spawn_local(async move {
            match request_accounts().await {
                Ok(address) => {
                    match get_chain_id().await {
                        Ok(chain_id) => {
                            w.write().set_connected(address, chain_id);
                            err.set(String::new());
                        }
                        Err(e) => err.set(e),
                    }
                }
                Err(e) => err.set(e),
            }
        });
    };

    let on_switch_network = move |_| {
        let mut err = error.clone();
        wasm_bindgen_futures::spawn_local(async move {
            if let Err(e) = switch_to_mantle().await {
                err.set(e);
            }
        });
    };

    let w = wallet.read();

    if w.is_connected() {
        rsx! {
            div {
                class: "flex items-center gap-3",
                div {
                    class: "flex items-center gap-2 px-3 py-1.5 rounded-full",
                    style: "background:#161B22;border:1px solid #14F195",
                    div {
                        class: "w-2 h-2 rounded-full animate-pulse",
                        style: "background:#14F195",
                    }
                    span {
                        class: "text-xs font-mono font-semibold",
                        style: "color:#FFFFFF",
                        "{format_address_short(w.address.as_deref().unwrap_or(\"\"))}"
                    }
                    span {
                        class: "text-xs font-mono",
                        style: "color:#14F195",
                        "{w.ata_balance:.2} ATA"
                    }
                }
                if !w.is_on_mantle() {
                    button {
                        class: "px-3 py-1.5 rounded-full text-xs font-semibold",
                        style: "background:#FFD166;color:#0D1117",
                        onclick: on_switch_network,
                        "Switch to Mantle"
                    }
                }
            }
        }
    } else {
        rsx! {
            div {
                button {
                    class: "px-4 py-2 rounded-lg font-semibold text-sm flex items-center gap-2",
                    style: "background:#9945FF;color:#FFFFFF",
                    onclick: on_connect,
                    "Connect Wallet"
                }
                if !error.read().is_empty() {
                    div {
                        class: "text-xs mt-1",
                        style: "color:#FF6B6B",
                        "{error}"
                    }
                }
            }
        }
    }
}
