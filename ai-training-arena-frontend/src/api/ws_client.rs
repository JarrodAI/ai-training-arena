use wasm_bindgen::prelude::*;
use wasm_bindgen::JsCast;
use web_sys::{MessageEvent, WebSocket};
use crate::types::battle::WsMessage;

pub struct WsClient {
    ws: WebSocket,
}

impl WsClient {
    pub fn connect() -> Result<Self, JsValue> {
        let ws = WebSocket::new("ws://localhost:8080/ws")?;
        ws.set_binary_type(web_sys::BinaryType::Arraybuffer);
        Ok(Self { ws })
    }

    pub fn subscribe<F>(&self, callback: F)
    where
        F: Fn(WsMessage) + 'static,
    {
        let on_message = Closure::wrap(Box::new(move |event: MessageEvent| {
            if let Some(text) = event.data().as_string() {
                if let Ok(msg) = serde_json::from_str::<WsMessage>(&text) {
                    callback(msg);
                }
            }
        }) as Box<dyn FnMut(MessageEvent)>);

        self.ws
            .set_onmessage(Some(on_message.as_ref().unchecked_ref()));
        on_message.forget();
    }

    pub fn send(&self, msg: &WsMessage) -> Result<(), JsValue> {
        let text = serde_json::to_string(msg)
            .map_err(|e| JsValue::from_str(&e.to_string()))?;
        self.ws.send_with_str(&text)
    }

    pub fn on_open<F>(&self, callback: F)
    where
        F: Fn() + 'static,
    {
        let on_open = Closure::wrap(Box::new(move |_: JsValue| {
            callback();
        }) as Box<dyn FnMut(JsValue)>);
        self.ws.set_onopen(Some(on_open.as_ref().unchecked_ref()));
        on_open.forget();
    }

    pub fn on_close<F>(&self, callback: F)
    where
        F: Fn() + 'static,
    {
        let on_close = Closure::wrap(Box::new(move |_: JsValue| {
            callback();
        }) as Box<dyn FnMut(JsValue)>);
        self.ws.set_onclose(Some(on_close.as_ref().unchecked_ref()));
        on_close.forget();
    }

    pub fn on_error<F>(&self, callback: F)
    where
        F: Fn(String) + 'static,
    {
        let on_error = Closure::wrap(Box::new(move |e: JsValue| {
            let msg = format!("{:?}", e);
            callback(msg);
        }) as Box<dyn FnMut(JsValue)>);
        self.ws.set_onerror(Some(on_error.as_ref().unchecked_ref()));
        on_error.forget();
    }

    pub fn close(&self) {
        let _ = self.ws.close();
    }
}
