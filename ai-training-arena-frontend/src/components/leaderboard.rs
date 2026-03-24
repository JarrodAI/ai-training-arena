use dioxus::prelude::*;
use crate::api::NodeApi;
use crate::components::{ClassFilterTabs, leaderboard_row::LeaderboardRow};
use crate::types::{agent::AgentClass, leaderboard::LeaderboardEntry};
use crate::types::wallet::WalletState;

#[component]
pub fn LeaderboardPage() -> Element {
    let mut selected_class = use_signal(|| AgentClass::A);
    let mut entries: Signal<Vec<LeaderboardEntry>> = use_signal(Vec::new);
    let mut page = use_signal(|| 0usize);
    let wallet = use_context::<Signal<WalletState>>();
    let mut loading = use_signal(|| false);

    // Load leaderboard on class change
    let mut entries_clone = entries.clone();
    let mut loading_clone = loading.clone();
    use_effect(move || {
        let cls = selected_class.read().clone();
        let mut e = entries_clone.clone();
        let mut l = loading_clone.clone();
        wasm_bindgen_futures::spawn_local(async move {
            l.set(true);
            match NodeApi::get_leaderboard(cls).await {
                Ok(data) => e.set(data),
                Err(_) => e.set(vec![]),
            }
            l.set(false);
        });
    });

    // Auto-refresh every 60s
    let mut entries_r = entries.clone();
    use_coroutine(move |_rx: UnboundedReceiver<()>| async move {
        loop {
            sleep_ms(60000).await;
            let cls = selected_class.read().clone();
            if let Ok(data) = NodeApi::get_leaderboard(cls).await {
                entries_r.set(data);
            }
        }
    });

    let page_size = 25;
    let start = *page.read() * page_size;
    let all_entries = entries.read();
    let display_entries: Vec<_> = all_entries.iter().skip(start).take(page_size).cloned().collect();
    let wallet_addr = wallet.read().address.clone();

    rsx! {
        div {
            class: "min-h-screen",
            style: "background:#0D1117",
            div {
                class: "max-w-7xl mx-auto px-4 py-8",
                h1 {
                    class: "text-3xl font-bold mb-6",
                    style: "color:#FFFFFF",
                    "Leaderboard"
                }

                ClassFilterTabs {
                    selected: Some(selected_class.read().clone()),
                    on_select: move |cls: Option<AgentClass>| {
                        if let Some(c) = cls {
                            selected_class.set(c);
                            page.set(0);
                        }
                    },
                }

                if *loading.read() {
                    div {
                        class: "text-center py-20",
                        div {
                            class: "w-8 h-8 mx-auto border-2 border-t-transparent rounded-full animate-spin",
                            style: "border-color:#9945FF",
                        }
                    }
                } else {
                    div {
                        class: "rounded-xl overflow-hidden",
                        style: "border:1px solid #30363D",
                        table {
                            class: "w-full",
                            thead {
                                tr {
                                    style: "background:#161B22",
                                    for header in ["Rank", "NFT ID", "Model", "Class", "ELO", "Battles", "Win Rate", "Rewards"] {
                                        th {
                                            class: "px-4 py-3 text-left text-xs font-semibold tracking-wider",
                                            style: "color:#8B949E",
                                            "{header}"
                                        }
                                    }
                                }
                            }
                            tbody {
                                for (i, entry) in display_entries.into_iter().enumerate() {
                                    {
                                        let rank = start + i + 1;
                                        let entry_key = entry.nft_id;
                                        let is_mine = wallet_addr
                                            .as_ref()
                                            .map(|a| a == &entry.owner)
                                            .unwrap_or(false);
                                        rsx! {
                                            LeaderboardRow {
                                                key: "{entry_key}",
                                                entry: entry,
                                                rank: rank,
                                                is_mine: is_mine,
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // Pagination
                    div {
                        class: "flex justify-center gap-4 mt-6",
                        if *page.read() > 0 {
                            button {
                                class: "px-6 py-2 rounded-lg font-semibold",
                                style: "background:#161B22;color:#FFFFFF;border:1px solid #30363D",
                                onclick: move |_| page.set(page() - 1),
                                "Previous"
                            }
                        }
                        if all_entries.len() > start + page_size {
                            button {
                                class: "px-6 py-2 rounded-lg font-semibold",
                                style: "background:#9945FF;color:#FFFFFF",
                                onclick: move |_| page.set(page() + 1),
                                "Load More"
                            }
                        }
                    }
                }
            }
        }
    }
}

async fn sleep_ms(ms: u32) {
    use wasm_bindgen_futures::JsFuture;
    use js_sys::Promise;
    let promise = Promise::new(&mut |resolve, _| {
        web_sys::window()
            .unwrap()
            .set_timeout_with_callback_and_timeout_and_arguments_0(&resolve, ms as i32)
            .unwrap();
    });
    let _ = JsFuture::from(promise).await;
}
