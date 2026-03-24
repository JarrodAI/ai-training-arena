use dioxus::prelude::*;

#[derive(Props, Clone, PartialEq)]
pub struct FundsRaisedBarProps {
    pub total_raised_usdc: f64,
    pub soft_cap_usdc: f64,
    pub hard_cap_usdc: f64,
}

#[component]
pub fn FundsRaisedBar(props: FundsRaisedBarProps) -> Element {
    let soft_pct = (props.total_raised_usdc / props.hard_cap_usdc * 100.0).min(100.0);
    let soft_marker_pct = (props.soft_cap_usdc / props.hard_cap_usdc * 100.0).min(100.0);

    let raised_m = props.total_raised_usdc / 1_000_000.0;
    let soft_m   = props.soft_cap_usdc     / 1_000_000.0;
    let hard_m   = props.hard_cap_usdc     / 1_000_000.0;

    let soft_reached = props.total_raised_usdc >= props.soft_cap_usdc;

    let bar_color = if soft_reached { "#00ff88" } else { "#ff6b35" };

    rsx! {
        div { class: "funds-raised-bar",
            div { class: "bar-header",
                h3 { "Funds Raised" }
                span { class: "raised-amount",
                    "${raised_m:.1}M / ${hard_m:.0}M"
                }
            }

            div { class: "bar-track",
                div {
                    class: "bar-fill",
                    style: "width: {soft_pct:.1}%; background: {bar_color};",
                }
                // Soft cap marker
                div {
                    class: "soft-cap-marker",
                    style: "left: {soft_marker_pct:.1}%;",
                    div { class: "marker-line" }
                    span { class: "marker-label", "Soft Cap ${soft_m:.0}M" }
                }
            }

            div { class: "bar-legend",
                div { class: "legend-item",
                    div { class: "legend-dot soft", }
                    span { "Soft Cap: ${soft_m:.0}M" }
                }
                div { class: "legend-item",
                    div { class: "legend-dot hard", }
                    span { "Hard Cap: ${hard_m:.0}M" }
                }
                if soft_reached {
                    div { class: "soft-cap-badge", "Soft Cap Reached!" }
                }
            }
        }
    }
}
