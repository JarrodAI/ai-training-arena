use dioxus::prelude::*;
use crate::components::BattleGrid;

#[component]
pub fn BattlesPage() -> Element {
    rsx! {
        div {
            class: "min-h-screen",
            style: "background:#0D1117",
            div {
                class: "max-w-7xl mx-auto px-4 py-8",
                h1 {
                    class: "text-3xl font-bold mb-8",
                    style: "color:#FFFFFF",
                    "Live Battles"
                }
                BattleGrid {}
            }
        }
    }
}
