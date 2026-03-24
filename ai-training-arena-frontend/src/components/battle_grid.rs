use dioxus::prelude::*;
use crate::components::{
    BattleSlotCard,
    BattleDetailModal,
    ClassFilterTabs,
    RoundTimer,
};
use crate::state::BattleState;
use crate::types::{agent::AgentClass, battle::{BattleSlot, BattleUpdate, WsMessage}};
use crate::types::marketplace::NodeStatus;

#[component]
pub fn BattleGrid() -> Element {
    let mut battle_state = use_signal(BattleState::new);
    let mut selected_class: Signal<Option<AgentClass>> = use_signal(|| None);
    let mut modal_slot: Signal<Option<BattleSlot>> = use_signal(|| None);
    let mut node_status = use_signal(|| NodeStatus::Offline);

    {
        let bs = battle_state.clone();
        let mut ns = node_status.clone();
        use_effect(move || {
            let bs2 = bs.clone();
            let mut ns2 = ns.clone();
            wasm_bindgen_futures::spawn_local(async move {
                ns2.set(NodeStatus::Connecting);
                match crate::api::WsClient::connect() {
                    Ok(client) => {
                        ns2.set(NodeStatus::Online);
                        let bs3 = bs2.clone();
                        client.subscribe(move |msg: WsMessage| {
                            if msg.msg_type == "battle_update" {
                                if let Ok(update) = serde_json::from_value::<BattleUpdate>(msg.payload.clone()) {
                                    // Use write_unchecked since we're in a Fn closure
                                    let mut writer = bs3.write_unchecked();
                                    writer.update_slot(update);
                                }
                            }
                        });
                        loop_forever().await;
                    }
                    Err(_) => {
                        ns2.set(NodeStatus::Offline);
                    }
                }
            });
        });
    }

    let filtered_slots = battle_state.read().get_filtered_slots(&selected_class.read());
    let status_color = node_status.read().color().to_string();
    let status_label = node_status.read().label().to_string();

    rsx! {
        div {
            class: "w-full max-w-7xl mx-auto px-4 py-8",

            div {
                class: "flex flex-wrap items-center justify-between gap-4 mb-6",
                h2 { class: "text-2xl font-bold", style: "color:#FFFFFF", "Featured Battles - Live Now" }
                RoundTimer {}
            }

            div {
                class: "flex items-center gap-2 mb-4",
                div { class: "w-2 h-2 rounded-full", style: format!("background:{}", status_color) }
                span { class: "text-xs", style: "color:#8B949E", "Node: {status_label}" }
            }

            ClassFilterTabs {
                selected: selected_class.read().clone(),
                on_select: move |cls| selected_class.set(cls),
            }

            div {
                class: "grid gap-4",
                style: "grid-template-columns:repeat(5,1fr)",
                for (i, slot) in filtered_slots.into_iter().enumerate() {
                    {
                        let s = slot.clone();
                        let s_key = s.slot_index;
                        rsx! {
                            BattleSlotCard {
                                key: "{s_key}",
                                slot: s,
                                position: i,
                                on_click: move |clicked: BattleSlot| modal_slot.set(Some(clicked)),
                            }
                        }
                    }
                }
            }

            div {
                class: "flex flex-wrap gap-4 mt-6 pt-4",
                style: "border-top:1px solid #30363D",
                for class in AgentClass::all() {
                    {
                        let color = class.color().to_string();
                        let label = class.label().to_string();
                        let name = class.name().to_string();
                        let params = class.params().to_string();
                        rsx! {
                            div {
                                key: "{label}",
                                class: "flex items-center gap-2",
                                div { class: "w-3 h-3 rounded-full", style: format!("background:{}", color) }
                                span { class: "text-xs", style: "color:#8B949E", "Class {label}: {name} ({params})" }
                            }
                        }
                    }
                }
            }
        }

        if let Some(slot) = modal_slot.read().clone() {
            BattleDetailModal {
                slot: slot,
                on_close: move |_| modal_slot.set(None),
            }
        }
    }
}

async fn loop_forever() {
    use wasm_bindgen_futures::JsFuture;
    use js_sys::Promise;
    loop {
        let promise = Promise::new(&mut |resolve, _| {
            web_sys::window().unwrap()
                .set_timeout_with_callback_and_timeout_and_arguments_0(&resolve, 60000)
                .unwrap();
        });
        let _ = JsFuture::from(promise).await;
    }
}
