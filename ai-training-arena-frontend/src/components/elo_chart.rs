use dioxus::prelude::*;
use crate::api::NodeApi;
use crate::types::battle::BattleResult;

#[component]
pub fn EloChart(nft_id: u64) -> Element {
    let battles: Signal<Vec<BattleResult>> = use_signal(Vec::new);
    let color = use_signal(|| "#9945FF".to_string());

    let mut battles_clone = battles.clone();
    use_effect(move || {
        let mut b = battles_clone.clone();
        wasm_bindgen_futures::spawn_local(async move {
            if let Ok(data) = NodeApi::get_battles(50).await {
                // Filter battles involving this NFT
                let relevant: Vec<BattleResult> = data
                    .into_iter()
                    .filter(|br| br.proposer_nft_id == nft_id || br.solver_nft_id == nft_id)
                    .collect();
                b.set(relevant);
            }
        });
    });

    let b = battles.read();
    if b.is_empty() {
        return rsx! {
            div {
                class: "rounded-xl p-4 text-center",
                style: "background:#161B22;border:1px solid #30363D",
                p { style: "color:#8B949E", "No battle history yet" }
            }
        };
    }

    // Build SVG elo chart
    let width = 400.0f64;
    let height = 120.0f64;
    let padding = 20.0f64;
    let n = b.len();

    // Simulate elo progression from battle scores
    let mut elo_vals: Vec<f64> = vec![1500.0];
    for battle in b.iter() {
        let last = *elo_vals.last().unwrap();
        let won = battle.winner_nft_id == nft_id;
        let delta = if won { 20.0 } else { -20.0 };
        elo_vals.push((last + delta).max(1000.0).min(3000.0));
    }

    let min_elo = elo_vals.iter().cloned().fold(f64::INFINITY, f64::min);
    let max_elo = elo_vals.iter().cloned().fold(f64::NEG_INFINITY, f64::max);
    let elo_range = (max_elo - min_elo).max(100.0);

    let points: Vec<(f64, f64)> = elo_vals
        .iter()
        .enumerate()
        .map(|(i, &e)| {
            let x = padding + (i as f64 / (elo_vals.len() - 1).max(1) as f64) * (width - 2.0 * padding);
            let y = padding + (1.0 - (e - min_elo) / elo_range) * (height - 2.0 * padding);
            (x, y)
        })
        .collect();

    let path_d: String = points
        .iter()
        .enumerate()
        .map(|(i, (x, y))| {
            if i == 0 {
                format!("M {:.1} {:.1}", x, y)
            } else {
                format!(" L {:.1} {:.1}", x, y)
            }
        })
        .collect();

    let line_color = color.read().clone();
    let last_elo = elo_vals.last().copied().unwrap_or(1500.0) as u32;

    rsx! {
        div {
            class: "rounded-xl p-4",
            style: "background:#161B22;border:1px solid #30363D",
            div {
                class: "flex justify-between items-center mb-3",
                span {
                    class: "text-xs font-semibold",
                    style: "color:#8B949E",
                    "ELO HISTORY (LAST {n} BATTLES)"
                }
                span {
                    class: "text-sm font-bold font-mono",
                    style: format!("color:{}", line_color),
                    "{last_elo} ELO"
                }
            }
            svg {
                width: "{width}",
                height: "{height}",
                "viewBox": "0 0 {width} {height}",
                // Grid lines
                line {
                    x1: "{padding}", y1: "{padding}",
                    x2: "{padding}", y2: "{height - padding}",
                    stroke: "#30363D", "stroke-width": "1",
                }
                line {
                    x1: "{padding}", y1: "{height - padding}",
                    x2: "{width - padding}", y2: "{height - padding}",
                    stroke: "#30363D", "stroke-width": "1",
                }
                // Trend line
                path {
                    d: "{path_d}",
                    fill: "none",
                    stroke: "{line_color}",
                    "stroke-width": "2",
                    "stroke-linecap": "round",
                    "stroke-linejoin": "round",
                }
                // Data points
                for (x, y) in &points {
                    circle {
                        cx: "{x}",
                        cy: "{y}",
                        r: "3",
                        fill: "{line_color}",
                    }
                }
            }
        }
    }
}
