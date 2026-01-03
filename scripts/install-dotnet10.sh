#!/bin/bash
# install-dotnet10.sh: Installs .NET 10 SDK on Linux (user-local install)
set -e

# Download dotnet-install.sh if not present
if [ ! -f dotnet-install.sh ]; then
    curl -L https://dot.net/v1/dotnet-install.sh -o dotnet-install.sh
    chmod +x dotnet-install.sh
fi

# Install .NET 10 SDK (latest 10.x)
./dotnet-install.sh --version 10.0.100

# Add to PATH and DOTNET_ROOT in ~/.bashrc if not already present
if ! grep -q 'export DOTNET_ROOT=\$HOME/.dotnet' ~/.bashrc; then
    echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc
fi
if ! grep -q 'export PATH=\$HOME/.dotnet:$PATH' ~/.bashrc; then
    echo 'export PATH=$HOME/.dotnet:$PATH' >> ~/.bashrc
fi

echo ".NET 10 SDK installed. Please run: source ~/.bashrc or restart your terminal."
