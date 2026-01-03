#!/bin/bash
# setup.sh: Runs all install scripts for project dependencies
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

bash "$SCRIPT_DIR/install-dotnet10.sh"
bash "$SCRIPT_DIR/install-sqlite3.sh"
bash "$SCRIPT_DIR/install-mosquitto.sh"
bash "$SCRIPT_DIR/install-docker.sh"

echo "All dependencies installed successfully."
