# =============================================================================
# MASTER BUILD LOOP — AI Training Arena (Windows PowerShell)
# Run by Mayor after each phase gate AND by Polecats after every 10 tasks
# =============================================================================

$ErrorActionPreference = "Continue"
$fail = 0
$projRoot = Split-Path $PSScriptRoot -Parent
Push-Location $projRoot

Write-Host "=== MASTER BUILD LOOP: AI Training Arena ===" -ForegroundColor Cyan
Write-Host "Directory: $projRoot"

# --- Codebase A: Smart Contracts ---
if (Test-Path "ai-training-arena-contracts") {
    Write-Host "`n[A] Compiling Smart Contracts..." -ForegroundColor Yellow
    Push-Location ai-training-arena-contracts
    npm ci --silent; if ($LASTEXITCODE -ne 0) { $fail = 1; Write-Host "FAIL: npm ci" -ForegroundColor Red }
    npx hardhat compile; if ($LASTEXITCODE -ne 0) { $fail = 1; Write-Host "FAIL: hardhat compile" -ForegroundColor Red }
    npx hardhat test; if ($LASTEXITCODE -ne 0) { $fail = 1; Write-Host "FAIL: hardhat tests" -ForegroundColor Red }
    Pop-Location
} else {
    Write-Host "[A] SKIP: contracts not scaffolded yet" -ForegroundColor DarkGray
}

# --- Codebase B: P2P Node ---
if (Test-Path "ai-training-arena-node") {
    Write-Host "`n[B] Building P2P Node (.NET 9)..." -ForegroundColor Yellow
    Push-Location ai-training-arena-node
    dotnet restore --verbosity quiet; if ($LASTEXITCODE -ne 0) { $fail = 1 }
    dotnet build --configuration Release --no-restore; if ($LASTEXITCODE -ne 0) { $fail = 1; Write-Host "FAIL: dotnet build" -ForegroundColor Red }
    dotnet test --configuration Release --no-build; if ($LASTEXITCODE -ne 0) { $fail = 1; Write-Host "FAIL: dotnet test" -ForegroundColor Red }
    Pop-Location
} else {
    Write-Host "[B] SKIP: node not scaffolded yet" -ForegroundColor DarkGray
}

# --- Codebase C: Frontend WASM ---
if (Test-Path "ai-training-arena-frontend") {
    Write-Host "`n[C] Building Frontend WASM..." -ForegroundColor Yellow
    Push-Location ai-training-arena-frontend
    cargo check; if ($LASTEXITCODE -ne 0) { $fail = 1; Write-Host "FAIL: cargo check" -ForegroundColor Red }
    cargo test; if ($LASTEXITCODE -ne 0) { $fail = 1; Write-Host "FAIL: cargo test" -ForegroundColor Red }
    wasm-pack build --target web; if ($LASTEXITCODE -ne 0) { $fail = 1; Write-Host "FAIL: wasm-pack build" -ForegroundColor Red }
    Pop-Location
} else {
    Write-Host "[C] SKIP: frontend not scaffolded yet" -ForegroundColor DarkGray
}

# --- Codebase D: Marketing Site ---
if (Test-Path "aitrainingarena.com") {
    Write-Host "`n[D] Building Marketing Site..." -ForegroundColor Yellow
    Push-Location aitrainingarena.com
    npm ci --silent; npm run build; if ($LASTEXITCODE -ne 0) { $fail = 1; Write-Host "FAIL: vite build" -ForegroundColor Red }
    Pop-Location
} else {
    Write-Host "[D] SKIP: marketing site not found" -ForegroundColor DarkGray
}

# --- Python Integration Tests ---
if (Test-Path "tests") {
    Write-Host "`n[PYTHON] Running integration tests..." -ForegroundColor Yellow
    Push-Location tests
    if (-not (Test-Path "pyenv")) {
        python -m venv pyenv
        & .\pyenv\Scripts\python.exe -m ensurepip --upgrade
        & .\pyenv\Scripts\pip.exe install -r requirements.txt -q
    }
    & .\pyenv\Scripts\Activate.ps1
    pytest -v --tb=short; if ($LASTEXITCODE -ne 0) { $fail = 1; Write-Host "FAIL: pytest" -ForegroundColor Red }
    deactivate
    Pop-Location
} else {
    Write-Host "[PYTHON] SKIP: tests/ not created yet" -ForegroundColor DarkGray
}

Pop-Location

Write-Host "`n============================================"
if ($fail -ne 0) {
    Write-Host "=== PHASE GATE FAILED — DO NOT PROCEED ===" -ForegroundColor Red
    Write-Host "============================================"
    exit 1
}
Write-Host "=== PHASE GATE PASSED ===" -ForegroundColor Green
Write-Host "============================================"
