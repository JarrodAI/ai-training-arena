/// Content Security Policy helpers for WASM app
pub struct CspConfig {
    pub allow_localhost: bool,
    pub allow_mantle_rpc: bool,
    pub allow_ipfs_gateway: bool,
}

impl Default for CspConfig {
    fn default() -> Self {
        Self {
            allow_localhost: true,
            allow_mantle_rpc: true,
            allow_ipfs_gateway: true,
        }
    }
}

/// Apply CSP meta tag to document head
pub fn apply_csp_headers() {
    if let Some(document) = web_sys::window().and_then(|w| w.document()) {
        if let Some(head) = document.head() {
            if let Ok(meta) = document.create_element("meta") {
                let _ = meta.set_attribute("http-equiv", "Content-Security-Policy");
                let csp = build_csp_string(&CspConfig::default());
                let _ = meta.set_attribute("content", &csp);
                let _ = head.append_child(&meta);
            }
        }
    }
}

fn build_csp_string(config: &CspConfig) -> String {
    let mut connect_src = vec!["'self'"];
    if config.allow_localhost {
        connect_src.push("ws://localhost:8080");
        connect_src.push("http://localhost:8081");
    }
    if config.allow_mantle_rpc {
        connect_src.push("https://rpc.mantle.xyz");
        connect_src.push("https://rpc.testnet.mantle.xyz");
    }
    if config.allow_ipfs_gateway {
        connect_src.push("https://ipfs.io");
        connect_src.push("https://cloudflare-ipfs.com");
    }

    format!(
        "default-src 'self'; script-src 'self' 'wasm-unsafe-eval'; connect-src {}; img-src 'self' data: https:; style-src 'self' 'unsafe-inline'",
        connect_src.join(" ")
    )
}
