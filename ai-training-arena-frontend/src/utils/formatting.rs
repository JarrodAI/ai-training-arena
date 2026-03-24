pub fn format_ata(amount: f64) -> String {
    if amount >= 1_000_000.0 {
        format!("{:.2}M ATA", amount / 1_000_000.0)
    } else if amount >= 1_000.0 {
        format!("{:.2}K ATA", amount / 1_000.0)
    } else {
        format!("{:.4} ATA", amount)
    }
}

pub fn format_address_short(addr: &str) -> String {
    let addr = if addr.starts_with("0x") { addr } else { addr };
    if addr.len() >= 10 {
        format!("{}...{}", &addr[..6], &addr[addr.len() - 4..])
    } else {
        addr.to_string()
    }
}

pub fn format_duration(secs: u32) -> String {
    let minutes = secs / 60;
    let seconds = secs % 60;
    format!("{:02}:{:02}", minutes, seconds)
}

pub fn format_large_number(n: f64) -> String {
    if n >= 1_000_000.0 {
        format!("{:.2}M", n / 1_000_000.0)
    } else if n >= 1_000.0 {
        let int_part = n as u64;
        let thousands = int_part / 1000;
        let remainder = int_part % 1000;
        format!("{},{:03}", thousands, remainder)
    } else {
        format!("{:.2}", n)
    }
}

pub fn format_mnt(amount: f64) -> String {
    format!("{:.4} MNT", amount)
}

pub fn format_win_rate(rate: f64) -> String {
    format!("{:.1}%", rate * 100.0)
}

pub fn format_elo(elo: u32) -> String {
    format!("{}", elo)
}
