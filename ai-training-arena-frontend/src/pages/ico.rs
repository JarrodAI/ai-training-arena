use dioxus::prelude::*;
use crate::components::ico::{
    PresaleTimer, NodeNftCard, NftTier, FundsRaisedBar,
    VestingDashboard, WhitelistCheck,
};
use crate::state::WalletState;
use crate::api::contracts::{encode_mint_nft_call, TransactionRequest, eth_send_transaction, request_accounts};

// Placeholder contract addresses - replace with actual deployed addresses
const NODE_TRAINER_NFT_ADDRESS: &str = "0x0000000000000000000000000000000000000001";
const NODE_SALE_ICO_ADDRESS: &str    = "0x0000000000000000000000000000000000000002";

#[component]
pub fn IcoPage() -> Element {
    let mut wallet = use_signal(WalletState::new);
    let mut buy_status = use_signal(|| String::new());

    let nft_tiers = NftTier::tiers(NODE_TRAINER_NFT_ADDRESS);

    let on_connect_wallet = move |_| {
        let mut wallet = wallet.clone();
        wasm_bindgen_futures::spawn_local(async move {
            match request_accounts().await {
                Ok(accounts) if !accounts.is_empty() => {
                    wallet.set(WalletState {
                        connected: true,
                        address: Some(accounts[0].clone()),
                        chain_id: None,
                        balance_eth: None,
                    });
                }
                Ok(_) => {}
                Err(e) => {
                    web_sys::console::error_1(&format!("Wallet error: {}", e).into());
                }
            }
        });
    };

    let on_buy_nft = move |tier_id: u8| {
        let wallet_addr = wallet.read().address.clone();
        let mut status = buy_status.clone();

        if let Some(from) = wallet_addr {
            wasm_bindgen_futures::spawn_local(async move {
                let calldata = encode_mint_nft_call(tier_id);
                let tx = TransactionRequest {
                    from,
                    to: NODE_TRAINER_NFT_ADDRESS.into(),
                    data: calldata,
                    value: None,
                };
                match eth_send_transaction(&tx).await {
                    Ok(hash) => status.set(format!("TX submitted: {}", hash)),
                    Err(e)   => status.set(format!("Error: {}", e)),
                }
            });
        }
    };

    let wallet_state = wallet.read().clone();
    let wallet_state_for_whitelist = wallet_state.clone();

    rsx! {
        div { class: "ico-page",
            // Header
            div { class: "ico-header",
                h1 { "AI Training Arena ICO" }
                p { class: "ico-tagline",
                    "Join the decentralized AI revolution. Stake. Train. Earn."
                }
                if wallet_state.is_connected() {
                    div { class: "wallet-connected",
                        span { class: "wallet-badge",
                            "Connected: {wallet_state.short_address()}"
                        }
                    }
                } else {
                    button {
                        class: "btn-connect-wallet",
                        onclick: on_connect_wallet,
                        "Connect Wallet"
                    }
                }
            }

            // Phase timer
            PresaleTimer {
                phase_name: String::from("FOUNDERS PRESALE"),
                end_timestamp_secs: 1780000000u64,
            }

            // Funds raised bar
            FundsRaisedBar {
                total_raised_usdc: 0.0,
                soft_cap_usdc: 120_000_000.0,
                hard_cap_usdc: 400_000_000.0,
            }

            // Whitelist check
            WhitelistCheck {
                address: wallet_state_for_whitelist.address.clone(),
                merkle_root: String::from("0x0000000000000000000000000000000000000000000000000000000000000000"),
            }

            // Buy status
            if !buy_status.read().is_empty() {
                div { class: "buy-status", "{buy_status}" }
            }

            // NFT Cards
            div { class: "nft-grid",
                h2 { "Node Trainer NFTs" }
                p { class: "nft-subtitle",
                    "Purchase a Node Trainer NFT to earn bonus ATA tokens and gain access to exclusive features."
                }
                div { class: "nft-cards",
                    for tier in nft_tiers.iter() {
                        NodeNftCard {
                            tier: tier.clone(),
                            wallet: wallet_state.clone(),
                            on_buy: on_buy_nft,
                        }
                    }
                }
            }

            // Vesting dashboard (empty until connected)
            VestingDashboard {
                entries: vec![],
                on_claim: move |_idx| {},
                on_claim_all: move |_| {},
            }
        }
    }
}
