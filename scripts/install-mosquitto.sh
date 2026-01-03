#!/bin/bash
# install-mosquitto.sh: Installs Mosquitto broker and clients on Linux
set -e

sudo apt-get update
sudo apt-get install -y mosquitto mosquitto-clients

echo "Mosquitto broker and clients installed successfully."
