use dioxus::prelude::*;

#[derive(Props, Clone, PartialEq)]
pub struct PresaleTimerProps {
    pub phase_name: String,
    pub end_timestamp_secs: u64,
}

#[component]
pub fn PresaleTimer(props: PresaleTimerProps) -> Element {
    let time_remaining = use_signal(|| compute_remaining(props.end_timestamp_secs));

    use_effect(move || {
        // In a full implementation, use gloo_timers or a Dioxus interval hook
        // to update every second. Scaffold shows the structure.
    });

    let remaining = time_remaining.read();

    rsx! {
        div { class: "presale-timer",
            h3 { "Phase: {props.phase_name}" }
            div { class: "countdown",
                span { class: "time-unit",
                    span { class: "value", "{remaining.days}" }
                    span { class: "label", "Days" }
                }
                span { class: "separator", ":" }
                span { class: "time-unit",
                    span { class: "value", "{remaining.hours}" }
                    span { class: "label", "Hours" }
                }
                span { class: "separator", ":" }
                span { class: "time-unit",
                    span { class: "value", "{remaining.minutes}" }
                    span { class: "label", "Mins" }
                }
                span { class: "separator", ":" }
                span { class: "time-unit",
                    span { class: "value", "{remaining.seconds}" }
                    span { class: "label", "Secs" }
                }
            }
        }
    }
}

#[derive(Clone, Default)]
struct TimeRemaining {
    days: u64,
    hours: u64,
    minutes: u64,
    seconds: u64,
}

fn compute_remaining(end_ts: u64) -> TimeRemaining {
    // In a real implementation, get current unix timestamp from js_sys::Date
    // and compute difference. Placeholder returns zero.
    let _ = end_ts;
    TimeRemaining { days: 0, hours: 0, minutes: 0, seconds: 0 }
}
