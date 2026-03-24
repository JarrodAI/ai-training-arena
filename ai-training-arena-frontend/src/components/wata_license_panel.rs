use dioxus::prelude::*;
use crate::types::agent::AgentClass;
use crate::utils::formatting::format_ata;

#[component]
pub fn WataLicensePanel(wallet_address: Option<String>, wata_balance: f64) -> Element {
    let mut selected_class = use_signal(|| AgentClass::A);
    let mut status = use_signal(|| String::new());
    let mut active_license: Signal<Option<(AgentClass, f64)>> = use_signal(|| None);

    let on_stake_license = move |_| {
        let cls = selected_class.read().clone();
        let required = cls.stake_wata() as f64;
        if wata_balance < required {
            status.set(format!("Insufficient wATA. Need {}, have {}", required, wata_balance));
            return;
        }
        let mut s = status.clone();
        let mut al = active_license.clone();
        let cls_clone = cls.clone();
        wasm_bindgen_futures::spawn_local(async move {
            s.set("Activating license...".to_string());
            // In production: call WrappedATA.stake on-chain
            al.set(Some((cls_clone, required)));
            s.set("License activated!".to_string());
        });
    };

    let on_withdraw = move |_| {
        let mut s = status.clone();
        let mut al = active_license.clone();
        if web_sys::window()
            .unwrap()
            .confirm_with_message("Withdrawing license has a 7-day cooldown. Continue?")
            .unwrap_or(false)
        {
            al.set(None);
            s.set("Withdrawal initiated. Funds available in 7 days.".to_string());
        }
    };

    rsx! {
        div {
            class: "rounded-xl p-6",
            style: "background:#161B22;border:1px solid #30363D",
            h2 { class: "text-xl font-semibold mb-2", style: "color:#FFFFFF", "wATA License" }
            p { class: "text-sm mb-4", style: "color:#8B949E", "Battle without owning an NFT — stake wATA for a class license." }

            // Balance
            div {
                class: "flex items-center justify-between px-3 py-2 rounded-lg mb-4",
                style: "background:#0D1117",
                span { style: "color:#8B949E;font-size:12px", "wATA Balance:" }
                span { class: "font-mono font-bold", style: "color:#9945FF", "{format_ata(wata_balance)}" }
            }

            // Active license display
            if let Some((cls, staked)) = active_license.read().clone() {
                div {
                    class: "rounded-xl p-4 mb-4",
                    style: format!("background:#0D1117;border:2px solid {}", cls.color()),
                    div {
                        class: "flex justify-between items-start",
                        div {
                            div { class: "text-xs", style: "color:#8B949E", "ACTIVE LICENSE" }
                            div { class: "text-lg font-bold", style: format!("color:{}", cls.color()), "Class {cls} · {cls.name()}" }
                            div { class: "text-sm", style: "color:#8B949E", "{format_ata(staked)} staked" }
                            div { class: "text-xs mt-1", style: "color:#8B949E", "No expiry until withdrawal" }
                        }
                        button {
                            class: "px-3 py-1.5 rounded-lg text-xs font-semibold",
                            style: "background:#FF6B6B22;color:#FF6B6B;border:1px solid #FF6B6B",
                            onclick: on_withdraw,
                            "Withdraw"
                        }
                    }
                }
            }

            // Class selection
            h3 { class: "text-sm font-semibold mb-3", style: "color:#FFFFFF", "Select License Class" }
            div {
                class: "grid grid-cols-5 gap-2 mb-4",
                for class in AgentClass::all() {
                    {
                        let cls = class.clone();
                        let cls_key = format!("{cls}");
                        let is_selected = *selected_class.read() == cls;
                        let color = cls.color().to_string();
                        let color2 = color.clone();
                        let can_afford = wata_balance >= cls.stake_wata() as f64;
                        rsx! {
                            button {
                                key: "{cls_key}",
                                class: "rounded-lg p-2 text-center transition-all",
                                style: if is_selected {
                                    format!("background:{};color:#0D1117;border:2px solid {}", color, color2)
                                } else if !can_afford {
                                    "background:#0D1117;color:#30363D;border:2px solid #30363D;opacity:0.5".to_string()
                                } else {
                                    format!("background:#0D1117;color:{};border:2px solid {}", color, color2)
                                },
                                onclick: move |_| selected_class.set(cls.clone()),
                                div { class: "text-xs font-bold", "{cls}" }
                                div { class: "text-xs mt-1", "{cls.stake_wata()} wATA" }
                            }
                        }
                    }
                }
            }

            button {
                class: "w-full py-3 rounded-xl font-bold",
                style: "background:#9945FF;color:#FFFFFF",
                onclick: on_stake_license,
                "Stake {selected_class.read().stake_wata()} wATA for Class {selected_class} License"
            }

            if !status.read().is_empty() {
                div {
                    class: "mt-3 text-xs p-2 rounded",
                    style: "background:#0D1117;color:#8B949E",
                    "{status}"
                }
            }
        }
    }
}
