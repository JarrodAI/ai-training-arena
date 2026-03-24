use serde::{Deserialize, Serialize};

#[derive(Clone, Debug, Serialize, Deserialize, PartialEq, Default)]
pub enum AgentClass {
    #[default]
    A,
    B,
    C,
    D,
    E,
}

impl AgentClass {
    pub fn color(&self) -> &str {
        match self {
            AgentClass::A => "#14F195",
            AgentClass::B => "#00C2FF",
            AgentClass::C => "#9945FF",
            AgentClass::D => "#FFD166",
            AgentClass::E => "#FF6B6B",
        }
    }

    pub fn reward_multiplier(&self) -> f64 {
        match self {
            AgentClass::A => 1.0,
            AgentClass::B => 1.2,
            AgentClass::C => 1.5,
            AgentClass::D => 2.0,
            AgentClass::E => 3.0,
        }
    }

    pub fn base_reward(&self) -> f64 {
        match self {
            AgentClass::A => 0.041,
            AgentClass::B => 0.049,
            AgentClass::C => 0.062,
            AgentClass::D => 0.082,
            AgentClass::E => 0.123,
        }
    }

    pub fn name(&self) -> &str {
        match self {
            AgentClass::A => "Scout",
            AgentClass::B => "Ranger",
            AgentClass::C => "Vanguard",
            AgentClass::D => "Titan",
            AgentClass::E => "Overlord",
        }
    }

    pub fn params(&self) -> &str {
        match self {
            AgentClass::A => "3B-7B",
            AgentClass::B => "7B-32B",
            AgentClass::C => "32B-70B",
            AgentClass::D => "70B-405B",
            AgentClass::E => "405B+",
        }
    }

    pub fn price_mnt(&self) -> u64 {
        match self {
            AgentClass::A => 10,
            AgentClass::B => 50,
            AgentClass::C => 200,
            AgentClass::D => 800,
            AgentClass::E => 3000,
        }
    }

    pub fn stake_wata(&self) -> u64 {
        match self {
            AgentClass::A => 100,
            AgentClass::B => 500,
            AgentClass::C => 2000,
            AgentClass::D => 8000,
            AgentClass::E => 30000,
        }
    }

    pub fn supply(&self) -> u32 {
        match self {
            AgentClass::A => 15000,
            AgentClass::B => 6000,
            AgentClass::C => 2500,
            AgentClass::D => 1200,
            AgentClass::E => 300,
        }
    }

    pub fn label(&self) -> &str {
        match self {
            AgentClass::A => "A",
            AgentClass::B => "B",
            AgentClass::C => "C",
            AgentClass::D => "D",
            AgentClass::E => "E",
        }
    }

    pub fn all() -> Vec<AgentClass> {
        vec![
            AgentClass::A,
            AgentClass::B,
            AgentClass::C,
            AgentClass::D,
            AgentClass::E,
        ]
    }
}

impl std::fmt::Display for AgentClass {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(f, "{}", self.label())
    }
}

#[derive(Clone, Debug, Serialize, Deserialize, PartialEq)]
pub struct Agent {
    pub nft_id: u64,
    pub class: AgentClass,
    pub model_name: String,
    pub elo_rating: u32,
    pub total_battles: u32,
    pub wins: u32,
    pub owner: String,
    pub is_active: bool,
    pub staked_amount: f64,
    pub win_rate: f64,
    pub total_rewards_earned: f64,
}

impl Agent {
    pub fn losses(&self) -> u32 {
        self.total_battles.saturating_sub(self.wins)
    }
}
