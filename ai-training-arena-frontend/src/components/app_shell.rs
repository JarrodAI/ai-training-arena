use dioxus::prelude::*;
use crate::components::NavBar;
use crate::Route;

#[component]
pub fn AppShell() -> Element {
    rsx! {
        div {
            style: "background:#0D1117;min-height:100vh",
            NavBar {}
            div {
                style: "padding-top:64px",
                Outlet::<Route> {}
            }
        }
    }
}
