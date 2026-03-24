use dioxus::prelude::*;
use crate::types::agent::AgentClass;

#[component]
pub fn ClassFilterTabs(
    selected: Option<AgentClass>,
    on_select: EventHandler<Option<AgentClass>>,
) -> Element {
    rsx! {
        div {
            class: "flex flex-wrap gap-2 mb-6",
            button {
                class: "px-4 py-2 rounded-full font-semibold text-sm transition-all",
                style: if selected.is_none() {
                    "background:#9945FF;color:#FFFFFF;border:2px solid #9945FF"
                } else {
                    "background:transparent;color:#8B949E;border:2px solid #30363D"
                },
                onclick: move |_| on_select(None),
                "ALL"
            }
            for class in AgentClass::all() {
                {
                    let cls = class.clone();
                    let is_selected = selected.as_ref() == Some(&cls);
                    let color = cls.color().to_string();
                    let label = cls.label().to_string();
                    let name = cls.name().to_string();
                    let color_clone = color.clone();
                    rsx! {
                        button {
                            key: "{label}",
                            class: "px-4 py-2 rounded-full font-semibold text-sm transition-all",
                            style: if is_selected {
                                format!("background:{};color:#0D1117;border:2px solid {}", color, color_clone)
                            } else {
                                format!("background:transparent;color:{};border:2px solid {}", color, color_clone)
                            },
                            onclick: move |_| on_select(Some(cls.clone())),
                            "{label} · {name}"
                        }
                    }
                }
            }
        }
    }
}
