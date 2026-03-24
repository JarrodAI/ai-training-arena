use dioxus::prelude::*;
use crate::types::battle::RoundInfo;
use crate::utils::formatting::format_duration;

#[component]
pub fn RoundTimer() -> Element {
    let mut round_info = use_signal(|| RoundInfo {
        round_number: 1,
        time_remaining_secs: 10800,
        battles_in_round: 0,
    });

    use_coroutine(move |_rx: UnboundedReceiver<()>| async move {
        loop {
            sleep_1s().await;
            let (remaining, round_num) = {
                let r = round_info.read();
                (r.time_remaining_secs, r.round_number)
            };
            let new_remaining = remaining.saturating_sub(1);
            if new_remaining == 0 {
                let next = (round_num % 8) + 1;
                round_info.write().round_number = next;
                round_info.write().time_remaining_secs = 10800;
            } else {
                round_info.write().time_remaining_secs = new_remaining;
            }
        }
    });

    let (round_num, remaining) = {
        let r = round_info.read();
        (r.round_number, r.time_remaining_secs)
    };

    rsx! {
        div {
            class: "flex items-center gap-3 px-4 py-2 rounded-full",
            style: "background:#161B22;border:1px solid #30363D",
            div {
                class: "w-2 h-2 rounded-full",
                style: "background:#14F195",
            }
            span {
                class: "font-mono font-bold text-sm",
                style: "color:#FFFFFF",
                "ROUND {round_num} - {format_duration(remaining)} REMAINING"
            }
        }
    }
}

async fn sleep_1s() {
    use wasm_bindgen_futures::JsFuture;
    use js_sys::Promise;
    let promise = Promise::new(&mut |resolve, _reject| {
        let window = web_sys::window().unwrap();
        window
            .set_timeout_with_callback_and_timeout_and_arguments_0(&resolve, 1000)
            .unwrap();
    });
    let _ = JsFuture::from(promise).await;
}
