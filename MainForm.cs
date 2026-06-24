using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;

namespace CPortTerminal
{
    public partial class MainForm : Form
    {
        private const int DefaultTerminalLines = 500;
        private const int MinTerminalLines = 16;
        private const int MaxTerminalLines = 5000;
        private const int HexBytesPerLine = 32;
        private const int MaxLogData = 512;
        private const int MacroButtonCount = 10;
        private const int SendHistoryLimit = 32;
        private const int WmDeviceChange = 0x0219;
        private const int DbtDeviceArrival = 0x8000;
        private const int DbtDeviceRemoveComplete = 0x8004;
        private const int DbtDevNodesChanged = 0x0007;
        private const string UserDataDirectoryName = "CPortTerminal";
        private const string SettingsFileBaseName = "CPortTerminal.ini";
        private const string LogFileBaseName = "CPortTerminal.log";
        private static readonly Color ConnectedTerminalBackColor = Color.FromArgb(0, 32, 0);
        private static readonly Color DisconnectedTerminalBackColor = Color.FromArgb(0, 0, 0);
        private static readonly Color DefaultEchoTerminalForeColor = Color.Cyan;
        private static readonly IntPtr InvalidHandleValue = new(-1);

        private IntPtr portHandle = InvalidHandleValue;
        private Thread? readerThread;
        private volatile bool readerStopping;
        private StreamWriter? logWriter;
        private Color echoTerminalForeColor = DefaultEchoTerminalForeColor;
        private bool applyingTopMost;
        private readonly StringBuilder terminalBuffer = new();
        private readonly object pendingTerminalLock = new();
        private readonly StringBuilder pendingTerminalText = new();
        private bool terminalFlushScheduled;
        private bool settingsDialogShowing;
        private uint pendingRxBytes;
        private int terminalLineLimit = DefaultTerminalLines;
        private volatile bool hexDisplayEnabled;
        private Image? camouflageSkin;
        private readonly string[] macroTexts = new string[MacroButtonCount];
        private readonly List<string> sendHistory = new();
        private readonly Button[] macroButtons = new Button[MacroButtonCount];
        private readonly ToolTip macroToolTip = new();
        private readonly ContextMenuStrip macroMenu = new();
        private readonly ToolStripMenuItem assignMacroItem = new("Assign");
        private readonly System.Windows.Forms.Timer portRefreshTimer = new();
        private FlowLayoutPanel? macroPanel;
        private SettingsForm? settingsForm;
        private int sendHistoryNavigationIndex = -1;
        private string sendHistoryDraft = string.Empty;

        public MainForm()
        {
            InitializeComponent();
            Array.Fill(macroTexts, string.Empty);
            InitializeMacroButtons();
            portRefreshTimer.Interval = 500;
            portRefreshTimer.Tick += PortRefreshTimer_Tick;

            System.Drawing.Icon? applicationIcon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            if (applicationIcon != null)
            {
                Icon = applicationIcon;
            }

            ApplyCamouflageSkin();
        }

        private void InitializeMacroButtons()
        {
            bottomPanel.Height = 78;
            sendTextBox.Top = 43;
            sendButton.Top = 42;
            echoCheckBox.Top = 5;
            clsCheckBox.Top = 29;
            crLfCheckBox.Top = 53;

            macroPanel = new FlowLayoutPanel
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                AutoScroll = true,
                FlowDirection = FlowDirection.LeftToRight,
                Location = new Point(12, 7),
                Size = new Size(bottomPanel.ClientSize.Width - 24, 28),
                WrapContents = false
            };
            bottomPanel.Controls.Add(macroPanel);
            macroPanel.SendToBack();
            macroToolTip.SetToolTip(echoCheckBox, "Show sent commands in terminal");
            macroToolTip.SetToolTip(clsCheckBox, "Clear send input after sending");
            assignMacroItem.Click += AssignMacroItem_Click;
            macroMenu.Items.Add(assignMacroItem);

            for (int index = 0; index < MacroButtonCount; index++)
            {
                Button button = new()
                {
                    Margin = new Padding(0, 0, 4, 0),
                    Size = new Size(44, 24),
                    Tag = index,
                    Text = $"F{index + 1}"
                };
                button.MouseClick += MacroButton_MouseClick;
                button.MouseDown += MacroButton_MouseDown;
                button.ContextMenuStrip = macroMenu;
                macroButtons[index] = button;
                macroPanel.Controls.Add(button);
                UpdateMacroHint(index);
            }
        }

        private void ApplyCamouflageSkin()
        {
            string skinPath = Path.Combine(AppContext.BaseDirectory, "Assets", "CamoSkin.png");
            if (!File.Exists(skinPath))
            {
                return;
            }

            camouflageSkin = Image.FromFile(skinPath);
            ApplySkin(topPanel);
            ApplySkin(bottomPanel);

            Color textColor = Color.FromArgb(240, 235, 202);
            Color buttonColor = Color.FromArgb(58, 70, 37);
            Color buttonBorderColor = Color.FromArgb(149, 142, 90);

            foreach (Button button in new[] { openButton, exitButton, sendButton }.Concat(macroButtons))
            {
                button.FlatStyle = FlatStyle.Flat;
                button.UseVisualStyleBackColor = false;
                button.BackColor = buttonColor;
                button.ForeColor = textColor;
                button.FlatAppearance.BorderColor = buttonBorderColor;
            }

            if (macroPanel != null)
            {
                ApplySkin(macroPanel);
            }

            foreach (CheckBox checkBox in new[] { dtrCheckBox, rtsCheckBox, holdCheckBox, hexCheckBox, echoCheckBox, clsCheckBox, crLfCheckBox })
            {
                checkBox.BackColor = Color.Transparent;
                checkBox.ForeColor = textColor;
            }

            sendTextBox.BackColor = Color.FromArgb(30, 36, 22);
            sendTextBox.ForeColor = textColor;
            statusStrip.BackColor = Color.FromArgb(36, 45, 25);
            statusStrip.ForeColor = textColor;
        }

        private void ApplySkin(Control control)
        {
            control.BackgroundImage = camouflageSkin;
            control.BackgroundImageLayout = ImageLayout.Tile;
        }

        private static string UserDataDirectory
        {
            get
            {
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                if (!string.IsNullOrWhiteSpace(appDataPath))
                {
                    return Path.Combine(appDataPath, UserDataDirectoryName);
                }

                string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                if (!string.IsNullOrWhiteSpace(localAppDataPath))
                {
                    return Path.Combine(localAppDataPath, UserDataDirectoryName);
                }

                return AppContext.BaseDirectory;
            }
        }

        private string SettingsFileName => Path.Combine(UserDataDirectory, SettingsFileBaseName);

        private string LegacySettingsFileName => Path.ChangeExtension(Application.ExecutablePath, ".ini");

        private string LogFileName => Path.Combine(UserDataDirectory, LogFileBaseName);

        private string ReadableSettingsFileName
        {
            get
            {
                if (File.Exists(SettingsFileName))
                {
                    return SettingsFileName;
                }

                return File.Exists(LegacySettingsFileName)
                    ? LegacySettingsFileName
                    : SettingsFileName;
            }
        }

        private bool IsConnected => portHandle != InvalidHandleValue;

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == WmDeviceChange
                && (m.WParam.ToInt32() == DbtDeviceArrival
                    || m.WParam.ToInt32() == DbtDeviceRemoveComplete
                    || m.WParam.ToInt32() == DbtDevNodesChanged))
            {
                SchedulePortRefresh();
            }
        }

        private void MainForm_Load(object? sender, EventArgs e)
        {
            trayIcon.Icon = Icon;
            InitLog();
            LogEvent("APPLICATION START");

            LoadAvailablePorts();
            baudComboBox.Items.AddRange(GetBaudRates().Cast<object>().ToArray());
            baudComboBox.Text = "19200";

            LoadSettings();
            sendTextBox.TextChanged += SendTextBox_TextChanged;
            sendTextBox.PreviewKeyDown += SendTextBox_PreviewKeyDown;
            terminalTextBox.MouseDoubleClick += TerminalTextBox_MouseDoubleClick;
            SetConnected(false);
            SetStatusMessage("Ready");
        }

        private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            portRefreshTimer.Stop();
            SaveSettings();
            trayIcon.Visible = false;
            CloseComPort();
            LogEvent("APPLICATION CLOSE");
            CloseLog();
        }

        private static string[] GetBaudRates()
        {
            return new[]
            {
                "1200", "2400", "4800", "9600", "19200",
                "38400", "57600", "115200", "250000", "500000"
            };
        }

        private void LoadAvailablePorts()
        {
            string oldPort = portComboBox.Text;
            List<string> ports = GetAvailablePorts();

            ports.Sort(CompareComPorts);

            portComboBox.Items.Clear();
            portComboBox.Items.AddRange(ports.Cast<object>().ToArray());

            if (!string.IsNullOrWhiteSpace(oldPort) && ports.Contains(oldPort))
            {
                portComboBox.Text = oldPort;
            }
            else if (ports.Count > 0)
            {
                portComboBox.SelectedIndex = 0;
            }
            else
            {
                portComboBox.SelectedIndex = -1;
            }

            SetConnected(IsConnected);
            settingsForm?.RefreshPorts(ports);
        }

        private List<string> GetAvailablePorts()
        {
            List<string> ports = new();

            try
            {
                using RegistryKey? key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DEVICEMAP\SERIALCOMM");
                if (key != null)
                {
                    foreach (string valueName in key.GetValueNames())
                    {
                        if (key.GetValue(valueName) is string port && IsComPortName(port) && !ports.Contains(port))
                        {
                            ports.Add(port);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogEvent("PORT LIST ERROR " + ex.Message);
            }

            ports.Sort(CompareComPorts);
            return ports;
        }

        private void SchedulePortRefresh()
        {
            if (IsConnected || IsDisposed)
            {
                return;
            }

            portRefreshTimer.Stop();
            portRefreshTimer.Start();
        }

        private void PortRefreshTimer_Tick(object? sender, EventArgs e)
        {
            portRefreshTimer.Stop();

            if (IsConnected || IsDisposed)
            {
                return;
            }

            string oldPort = portComboBox.Text;
            LoadAvailablePorts();
            string newPort = portComboBox.Text;

            if (string.Equals(oldPort, newPort, StringComparison.OrdinalIgnoreCase))
            {
                SetStatusMessage("Port list refreshed");
            }
            else if (string.IsNullOrWhiteSpace(newPort))
            {
                SetStatusMessage("No COM ports found");
            }
            else
            {
                SetStatusMessage($"Port list refreshed: {newPort} selected");
            }
        }

        private static bool IsComPortName(string value)
        {
            return value.StartsWith("COM", StringComparison.OrdinalIgnoreCase)
                && int.TryParse(value[3..], out _);
        }

        private static int CompareComPorts(string left, string right)
        {
            int leftNumber = GetComPortNumber(left);
            int rightNumber = GetComPortNumber(right);
            int numberCompare = leftNumber.CompareTo(rightNumber);
            return numberCompare != 0
                ? numberCompare
                : string.Compare(left, right, StringComparison.OrdinalIgnoreCase);
        }

        private static int GetComPortNumber(string portName)
        {
            return portName.StartsWith("COM", StringComparison.OrdinalIgnoreCase)
                && int.TryParse(portName[3..], out int number)
                    ? number
                    : int.MaxValue;
        }

        private void LoadSettings()
        {
            Dictionary<string, string> settings = ReadSettingsFile();
            RestoreWindowBounds(settings);

            if (settings.TryGetValue("Port", out string? port) && portComboBox.Items.Contains(port))
            {
                portComboBox.Text = port;
            }

            if (settings.TryGetValue("Baud", out string? baud))
            {
                baudComboBox.Text = baud;
            }

            if (settings.TryGetValue("SendText", out string? sendText))
            {
                sendTextBox.Text = DecodeMacroText(sendText);
            }

            for (int index = 0; index < MacroButtonCount; index++)
            {
                if (settings.TryGetValue($"MacroF{index + 1}", out string? macroText))
                {
                    macroTexts[index] = DecodeMacroText(macroText);
                }

                UpdateMacroHint(index);
            }

            LoadSendHistory(settings);

            if (settings.TryGetValue("StayOnTop", out string? stayOnTop)
                && bool.TryParse(stayOnTop, out bool value))
            {
                SetStayOnTop(value);
            }

            if (settings.TryGetValue("DtrOnOpen", out string? dtrOnOpen)
                && bool.TryParse(dtrOnOpen, out bool dtrValue))
            {
                dtrCheckBox.Checked = dtrValue;
            }

            if (settings.TryGetValue("RtsOnOpen", out string? rtsOnOpen)
                && bool.TryParse(rtsOnOpen, out bool rtsValue))
            {
                rtsCheckBox.Checked = rtsValue;
            }

            if (settings.TryGetValue("CrLf", out string? crLf)
                && bool.TryParse(crLf, out bool crLfValue))
            {
                crLfCheckBox.Checked = crLfValue;
            }

            if (settings.TryGetValue("ClearBeforeSend", out string? clearBeforeSend)
                && bool.TryParse(clearBeforeSend, out bool clearValue))
            {
                clsCheckBox.Checked = clearValue;
            }

            if (settings.TryGetValue("Echo", out string? echo)
                && bool.TryParse(echo, out bool echoValue))
            {
                echoCheckBox.Checked = echoValue;
            }

            if (settings.TryGetValue("EchoColor", out string? echoColor)
                && TryDecodeColor(echoColor, out Color echoColorValue))
            {
                echoTerminalForeColor = echoColorValue;
            }

            if (settings.TryGetValue("BufferLines", out string? bufferLines)
                && int.TryParse(bufferLines, out int lineLimit))
            {
                terminalLineLimit = Math.Clamp(lineLimit, MinTerminalLines, MaxTerminalLines);
            }

            if (settings.TryGetValue("HexDisplay", out string? hexDisplay)
                && bool.TryParse(hexDisplay, out bool hexValue))
            {
                hexDisplayEnabled = hexValue;
                hexCheckBox.Checked = hexValue;
            }
        }

        private void SaveSettings()
        {
            try
            {
                Rectangle windowBounds = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;
                List<string> settings = new()
                {
                    "Port=" + portComboBox.Text,
                    "Baud=" + baudComboBox.Text,
                    "SendText=base64:" + EncodeMacroText(sendTextBox.Text),
                    "StayOnTop=" + TopMost.ToString(),
                    "DtrOnOpen=" + dtrCheckBox.Checked.ToString(),
                    "RtsOnOpen=" + rtsCheckBox.Checked.ToString(),
                    "CrLf=" + crLfCheckBox.Checked.ToString(),
                    "ClearBeforeSend=" + clsCheckBox.Checked.ToString(),
                    "Echo=" + echoCheckBox.Checked.ToString(),
                    "EchoColor=" + EncodeColor(echoTerminalForeColor),
                    "BufferLines=" + terminalLineLimit.ToString(),
                    "HexDisplay=" + hexCheckBox.Checked.ToString(),
                    "WindowLeft=" + windowBounds.Left.ToString(),
                    "WindowTop=" + windowBounds.Top.ToString(),
                    "WindowWidth=" + windowBounds.Width.ToString(),
                    "WindowHeight=" + windowBounds.Height.ToString()
                };

                settings.AddRange(macroTexts.Select((macroText, index) =>
                    $"MacroF{index + 1}=base64:{EncodeMacroText(macroText)}"));
                settings.Add("SendHistoryCount=" + sendHistory.Count.ToString());
                settings.AddRange(sendHistory.Select((historyText, index) =>
                    $"SendHistory{index + 1}=base64:{EncodeMacroText(historyText)}"));
                EnsureFileDirectory(SettingsFileName);
                File.WriteAllLines(SettingsFileName, settings);
            }
            catch (Exception ex)
            {
                LogEvent("SETTINGS SAVE ERROR " + ex.Message);
            }
        }

        private void RestoreWindowBounds(IReadOnlyDictionary<string, string> settings)
        {
            if (!TryReadWindowBounds(settings, out Rectangle savedBounds))
            {
                return;
            }

            if (Screen.AllScreens.Any(screen => screen.WorkingArea.Contains(savedBounds)))
            {
                StartPosition = FormStartPosition.Manual;
                Bounds = savedBounds;
                return;
            }

            Screen screenForCentering = Screen.PrimaryScreen ?? Screen.FromControl(this);
            Rectangle workingArea = screenForCentering.WorkingArea;
            Size = new Size(
                Math.Min(savedBounds.Width, workingArea.Width),
                Math.Min(savedBounds.Height, workingArea.Height));
            StartPosition = FormStartPosition.Manual;
            Location = new Point(
                workingArea.Left + (workingArea.Width - Width) / 2,
                workingArea.Top + (workingArea.Height - Height) / 2);
        }

        private bool TryReadWindowBounds(IReadOnlyDictionary<string, string> settings, out Rectangle bounds)
        {
            bounds = Rectangle.Empty;
            return settings.TryGetValue("WindowLeft", out string? leftValue)
                && settings.TryGetValue("WindowTop", out string? topValue)
                && settings.TryGetValue("WindowWidth", out string? widthValue)
                && settings.TryGetValue("WindowHeight", out string? heightValue)
                && int.TryParse(leftValue, out int left)
                && int.TryParse(topValue, out int top)
                && int.TryParse(widthValue, out int width)
                && int.TryParse(heightValue, out int height)
                && width >= MinimumSize.Width
                && height >= MinimumSize.Height
                && (bounds = new Rectangle(left, top, width, height)) != Rectangle.Empty;
        }

        private Dictionary<string, string> ReadSettingsFile()
        {
            Dictionary<string, string> settings = new(StringComparer.OrdinalIgnoreCase);

            try
            {
                string settingsFileName = ReadableSettingsFileName;
                if (!File.Exists(settingsFileName))
                {
                    return settings;
                }

                foreach (string line in File.ReadLines(settingsFileName))
                {
                    int separator = line.IndexOf('=');
                    if (separator > 0)
                    {
                        settings[line[..separator]] = line[(separator + 1)..];
                    }
                }

                if (!string.Equals(settingsFileName, SettingsFileName, StringComparison.OrdinalIgnoreCase))
                {
                    LogEvent("SETTINGS LOADED FROM LEGACY PATH " + settingsFileName);
                }
            }
            catch (Exception ex)
            {
                LogEvent("SETTINGS LOAD ERROR " + ex.Message);
            }

            return settings;
        }

        private void LoadSendHistory(IReadOnlyDictionary<string, string> settings)
        {
            sendHistory.Clear();

            int savedHistoryCount = SendHistoryLimit;
            if (settings.TryGetValue("SendHistoryCount", out string? countText)
                && int.TryParse(countText, out int parsedCount))
            {
                savedHistoryCount = Math.Clamp(parsedCount, 0, SendHistoryLimit);
            }

            for (int index = 0; index < savedHistoryCount; index++)
            {
                if (settings.TryGetValue($"SendHistory{index + 1}", out string? historyText))
                {
                    string decodedText = DecodeMacroText(historyText);
                    if (decodedText.Length > 0)
                    {
                        sendHistory.Add(decodedText);
                    }
                }
            }

            ResetSendHistoryNavigation();
        }

        private static void EnsureFileDirectory(string fileName)
        {
            string? directoryName = Path.GetDirectoryName(fileName);
            if (!string.IsNullOrWhiteSpace(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
        }

        private void SetConnected(bool connected)
        {
            portComboBox.Enabled = !connected;
            baudComboBox.Enabled = !connected;
            openButton.Enabled = connected || portComboBox.Items.Count > 0;
            openButton.Text = connected ? "Close" : "Open";
            closeButton.Enabled = connected;
            sendTextBox.Enabled = true;
            sendButton.Enabled = connected;
            crLfCheckBox.Enabled = connected;
            clsCheckBox.Enabled = connected;
            echoCheckBox.Enabled = connected;
            dtrCheckBox.Enabled = true;
            rtsCheckBox.Enabled = true;
            terminalOpenCloseItem.Text = connected ? "Close" : "Open";
            terminalTextBox.BackColor = connected ? ConnectedTerminalBackColor : DisconnectedTerminalBackColor;
            UpdatePortStatus();
        }

        private string SelectedPortText()
        {
            return string.IsNullOrWhiteSpace(portComboBox.Text) ? "no port selected" : portComboBox.Text;
        }

        private void UpdatePortStatus()
        {
            portStatusLabel.Text = IsConnected
                ? $"{portComboBox.Text} open, {baudComboBox.Text} baud"
                : $"{SelectedPortText()}, {baudComboBox.Text} baud";
        }

        private void DtrCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            UpdateControlLine("DTR", dtrCheckBox.Checked, SetDtr, ClearDtr);
        }

        private void RtsCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            UpdateControlLine("RTS", rtsCheckBox.Checked, SetRts, ClearRts);
        }

        private void ApplyControlLineStates()
        {
            UpdateControlLine("DTR", dtrCheckBox.Checked, SetDtr, ClearDtr, showStatus: false);
            UpdateControlLine("RTS", rtsCheckBox.Checked, SetRts, ClearRts, showStatus: false);
        }

        private void UpdateControlLine(string name, bool enabled, uint setCommand, uint clearCommand, bool showStatus = true)
        {
            if (!IsConnected)
            {
                return;
            }

            if (EscapeCommFunction(portHandle, enabled ? setCommand : clearCommand))
            {
                if (showStatus)
                {
                    SetStatusMessage($"{name} {(enabled ? "set" : "cleared")}");
                }

                return;
            }

            SetStatusMessage($"Could not update {name}");
        }

        private void SetStatusMessage(string message)
        {
            messageStatusLabel.Text = message;
        }

        private void SettingsButton_Click(object? sender, EventArgs e)
        {
            ShowSettingsDialog();
        }

        private void ShowSettingsDialog()
        {
            if (settingsDialogShowing)
            {
                return;
            }

            settingsDialogShowing = true;
            if (!IsConnected)
            {
                LoadAvailablePorts();
            }

            bool wasTopMost = TopMost;
            using SettingsForm settingsDialog = new(
                portComboBox.Items.Cast<object>().Select(item => item.ToString() ?? string.Empty),
                portComboBox.Text,
                GetBaudRates(),
                baudComboBox.Text,
                wasTopMost,
                !IsConnected,
                terminalLineLimit,
                echoTerminalForeColor);
            settingsForm = settingsDialog;

            DialogResult result;
            try
            {
                if (wasTopMost)
                {
                    TopMost = false;
                }

                result = settingsDialog.ShowDialog(this);

                if (result != DialogResult.OK)
                {
                    return;
                }

                if (!IsConnected)
                {
                    portComboBox.Text = settingsDialog.SelectedPort;
                    baudComboBox.Text = settingsDialog.SelectedBaudRate;
                }

                SetStayOnTop(settingsDialog.StayOnTop);
                echoTerminalForeColor = settingsDialog.EchoColor;
                int terminalLengthBeforeTrim = terminalBuffer.Length;
                terminalLineLimit = settingsDialog.BufferLines;
                TrimTerminalBuffer();
                TrimTerminalDisplay(terminalLengthBeforeTrim - terminalBuffer.Length);

                SaveSettings();
                SetConnected(IsConnected);
                SetStatusMessage("Settings applied");
            }
            finally
            {
                settingsForm = null;

                if (wasTopMost && !IsDisposed)
                {
                    TopMost = topMostCheckBox.Checked;
                }

                settingsDialogShowing = false;
            }
        }

        private void OpenButton_Click(object? sender, EventArgs e)
        {
            if (IsConnected)
            {
                CloseComPortFromUserAction();
                AppendTerminalText("\r\n[Closed]\r\n");
                SetStatusMessage("Port closed");
                return;
            }

            if (!int.TryParse(baudComboBox.Text, out int baudRate))
            {
                MessageBox.Show("Enter a valid baud rate.", "COM Port Terminal",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (OpenComPort(portComboBox.Text, baudRate))
            {
                SaveSettings();
                SetConnected(true);
                AppendTerminalText($"[Opened {portComboBox.Text}, {baudRate} baud]\r\n");
                SetStatusMessage($"Port {portComboBox.Text} opened");
                sendTextBox.Focus();
            }
        }

        private bool OpenComPort(string portName, int baudRate)
        {
            if (string.IsNullOrWhiteSpace(portName))
            {
                MessageBox.Show("Select a COM port.", "COM Port Terminal",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            string winApiPortName = @"\\.\" + portName;
            portHandle = CreateFile(winApiPortName, FileAccessGenericRead | FileAccessGenericWrite, 0,
                IntPtr.Zero, OpenExisting, 0, IntPtr.Zero);

            if (portHandle == InvalidHandleValue)
            {
                SetStatusMessage($"Could not open {portName}");
                ShowLastWin32Error("Could not open port " + portName + ".");
                return false;
            }

            if (!ConfigurePort(baudRate))
            {
                CloseComPort();
                return false;
            }

            SetupComm(portHandle, 4096, 4096);
            PurgeComm(portHandle, PurgeRxClear | PurgeTxClear);
            ApplyControlLineStates();

            readerStopping = false;
            readerThread = new Thread(ReadLoop)
            {
                IsBackground = true,
                Name = "COM port reader"
            };
            readerThread.Start();

            LogEvent($"PORT OPEN {portName} {baudRate} baud");
            return true;
        }

        private bool ConfigurePort(int baudRate)
        {
            Dcb dcb = new()
            {
                DCBlength = Marshal.SizeOf<Dcb>()
            };

            if (!GetCommState(portHandle, ref dcb))
            {
                SetStatusMessage("Could not read port settings");
                ShowLastWin32Error("Could not read port settings.");
                return false;
            }

            dcb.BaudRate = (uint)baudRate;
            dcb.ByteSize = 8;
            dcb.Parity = 0;
            dcb.StopBits = 0;
            dcb.Flags = 1;

            if (!SetCommState(portHandle, ref dcb))
            {
                SetStatusMessage("Could not apply port settings");
                ShowLastWin32Error("Could not apply port settings.");
                return false;
            }

            CommTimeouts timeouts = BuildCommTimeouts((uint)baudRate);
            if (!SetCommTimeouts(portHandle, ref timeouts))
            {
                SetStatusMessage("Could not set port timeouts");
                ShowLastWin32Error("Could not set port timeouts.");
                return false;
            }

            LogEvent($"TIMEOUTS RI={timeouts.ReadIntervalTimeout} RTC={timeouts.ReadTotalTimeoutConstant} " +
                $"WTM={timeouts.WriteTotalTimeoutMultiplier} WTC={timeouts.WriteTotalTimeoutConstant}");
            return true;
        }

        private static CommTimeouts BuildCommTimeouts(uint baudRate)
        {
            return new CommTimeouts
            {
                ReadIntervalTimeout = ClampDword(BytesTimeMs(2, baudRate), 2, 20),
                ReadTotalTimeoutMultiplier = 0,
                ReadTotalTimeoutConstant = ClampDword(BytesTimeMs(8, baudRate), 5, 50),
                WriteTotalTimeoutMultiplier = ClampDword(BytesTimeMs(1, baudRate), 1, 10),
                WriteTotalTimeoutConstant = ClampDword(BytesTimeMs(64, baudRate), 30, 300)
            };
        }

        private static uint BytesTimeMs(uint bytes, uint baudRate)
        {
            if (baudRate == 0)
            {
                baudRate = 9600;
            }

            return ((bytes * 10 * 1000) + baudRate - 1) / baudRate;
        }

        private static uint ClampDword(uint value, uint min, uint max)
        {
            return Math.Min(Math.Max(value, min), max);
        }

        private void CloseComPort()
        {
            readerStopping = true;

            if (portHandle != InvalidHandleValue)
            {
                CancelIoEx(portHandle, IntPtr.Zero);
            }

            if (readerThread != null)
            {
                if (Thread.CurrentThread != readerThread && !readerThread.Join(1000))
                {
                    LogEvent("READER STOP TIMEOUT");
                }

                readerThread = null;
            }

            if (portHandle != InvalidHandleValue)
            {
                LogEvent("PORT CLOSE " + portComboBox.Text);
                CloseHandle(portHandle);
                portHandle = InvalidHandleValue;
                SetStatusMessage("Port closed");
            }

            SetConnected(false);
        }

        private void CloseComPortFromUserAction()
        {
            CloseComPort();
            LoadAvailablePorts();
        }

        private void ReadLoop()
        {
            byte[] buffer = new byte[256];

            while (!readerStopping && portHandle != InvalidHandleValue)
            {
                if (!ReadFile(portHandle, buffer, (uint)buffer.Length, out uint readCount, IntPtr.Zero))
                {
                    if (!readerStopping)
                    {
                        BeginInvoke(() =>
                        {
                            AppendTerminalText("\r\n[Port read error]\r\n");
                            SetStatusMessage("Port read error");
                            CloseComPort();
                        });
                    }

                    break;
                }

                if (readCount > 0)
                {
                    byte[] data = buffer.Take((int)readCount).ToArray();
                    LogData("RX", data);
                    QueueTerminalData(data, readCount);
                }
            }
        }

        private void QueueTerminalData(byte[] data, uint byteCount)
        {
            string text = hexDisplayEnabled
                ? FormatHexDump(data)
                : Encoding.Default.GetString(data);

            QueueTerminalText(text, byteCount);
        }

        private void QueueTerminalText(string text, uint byteCount)
        {
            bool shouldSchedule = false;

            lock (pendingTerminalLock)
            {
                pendingTerminalText.Append(text);
                TrimTextBufferToLastLines(pendingTerminalText, terminalLineLimit);
                pendingRxBytes += byteCount;

                if (!terminalFlushScheduled && !holdCheckBox.Checked)
                {
                    terminalFlushScheduled = true;
                    shouldSchedule = true;
                }
            }

            if (shouldSchedule)
            {
                BeginPendingTerminalFlush();
            }
        }

        private void SchedulePendingTerminalFlush()
        {
            bool shouldSchedule = false;

            lock (pendingTerminalLock)
            {
                if (!terminalFlushScheduled && pendingTerminalText.Length > 0 && !holdCheckBox.Checked)
                {
                    terminalFlushScheduled = true;
                    shouldSchedule = true;
                }
            }

            if (!shouldSchedule || IsDisposed || !IsHandleCreated)
            {
                return;
            }

            BeginPendingTerminalFlush();
        }

        private void BeginPendingTerminalFlush()
        {
            if (IsDisposed || !IsHandleCreated)
            {
                return;
            }

            try
            {
                BeginInvoke(FlushPendingTerminalText);
            }
            catch (InvalidOperationException)
            {
                lock (pendingTerminalLock)
                {
                    terminalFlushScheduled = false;
                }
            }
        }

        private void FlushPendingTerminalText()
        {
            string text;
            uint byteCount;

            lock (pendingTerminalLock)
            {
                if (holdCheckBox.Checked)
                {
                    terminalFlushScheduled = false;
                    return;
                }

                text = pendingTerminalText.ToString();
                byteCount = pendingRxBytes;
                pendingTerminalText.Clear();
                pendingRxBytes = 0;
                terminalFlushScheduled = false;
            }

            if (text.Length == 0)
            {
                return;
            }

            AppendTerminalText(text);
            SetStatusMessage($"RX {byteCount} bytes");
        }

        private void HoldCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            if (holdCheckBox.Checked)
            {
                SetStatusMessage("Display hold");
                return;
            }

            SetStatusMessage("Display resumed");
            SchedulePendingTerminalFlush();
        }

        private void HexCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            hexDisplayEnabled = hexCheckBox.Checked;
            SaveSettings();
            SetStatusMessage(hexDisplayEnabled ? "HEX display on" : "HEX display off");
        }

        private void EchoCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            SaveSettings();
            SetStatusMessage(echoCheckBox.Checked ? "ECHO on" : "ECHO off");
        }

        private void SendButton_Click(object? sender, EventArgs e)
        {
            SendCurrentEditText();
        }

        private void MacroButton_MouseClick(object? sender, MouseEventArgs e)
        {
            if (sender is not Button { Tag: int index })
            {
                return;
            }

            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            string macroText = macroTexts[index];
            if (string.IsNullOrEmpty(macroText))
            {
                SetStatusMessage($"F{index + 1} is empty");
                return;
            }

            sendTextBox.Text = RemoveTrailingCrLf(macroText);
            if (IsConnected)
            {
                SendText(macroText, appendCrLf: false);
            }
            else
            {
                SetStatusMessage($"F{index + 1} copied; port is closed");
            }
        }

        private void SetMacroFromSendText(int index)
        {
            macroTexts[index] = crLfCheckBox.Checked
                ? sendTextBox.Text + "\r\n"
                : sendTextBox.Text;
            UpdateMacroHint(index);
            SaveSettings();
            SetStatusMessage($"F{index + 1} saved");
        }

        private void UpdateMacroHint(int index)
        {
            string hint = string.IsNullOrEmpty(macroTexts[index])
                ? $"F{index + 1} is empty"
                : macroTexts[index].Replace("\r", "\\r").Replace("\n", "\\n");
            macroToolTip.SetToolTip(macroButtons[index], hint);
        }

        private void MacroButton_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && sender is Button button)
            {
                button.Focus();
            }
        }

        private void AssignMacroItem_Click(object? sender, EventArgs e)
        {
            if (macroMenu.SourceControl is Button { Tag: int index } button)
            {
                button.Focus();
                SetMacroFromSendText(index);
            }
        }

        private static string RemoveTrailingCrLf(string text)
        {
            return text.EndsWith("\r\n", StringComparison.Ordinal)
                ? text[..^2]
                : text;
        }

        private static string EncodeMacroText(string? text)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(text ?? string.Empty));
        }

        private static string DecodeMacroText(string text)
        {
            const string prefix = "base64:";
            if (!text.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return text;
            }

            try
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(text[prefix.Length..]));
            }
            catch (FormatException)
            {
                return string.Empty;
            }
        }

        private static string EncodeColor(Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        private static bool TryDecodeColor(string text, out Color color)
        {
            color = DefaultEchoTerminalForeColor;
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            try
            {
                color = ColorTranslator.FromHtml(text.Trim());
                return true;
            }
            catch
            {
                if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int argb))
                {
                    color = Color.FromArgb(argb);
                    return true;
                }

                return false;
            }
        }

        private void SendTextBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                SendCurrentEditText();
                return;
            }

            if (e.KeyCode == Keys.Up)
            {
                e.SuppressKeyPress = true;
                NavigateSendHistory(-1);
                return;
            }

            if (e.KeyCode == Keys.Down)
            {
                e.SuppressKeyPress = true;
                NavigateSendHistory(1);
                return;
            }

            ResetSendHistoryNavigation();
        }

        private void SendTextBox_PreviewKeyDown(object? sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode is Keys.Up or Keys.Down)
            {
                e.IsInputKey = true;
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            Keys keyCode = keyData & Keys.KeyCode;
            Keys modifiers = keyData & Keys.Modifiers;

            if (sendTextBox.Focused && modifiers == Keys.None)
            {
                if (keyCode == Keys.Up)
                {
                    NavigateSendHistory(-1);
                    return true;
                }

                if (keyCode == Keys.Down)
                {
                    NavigateSendHistory(1);
                    return true;
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void SendTextBox_TextChanged(object? sender, EventArgs e)
        {
            SaveSettings();
        }

        private void SendCurrentEditText()
        {
            string text = sendTextBox.Text;
            bool sent = SendText(text);
            if (sent)
            {
                AddSendHistory(text);
            }

            SaveSettings();
            ResetSendHistoryNavigation();
        }

        private bool SendText(string text, bool appendCrLf = true)
        {
            if (!IsConnected)
            {
                return false;
            }

            string outgoingText = appendCrLf && crLfCheckBox.Checked ? text + "\r\n" : text;
            byte[] data = Encoding.Default.GetBytes(outgoingText);

            if (!WriteFile(portHandle, data, (uint)data.Length, out uint written, IntPtr.Zero))
            {
                SetStatusMessage("Send failed");
                ShowLastWin32Error("Could not send data.");
                CloseComPort();
                return false;
            }

            if (written > 0)
            {
                LogEvent("SEND BUTTON CLICK");
                LogData("TX", data.Take((int)written).ToArray());
                if (echoCheckBox.Checked)
                {
                    AppendTerminalText(Encoding.Default.GetString(data, 0, (int)written), echoTerminalForeColor);
                }

                SetStatusMessage($"TX {written} bytes");
                if (clsCheckBox.Checked)
                {
                    sendTextBox.Clear();
                }
            }

            sendTextBox.Focus();
            return written > 0;
        }

        private void AddSendHistory(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            sendHistory.Add(text);
            while (sendHistory.Count > SendHistoryLimit)
            {
                sendHistory.RemoveAt(0);
            }
        }

        private void NavigateSendHistory(int direction)
        {
            if (sendHistory.Count == 0)
            {
                return;
            }

            if (sendHistoryNavigationIndex == -1)
            {
                sendHistoryDraft = sendTextBox.Text;
                if (direction < 0)
                {
                    sendHistoryNavigationIndex = sendHistory.Count > 1
                        && string.Equals(sendHistoryDraft, sendHistory[^1], StringComparison.Ordinal)
                            ? sendHistory.Count - 2
                            : sendHistory.Count - 1;
                }
                else
                {
                    sendHistoryNavigationIndex = sendHistory.Count > 1
                        && string.Equals(sendHistoryDraft, sendHistory[0], StringComparison.Ordinal)
                            ? 1
                            : 0;
                }
            }
            else
            {
                int step = direction < 0 ? -1 : 1;
                sendHistoryNavigationIndex =
                    (sendHistoryNavigationIndex + step + sendHistory.Count) % sendHistory.Count;
            }

            SetSendTextBoxText(sendHistory[sendHistoryNavigationIndex]);
        }

        private void ResetSendHistoryNavigation()
        {
            sendHistoryNavigationIndex = -1;
            sendHistoryDraft = string.Empty;
        }

        private void SetSendTextBoxText(string text)
        {
            sendTextBox.Text = text;
            sendTextBox.SelectionStart = sendTextBox.TextLength;
            sendTextBox.SelectionLength = 0;
        }

        private void AppendTerminalText(string text)
        {
            AppendTerminalText(text, terminalTextBox.ForeColor);
        }

        private void AppendTerminalText(string text, Color color)
        {
            if (text.Length == 0)
            {
                return;
            }

            terminalBuffer.Append(text);
            int lengthAfterAppend = terminalBuffer.Length;
            TrimTerminalBuffer();
            int removedChars = lengthAfterAppend - terminalBuffer.Length;

            terminalTextBox.SelectionStart = terminalTextBox.TextLength;
            terminalTextBox.SelectionLength = 0;
            terminalTextBox.SelectionColor = color;
            terminalTextBox.AppendText(text);

            TrimTerminalDisplay(removedChars);
        }

        private void TrimTerminalBuffer()
        {
            TrimTextBufferToLastLines(terminalBuffer, terminalLineLimit);
        }

        private void TrimTerminalDisplay(int removedChars)
        {
            if (removedChars > 0 && terminalTextBox.TextLength > 0)
            {
                int removeCount = Math.Min(removedChars, terminalTextBox.TextLength);
                terminalTextBox.Select(0, removeCount);
                terminalTextBox.SelectedText = string.Empty;
            }

            terminalTextBox.SelectionStart = terminalTextBox.TextLength;
            terminalTextBox.SelectionLength = 0;
            terminalTextBox.SelectionColor = terminalTextBox.ForeColor;
            terminalTextBox.ScrollToCaret();
        }

        private static void TrimTextBufferToLastLines(StringBuilder buffer, int maxLines)
        {
            int lineBreaks = 0;
            int lineLimit = Math.Clamp(maxLines, MinTerminalLines, MaxTerminalLines);

            for (int i = buffer.Length - 1; i >= 0; i--)
            {
                if (buffer[i] != '\n')
                {
                    continue;
                }

                lineBreaks++;
                if (lineBreaks > lineLimit)
                {
                    buffer.Remove(0, i + 1);
                    return;
                }
            }
        }

        private static string FormatHexDump(byte[] data)
        {
            StringBuilder result = new();

            for (int offset = 0; offset < data.Length; offset += HexBytesPerLine)
            {
                int count = Math.Min(HexBytesPerLine, data.Length - offset);

                for (int i = 0; i < HexBytesPerLine; i++)
                {
                    if (i < count)
                    {
                        result.Append(data[offset + i].ToString("X2"));
                    }
                    else
                    {
                        result.Append("  ");
                    }

                    if (i < HexBytesPerLine - 1)
                    {
                        result.Append(' ');
                    }
                }

                result.Append("  ");

                for (int i = 0; i < count; i++)
                {
                    byte value = data[offset + i];
                    result.Append(value is >= 32 and <= 126 ? (char)value : '.');
                }

                result.AppendLine();
            }

            return result.ToString();
        }

        private void PortComboBox_DropDown(object? sender, EventArgs e)
        {
            if (!IsConnected)
            {
                LoadAvailablePorts();
            }

            SetConnected(IsConnected);
        }

        private void CloseButton_Click(object? sender, EventArgs e)
        {
            CloseComPortFromUserAction();
            AppendTerminalText("\r\n[Closed]\r\n");
            SetStatusMessage("Port closed");
        }

        private void TerminalMenu_Opening(object? sender, CancelEventArgs e)
        {
            terminalCopyItem.Enabled = terminalTextBox.TextLength > 0;
            terminalClearItem.Enabled = terminalTextBox.TextLength > 0 || pendingTerminalText.Length > 0;
        }

        private void CopyTerminalItem_Click(object? sender, EventArgs e)
        {
            string text = terminalTextBox.SelectedText;
            if (string.IsNullOrEmpty(text))
            {
                text = terminalTextBox.Text;
            }

            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            Clipboard.SetText(text);
            SetStatusMessage("Copied");
        }

        private void ClearTerminalItem_Click(object? sender, EventArgs e)
        {
            ClearTerminal();
        }

        private void TerminalTextBox_MouseDoubleClick(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ClearTerminal();
            }
        }

        private void ClearTerminal()
        {
            terminalBuffer.Clear();
            lock (pendingTerminalLock)
            {
                pendingTerminalText.Clear();
                pendingRxBytes = 0;
                terminalFlushScheduled = false;
            }

            terminalTextBox.Clear();
            terminalTextBox.SelectionColor = terminalTextBox.ForeColor;
            SetStatusMessage("Terminal cleared");
        }

        private void ExitButton_Click(object? sender, EventArgs e)
        {
            Close();
        }

        private void TopMostCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            if (!applyingTopMost)
            {
                SetStayOnTop(topMostCheckBox.Checked);
                SaveSettings();
                LogEvent(TopMost ? "STAY ON TOP ON" : "STAY ON TOP OFF");
            }
        }

        private void TrayTopMostItem_CheckedChanged(object? sender, EventArgs e)
        {
            if (!applyingTopMost)
            {
                if (sender == terminalTopMostItem)
                {
                    SetStayOnTop(terminalTopMostItem.Checked);
                }
                else
                {
                    SetStayOnTop(trayTopMostItem.Checked);
                }

                SaveSettings();
                LogEvent(TopMost ? "STAY ON TOP ON" : "STAY ON TOP OFF");
            }
        }

        private void SetStayOnTop(bool value)
        {
            applyingTopMost = true;
            TopMost = value;
            topMostCheckBox.Checked = value;
            trayTopMostItem.Checked = value;
            terminalTopMostItem.Checked = value;
            applyingTopMost = false;
        }

        private void TrayIcon_DoubleClick(object? sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            Activate();
        }

        private void InitLog()
        {
            try
            {
                EnsureFileDirectory(LogFileName);
                logWriter = new StreamWriter(LogFileName, append: true, Encoding.UTF8)
                {
                    AutoFlush = true
                };
                logWriter.WriteLine();
                LogEvent("LOG OPENED");
            }
            catch
            {
                logWriter = null;
            }
        }

        private void CloseLog()
        {
            if (logWriter == null)
            {
                return;
            }

            try
            {
                LogEvent("LOG CLOSED");
                logWriter.Dispose();
            }
            catch
            {
                // Logging should never prevent the app from closing.
            }
            finally
            {
                logWriter = null;
            }
        }

        private void LogEvent(string message)
        {
            try
            {
                logWriter?.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}");
            }
            catch
            {
                logWriter = null;
            }
        }

        private void LogData(string prefix, byte[] data)
        {
            string escaped = EscapeLogData(data);
            LogEvent($"{prefix} {data.Length} bytes: {escaped}");
        }

        private static string EscapeLogData(byte[] data)
        {
            StringBuilder result = new();
            int size = Math.Min(data.Length, MaxLogData);

            for (int i = 0; i < size; i++)
            {
                byte value = data[i];
                result.Append(value switch
                {
                    9 => @"\t",
                    10 => @"\n",
                    13 => @"\r",
                    >= 32 and <= 126 => (char)value,
                    _ => @"\x" + value.ToString("X2")
                });
            }

            if (data.Length > MaxLogData)
            {
                result.Append("...");
            }

            return result.ToString();
        }

        private static void ShowLastWin32Error(string message)
        {
            string error = new Win32Exception(Marshal.GetLastWin32Error()).Message;
            MessageBox.Show(message + Environment.NewLine + error, "COM Port Terminal",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private const uint FileAccessGenericRead = 0x80000000;
        private const uint FileAccessGenericWrite = 0x40000000;
        private const uint OpenExisting = 3;
        private const uint PurgeRxClear = 0x0008;
        private const uint PurgeTxClear = 0x0004;
        private const uint SetDtr = 5;
        private const uint ClearDtr = 6;
        private const uint SetRts = 3;
        private const uint ClearRts = 4;

        [StructLayout(LayoutKind.Sequential)]
        private struct Dcb
        {
            public int DCBlength;
            public uint BaudRate;
            public uint Flags;
            public ushort wReserved;
            public ushort XonLim;
            public ushort XoffLim;
            public byte ByteSize;
            public byte Parity;
            public byte StopBits;
            public sbyte XonChar;
            public sbyte XoffChar;
            public sbyte ErrorChar;
            public sbyte EofChar;
            public sbyte EvtChar;
            public ushort wReserved1;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CommTimeouts
        {
            public uint ReadIntervalTimeout;
            public uint ReadTotalTimeoutMultiplier;
            public uint ReadTotalTimeoutConstant;
            public uint WriteTotalTimeoutMultiplier;
            public uint WriteTotalTimeoutConstant;
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode,
            IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetCommState(IntPtr hFile, ref Dcb lpDCB);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetCommState(IntPtr hFile, ref Dcb lpDCB);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetCommTimeouts(IntPtr hFile, ref CommTimeouts lpCommTimeouts);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetupComm(IntPtr hFile, uint dwInQueue, uint dwOutQueue);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool PurgeComm(IntPtr hFile, uint dwFlags);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool EscapeCommFunction(IntPtr hFile, uint dwFunc);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadFile(IntPtr hFile, byte[] lpBuffer, uint nNumberOfBytesToRead,
            out uint lpNumberOfBytesRead, IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteFile(IntPtr hFile, byte[] lpBuffer, uint nNumberOfBytesToWrite,
            out uint lpNumberOfBytesWritten, IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CancelIoEx(IntPtr hFile, IntPtr lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);
    }
}
