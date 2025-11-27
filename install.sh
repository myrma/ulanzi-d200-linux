#!/bin/bash
# Ulanzi D200 Manager Installation Script

set -e

echo "╔════════════════════════════════════════════════════════════╗"
echo "║     Ulanzi D200 Manager - Installation Script             ║"
echo "╚════════════════════════════════════════════════════════════╝"
echo

# Check if running from correct directory
if [ ! -f "setup.py" ]; then
    echo "✗ Error: setup.py not found. Run this script from the project root."
    exit 1
fi

# Step 1: Create virtual environment
echo "1. Creating virtual environment..."
if [ ! -d "venv" ]; then
    python3 -m venv venv
    echo "   ✓ Virtual environment created"
else
    echo "   ✓ Virtual environment already exists"
fi

# Step 2: Activate and install
echo "2. Installing package..."
source venv/bin/activate
pip install -q -e .
echo "   ✓ Package installed"

# Step 3: Install udev rule
echo "3. Installing udev rule..."
if [ -w "/etc/udev/rules.d/" ]; then
    sudo cp 99-ulanzi.rules /etc/udev/rules.d/
    sudo udevadm control --reload-rules
    sudo udevadm trigger
    echo "   ✓ Udev rule installed"
else
    echo "   ⚠ Udev rule requires sudo. Run:"
    echo "     sudo cp 99-ulanzi.rules /etc/udev/rules.d/"
    echo "     sudo udevadm control --reload-rules"
    echo "     sudo udevadm trigger"
fi

# Step 4: Create config directories
echo "4. Creating configuration directories..."
mkdir -p ~/.config/ulanzi
mkdir -p ~/.local/share/ulanzi
echo "   ✓ Directories created"

# Step 5: Generate example config
echo "5. Generating example configuration..."
if [ ! -f ~/.config/ulanzi/config.yaml ]; then
    ulanzi-manager generate-config ~/.config/ulanzi/config.yaml
    echo "   ✓ Configuration generated at ~/.config/ulanzi/config.yaml"
else
    echo "   ✓ Configuration already exists"
fi

echo
echo "╔════════════════════════════════════════════════════════════╗"
echo "║              ✓ Installation Complete!                     ║"
echo "╚════════════════════════════════════════════════════════════╝"
echo
echo "Next steps:"
echo "1. Reconnect your Ulanzi D200 device (if not already connected)"
echo "2. Edit configuration: nano ~/.config/ulanzi/config.yaml"
echo "3. Validate: ulanzi-manager validate ~/.config/ulanzi/config.yaml"
echo "4. Configure device: ulanzi-manager configure ~/.config/ulanzi/config.yaml"
echo "5. Start daemon: ulanzi-daemon ~/.config/ulanzi/config.yaml"
echo
echo "For more info, see README.md or QUICKSTART.md"
echo
