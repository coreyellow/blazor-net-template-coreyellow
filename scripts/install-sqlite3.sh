#!/bin/bash
# install-sqlite3.sh: Installs sqlite3 on Linux
set -e

sudo apt-get update
sudo apt-get install -y sqlite3 libsqlite3-dev

echo "sqlite3 installed successfully."
