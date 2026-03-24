use dioxus::prelude::*;
use crate::components::node_status_bar::NodeStatusBar;

#[component]
pub fn SettingsPage() -> Element {
    let mut auto_battle = use_signal(|| false);
    let mut node_url = use_signal(|| "localhost:8080".to_string());

    rsx! {
        div {
            class: "min-h-screen",
            style: "background:#0D1117",
            div {
                class: "max-w-3xl mx-auto px-4 py-8",
                h1 {
                    class: "text-3xl font-bold mb-8",
                    style: "color:#FFFFFF",
                    "Settings"
                }
                div {
                    class: "rounded-xl p-6 mb-6",
                    style: "background:#161B22;border:1px solid #30363D",
                    h2 {
                        class: "text-xl font-semibold mb-4",
                        style: "color:#FFFFFF",
                        "Node Connection"
                    }
                    div {
                        class: "mb-4",
                        label {
                            class: "block text-sm font-medium mb-2",
                            style: "color:#8B949E",
                            "Node WebSocket URL"
                        }
                        input {
                            class: "w-full px-4 py-2 rounded-lg",
                            style: "background:#0D1117;border:1px solid #30363D;color:#FFFFFF",
                            value: "{node_url}",
                            oninput: move |e| node_url.set(e.value()),
                        }
                    }
                    NodeStatusBar {}
                }
                div {
                    class: "rounded-xl p-6 mb-6",
                    style: "background:#161B22;border:1px solid #30363D",
                    h2 {
                        class: "text-xl font-semibold mb-4",
                        style: "color:#FFFFFF",
                        "Battle Settings"
                    }
                    div {
                        class: "flex items-center justify-between",
                        span {
                            style: "color:#FFFFFF",
                            "Auto-Battle"
                        }
                        button {
                            class: "px-4 py-2 rounded-lg font-semibold",
                            style: if *auto_battle.read() { "background:#14F195;color:#0D1117" } else { "background:#30363D;color:#8B949E" },
                            onclick: move |_| auto_battle.set(!auto_battle()),
                            if *auto_battle.read() { "Enabled" } else { "Disabled" }
                        }
                    }
                }
            }
        }
    }
}
