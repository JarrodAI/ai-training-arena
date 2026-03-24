use dioxus::prelude::*;

#[derive(Props, Clone, PartialEq)]
pub struct WhitelistCheckProps {
    pub address: Option<String>,
    pub merkle_root: String,
}

#[derive(Clone, Debug, PartialEq)]
pub enum WhitelistStatus {
    NotChecked,
    Checking,
    Whitelisted { proof: Vec<String> },
    NotWhitelisted,
    NoWallet,
}

#[component]
pub fn WhitelistCheck(props: WhitelistCheckProps) -> Element {
    let status = use_signal(|| match &props.address {
        None => WhitelistStatus::NoWallet,
        Some(_) => WhitelistStatus::NotChecked,
    });

    let status_read = status.read();

    rsx! {
        div { class: "whitelist-check",
            h3 { "Founders Presale Whitelist" }

            match &*status_read {
                WhitelistStatus::NoWallet => rsx! {
                    div { class: "whitelist-status no-wallet",
                        span { "Connect your wallet to check whitelist status" }
                    }
                },
                WhitelistStatus::NotChecked => rsx! {
                    div { class: "whitelist-status not-checked",
                        if let Some(addr) = &props.address {
                            p { "Checking: {addr}" }
                        }
                        button {
                            class: "btn-check",
                            onclick: move |_| {
                                // In full impl: compute Merkle proof client-side
                                // using the merkle_root and a known leaf set
                            },
                            "Check Whitelist Status"
                        }
                    }
                },
                WhitelistStatus::Checking => rsx! {
                    div { class: "whitelist-status checking",
                        span { "Verifying Merkle proof..." }
                    }
                },
                WhitelistStatus::Whitelisted { .. } => rsx! {
                    div { class: "whitelist-status whitelisted",
                        span { class: "check-icon", "Whitelisted for Founders Presale" }
                        p { "You have access to the founders presale at $3.00/ATA" }
                        p { class: "proof-preview",
                            "Merkle proof verified"
                        }
                    }
                },
                WhitelistStatus::NotWhitelisted => rsx! {
                    div { class: "whitelist-status not-whitelisted",
                        span { class: "x-icon", "Not on founders whitelist" }
                        p { "You can participate in public sale tiers (TIER_1 - TIER_4)" }
                    }
                },
            }
        }
    }
}
