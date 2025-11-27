# Ulanzi D200 Manager - Complete Index

## Quick Start (5 minutes)

1. **Identify buttons**:
   ```bash
   source venv/bin/activate
   ulanzi-manager debug
   ```

2. **Update config**:
   ```bash
   nano config.yaml
   ```

3. **Configure device**:
   ```bash
   ulanzi-manager configure config.yaml
   ```

4. **Start daemon**:
   ```bash
   ulanzi-daemon config.yaml
   ```

## Documentation

### Getting Started
- **[QUICKSTART.md](QUICKSTART.md)** - 5-minute setup guide
- **[SETUP.md](SETUP.md)** - Detailed setup and udev rule installation
- **[INSTALL.md](INSTALL.md)** - Installation instructions

### Usage & Reference
- **[README.md](README.md)** - Complete feature documentation
- **[QUICK_REFERENCE.md](QUICK_REFERENCE.md)** - Quick reference card with commands and templates
- **[DEBUG.md](DEBUG.md)** - Debug guide and troubleshooting

### Project Information
- **[PROJECT_SUMMARY.md](PROJECT_SUMMARY.md)** - Technical architecture
- **[COMPLETION_REPORT.md](COMPLETION_REPORT.md)** - Project status and features
- **[FIXES.md](FIXES.md)** - Recent fixes and improvements
- **[UPDATES.md](UPDATES.md)** - Latest changes summary

## Core Features

### Device Control
- ✓ USB HID communication with Ulanzi D200
- ✓ Button image and label configuration
- ✓ Brightness control
- ✓ Real-time button press detection

### Button Actions
- ✓ **Command** - Execute shell commands
- ✓ **App** - Launch applications
- ✓ **Key** - Send keyboard shortcuts
- ✓ **OBS** - Control OBS Studio (scene switching, recording, streaming)

### Daemon Service
- ✓ Background service for button monitoring
- ✓ Systemd integration
- ✓ Comprehensive logging
- ✓ Auto-reconnection on device disconnect

### Debug Features
- ✓ **Debug mode** - Show button presses in real-time
- ✓ **Enhanced logging** - Detailed button and image logging
- ✓ **Configuration validation** - Check config before applying
- ✓ **Test mode** - Test single button images

## Commands

### Device Management
```bash
ulanzi-manager status              # Check device connection
ulanzi-manager brightness 80       # Set brightness (0-100)
ulanzi-manager configure config.yaml  # Apply configuration
```

### Configuration
```bash
ulanzi-manager validate config.yaml    # Validate configuration
ulanzi-manager generate-config file.yaml  # Generate example config
```

### Testing & Debugging
```bash
ulanzi-manager test-image 0 icon.png   # Test image on button 0
ulanzi-manager debug                   # Show button presses (NEW)
```

### Daemon
```bash
ulanzi-daemon config.yaml          # Start background daemon
```

## Configuration

### Basic Template
```yaml
brightness: 100

buttons:
  - image: ./icons/button0.png
    label: Button 0
    action: command
    params:
      cmd: "echo 'Hello'"
  
  - image: ./icons/button1.png
    label: Button 1
    action: app
    params:
      name: firefox
  
  - null  # Empty button
```

### Action Types

**Command**:
```yaml
action: command
params:
  cmd: "notify-send 'Hello!'"
```

**App**:
```yaml
action: app
params:
  name: firefox
```

**Keyboard**:
```yaml
action: key
params:
  keys: "ctrl+alt+t"
```

**OBS**:
```yaml
action: obs
params:
  action: toggle_scene
  scene1: "Gaming"
  scene2: "Desktop"
```

## Button Layout

```
Top Row:     0  1  2  3  4
Middle Row:  5  6  7  8  9
Bottom Row: 10 11 12
Clock:       13 (Big button with clock display)
```

Use `ulanzi-manager debug` to identify which physical button corresponds to each index.

## Image Requirements

- **Format**: PNG
- **Size**: 196×196 pixels
- **Color Space**: RGB or RGBA

Generate with Python:
```python
from PIL import Image, ImageDraw, ImageFont

img = Image.new('RGB', (196, 196), color='blue')
draw = ImageDraw.Draw(img)
font = ImageFont.truetype("/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf", 32)
draw.text((98, 98), "Text", fill='white', font=font, anchor='mm')
img.save('button.png')
```

## Troubleshooting

### Device Not Found
```bash
sudo cp 99-ulanzi.rules /etc/udev/rules.d/
sudo udevadm control --reload-rules
sudo udevadm trigger
# Reconnect device
```

### Images Not Showing
1. Check image paths: `ulanzi-manager validate config.yaml`
2. Verify format: `file icons/button.png`
3. Test single image: `ulanzi-manager test-image 0 icons/button.png`
4. Check logs: `tail -f ~/.local/share/ulanzi/daemon.log`

### Button 13 Error
- Device only has 13 buttons (0-12)
- Use `ulanzi-manager debug` to identify buttons

### OBS Not Connecting
- Enable WebSocket Server in OBS (Tools → WebSocket Server Settings)
- Check OBS is running
- Verify WebSocket URL in logs

## File Structure

```
ulanzi/
├── ulanzi_manager/           # Main package
│   ├── __init__.py
│   ├── device.py             # USB HID protocol
│   ├── config.py             # Configuration parser
│   ├── actions.py            # Action handlers
│   ├── daemon.py             # Background service
│   └── cli.py                # Command-line interface
├── icons/                    # Button images (196×196 PNG)
├── systemd/
│   └── ulanzi-daemon.service # Systemd service file
├── config.yaml               # Example configuration
├── setup.py                  # Package setup
├── requirements.txt          # Dependencies
├── 99-ulanzi.rules          # Udev rule for USB access
├── install.sh               # Installation script
├── create_icons.py          # Icon generator
├── verify.py                # Verification script
└── [Documentation files]
```

## Logs

```bash
# Real-time logs
tail -f ~/.local/share/ulanzi/daemon.log

# Last 50 lines
tail -50 ~/.local/share/ulanzi/daemon.log

# Search for errors
grep ERROR ~/.local/share/ulanzi/daemon.log

# Search for button presses
grep "Button.*pressed" ~/.local/share/ulanzi/daemon.log

# Search for image additions
grep "Added image" ~/.local/share/ulanzi/daemon.log
```

## Setup Checklist

- [ ] Install udev rule
- [ ] Reload udev and reconnect device
- [ ] Run `ulanzi-manager debug` to identify buttons
- [ ] Create/prepare 196×196 PNG images
- [ ] Update config.yaml
- [ ] Validate: `ulanzi-manager validate config.yaml`
- [ ] Configure device: `ulanzi-manager configure config.yaml`
- [ ] Check logs: `tail -f ~/.local/share/ulanzi/daemon.log`
- [ ] Start daemon: `ulanzi-daemon config.yaml`

## Dependencies

- Python 3.8+
- pyusb - USB communication
- hidapi - HID device access
- pyyaml - Configuration parsing
- obs-websocket-py - OBS control
- pillow - Image processing
- python-daemon - Daemon service
- xdotool - Keyboard control (optional, for key action)

## Support

### For Setup Issues
→ See [SETUP.md](SETUP.md)

### For Debugging
→ See [DEBUG.md](DEBUG.md)

### For Quick Reference
→ See [QUICK_REFERENCE.md](QUICK_REFERENCE.md)

### For Full Documentation
→ See [README.md](README.md)

### For Recent Changes
→ See [UPDATES.md](UPDATES.md)

## Key Commands

| Task | Command |
|------|---------|
| Identify buttons | `ulanzi-manager debug` |
| Check device | `ulanzi-manager status` |
| Set brightness | `ulanzi-manager brightness 80` |
| Validate config | `ulanzi-manager validate config.yaml` |
| Configure device | `ulanzi-manager configure config.yaml` |
| Test image | `ulanzi-manager test-image 0 icon.png` |
| View logs | `tail -f ~/.local/share/ulanzi/daemon.log` |
| Start daemon | `ulanzi-daemon config.yaml` |

## Next Steps

1. **Read**: [QUICKSTART.md](QUICKSTART.md) for 5-minute setup
2. **Debug**: `ulanzi-manager debug` to identify buttons
3. **Configure**: Update config.yaml with your buttons
4. **Test**: `ulanzi-manager configure config.yaml`
5. **Run**: `ulanzi-daemon config.yaml`

---

**Status**: ✓ Complete and tested
**Last Updated**: 2025-11-27
**Version**: 1.0

For more information, see the documentation files listed above.
