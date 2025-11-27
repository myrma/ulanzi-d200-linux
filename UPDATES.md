# Latest Updates - Debug Mode & Image Logging

## Summary

Added comprehensive debugging features to help identify buttons and troubleshoot image display issues.

## What's New

### 1. Debug Mode Command ✓

**Command**: `ulanzi-manager debug`

Shows button presses in real-time with visual layout reference.

**Usage**:
```bash
source venv/bin/activate
ulanzi-manager debug
```

**Output**:
```
INFO:ulanzi_manager.cli:Debug mode: Press buttons to see their index
INFO:ulanzi_manager.cli:Button layout:
INFO:ulanzi_manager.cli:  0  1  2  3  4
INFO:ulanzi_manager.cli:  5  6  7  8  9
INFO:ulanzi_manager.cli: 10 11 12
INFO:ulanzi_manager.cli:
INFO:ulanzi_manager.cli:Waiting for button presses (Ctrl+C to exit)...
INFO:ulanzi_manager.cli:>>> BUTTON 0 PRESSED <<<
INFO:ulanzi_manager.cli:>>> BUTTON 1 PRESSED <<<
```

### 2. Enhanced Button Press Logging ✓

Button presses now logged with more detail:

```
INFO:ulanzi_manager.daemon:Button 0 pressed (state=0)
INFO:ulanzi_manager.daemon:Executing action: app - Firefox
```

Shows:
- Button index
- Button state
- Action type
- Button label

### 3. Image Processing Logging ✓

Detailed logging for image handling:

```
DEBUG:ulanzi_manager.device:Added image for button 0: ./icons/firefox.png
DEBUG:ulanzi_manager.device:Added image for button 1: ./icons/obs.png
INFO:ulanzi_manager.device:Set 10 button(s) with 10 image(s)
```

Shows:
- Which images are being added
- How many images total
- Warnings for missing images

### 4. Manifest Logging ✓

Device manifest is logged for debugging:

```
DEBUG:ulanzi_manager.device:Manifest: {
  "0_0": {
    "State": 0,
    "ViewParam": [
      {
        "Text": "Firefox",
        "Icon": "icons/icon_0.png"
      }
    ]
  },
  ...
}
```

## New Documentation

### DEBUG.md
Comprehensive debug guide covering:
- How to use debug mode
- Troubleshooting image display
- Common issues and solutions
- Testing workflow
- Custom image creation

### FIXES.md
Summary of recent fixes:
- Debug mode added
- Button 13 error clarified
- Image display debugging
- Enhanced logging

### QUICK_REFERENCE.md
Quick reference card with:
- Common commands
- Configuration templates
- Action types
- Troubleshooting table
- Setup checklist

### UPDATES.md
This file - summary of latest changes

## Files Modified

### ulanzi_manager/cli.py
- Added `cmd_debug()` method
- Added debug command to argument parser
- Updated help text with debug example

### ulanzi_manager/daemon.py
- Enhanced button press logging
- Shows button state and action details
- Better error messages

### ulanzi_manager/device.py
- Enhanced image processing logging
- Shows which images are added
- Logs manifest being sent
- Warns about missing images

## How to Use

### Identify Buttons

```bash
source venv/bin/activate
ulanzi-manager debug
```

Press each button and note the index shown.

### Check Image Status

```bash
source venv/bin/activate
ulanzi-manager configure config.yaml
```

Look for output like:
```
INFO:ulanzi_manager.device:Set 10 button(s) with 10 image(s)
```

If it says `0 image(s)`, images aren't being found.

### View Detailed Logs

```bash
tail -f ~/.local/share/ulanzi/daemon.log
```

Look for:
- `Button X pressed` - Button press events
- `Added image for button X` - Image additions
- `Executing action` - Action execution
- `ERROR` - Any errors

## Troubleshooting Images

### Step 1: Validate Configuration
```bash
ulanzi-manager validate config.yaml
```

Check for missing image files.

### Step 2: Test Single Image
```bash
ulanzi-manager test-image 0 ./icons/firefox.png
```

Check logs for:
```
DEBUG:ulanzi_manager.device:Added image for button 0: ./icons/firefox.png
```

### Step 3: Configure Device
```bash
ulanzi-manager configure config.yaml
```

Check output for:
```
INFO:ulanzi_manager.device:Set X button(s) with Y image(s)
```

If Y is 0, images aren't being found.

### Step 4: Check Image Format
```bash
file icons/firefox.png
```

Must be PNG, 196×196 pixels.

### Step 5: View Logs
```bash
tail -f ~/.local/share/ulanzi/daemon.log
```

Look for warnings about missing images.

## Common Issues Fixed

### Issue: "No config for button 13"
- **Cause**: Device only has 13 buttons (0-12)
- **Solution**: Use debug mode to identify which button you pressed
- **Status**: ✓ Fixed with better logging

### Issue: Images not showing
- **Cause**: Missing images, wrong format, or wrong paths
- **Solution**: Check logs, validate config, verify image format
- **Status**: ✓ Enhanced logging to help diagnose

### Issue: Don't know which button is which
- **Cause**: No visual feedback on button presses
- **Solution**: Use `ulanzi-manager debug` command
- **Status**: ✓ Debug mode added

## Testing

All changes verified:
- ✓ Debug command works
- ✓ Button press logging works
- ✓ Image logging works
- ✓ All imports successful
- ✓ Configuration parsing works
- ✓ Action executor works

## Next Steps

1. **Identify your buttons**:
   ```bash
   ulanzi-manager debug
   ```

2. **Prepare images** (196×196 PNG):
   - Create or find images
   - Place in `icons/` directory

3. **Update configuration**:
   ```bash
   nano ~/.config/ulanzi/config.yaml
   ```

4. **Validate**:
   ```bash
   ulanzi-manager validate ~/.config/ulanzi/config.yaml
   ```

5. **Configure device**:
   ```bash
   ulanzi-manager configure ~/.config/ulanzi/config.yaml
   ```

6. **Check logs**:
   ```bash
   tail -f ~/.local/share/ulanzi/daemon.log
   ```

7. **Start daemon**:
   ```bash
   ulanzi-daemon ~/.config/ulanzi/config.yaml
   ```

## Documentation Map

| Document | Purpose |
|----------|---------|
| README.md | Full feature documentation |
| QUICKSTART.md | 5-minute quick start |
| SETUP.md | Setup and udev rule guide |
| DEBUG.md | Debug guide and troubleshooting |
| FIXES.md | Recent fixes summary |
| QUICK_REFERENCE.md | Quick reference card |
| UPDATES.md | This file - latest changes |
| PROJECT_SUMMARY.md | Technical architecture |
| COMPLETION_REPORT.md | Project status |

## Command Reference

| Command | Purpose |
|---------|---------|
| `ulanzi-manager status` | Check device connection |
| `ulanzi-manager brightness 80` | Set brightness |
| `ulanzi-manager configure config.yaml` | Apply configuration |
| `ulanzi-manager test-image 0 icon.png` | Test single button |
| `ulanzi-manager validate config.yaml` | Validate configuration |
| `ulanzi-manager generate-config config.yaml` | Generate example |
| `ulanzi-manager debug` | **NEW** - Show button presses |
| `ulanzi-daemon config.yaml` | Start background daemon |

## Logging Levels

Check logs with different detail levels:

```bash
# All logs
tail -f ~/.local/share/ulanzi/daemon.log

# Only errors
grep ERROR ~/.local/share/ulanzi/daemon.log

# Only button presses
grep "Button.*pressed" ~/.local/share/ulanzi/daemon.log

# Only image additions
grep "Added image" ~/.local/share/ulanzi/daemon.log

# Only actions
grep "Executing action" ~/.local/share/ulanzi/daemon.log
```

## Support

For help:
1. Read DEBUG.md for troubleshooting
2. Check QUICK_REFERENCE.md for common tasks
3. View logs: `tail -f ~/.local/share/ulanzi/daemon.log`
4. Use debug mode: `ulanzi-manager debug`

---

**Status**: ✓ All updates applied and tested
**Date**: 2025-11-27
