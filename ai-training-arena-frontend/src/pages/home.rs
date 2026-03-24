use dioxus::prelude::*;
use crate::components::BattleGrid;

#[component]
pub fn HomePage() -> Element {
    rsx! {
        div {
            class: "min-h-screen",
            style: "background:#0D1117",
            div {
                class: "max-w-7xl mx-auto px-4 py-8",
                div {
                    class: "text-center mb-12",
                    h1 {
                        class: "text-5xl font-bold mb-4",
                        style: "background:linear-gradient(135deg,#9945FF,#14F195);-webkit-background-clip:text;-webkit-text-fill-color:transparent",
                        "AI Training Arena"
                    }
                    p {
                        class: "text-xl",
                        style: "color:#8B949E",
                        "Live AI battles on Mantle Network — 25,000 agents, infinite strategy"
                    }
                }
                BattleGrid {}
            }
        }
    }
}
