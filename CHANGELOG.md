# Changelog

## 1.2.11 - 2026-06-24

- Kept DTR and RTS available both before and after opening a port.
- Applied the selected DTR/RTS states when a port opens and switch the lines immediately while it is open.
- Kept disabled checkbox captions readable on the camouflage background.

## 1.2.10 - 2026-06-23

- Added a compact ECHO color picker to the Settings dialog.
- Saved the selected ECHO color in the AppData `.ini` file as `EchoColor=#RRGGBB`.
- Kept existing terminal text colors intact when applying Settings buffer changes.

## 1.2.9 - 2026-06-23

- Added an ECHO checkbox above CLS to show sent commands in the terminal output.
- Displayed echoed commands in cyan while keeping received terminal text green.
- Saved the ECHO setting in the AppData `.ini` file.

## 1.2.8 - 2026-06-23

- Made Up/Down send history recall wrap around in a loop.
- Changed CLS to clear the send input after a successful send instead of clearing the terminal output.
- Added left-button double-click clearing for the terminal output.

## 1.2.7 - 2026-06-23

- Kept the send input enabled while the port is closed so command history can be recalled after startup.
- Captured Up/Down history navigation through command-key handling for reliable recall in the send input.

## 1.2.6 - 2026-06-23

- Autosaved the send input as soon as it changes, not only on send or close.
- Improved Up history recall when the send input already contains the last command.

## 1.2.5 - 2026-06-23

- Added a saved 32-command send history with Up/Down recall in the send input.
- Persisted the current send input, F1-F10 macros, and send history in the AppData `.ini` file.

## 1.2.4 - 2026-06-23

- Fixed settings saving when F1-F10 macro buttons are still empty.
- Added an A/B test publish script for alternating `CPortTerminal-A.exe` and `CPortTerminal-B.exe` builds.

## 1.2.3 - 2026-06-23

- Moved settings and log files to the user's AppData folder so settings persist when the app runs from a protected install folder.
- Kept compatibility with the old `.ini` file beside the executable and migrate it on the next save.
- Saved the send input text as Base64 to preserve any embedded CR/LF exactly.

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
