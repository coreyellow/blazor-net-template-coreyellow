#!/bin/bash
set -e

# Installation script for BlazorNetApp

echo "Installing BlazorNetApp..."

# Check if running as root
if [ "$EUID" -ne 0 ]; then 
   echo "Please run as root (use sudo)"
   exit 1
fi

# Create application directory
echo "Creating application directory..."
mkdir -p /opt/blazor-net-app

# Copy application files
echo "Copying application files..."
cp -r ./publish/* /opt/blazor-net-app/

# Set permissions
echo "Setting permissions..."
chown -R www-data:www-data /opt/blazor-net-app
chmod -R 755 /opt/blazor-net-app

# Create data directory for database
echo "Creating data directory..."
mkdir -p /var/lib/blazor-net-app
chown www-data:www-data /var/lib/blazor-net-app
chmod 750 /var/lib/blazor-net-app

# Copy systemd service file
echo "Installing systemd service..."
cp deployment/systemd/blazor-net-app.service /etc/systemd/system/

# Reload systemd
systemctl daemon-reload

# Enable and start service
echo "Enabling and starting service..."
systemctl enable blazor-net-app.service
systemctl start blazor-net-app.service

echo ""
echo "✓ BlazorNetApp installed successfully!"
echo ""
echo "Service status:"
systemctl status blazor-net-app.service --no-pager
echo ""
echo "Access the application at:"
echo "  Web UI: http://localhost:5000"
echo "  API Documentation: http://localhost:5000/swagger"
echo ""
echo "Manage the service with:"
echo "  sudo systemctl status blazor-net-app"
echo "  sudo systemctl restart blazor-net-app"
echo "  sudo systemctl stop blazor-net-app"
echo "  sudo systemctl start blazor-net-app"
echo ""
