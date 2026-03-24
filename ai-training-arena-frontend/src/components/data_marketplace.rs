use dioxus::prelude::*;
use crate::api::NodeApi;
use crate::components::{data_listing_card::DataListingCard, list_data_modal::ListDataModal};
use crate::types::{
    agent::AgentClass,
    marketplace::{DataCategory, DataListing},
    wallet::WalletState,
};

#[component]
pub fn DataMarketplacePage() -> Element {
    let wallet = use_context::<Signal<WalletState>>();
    let mut listings: Signal<Vec<DataListing>> = use_signal(Vec::new);
    let mut category_filter = use_signal(|| DataCategory::All);
    let mut sort = use_signal(|| "newest".to_string());
    let mut page = use_signal(|| 0usize);
    let mut show_list_modal = use_signal(|| false);
    let mut loading = use_signal(|| false);

    use_effect(move || {
        let mut l = listings.clone();
        let mut loading_ = loading.clone();
        wasm_bindgen_futures::spawn_local(async move {
            loading_.set(true);
            // In production: fetch from node API
            l.set(vec![]);
            loading_.set(false);
        });
    });

    let w = wallet.read();

    rsx! {
        div {
            class: "min-h-screen",
            style: "background:#0D1117",
            div {
                class: "max-w-7xl mx-auto px-4 py-8",
                div {
                    class: "flex items-center justify-between mb-6",
                    div {
                        h1 { class: "text-3xl font-bold", style: "color:#FFFFFF", "Data Marketplace" }
                        p { class: "text-sm mt-1", style: "color:#8B949E", "Own Your Training Data" }
                    }
                    if w.is_connected() {
                        button {
                            class: "px-4 py-2 rounded-lg font-semibold",
                            style: "background:#9945FF;color:#FFFFFF",
                            onclick: move |_| show_list_modal.set(true),
                            "List My Data"
                        }
                    }
                }

                // Filter bar
                div {
                    class: "flex flex-wrap gap-3 mb-6",
                    div {
                        class: "flex gap-2",
                        for cat in DataCategory::all() {
                            {
                                let c = cat.clone();
                                let c_key = c.label().to_string();
                                let is_selected = *category_filter.read() == c;
                                rsx! {
                                    button {
                                        key: "{c_key}",
                                        class: "px-3 py-1.5 rounded-full text-xs font-semibold",
                                        style: if is_selected {
                                            "background:#9945FF;color:#FFFFFF"
                                        } else {
                                            "background:#161B22;color:#8B949E;border:1px solid #30363D"
                                        },
                                        onclick: move |_| { category_filter.set(c.clone()); page.set(0); },
                                        "{c.label()}"
                                    }
                                }
                            }
                        }
                    }
                    select {
                        class: "px-3 py-1.5 rounded-lg text-xs",
                        style: "background:#161B22;color:#8B949E;border:1px solid #30363D",
                        onchange: move |e| sort.set(e.value()),
                        option { value: "newest", "Newest" }
                        option { value: "cheapest", "Cheapest" }
                        option { value: "popular", "Most Popular" }
                    }
                }

                if *loading.read() {
                    div { class: "text-center py-20",
                        div { class: "w-8 h-8 mx-auto border-2 border-t-transparent rounded-full animate-spin", style: "border-color:#9945FF" }
                    }
                } else if listings.read().is_empty() {
                    div { class: "text-center py-20",
                        p { style: "color:#8B949E;font-size:18px", "No listings found" }
                        p { class: "text-sm mt-2", style: "color:#8B949E", "Be the first to list your training data" }
                    }
                } else {
                    div {
                        class: "grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-3",
                        for listing in listings.read().iter().cloned() {
                            { let k = listing.listing_id; rsx! { DataListingCard { key: "{k}", listing: listing } } }
                        }
                    }
                }
            }
        }

        if *show_list_modal.read() {
            ListDataModal {
                show: *show_list_modal.read(),
                on_close: move |_| show_list_modal.set(false),
                owned_agents: wallet.read().agents.clone(),
                wallet_address: wallet.read().address.clone(),
            }
        }
    }
}
