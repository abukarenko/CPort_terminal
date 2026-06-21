# CPort Terminal

Windows Forms COM port terminal for testing serial devices.

## Features

- Open and close COM ports with selectable baud rate.
- Terminal-style application icon in the window title, taskbar, tray, and executable.
- Khaki camouflage skin on the control panels, while the terminal remains high-contrast.
- Saved window size and position with safe centering when a display layout changes.
- Ten F1-F10 saved-send buttons: right-click to save the current command, left-click to send it, and hover to preview it.
- Optional DTR and RTS control on port open. RTS is off by default, so the terminal does not assert it itself; the serial driver may still release the line when the port is closed.
- HOLD mode to freeze terminal output while buffering incoming data.
- HEX display mode with 32 bytes per line and text view.
- Right-click menu with Open/Close, Settings, Copy, Clear, and Always on top.
- Configurable terminal buffer size.
- Status bar for port and activity messages.
