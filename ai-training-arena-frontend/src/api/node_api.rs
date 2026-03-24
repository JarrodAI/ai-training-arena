use serde::{Deserialize, Serialize};
use crate::types::{
    agent::{Agent, AgentClass},
    battle::BattleResult,
    leaderboard::LeaderboardEntry,
    marketplace::NodeStatus,
};

const NODE_API_BASE: &str = "http://localhost:8081";

#[derive(Debug, Clone)]
pub enum ApiError {
    NetworkError(String),
    ParseError(String),
    ServerError(u16, String),
}

impl std::fmt::Display for ApiError {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        match self {
            ApiError::NetworkError(e) => write!(f, "Network error: {}", e),
            ApiError::ParseError(e) => write!(f, "Parse error: {}", e),
            ApiError::ServerError(code, msg) => write!(f, "Server error {}: {}", code, msg),
        }
    }
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct NodeStatusResponse {
    pub status: String,
    pub version: String,
    pub peer_count: u32,
    pub uptime_secs: u64,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct TxHash(pub String);

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct StakeRequest {
    pub nft_id: u64,
    pub amount: f64,
}

pub struct NodeApi;

impl NodeApi {
    async fn get<T: for<'de> Deserialize<'de>>(path: &str) -> Result<T, ApiError> {
        let url = format!("{}{}", NODE_API_BASE, path);
        let resp = gloo_net::http::Request::get(&url)
            .send()
            .await
            .map_err(|e| ApiError::NetworkError(e.to_string()))?;

        let text = resp
            .text()
            .await
            .map_err(|e| ApiError::ParseError(e.to_string()))?;

        serde_json::from_str(&text).map_err(|e| ApiError::ParseError(e.to_string()))
    }

    async fn post<B: Serialize, T: for<'de> Deserialize<'de>>(
        path: &str,
        body: &B,
    ) -> Result<T, ApiError> {
        let url = format!("{}{}", NODE_API_BASE, path);
        let body_str =
            serde_json::to_string(body).map_err(|e| ApiError::ParseError(e.to_string()))?;

        let resp = gloo_net::http::Request::post(&url)
            .header("Content-Type", "application/json")
            .body(body_str)
            .map_err(|e| ApiError::NetworkError(e.to_string()))?
            .send()
            .await
            .map_err(|e| ApiError::NetworkError(e.to_string()))?;

        let text = resp
            .text()
            .await
            .map_err(|e| ApiError::ParseError(e.to_string()))?;

        serde_json::from_str(&text).map_err(|e| ApiError::ParseError(e.to_string()))
    }

    pub async fn get_status() -> Result<NodeStatus, ApiError> {
        let resp: NodeStatusResponse = Self::get("/api/status").await?;
        if resp.status == "online" {
            Ok(NodeStatus::Online)
        } else {
            Ok(NodeStatus::Offline)
        }
    }

    pub async fn get_agents() -> Result<Vec<Agent>, ApiError> {
        Self::get("/api/agents").await
    }

    pub async fn get_battles(last: u32) -> Result<Vec<BattleResult>, ApiError> {
        Self::get(&format!("/api/battles?last={}", last)).await
    }

    pub async fn get_leaderboard(class: AgentClass) -> Result<Vec<LeaderboardEntry>, ApiError> {
        Self::get(&format!("/api/leaderboard/{}", class.label())).await
    }

    pub async fn get_rewards() -> Result<f64, ApiError> {
        #[derive(Deserialize)]
        struct RewardsResp {
            pending: f64,
        }
        let resp: RewardsResp = Self::get("/api/rewards").await?;
        Ok(resp.pending)
    }

    pub async fn toggle_auto_battle(enabled: bool) -> Result<(), ApiError> {
        #[derive(Serialize)]
        struct Body {
            enabled: bool,
        }
        #[derive(Deserialize)]
        struct Resp {}
        let _: Resp = Self::post("/api/auto-battle", &Body { enabled }).await?;
        Ok(())
    }

    pub async fn claim_rewards() -> Result<TxHash, ApiError> {
        #[derive(Serialize)]
        struct Body {}
        #[derive(Deserialize)]
        struct Resp {
            tx_hash: String,
        }
        let resp: Resp = Self::post("/api/claim-rewards", &Body {}).await?;
        Ok(TxHash(resp.tx_hash))
    }

    pub async fn stake(nft_id: u64, amount: f64) -> Result<TxHash, ApiError> {
        #[derive(Deserialize)]
        struct Resp {
            tx_hash: String,
        }
        let resp: Resp = Self::post("/api/stake", &StakeRequest { nft_id, amount }).await?;
        Ok(TxHash(resp.tx_hash))
    }

    pub async fn unstake(nft_id: u64) -> Result<TxHash, ApiError> {
        #[derive(Serialize)]
        struct Body {
            nft_id: u64,
        }
        #[derive(Deserialize)]
        struct Resp {
            tx_hash: String,
        }
        let resp: Resp = Self::post("/api/unstake", &Body { nft_id }).await?;
        Ok(TxHash(resp.tx_hash))
    }

    pub async fn pin_to_ipfs(data: &str) -> Result<String, ApiError> {
        #[derive(Serialize)]
        struct Body<'a> {
            data: &'a str,
        }
        #[derive(Deserialize)]
        struct Resp {
            cid: String,
        }
        let resp: Resp = Self::post("/api/ipfs/pin", &Body { data }).await?;
        Ok(resp.cid)
    }
}
