# AI Training Arena Node — Windows Installer
# Installs .NET 9 SDK, builds the node, and registers as a Windows Service.
# Usage: Run as Administrator in PowerShell

#Requires -RunAsAdministrator

param(
    [string]$InstallDir = "$env:LOCALAPPDATA\AITrainingArena",
    [string]$RepoUrl = "https://github.com/aitrainingarena/ai-training-arena-node"
)

$ErrorActionPreference = "Stop"
$ServiceName = "AITrainingArenaNode"

Write-Host "=== AI Training Arena Node Installer (Windows) ===" -ForegroundColor Cyan
Write-Host "Install directory: $InstallDir"

# 1. Install .NET 9 SDK
Write-Host "[1/5] Checking .NET 9 SDK..." -ForegroundColor Yellow
$dotnetVersion = $null
try {
    $dotnetVersion = (& dotnet --version 2>&1)[0]
} catch {}

if (-not $dotnetVersion -or [int]$dotnetVersion.Split('.')[0] -lt 9) {
    Write-Host "  Downloading .NET 9 SDK installer..."
    $dotnetInstaller = "$env:TEMP\dotnet-install.ps1"
    Invoke-WebRequest -Uri "https://dot.net/v1/dotnet-install.ps1" -OutFile $dotnetInstaller
    & $dotnetInstaller -Version latest -InstallDir "$env:LOCALAPPDATA\Microsoft\dotnet"
    $env:PATH = "$env:LOCALAPPDATA\Microsoft\dotnet;$env:PATH"
} else {
    Write-Host "  .NET SDK found: $dotnetVersion" -ForegroundColor Green
}

# 2. Clone or update repository
Write-Host "[2/5] Fetching source..." -ForegroundColor Yellow
if (Test-Path $InstallDir) {
    Push-Location $InstallDir
    git pull --quiet
    Pop-Location
} else {
    git clone --depth=1 $RepoUrl $InstallDir
}

# 3. Build in Release mode
Write-Host "[3/5] Building node..." -ForegroundColor Yellow
Push-Location $InstallDir
dotnet restore --quiet
dotnet build --configuration Release --quiet
Pop-Location

# 4. Copy default config
Write-Host "[4/5] Configuring..." -ForegroundColor Yellow
$configDir = "$InstallDir\config"
New-Item -ItemType Directory -Force -Path $configDir | Out-Null
$configFile = "$configDir\appsettings.json"
if (-not (Test-Path $configFile)) {
    Copy-Item "$InstallDir\src\AITrainingArena.API\appsettings.json" $configFile
    Write-Host "  -> Config written to $configFile" -ForegroundColor Cyan
    Write-Host "  -> IMPORTANT: Edit $configFile to set WalletAddress and ModelPath" -ForegroundColor Yellow
}

# 5. Register Windows Service
Write-Host "[5/5] Registering Windows Service..." -ForegroundColor Yellow
$binaryPath = "$InstallDir\src\AITrainingArena.API\bin\Release\net9.0\AITrainingArena.API.exe"

# Remove existing service if present
$existing = Get-Service -Name $ServiceName -ErrorAction SilentlyContinue
if ($existing) {
    Stop-Service $ServiceName -Force -ErrorAction SilentlyContinue
    & sc.exe delete $ServiceName | Out-Null
    Start-Sleep -Seconds 2
}

New-Service `
    -Name $ServiceName `
    -BinaryPathName "`"$binaryPath`" --contentRoot `"$InstallDir`"" `
    -DisplayName "AI Training Arena P2P Node" `
    -Description "Runs the AI Training Arena P2P node for battle participation and reward earning." `
    -StartupType Automatic | Out-Null

Write-Host ""
Write-Host "=== Installation complete! ===" -ForegroundColor Green
Write-Host "Start service:  Start-Service $ServiceName"
Write-Host "Stop service:   Stop-Service $ServiceName"
Write-Host "View logs:      Get-EventLog -LogName Application -Source $ServiceName -Newest 50"
Write-Host ""
Write-Host "NEXT STEPS:"
Write-Host "  1. Edit $configFile"
Write-Host "  2. Set WalletAddress, WalletPrivateKey, and ModelPath"
Write-Host "  3. Run: Start-Service $ServiceName"
