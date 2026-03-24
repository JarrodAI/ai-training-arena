use dioxus::prelude::*;
use crate::components::{WalletConnect, NodeStatusBar};

#[component]
pub fn NavBar() -> Element {
    rsx! {
        nav {
            class: "fixed top-0 left-0 right-0 z-40 flex items-center justify-between px-6 py-3",
            style: "background:rgba(13,17,23,0.95);backdrop-filter:blur(12px);border-bottom:1px solid #30363D",

            a {
                href: "/",
                class: "text-lg font-bold tracking-tight",
                style: "background:linear-gradient(135deg,#9945FF,#14F195);-webkit-background-clip:text;-webkit-text-fill-color:transparent;text-decoration:none",
                "AI TRAINING ARENA"
            }

            div {
                class: "hidden md:flex items-center gap-6",
                a { href: "/battles", class: "text-sm font-medium", style: "color:#8B949E;text-decoration:none", "Arena" }
                a { href: "/", class: "text-sm font-medium", style: "color:#8B949E;text-decoration:none", "Leaderboard" }
                a { href: "/dashboard", class: "text-sm font-medium", style: "color:#8B949E;text-decoration:none", "Dashboard" }
                a { href: "/marketplace", class: "text-sm font-medium", style: "color:#8B949E;text-decoration:none", "Marketplace" }
                a { href: "/governance", class: "text-sm font-medium", style: "color:#8B949E;text-decoration:none", "Governance" }
                a { href: "/mint", class: "text-sm font-medium", style: "color:#8B949E;text-decoration:none", "Mint" }
            }

            div {
                class: "flex items-center gap-3",
                NodeStatusBar {}
                WalletConnect {}
            }
        }
    }
}
