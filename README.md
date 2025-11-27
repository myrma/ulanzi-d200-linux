# Ulanzi D200 Manager

A Linux application for managing the Ulanzi D200 StreamDeck device. Configure button images, labels, and actions to control OBS Studio, launch applications, execute commands, and more.

## Features

- 🎨 **Custom Button Images** - Set 196×196 PNG images for each button
- 🏷️ **Button Labels** - Add text labels to buttons with customizable styling
- 🎬 **OBS Integration** - Control OBS Studio scenes, sources, recording, and streaming
- 🚀 **App Launcher** - Launch applications with a button press
- ⌨️ **Keyboard Shortcuts** - Simulate keyboard input
- 💻 **Shell Commands** - Execute arbitrary shell commands
- 🔄 **Hot-Reload** - Update configuration without restarting
- 🌙 **Background Daemon** - Run as a systemd service

## Installation

### Prerequisites

- Python 3.8+
- Linux with USB support
- `xdotool` (for keyboard shortcuts): `sudo apt install xdotool`

### Setup

1. Clone the repository:
```bash
cd /home/lucas/Works/VibeCodedProjects/ulanzi
```

2. Install dependencies:
```bash
pip install -r requirements.txt
```

3. Install the package:
```bash
pip install -e .
```

4. Install udev rule for device access:
```bash
sudo cp 99-ulanzi.rules /etc/udev/rules.d/
sudo udevadm control --reload-rules
sudo udevadm trigger
```

Reconnect your device after this step.

5. Create configuration directory:
```bash
mkdir -p ~/.config/ulanzi
mkdir -p ~/.local/share/ulanzi
```

## Quick Start

### 0. Install Udev Rule (Required!)

```bash
sudo cp 99-ulanzi.rules /etc/udev/rules.d/
sudo udevadm control --reload-rules
sudo udevadm trigger
```

Then reconnect your device.

### 1. Generate Example Configuration

```bash
ulanzi-manager generate-config ~/.config/ulanzi/config.yaml
```

### 2. Edit Configuration

Edit `~/.config/ulanzi/config.yaml` to define your buttons and actions.

### 3. Test Configuration

```bash
ulanzi-manager validate ~/.config/ulanzi/config.yaml
```

### 4. Configure Device

```bash
ulanzi-manager configure ~/.config/ulanzi/config.yaml
```

### 5. Start Daemon

```bash
ulanzi-daemon ~/.config/ulanzi/config.yaml
```

## Configuration

### Basic Structure

```yaml
# Global settings
brightness: 100

# Label styling
label_style:
  Align: bottom
  Color: 0xFFFFFF
  FontName: Roboto
  ShowTitle: true
  Size: 10
  Weight: 80

# OBS Studio settings (optional)
obs:
  host: localhost
  port: 4444
  password: null

# Button definitions (13 buttons total, 0-12)
buttons:
  - image: ./icons/button1.png
    label: Button 1
    action: command
    params:
      cmd: "echo 'Button pressed'"

  - null  # Empty button
```

### Button Layout

```
0  1  2  3  4
5  6  7  8  9
10 11 12
```

### Action Types

#### Command Action
Execute shell commands:
```yaml
action: command
params:
  cmd: "firefox"
```

#### App Action
Launch applications:
```yaml
action: app
params:
  name: "firefox"
```

#### Key Action
Simulate keyboard input (requires `xdotool`):
```yaml
action: key
params:
  keys: "ctrl+alt+t"
```

#### OBS Action
Control OBS Studio via WebSocket:

**Toggle Scene:**
```yaml
action: obs
params:
  action: toggle_scene
  scene1: "Scene 1"
  scene2: "Scene 2"
```

**Set Scene:**
```yaml
action: obs
params:
  action: set_scene
  scene: "Scene 1"
```

**Toggle Source Visibility:**
```yaml
action: obs
params:
  action: toggle_source
  scene: "Scene 1"
  source: "Camera"
```

**Toggle Recording:**
```yaml
action: obs
params:
  action: toggle_recording
```

**Toggle Streaming:**
```yaml
action: obs
params:
  action: toggle_streaming
```

## CLI Commands

### Status
Check device connection:
```bash
ulanzi-manager status
```

### Brightness
Set display brightness (0-100):
```bash
ulanzi-manager brightness 80
```

### Configure
Apply configuration from file:
```bash
ulanzi-manager configure ~/.config/ulanzi/config.yaml
```

### Test Image
Test an image on a specific button:
```bash
ulanzi-manager test-image 0 ~/icon.png --label "Test"
```

### Validate
Validate configuration file:
```bash
ulanzi-manager validate ~/.config/ulanzi/config.yaml
```

### Generate Config
Generate example configuration:
```bash
ulanzi-manager generate-config config.yaml
```

### Daemon
Start background daemon:
```bash
ulanzi-daemon ~/.config/ulanzi/config.yaml
```

## Systemd Service

### Install Service

```bash
mkdir -p ~/.config/systemd/user
cp systemd/ulanzi-daemon.service ~/.config/systemd/user/
systemctl --user daemon-reload
```

### Enable and Start

```bash
systemctl --user enable ulanzi-daemon
systemctl --user start ulanzi-daemon
```

### Check Status

```bash
systemctl --user status ulanzi-daemon
```

### View Logs

```bash
journalctl --user -u ulanzi-daemon -f
```

## Image Preparation

Button images should be:
- **Format:** PNG
- **Size:** 196×196 pixels
- **Color Space:** RGB or RGBA

### Create Icons

Using ImageMagick:
```bash
convert -size 196x196 xc:blue -pointsize 40 -fill white -gravity center \
  -annotate +0+0 "OBS" icon.png
```

Using Python PIL:
```python
from PIL import Image, ImageDraw, ImageFont

img = Image.new('RGB', (196, 196), color='blue')
draw = ImageDraw.Draw(img)
draw.text((98, 98), "OBS", fill='white', anchor='mm')
img.save('icon.png')
```

## Troubleshooting

### Device Not Found / Open Failed
```
RuntimeError: Ulanzi D200 device not found
ERROR: open failed
```

**Solution:** Install the udev rule:
```bash
sudo cp 99-ulanzi.rules /etc/udev/rules.d/
sudo udevadm control --reload-rules
sudo udevadm trigger
```

Then reconnect your device. Verify with:
```bash
lsusb | grep 2207
```

### OBS Connection Failed
```
Failed to connect to OBS
```

**Solution:** Ensure OBS WebSocket server is enabled:
1. Open OBS
2. Tools → WebSocket Server Settings
3. Enable WebSocket Server
4. Note the port (default: 4444)

### Keyboard Shortcuts Not Working
```
xdotool not found
```

**Solution:** Install xdotool:
```bash
sudo apt install xdotool
```

### Permission Denied on USB Device
```
PermissionError: [Errno 13] Permission denied
```

**Solution:** Make sure the udev rule is installed:
```bash
sudo cp 99-ulanzi.rules /etc/udev/rules.d/
sudo udevadm control --reload-rules
sudo udevadm trigger
```

Then reconnect the device.

## Example Configuration

See `config.yaml` for a complete example with OBS integration.

## Development

### Project Structure

```
ulanzi/
├── ulanzi_manager/
│   ├── __init__.py
│   ├── cli.py              # CLI interface
│   ├── daemon.py           # Background daemon
│   ├── device.py           # USB device communication
│   ├── config.py           # Configuration parser
│   └── actions.py          # Action handlers
├── systemd/
│   └── ulanzi-daemon.service
├── setup.py
├── requirements.txt
└── README.md
```

### Logging

Daemon logs are written to:
```
~/.local/share/ulanzi/daemon.log
```

View real-time logs:
```bash
tail -f ~/.local/share/ulanzi/daemon.log
```

## License

MIT

## References

- [Ulanzi D200 Protocol](https://github.com/redphx/strmdck)
- [OBS WebSocket Protocol](https://github.com/obsproject/obs-websocket)


## Disclaimer

Yes, I vibecoded that and manually fixed some wrong stuff.