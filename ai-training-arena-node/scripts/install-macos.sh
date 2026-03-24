#!/bin/bash
# AI Training Arena Node — macOS Installer
# Installs .NET 9 SDK via Homebrew and registers a launchd service for auto-start.
# Usage: bash install-macos.sh

set -e

INSTALL_DIR="$HOME/Library/Application Support/AITrainingArena"
SERVICE_LABEL="com.aitrainingarena.node"
PLIST_PATH="$HOME/Library/LaunchAgents/$SERVICE_LABEL.plist"
REPO_URL="https://github.com/aitrainingarena/ai-training-arena-node"

echo "=== AI Training Arena Node Installer (macOS) ==="
echo "Install directory: $INSTALL_DIR"

# ─── 1. Install Homebrew if needed ────────────────────────────────────────────
if ! command -v brew &>/dev/null; then
  echo "[1/5] Installing Homebrew..."
  /bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
else
  echo "[1/5] Homebrew found: $(brew --version | head -1)"
fi

# ─── 2. Install .NET 9 SDK ────────────────────────────────────────────────────
if ! command -v dotnet &>/dev/null || [[ $(dotnet --version | cut -d. -f1) -lt 9 ]]; then
  echo "[2/5] Installing .NET 9 SDK..."
  brew install --cask dotnet-sdk
else
  echo "[2/5] .NET SDK found: $(dotnet --version)"
fi

# ─── 3. Clone or update repository ───────────────────────────────────────────
echo "[3/5] Fetching source..."
mkdir -p "$INSTALL_DIR"
if [ -d "$INSTALL_DIR/.git" ]; then
  git -C "$INSTALL_DIR" pull --quiet
else
  git clone --depth=1 "$REPO_URL" "$INSTALL_DIR"
fi

# ─── 4. Build ─────────────────────────────────────────────────────────────────
echo "[4/5] Building node (Release)..."
cd "$INSTALL_DIR"
dotnet restore --quiet
dotnet build --configuration Release --quiet

# Copy config if needed
CONFIG_DIR="$INSTALL_DIR/config"
mkdir -p "$CONFIG_DIR"
if [ ! -f "$CONFIG_DIR/appsettings.json" ]; then
  cp src/AITrainingArena.API/appsettings.json "$CONFIG_DIR/appsettings.json"
  echo "  -> Config written to $CONFIG_DIR/appsettings.json"
  echo "  -> IMPORTANT: Edit this file to set WalletAddress and ModelPath"
fi

# ─── 5. Install launchd plist ─────────────────────────────────────────────────
echo "[5/5] Installing launchd service..."
DOTNET_PATH="$(which dotnet)"
BINARY_PATH="$INSTALL_DIR/src/AITrainingArena.API/bin/Release/net9.0/AITrainingArena.API.dll"
LOG_DIR="$INSTALL_DIR/logs"
mkdir -p "$LOG_DIR"

cat > "$PLIST_PATH" <<EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>Label</key>
    <string>$SERVICE_LABEL</string>
    <key>ProgramArguments</key>
    <array>
        <string>$DOTNET_PATH</string>
        <string>$BINARY_PATH</string>
    </array>
    <key>WorkingDirectory</key>
    <string>$INSTALL_DIR</string>
    <key>RunAtLoad</key>
    <true/>
    <key>KeepAlive</key>
    <true/>
    <key>StandardOutPath</key>
    <string>$LOG_DIR/node.log</string>
    <key>StandardErrorPath</key>
    <string>$LOG_DIR/node-error.log</string>
    <key>EnvironmentVariables</key>
    <dict>
        <key>ASPNETCORE_ENVIRONMENT</key>
        <string>Production</string>
    </dict>
</dict>
</plist>
EOF

# Unload existing if any
launchctl unload "$PLIST_PATH" 2>/dev/null || true
# Load the new service
launchctl load "$PLIST_PATH"

echo ""
echo "=== Installation complete! ==="
echo "Start:  launchctl start $SERVICE_LABEL"
echo "Stop:   launchctl stop $SERVICE_LABEL"
echo "Logs:   tail -f $LOG_DIR/node.log"
echo ""
echo "NEXT STEPS:"
echo "  1. Edit $CONFIG_DIR/appsettings.json"
echo "  2. Set WalletAddress, WalletPrivateKey, and ModelPath"
echo "  3. Restart: launchctl stop $SERVICE_LABEL && launchctl start $SERVICE_LABEL"
