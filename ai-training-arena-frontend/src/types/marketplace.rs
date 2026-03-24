use serde::{Deserialize, Serialize};

#[derive(Clone, Debug, Serialize, Deserialize, PartialEq, Default)]
pub enum DataCategory {
    #[default]
    All,
    BattleLog,
    ModelCheckpoint,
    QuestionCorpus,
    TrainingSet,
}

impl DataCategory {
    pub fn label(&self) -> &str {
        match self {
            DataCategory::All => "ALL",
            DataCategory::BattleLog => "BATTLE_LOG",
            DataCategory::ModelCheckpoint => "MODEL_CHECKPOINT",
            DataCategory::QuestionCorpus => "QUESTION_CORPUS",
            DataCategory::TrainingSet => "TRAINING_SET",
        }
    }

    pub fn all() -> Vec<DataCategory> {
        vec![
            DataCategory::All,
            DataCategory::BattleLog,
            DataCategory::ModelCheckpoint,
            DataCategory::QuestionCorpus,
            DataCategory::TrainingSet,
        ]
    }
}

#[derive(Clone, Debug, Serialize, Deserialize, PartialEq)]
pub struct DataListing {
    pub listing_id: u64,
    pub seller_nft_id: u64,
    pub seller_address: String,
    pub category: DataCategory,
    pub ipfs_hash: String,
    pub price_ata: f64,
    pub total_sales: u32,
    pub description: String,
    pub created_at: u64,
    pub is_active: bool,
}

#[derive(Clone, Debug, Serialize, Deserialize, PartialEq, Default)]
pub enum ProposalStatus {
    #[default]
    Active,
    Succeeded,
    Defeated,
    Queued,
    Executed,
    Cancelled,
}

impl ProposalStatus {
    pub fn label(&self) -> &str {
        match self {
            ProposalStatus::Active => "Active",
            ProposalStatus::Succeeded => "Succeeded",
            ProposalStatus::Defeated => "Defeated",
            ProposalStatus::Queued => "Queued",
            ProposalStatus::Executed => "Executed",
            ProposalStatus::Cancelled => "Cancelled",
        }
    }

    pub fn color(&self) -> &str {
        match self {
            ProposalStatus::Active => "#14F195",
            ProposalStatus::Succeeded => "#00C2FF",
            ProposalStatus::Defeated => "#FF6B6B",
            ProposalStatus::Queued => "#FFD166",
            ProposalStatus::Executed => "#9945FF",
            ProposalStatus::Cancelled => "#8B949E",
        }
    }
}

#[derive(Clone, Debug, Serialize, Deserialize, PartialEq)]
pub struct Proposal {
    pub proposal_id: u64,
    pub title: String,
    pub description: String,
    pub proposer: String,
    pub status: ProposalStatus,
    pub votes_for: u64,
    pub votes_against: u64,
    pub votes_abstain: u64,
    pub voting_end_timestamp: u64,
    pub quorum_reached: bool,
    pub target_contract: String,
}

impl Proposal {
    pub fn total_votes(&self) -> u64 {
        self.votes_for + self.votes_against + self.votes_abstain
    }

    pub fn for_pct(&self) -> f64 {
        let total = self.total_votes();
        if total == 0 {
            0.0
        } else {
            self.votes_for as f64 / total as f64 * 100.0
        }
    }

    pub fn against_pct(&self) -> f64 {
        let total = self.total_votes();
        if total == 0 {
            0.0
        } else {
            self.votes_against as f64 / total as f64 * 100.0
        }
    }

    pub fn abstain_pct(&self) -> f64 {
        let total = self.total_votes();
        if total == 0 {
            0.0
        } else {
            self.votes_abstain as f64 / total as f64 * 100.0
        }
    }
}

#[derive(Clone, Debug, Serialize, Deserialize, PartialEq, Default)]
pub enum NodeStatus {
    #[default]
    Offline,
    Connecting,
    Online,
    Error(String),
}

impl NodeStatus {
    pub fn label(&self) -> &str {
        match self {
            NodeStatus::Offline => "Offline",
            NodeStatus::Connecting => "Connecting...",
            NodeStatus::Online => "Online",
            NodeStatus::Error(_) => "Error",
        }
    }

    pub fn color(&self) -> &str {
        match self {
            NodeStatus::Offline => "#8B949E",
            NodeStatus::Connecting => "#FFD166",
            NodeStatus::Online => "#14F195",
            NodeStatus::Error(_) => "#FF6B6B",
        }
    }
}
