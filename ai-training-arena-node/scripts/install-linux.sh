#!/bin/bash
# AI Training Arena Node — Linux Installer
# Installs .NET 9 SDK, builds the node, and registers a systemd service.
# Usage: bash install-linux.sh

set -e

REPO_URL="https://github.com/aitrainingarena/ai-training-arena-node"
INSTALL_DIR="$HOME/.local/share/ai-training-arena"
SERVICE_NAME="ai-training-arena-node"

echo "=== AI Training Arena Node Installer (Linux) ==="
echo "Install directory: $INSTALL_DIR"

# ─── 1. Install .NET 9 SDK ────────────────────────────────────────────────────
if ! command -v dotnet &>/dev/null || [[ $(dotnet --version | cut -d. -f1) -lt 9 ]]; then
  echo "[1/5] Installing .NET 9 SDK..."
  wget -q https://dot.net/v1/dotnet-install.sh -O /tmp/dotnet-install.sh
  chmod +x /tmp/dotnet-install.sh
  /tmp/dotnet-install.sh --version latest --install-dir "$HOME/.dotnet"
  export PATH="$HOME/.dotnet:$PATH"
  echo 'export PATH="$HOME/.dotnet:$PATH"' >> "$HOME/.bashrc"
else
  echo "[1/5] .NET SDK found: $(dotnet --version)"
fi

# ─── 2. Clone or update repository ───────────────────────────────────────────
echo "[2/5] Fetching source..."
if [ -d "$INSTALL_DIR" ]; then
  git -C "$INSTALL_DIR" pull --quiet
else
  git clone --depth=1 "$REPO_URL" "$INSTALL_DIR"
fi

# ─── 3. Build in Release mode ─────────────────────────────────────────────────
echo "[3/5] Building node (Release)..."
cd "$INSTALL_DIR"
dotnet restore --quiet
dotnet build --configuration Release --quiet

# ─── 4. Copy default config ───────────────────────────────────────────────────
echo "[4/5] Configuring..."
CONFIG_DIR="$INSTALL_DIR/config"
mkdir -p "$CONFIG_DIR"
if [ ! -f "$CONFIG_DIR/appsettings.json" ]; then
  cp src/AITrainingArena.API/appsettings.json "$CONFIG_DIR/appsettings.json"
  echo "  -> Config written to $CONFIG_DIR/appsettings.json"
  echo "  -> IMPORTANT: Edit $CONFIG_DIR/appsettings.json to set your WalletAddress and ModelPath"
fi

# ─── 5. Install systemd service ───────────────────────────────────────────────
echo "[5/5] Installing systemd service..."
DOTNET_PATH="$HOME/.dotnet/dotnet"
BINARY_PATH="$INSTALL_DIR/src/AITrainingArena.API/bin/Release/net9.0/AITrainingArena.API.dll"

cat > /tmp/ai-training-arena-node.service <<EOF
[Unit]
Description=AI Training Arena P2P Node
After=network.target

[Service]
Type=simple
User=$USER
WorkingDirectory=$INSTALL_DIR
ExecStart=$DOTNET_PATH $BINARY_PATH
Restart=on-failure
RestartSec=10
Environment="ASPNETCORE_ENVIRONMENT=Production"

[Install]
WantedBy=multi-user.target
EOF

if command -v systemctl &>/dev/null; then
  sudo cp /tmp/ai-training-arena-node.service /etc/systemd/system/$SERVICE_NAME.service
  sudo systemctl daemon-reload
  sudo systemctl enable $SERVICE_NAME
  echo ""
  echo "=== Installation complete! ==="
  echo "Start:  sudo systemctl start $SERVICE_NAME"
  echo "Status: sudo systemctl status $SERVICE_NAME"
  echo "Logs:   journalctl -u $SERVICE_NAME -f"
else
  echo ""
  echo "=== Installation complete (no systemd found) ==="
  echo "Start manually: $DOTNET_PATH $BINARY_PATH"
fi

echo ""
echo "NEXT STEPS:"
echo "  1. Edit $CONFIG_DIR/appsettings.json"
echo "  2. Set WalletAddress and WalletPrivateKey"
echo "  3. Set ModelPath to your ONNX model file"
echo "  4. Start the node service"
