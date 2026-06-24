# CPort Terminal

Windows Forms COM port terminal for testing serial devices.

## Features

- Open and close COM ports with selectable baud rate.
- Terminal-style application icon in the window title, taskbar, tray, and executable.
- Khaki camouflage skin on the control panels, while the terminal remains high-contrast.
- Saved window size and position with safe centering when a display layout changes.
- Settings are stored in the user's AppData folder, so they persist from protected install folders.
- Ten F1-F10 saved-send buttons: use right-click > Assign to save the current command (including CR/LF), left-click to send it, and hover to preview it.
- The send input and the last 32 sent commands are saved in the `.ini`; use Up/Down in the send input to recall history in a loop.
- The CLS checkbox clears the send input after a successful send; double-click the terminal output to clear it.
- The ECHO checkbox shows sent commands in the terminal output; its color can be changed in Settings.
- Optional DTR and RTS control on port open. RTS is off by default, so the terminal does not assert it itself; the serial driver may still release the line when the port is closed.
- HOLD mode to freeze terminal output while buffering incoming data.
- HEX display mode with 32 bytes per line and text view.
- Right-click menu with Open/Close, Settings, Copy, Clear, and Always on top.
- Configurable terminal buffer size.
- Status bar for port and activity messages.

## Test builds

Use `tools\Publish-TestSlot.ps1` to publish alternating test builds:

- `CPortTerminal-A.exe`
- `CPortTerminal-B.exe`

The script writes to the slot that is not currently running. If neither slot is running, it alternates based on the newest existing build.
