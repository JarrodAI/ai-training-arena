use dioxus::prelude::*;
use ai_training_arena_frontend::{
    Route,
    state::AppState,
    types::{wallet::WalletState, marketplace::NodeStatus},
};

#[component]
fn NotFound(route: Vec<String>) -> Element {
    rsx! {
        div {
            class: "min-h-screen flex items-center justify-center",
            style: "background:#0D1117;color:#FFFFFF",
            div {
                class: "text-center",
                h1 { class: "text-4xl font-bold mb-4", "404" }
                p { class: "text-gray-400 mb-8", "Page not found" }
                a {
                    href: "/",
                    class: "px-6 py-3 rounded-lg font-semibold",
                    style: "background:#9945FF;color:#FFFFFF;text-decoration:none",
                    "Return to Arena"
                }
            }
        }
    }
}

fn app() -> Element {
    let app_state = use_signal(AppState::new);
    let wallet = use_signal(WalletState::new);
    let node_status = use_signal(|| NodeStatus::Offline);

    use_context_provider(|| app_state);
    use_context_provider(|| wallet);
    use_context_provider(|| node_status);

    rsx! {
        Router::<Route> {}
    }
}

fn main() {
    dioxus::launch(app);
}
