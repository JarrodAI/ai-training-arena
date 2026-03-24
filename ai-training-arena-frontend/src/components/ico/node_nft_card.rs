use dioxus::prelude::*;
use crate::state::WalletState;

#[derive(Clone, Debug, PartialEq)]
pub struct NftTier {
    pub tier_id: u8,
    pub name: String,
    pub price_usdc: String,
    pub bonus_ata: String,
    pub max_supply: u32,
    pub remaining: u32,
    pub color: String,
    pub contract_address: String,
}

impl NftTier {
    pub fn tiers(contract_address: &str) -> Vec<NftTier> {
        vec![
            NftTier {
                tier_id: 0,
                name: "Bronze".into(),
                price_usdc: "5,000".into(),
                bonus_ata: "10,000".into(),
                max_supply: 15000,
                remaining: 15000,
                color: "#cd7f32".into(),
                contract_address: contract_address.into(),
            },
            NftTier {
                tier_id: 1,
                name: "Silver".into(),
                price_usdc: "25,000".into(),
                bonus_ata: "75,000".into(),
                max_supply: 6000,
                remaining: 6000,
                color: "#c0c0c0".into(),
                contract_address: contract_address.into(),
            },
            NftTier {
                tier_id: 2,
                name: "Gold".into(),
                price_usdc: "100,000".into(),
                bonus_ata: "400,000".into(),
                max_supply: 2500,
                remaining: 2500,
                color: "#ffd700".into(),
                contract_address: contract_address.into(),
            },
            NftTier {
                tier_id: 3,
                name: "Platinum".into(),
                price_usdc: "400,000".into(),
                bonus_ata: "2,000,000".into(),
                max_supply: 1200,
                remaining: 1200,
                color: "#e5e4e2".into(),
                contract_address: contract_address.into(),
            },
            NftTier {
                tier_id: 4,
                name: "Diamond".into(),
                price_usdc: "1,500,000".into(),
                bonus_ata: "10,000,000".into(),
                max_supply: 300,
                remaining: 300,
                color: "#b9f2ff".into(),
                contract_address: contract_address.into(),
            },
        ]
    }
}

#[derive(Props, Clone, PartialEq)]
pub struct NodeNftCardProps {
    pub tier: NftTier,
    pub wallet: WalletState,
    pub on_buy: EventHandler<u8>,
}

#[component]
pub fn NodeNftCard(props: NodeNftCardProps) -> Element {
    let sold_out = props.tier.remaining == 0;
    let can_buy = props.wallet.is_connected() && !sold_out;
    let tier_id = props.tier.tier_id;

    rsx! {
        div {
            class: "nft-card",
            style: "border-color: {props.tier.color};",

            // 3D card visual placeholder (Canvas would be used with WebGl2RenderingContext)
            div { class: "nft-visual",
                style: "background: linear-gradient(135deg, {props.tier.color}44, {props.tier.color}88);",
                div { class: "nft-tier-badge",
                    style: "color: {props.tier.color};",
                    "{props.tier.name}"
                }
                div { class: "nft-glow",
                    style: "box-shadow: 0 0 20px {props.tier.color}66;"
                }
            }

            div { class: "nft-info",
                h3 { class: "nft-name", "{props.tier.name} Node Trainer" }

                div { class: "nft-stat",
                    span { class: "label", "Price: " }
                    span { class: "value", "${props.tier.price_usdc} USDC" }
                }
                div { class: "nft-stat",
                    span { class: "label", "Bonus ATA: " }
                    span { class: "value", "{props.tier.bonus_ata} ATA" }
                }
                div { class: "nft-stat",
                    span { class: "label", "Remaining: " }
                    span { class: "value", "{props.tier.remaining} / {props.tier.max_supply}" }
                }

                if sold_out {
                    button { class: "btn-sold-out", disabled: true, "SOLD OUT" }
                } else if can_buy {
                    button {
                        class: "btn-buy",
                        onclick: move |_| props.on_buy.call(tier_id),
                        "Buy {props.tier.name} NFT"
                    }
                } else {
                    button { class: "btn-connect", disabled: true, "Connect Wallet to Buy" }
                }
            }
        }
    }
}
