use wasm_bindgen::prelude::*;
use serde::{Deserialize, Serialize};
use wasm_bindgen_futures::JsFuture;
use js_sys::{Object, Promise, Reflect};

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct TransactionRequest {
    pub from: String,
    pub to: String,
    pub data: String,
    pub value: Option<String>,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct ICOPhaseInfo {
    pub phase: u8,
    pub phase_name: String,
    pub price_per_ata_usdc: String,
    pub allocation: String,
    pub sold: String,
    pub remaining: String,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct FundsRaisedInfo {
    pub total_raised_usdc: String,
    pub soft_cap_usdc: String,
    pub hard_cap_usdc: String,
    pub soft_cap_pct: f64,
    pub hard_cap_pct: f64,
}

pub fn encode_buy_call(ata_amount_wei: &str) -> String {
    // Function selector for buy(uint256): keccak256("buy(uint256)")[0..4]
    // = 0xd96a094a
    // ABI-encode uint256 parameter (padded to 32 bytes)
    let selector = "d96a094a";
    let padded = format!("{:0>64}", &ata_amount_wei[2..]); // strip 0x, pad to 64 hex chars
    format!("0x{}{}", selector, padded)
}

pub fn encode_buy_founders_call(ata_amount_wei: &str, proof: &[String]) -> String {
    // Function selector for buyFounders(uint256,bytes32[]): 0x...
    // Simplified encoding - in production use ethers-rs or similar
    let selector = "e2df0ce0"; // placeholder - compute from abi
    let amount_padded = format!("{:0>64}", &ata_amount_wei[2..]);
    // For a full implementation, ABI-encode the proof array
    format!("0x{}{}", selector, amount_padded)
}

pub fn encode_mint_nft_call(tier: u8) -> String {
    // Function selector for mint(uint8): keccak256("mint(uint8)")[0..4]
    let selector = "6a627842"; // placeholder
    let tier_padded = format!("{:0>64x}", tier);
    format!("0x{}{}", selector, tier_padded)
}

pub async fn eth_send_transaction(tx: &TransactionRequest) -> Result<String, String> {
    let window = web_sys::window().ok_or("no window")?;
    let ethereum = Reflect::get(&window, &JsValue::from_str("ethereum"))
        .map_err(|_| "MetaMask not found")?;

    if ethereum.is_undefined() || ethereum.is_null() {
        return Err("MetaMask not installed".to_string());
    }

    let params = js_sys::Array::new();
    let tx_obj = Object::new();
    Reflect::set(&tx_obj, &JsValue::from_str("from"), &JsValue::from_str(&tx.from))
        .map_err(|_| "failed to set from")?;
    Reflect::set(&tx_obj, &JsValue::from_str("to"), &JsValue::from_str(&tx.to))
        .map_err(|_| "failed to set to")?;
    Reflect::set(&tx_obj, &JsValue::from_str("data"), &JsValue::from_str(&tx.data))
        .map_err(|_| "failed to set data")?;
    if let Some(value) = &tx.value {
        Reflect::set(&tx_obj, &JsValue::from_str("value"), &JsValue::from_str(value))
            .map_err(|_| "failed to set value")?;
    }
    params.push(&tx_obj);

    let request_fn = Reflect::get(&ethereum, &JsValue::from_str("request"))
        .map_err(|_| "no request fn")?;
    let request_fn = js_sys::Function::from(request_fn);

    let args_obj = Object::new();
    Reflect::set(&args_obj, &JsValue::from_str("method"), &JsValue::from_str("eth_sendTransaction"))
        .map_err(|_| "failed to set method")?;
    Reflect::set(&args_obj, &JsValue::from_str("params"), &params)
        .map_err(|_| "failed to set params")?;

    let promise = request_fn
        .call1(&ethereum, &args_obj)
        .map_err(|e| format!("call failed: {:?}", e))?;
    let promise = Promise::from(promise);

    JsFuture::from(promise)
        .await
        .map(|v| v.as_string().unwrap_or_default())
        .map_err(|e| format!("tx rejected: {:?}", e))
}

pub async fn request_accounts() -> Result<Vec<String>, String> {
    let window = web_sys::window().ok_or("no window")?;
    let ethereum = Reflect::get(&window, &JsValue::from_str("ethereum"))
        .map_err(|_| "MetaMask not found")?;

    if ethereum.is_undefined() {
        return Err("MetaMask not installed".to_string());
    }

    let request_fn = Reflect::get(&ethereum, &JsValue::from_str("request"))
        .map_err(|_| "no request fn")?;
    let request_fn = js_sys::Function::from(request_fn);

    let args_obj = Object::new();
    Reflect::set(&args_obj, &JsValue::from_str("method"), &JsValue::from_str("eth_requestAccounts"))
        .map_err(|_| "failed to set method")?;

    let promise = request_fn
        .call1(&ethereum, &args_obj)
        .map_err(|e| format!("call failed: {:?}", e))?;
    let promise = Promise::from(promise);

    let result = JsFuture::from(promise)
        .await
        .map_err(|e| format!("accounts rejected: {:?}", e))?;

    let accounts = js_sys::Array::from(&result);
    Ok(accounts
        .iter()
        .filter_map(|v| v.as_string())
        .collect())
}

