#!/bin/bash
set -e

# Build DEB package for blazor-net-app
# Usage: ./build-deb.sh <arch> <version>
# Example: ./build-deb.sh amd64 1.0.0

if [ $# -ne 2 ]; then
    echo "Usage: $0 <arch> <version>"
    echo "Example: $0 amd64 1.0.0"
    exit 1
fi

ARCH=$1
VERSION=$2

echo "Building DEB package for architecture: ${ARCH}, version: ${VERSION}"

# Map architecture to .NET runtime
if [ "${ARCH}" = "amd64" ]; then
    RUNTIME="linux-x64"
elif [ "${ARCH}" = "arm64" ]; then
    RUNTIME="linux-arm64"
else
    echo "Unsupported architecture: ${ARCH}"
    exit 1
fi

# Create build directory
mkdir -p build

# Setup .NET if not already available (for local testing)
if ! command -v dotnet &> /dev/null; then
    echo "Error: dotnet command not found. Please install .NET SDK."
    exit 1
fi

# Publish application
echo "Publishing application for ${RUNTIME}..."
dotnet publish src/BlazorNetApp.Api/BlazorNetApp.Api.csproj \
    --configuration Release \
    --output publish/${ARCH}/opt/blazor-net-app \
    --runtime ${RUNTIME} \
    --self-contained true

# Create DEB package structure
PACKAGE_DIR="deb-package-${ARCH}"
echo "Creating DEB package structure in ${PACKAGE_DIR}..."

# Create directory structure
mkdir -p ${PACKAGE_DIR}/DEBIAN
mkdir -p ${PACKAGE_DIR}/opt/blazor-net-app
mkdir -p ${PACKAGE_DIR}/etc/systemd/system
mkdir -p ${PACKAGE_DIR}/usr/share/doc/blazor-net-app

# Copy application files
echo "Copying application files..."
cp -r publish/${ARCH}/opt/blazor-net-app/* ${PACKAGE_DIR}/opt/blazor-net-app/

# Copy systemd service file
echo "Copying systemd service file..."
cp deployment/systemd/blazor-net-app.service ${PACKAGE_DIR}/etc/systemd/system/

# Copy and process control file from template
echo "Creating control file from template..."
sed -e "s/{{VERSION}}/${VERSION}/g" \
    -e "s/{{ARCH}}/${ARCH}/g" \
    debian/DEBIAN/control.template > ${PACKAGE_DIR}/DEBIAN/control

# Copy DEBIAN scripts from templates
echo "Copying DEBIAN scripts from templates..."
cp debian/DEBIAN/postinst ${PACKAGE_DIR}/DEBIAN/postinst
cp debian/DEBIAN/prerm ${PACKAGE_DIR}/DEBIAN/prerm
cp debian/DEBIAN/postrm ${PACKAGE_DIR}/DEBIAN/postrm

# Ensure scripts are executable
chmod 755 ${PACKAGE_DIR}/DEBIAN/postinst
chmod 755 ${PACKAGE_DIR}/DEBIAN/prerm
chmod 755 ${PACKAGE_DIR}/DEBIAN/postrm

# Build DEB package
echo "Building DEB package..."
dpkg-deb --build ${PACKAGE_DIR}

# Move to build directory with proper naming
PACKAGE_NAME="blazor-net-app_${VERSION}_${ARCH}.deb"
mv ${PACKAGE_DIR}.deb build/${PACKAGE_NAME}

echo "✓ DEB package created: build/${PACKAGE_NAME}"

# Cleanup
rm -rf ${PACKAGE_DIR}
rm -rf publish/${ARCH}

echo "✓ Build complete!"
