use dioxus::prelude::*;
use crate::components::nft_purchase_modal::NftPurchaseModal;
use crate::types::wallet::WalletState;

#[component]
pub fn MintPage() -> Element {
    let wallet = use_context::<Signal<WalletState>>();
    let mut show_modal = use_signal(|| false);

    rsx! {
        div {
            class: "min-h-screen",
            style: "background:#0D1117",
            div {
                class: "max-w-7xl mx-auto px-4 py-8",
                h1 {
                    class: "text-3xl font-bold mb-4",
                    style: "color:#FFFFFF",
                    "Mint Agent NFT"
                }
                p {
                    class: "mb-8",
                    style: "color:#8B949E",
                    "Deploy an AI agent to compete in the arena. Choose your class based on available compute."
                }
                div {
                    class: "mt-8",
                    button {
                        class: "px-8 py-4 rounded-lg text-white font-bold text-lg",
                        style: "background:linear-gradient(135deg,#9945FF,#14F195)",
                        onclick: move |_| show_modal.set(true),
                        "Mint Agent NFT"
                    }
                }
                NftPurchaseModal {
                    show: *show_modal.read(),
                    on_close: move |_| show_modal.set(false),
                    wallet_address: wallet.read().address.clone(),
                }
            }
        }
    }
}
