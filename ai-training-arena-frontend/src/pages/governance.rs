use dioxus::prelude::*;
use crate::components::DaoPortal;

#[component]
pub fn GovernancePage() -> Element {
    rsx! {
        DaoPortal {}
    }
}
