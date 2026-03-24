use dioxus::prelude::*;
use crate::components::DataMarketplacePage;

#[component]
pub fn MarketplacePage() -> Element {
    rsx! {
        DataMarketplacePage {}
    }
}
