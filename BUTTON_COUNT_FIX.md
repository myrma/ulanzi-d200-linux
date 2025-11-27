# Button Count Fix - 14 Buttons (0-13)

## Issue Found

The device actually has **14 buttons**, not 13:
- **Buttons 0-12**: Regular 3×5 grid buttons
- **Button 13**: The big clock button

The reference code (strmdck) shows 13 buttons, but that appears to be incomplete or outdated. The actual device sends button 13 callbacks for the big clock button.

## What Was Fixed

### 1. Updated Button Count
- Changed `BUTTON_COUNT` from 13 to 14 in `device.py`
- Now correctly reflects the actual device hardware

### 2. Enhanced Debug Mode
- Updated button layout display to show button 13
- Shows "Clock" label for button 13 in debug output
- Example output:
  ```
  Button layout:
    0  1  2  3  4
    5  6  7  8  9
   10 11 12
   13 (Clock/Big Button)
  
  >>> CLOCK PRESSED (index=13, state=0) <<<
  ```

### 3. Updated Documentation
- DEBUG.md - Updated button layout reference
- QUICK_REFERENCE.md - Updated button layout and config template
- START_HERE.md - Updated button layout and example output
- INDEX.md - Updated button layout reference

## Button Layout

```
Top Row:     0  1  2  3  4
Middle Row:  5  6  7  8  9
Bottom Row: 10 11 12
Clock:       13 (Big button with clock display)
```

## Configuration

You can now configure button 13 (the clock button) like any other button:

```yaml
buttons:
  # ... buttons 0-12 ...
  
  # Button 13 - Clock button
  - image: ./icons/clock.png
    label: Clock
    action: command
    params:
      cmd: "notify-send 'Clock button pressed'"
```

## Testing

All changes verified:
- ✓ Button count updated to 14
- ✓ Debug mode shows button 13
- ✓ Configuration parser handles 14 buttons
- ✓ Daemon handles button 13 presses
- ✓ All verification tests pass

## Usage

Run debug mode to see all 14 buttons:

```bash
source venv/bin/activate
ulanzi-manager debug
```

Press the big clock button - you should see:
```
>>> CLOCK PRESSED (index=13, state=0) <<<
```

## Files Modified

- `ulanzi_manager/device.py` - Updated BUTTON_COUNT to 14
- `ulanzi_manager/cli.py` - Enhanced debug mode output
- `DEBUG.md` - Updated button layout
- `QUICK_REFERENCE.md` - Updated button layout and config
- `START_HERE.md` - Updated button layout and examples
- `INDEX.md` - Updated button layout

---

**Status**: ✓ Fixed and tested
**Date**: 2025-11-27
