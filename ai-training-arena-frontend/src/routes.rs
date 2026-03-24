use dioxus::prelude::*;
use crate::pages::{
    BattlesPage, DashboardPage, GovernancePage, HomePage, MarketplacePage,
    MintPage, SettingsPage, StakingPage,
};
use crate::components::AppShell;

#[component]
fn NotFound(route: Vec<String>) -> Element {
    rsx! {
        div {
            class: "min-h-screen flex items-center justify-center",
            style: "background:#0D1117;color:#FFFFFF",
            div {
                class: "text-center",
                h1 { class: "text-4xl font-bold mb-4", "404" }
                p { class: "mb-4", style: "color:#8B949E", "Page not found" }
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

#[derive(Routable, Clone, PartialEq)]
pub enum Route {
    #[layout(AppShell)]
    #[route("/", HomePage)]
    Home,
    #[route("/battles", BattlesPage)]
    Battles,
    #[route("/mint", MintPage)]
    Mint,
    #[route("/staking", StakingPage)]
    Staking,
    #[route("/marketplace", MarketplacePage)]
    Marketplace,
    #[route("/governance", GovernancePage)]
    Governance,
    #[route("/dashboard", DashboardPage)]
    Dashboard,
    #[route("/settings", SettingsPage)]
    Settings,
    #[end_layout]
    #[route("/:..route", NotFound)]
    NotFound { route: Vec<String> },
}
