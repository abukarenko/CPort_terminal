# Changelog

## 1.2.2 - 2026-06-21

- Removed confirmation from macro assignment.
- Added the CLS option to clear the terminal before sending, with a hover hint.
- Fixed CR/LF persistence and exact macro playback.

## 1.2.1 - 2026-06-21

- Added a dedicated `Assign` context menu for F1-F10 macro buttons.
- Saved CR/LF as part of a macro command and prevented a second CR/LF from being appended when it is sent.
- Removed the terminal context menu from non-terminal controls.

## 1.2.0 - 2026-06-21

- Saved and restored the main window position and size; an off-screen window is centered safely after display changes.
- Applied the camouflage skin to the Settings dialog.
- Added ten saved-send macros, F1 through F10, with confirmation on right-click and immediate send on left-click.
- Added hover hints showing the command assigned to each macro button.

## 1.1.1 - 2026-06-21

- Added a khaki camouflage skin to the control panels.
- Styled buttons, checkboxes, input, and status bar in a high-contrast field palette.
- Kept the terminal display black and green for readability.

## 1.1.0 - 2026-06-21

- Added an explicit **RTS** option beside **DTR**. It is disabled by default.
- Removed the unconditional `SetRts` call that previously asserted RTS whenever a port opened.
- Persisted the RTS choice in the application `.ini` file.
- Kept RTS and DTR settings read-only while a port is open, so the selected line state always matches the active connection.
- Added a terminal-style application icon for the window, taskbar, tray, and executable.

### Important

Closing a Windows COM-port handle releases its control lines through the serial driver. The terminal no longer asserts RTS implicitly, but it cannot guarantee the physical RTS level after the handle has been closed. Devices that require a fixed level while disconnected need an appropriate hardware pull resistor or a port that remains open.
