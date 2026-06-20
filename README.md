# CPort Terminal

Windows Forms COM port terminal for testing serial devices.

## Features

- Open and close COM ports with selectable baud rate.
- Optional DTR and RTS control on port open. RTS is off by default, so the terminal does not assert it itself; the serial driver may still release the line when the port is closed.
- HOLD mode to freeze terminal output while buffering incoming data.
- HEX display mode with 32 bytes per line and text view.
- Right-click menu with Open/Close, Settings, Copy, Clear, and Always on top.
- Configurable terminal buffer size.
- Status bar for port and activity messages.
