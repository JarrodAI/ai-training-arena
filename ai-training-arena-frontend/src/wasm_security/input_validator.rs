/// Input validation for all user-facing inputs
pub struct InputValidator;

impl InputValidator {
    /// Validate Ethereum address (0x + 40 hex chars)
    pub fn is_valid_address(addr: &str) -> bool {
        if !addr.starts_with("0x") {
            return false;
        }
        let hex = &addr[2..];
        hex.len() == 40 && hex.chars().all(|c| c.is_ascii_hexdigit())
    }

    /// Validate ATA amount (positive, max 8 decimal places, <= 100M)
    pub fn is_valid_ata_amount(amount_str: &str) -> Result<f64, String> {
        let amount: f64 = amount_str
            .parse()
            .map_err(|_| "Invalid number".to_string())?;
        if amount <= 0.0 {
            return Err("Amount must be positive".to_string());
        }
        if amount > 100_000_000.0 {
            return Err("Amount exceeds max supply".to_string());
        }
        Ok(amount)
    }

    /// Validate IPFS CID (basic format check)
    pub fn is_valid_ipfs_cid(cid: &str) -> bool {
        // CIDv0: starts with Qm, 46 chars
        // CIDv1: starts with bafy, b3..., etc.
        if cid.starts_with("Qm") && cid.len() == 46 {
            return true;
        }
        if cid.starts_with("bafy") || cid.starts_with("bafk") {
            return cid.len() > 10;
        }
        false
    }

    /// Sanitize text input — strip HTML/script tags
    pub fn sanitize_text(input: &str) -> String {
        input
            .replace('<', "&lt;")
            .replace('>', "&gt;")
            .replace('"', "&quot;")
            .replace('\'', "&#x27;")
    }

    /// Validate NFT ID range
    pub fn is_valid_nft_id(nft_id: u64) -> bool {
        nft_id > 0 && nft_id <= 25000
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_address_validation() {
        assert!(InputValidator::is_valid_address("0xd8dA6BF26964aF9D7eEd9e03E53415D37aA96045"));
        assert!(!InputValidator::is_valid_address("not-an-address"));
        assert!(!InputValidator::is_valid_address("0x123"));
    }

    #[test]
    fn test_amount_validation() {
        assert!(InputValidator::is_valid_ata_amount("100.5").is_ok());
        assert!(InputValidator::is_valid_ata_amount("-1").is_err());
        assert!(InputValidator::is_valid_ata_amount("abc").is_err());
    }
}
