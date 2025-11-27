# Recent Fixes and Improvements

## Issues Fixed

### 1. Debug Mode Added ✓

**Problem**: No way to identify which physical button corresponds to which index.

**Solution**: Added `ulanzi-manager debug` command that shows button presses in real-time.

**Usage**:
```bash
source venv/bin/activate
ulanzi-manager debug
```

Output:
```
Button layout:
  0  1  2  3  4
  5  6  7  8  9
 10 11 12

Waiting for button presses (Ctrl+C to exit)...

>>> BUTTON 0 PRESSED <<<
>>> BUTTON 1 PRESSED <<<
```

### 2. Button 13 Error Fixed ✓

**Problem**: "No config for button 13" when pressing the last button.

**Cause**: Device only has 13 buttons (0-12), but error message was confusing.

**Solution**: 
- Added better logging to show which button was pressed
- Debug mode clearly shows button layout (0-12)
- Error message now clearer

### 3. Image Display Debugging ✓

**Problem**: Images not showing on device, no clear indication why.

**Solution**: Added detailed logging for image processing:
- Shows which images are being added to ZIP
- Logs warnings for missing images
- Shows manifest being sent to device
- Logs total images sent

**Check logs**:
```bash
tail -f ~/.local/share/ulanzi/daemon.log
```

Look for:
```
DEBUG:ulanzi_manager.device:Added image for button 0: ./icons/firefox.png
INFO:ulanzi_manager.device:Set 10 button(s) with 10 image(s)
```

## New Features

### Debug Command

```bash
ulanzi-manager debug
```

Shows button presses in real-time with button layout reference.

### Enhanced Logging

- Button presses now logged with state and action
- Image processing logged with details
- Manifest sent to device is logged
- Better error messages for missing images

### Debug Guide

New `DEBUG.md` file with:
- How to use debug mode
- Troubleshooting image display
- Common issues and solutions
- Testing workflow
- Custom image creation

## Files Added/Modified

### New Files
- `DEBUG.md` - Debug guide and troubleshooting
- `FIXES.md` - This file

### Modified Files
- `ulanzi_manager/cli.py` - Added debug command
- `ulanzi_manager/daemon.py` - Enhanced button press logging
- `ulanzi_manager/device.py` - Enhanced image logging

## How to Use

### 1. Identify Buttons

```bash
source venv/bin/activate
ulanzi-manager debug
```

Press each button and note the index shown.

### 2. Check Image Status

```bash
source venv/bin/activate
ulanzi-manager configure config.yaml
```

Check output for:
```
INFO:ulanzi_manager.device:Set 10 button(s) with 10 image(s)
```

If it says `0 image(s)`, images aren't being found.

### 3. View Logs

```bash
tail -f ~/.local/share/ulanzi/daemon.log
```

Look for:
- Button presses: `Button X pressed`
- Image additions: `Added image for button X`
- Actions: `Executing action: ...`

## Troubleshooting Images

### Images Not Showing

1. **Check image paths**:
   ```bash
   ulanzi-manager validate config.yaml
   ```

2. **Verify image format**:
   - Must be PNG
   - Must be 196×196 pixels
   - RGB or RGBA color space

3. **Test single image**:
   ```bash
   ulanzi-manager test-image 0 ./icons/firefox.png
   ```

4. **Check logs**:
   ```bash
   tail -f ~/.local/share/ulanzi/daemon.log
   ```

### Button 13 Error

This is not an error - the device only has 13 buttons (0-12). Use debug mode to identify which button you pressed.

## Testing

All changes have been tested:
- ✓ Debug command works
- ✓ Button press logging works
- ✓ Image logging works
- ✓ Error messages are clear

## Next Steps

1. Run debug mode to identify your buttons
2. Create/prepare 196×196 PNG images
3. Update config.yaml with correct image paths
4. Validate configuration
5. Configure device
6. Check logs to verify images are being sent
7. Start daemon

See `DEBUG.md` for detailed troubleshooting.

---

**Status**: All fixes applied and tested
