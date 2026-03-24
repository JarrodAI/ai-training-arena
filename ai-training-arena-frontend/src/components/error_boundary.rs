use dioxus::prelude::*;

#[component]
pub fn ErrorBoundary(message: String) -> Element {
    rsx! {
        div {
            class: "min-h-screen flex items-center justify-center",
            style: "background:#0D1117",
            div {
                class: "text-center p-8 rounded-2xl max-w-md",
                style: "background:#161B22;border:1px solid #FF6B6B",
                h2 {
                    class: "text-xl font-bold mb-4",
                    style: "color:#FF6B6B",
                    "Something went wrong"
                }
                p {
                    class: "text-sm mb-6",
                    style: "color:#8B949E",
                    "{message}"
                }
                button {
                    class: "px-6 py-3 rounded-lg font-semibold",
                    style: "background:#9945FF;color:#FFFFFF",
                    onclick: |_| {
                        if let Some(window) = web_sys::window() {
                            let _ = window.location().reload();
                        }
                    },
                    "Reload Page"
                }
            }
        }
    }
}
