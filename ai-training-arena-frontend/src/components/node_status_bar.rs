use dioxus::prelude::*;
use crate::types::marketplace::NodeStatus;

#[component]
pub fn NodeStatusBar() -> Element {
    let node_status = use_context::<Signal<NodeStatus>>();
    let status = node_status.read();
    let color = status.color().to_string();
    let label = status.label().to_string();

    rsx! {
        div {
            class: "flex items-center gap-2 px-3 py-1.5 rounded-full",
            style: format!("background:#161B22;border:1px solid {}44", color),
            div {
                class: "w-2 h-2 rounded-full",
                style: format!("background:{}", color),
            }
            span {
                class: "text-xs font-mono",
                style: format!("color:{}", color),
                "Node: {label}"
            }
        }
    }
}
