use wasm_bindgen::prelude::*;
use wasm_bindgen_futures::JsFuture;
use js_sys::{Object, Promise, Reflect};

fn get_ethereum() -> Result<JsValue, String> {
    let window = web_sys::window().ok_or("no window")?;
    let eth = Reflect::get(&window, &JsValue::from_str("ethereum"))
        .map_err(|_| "MetaMask not found")?;
    if eth.is_undefined() || eth.is_null() {
        return Err("MetaMask not installed".to_string());
    }
    Ok(eth)
}

async fn eth_request(method: &str, params: Option<JsValue>) -> Result<JsValue, String> {
    let ethereum = get_ethereum()?;
    let request_fn = Reflect::get(&ethereum, &JsValue::from_str("request"))
        .map_err(|_| "no request fn")?;
    let request_fn = js_sys::Function::from(request_fn);

    let args_obj = Object::new();
    Reflect::set(
        &args_obj,
        &JsValue::from_str("method"),
        &JsValue::from_str(method),
    )
    .map_err(|_| "failed to set method")?;

    if let Some(p) = params {
        Reflect::set(&args_obj, &JsValue::from_str("params"), &p)
            .map_err(|_| "failed to set params")?;
    }

    let promise = request_fn
        .call1(&ethereum, &args_obj)
        .map_err(|e| format!("call failed: {:?}", e))?;
    let promise = Promise::from(promise);

    JsFuture::from(promise)
        .await
        .map_err(|e| format!("rejected: {:?}", e))
}

pub async fn request_accounts() -> Result<String, String> {
    let params = js_sys::Array::new();
    let result = eth_request("eth_requestAccounts", Some(params.into())).await?;
    let accounts = js_sys::Array::from(&result);
    accounts
        .get(0)
        .as_string()
        .ok_or_else(|| "no accounts returned".to_string())
}

pub async fn get_chain_id() -> Result<u64, String> {
    let result = eth_request("eth_chainId", None).await?;
    let chain_hex = result.as_string().ok_or("invalid chainId")?;
    let chain_hex = chain_hex.trim_start_matches("0x");
    u64::from_str_radix(chain_hex, 16).map_err(|e| e.to_string())
}

pub async fn sign_message(msg: &str) -> Result<String, String> {
    let address = request_accounts().await?;
    let params = js_sys::Array::new();
    params.push(&JsValue::from_str(msg));
    params.push(&JsValue::from_str(&address));
    let result = eth_request("personal_sign", Some(params.into())).await?;
    result.as_string().ok_or("invalid signature".to_string())
}

pub async fn get_ata_balance(address: &str, contract_address: &str) -> Result<f64, String> {
    // balanceOf(address) selector: 0x70a08231
    let padded_addr = format!("{:0>64}", &address[2..]);
    let data = format!("0x70a08231{}", padded_addr);

    let params = js_sys::Array::new();
    let call_obj = Object::new();
    Reflect::set(
        &call_obj,
        &JsValue::from_str("to"),
        &JsValue::from_str(contract_address),
    )
    .map_err(|_| "failed to set to")?;
    Reflect::set(
        &call_obj,
        &JsValue::from_str("data"),
        &JsValue::from_str(&data),
    )
    .map_err(|_| "failed to set data")?;
    params.push(&call_obj);
    params.push(&JsValue::from_str("latest"));

    let result = eth_request("eth_call", Some(params.into())).await?;
    let hex = result.as_string().ok_or("invalid balance response")?;
    let hex = hex.trim_start_matches("0x");
    if hex.is_empty() {
        return Ok(0.0);
    }
    let raw = u128::from_str_radix(hex, 16).map_err(|e| e.to_string())?;
    // Convert from 18 decimals
    Ok(raw as f64 / 1e18)
}

pub async fn switch_to_mantle() -> Result<(), String> {
    let params = js_sys::Array::new();
    let chain_obj = Object::new();
    Reflect::set(
        &chain_obj,
        &JsValue::from_str("chainId"),
        &JsValue::from_str("0x1388"), // 5000 in hex
    )
    .map_err(|_| "failed")?;
    params.push(&chain_obj);
    eth_request("wallet_switchEthereumChain", Some(params.into()))
        .await
        .map(|_| ())
}

pub async fn eth_send_transaction(
    from: &str,
    to: &str,
    data: &str,
    value: Option<&str>,
) -> Result<String, String> {
    let params = js_sys::Array::new();
    let tx_obj = Object::new();
    Reflect::set(&tx_obj, &JsValue::from_str("from"), &JsValue::from_str(from))
        .map_err(|_| "failed")?;
    Reflect::set(&tx_obj, &JsValue::from_str("to"), &JsValue::from_str(to))
        .map_err(|_| "failed")?;
    Reflect::set(&tx_obj, &JsValue::from_str("data"), &JsValue::from_str(data))
        .map_err(|_| "failed")?;
    if let Some(v) = value {
        Reflect::set(&tx_obj, &JsValue::from_str("value"), &JsValue::from_str(v))
            .map_err(|_| "failed")?;
    }
    params.push(&tx_obj);

    let result = eth_request("eth_sendTransaction", Some(params.into())).await?;
    result.as_string().ok_or("no tx hash".to_string())
}
