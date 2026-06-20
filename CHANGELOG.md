# Changelog

## 1.1.0 - 2026-06-21

- Added an explicit **RTS** option beside **DTR**. It is disabled by default.
- Removed the unconditional `SetRts` call that previously asserted RTS whenever a port opened.
- Persisted the RTS choice in the application `.ini` file.
- Kept RTS and DTR settings read-only while a port is open, so the selected line state always matches the active connection.

### Important

Closing a Windows COM-port handle releases its control lines through the serial driver. The terminal no longer asserts RTS implicitly, but it cannot guarantee the physical RTS level after the handle has been closed. Devices that require a fixed level while disconnected need an appropriate hardware pull resistor or a port that remains open.
