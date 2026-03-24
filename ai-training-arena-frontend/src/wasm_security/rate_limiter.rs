use std::collections::HashMap;

/// Client-side rate limiter for API calls
pub struct RateLimiter {
    calls: HashMap<String, Vec<f64>>,
    window_ms: f64,
    max_calls: usize,
}

impl RateLimiter {
    pub fn new(window_ms: f64, max_calls: usize) -> Self {
        Self {
            calls: HashMap::new(),
            window_ms,
            max_calls,
        }
    }

    pub fn check(&mut self, key: &str) -> bool {
        let now = js_sys::Date::now();
        let cutoff = now - self.window_ms;

        let entry = self.calls.entry(key.to_string()).or_default();
        entry.retain(|&t| t > cutoff);

        if entry.len() >= self.max_calls {
            return false;
        }

        entry.push(now);
        true
    }

    pub fn reset(&mut self, key: &str) {
        self.calls.remove(key);
    }
}
