namespace CPortTerminal
{
    public sealed class SettingsForm : Form
    {
        private readonly ComboBox portComboBox;
        private readonly ComboBox baudComboBox;
        private readonly NumericUpDown bufferLinesUpDown;
        private readonly CheckBox topMostCheckBox;
        private readonly Button echoColorButton;
        private readonly Button okButton;
        private readonly Button cancelButton;
        private Color echoColor;
        private Image? camouflageSkin;

        public SettingsForm(IEnumerable<string> ports, string selectedPort, IEnumerable<string> baudRates,
            string selectedBaudRate, bool stayOnTop, bool serialSettingsEnabled, int bufferLines, Color selectedEchoColor)
        {
            Text = "Settings";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(360, 240);

            Label portLabel = new()
            {
                AutoSize = true,
                Location = new Point(18, 22),
                Text = "Port"
            };

            portComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(130, 18),
                Size = new Size(205, 23),
                Enabled = serialSettingsEnabled
            };
            portComboBox.Items.AddRange(ports.Cast<object>().ToArray());
            if (!string.IsNullOrWhiteSpace(selectedPort) && portComboBox.Items.Contains(selectedPort))
            {
                portComboBox.Text = selectedPort;
            }
            else if (portComboBox.Items.Count > 0)
            {
                portComboBox.SelectedIndex = 0;
            }

            Label baudLabel = new()
            {
                AutoSize = true,
                Location = new Point(18, 62),
                Text = "Baud rate"
            };

            baudComboBox = new ComboBox
            {
                Location = new Point(130, 58),
                Size = new Size(205, 23),
                Enabled = serialSettingsEnabled
            };
            baudComboBox.Items.AddRange(baudRates.Cast<object>().ToArray());
            baudComboBox.Text = selectedBaudRate;

            Label bufferLinesLabel = new()
            {
                AutoSize = true,
                Location = new Point(18, 102),
                Text = "Buffer lines"
            };

            bufferLinesUpDown = new NumericUpDown
            {
                Location = new Point(130, 98),
                Minimum = 16,
                Maximum = 5000,
                Increment = 50,
                Size = new Size(205, 23),
                Value = Math.Clamp(bufferLines, 16, 5000)
            };

            Label echoColorLabel = new()
            {
                AutoSize = true,
                Location = new Point(18, 136),
                Text = "Echo color"
            };

            echoColor = selectedEchoColor;
            echoColorButton = new Button
            {
                Location = new Point(130, 132),
                Size = new Size(96, 25)
            };
            echoColorButton.Click += EchoColorButton_Click;
            UpdateEchoColorButton();

            topMostCheckBox = new CheckBox
            {
                AutoSize = true,
                Location = new Point(130, 164),
                Text = "Always on top",
                Checked = stayOnTop
            };

            okButton = new Button
            {
                DialogResult = DialogResult.OK,
                Location = new Point(170, 195),
                Size = new Size(80, 28),
                Text = "OK"
            };

            cancelButton = new Button
            {
                DialogResult = DialogResult.Cancel,
                Location = new Point(255, 195),
                Size = new Size(80, 28),
                Text = "Cancel"
            };

            AcceptButton = okButton;
            CancelButton = cancelButton;
            Controls.AddRange(new Control[]
            {
                portLabel,
                portComboBox,
                baudLabel,
                baudComboBox,
                bufferLinesLabel,
                bufferLinesUpDown,
                echoColorLabel,
                echoColorButton,
                topMostCheckBox,
                okButton,
                cancelButton
            });

            ApplyCamouflageSkin();
        }

        private void ApplyCamouflageSkin()
        {
            string skinPath = Path.Combine(AppContext.BaseDirectory, "Assets", "CamoSkin.png");
            if (!File.Exists(skinPath))
            {
                return;
            }

            camouflageSkin = Image.FromFile(skinPath);
            BackgroundImage = camouflageSkin;
            BackgroundImageLayout = ImageLayout.Tile;

            Color textColor = Color.FromArgb(240, 235, 202);
            Color controlColor = Color.FromArgb(30, 36, 22);
            Color buttonColor = Color.FromArgb(58, 70, 37);
            Color buttonBorderColor = Color.FromArgb(149, 142, 90);

            foreach (Label label in Controls.OfType<Label>())
            {
                label.BackColor = Color.Transparent;
                label.ForeColor = textColor;
            }

            topMostCheckBox.BackColor = Color.Transparent;
            topMostCheckBox.ForeColor = textColor;

            foreach (Control control in new Control[] { portComboBox, baudComboBox, bufferLinesUpDown })
            {
                control.BackColor = controlColor;
                control.ForeColor = textColor;
            }

            echoColorButton.FlatStyle = FlatStyle.Flat;
            echoColorButton.UseVisualStyleBackColor = false;
            echoColorButton.FlatAppearance.BorderColor = buttonBorderColor;
            UpdateEchoColorButton();

            foreach (Button button in new[] { okButton, cancelButton })
            {
                button.FlatStyle = FlatStyle.Flat;
                button.UseVisualStyleBackColor = false;
                button.BackColor = buttonColor;
                button.ForeColor = textColor;
                button.FlatAppearance.BorderColor = buttonBorderColor;
            }
        }

        private void EchoColorButton_Click(object? sender, EventArgs e)
        {
            using ColorDialog colorDialog = new()
            {
                Color = echoColor,
                FullOpen = false
            };

            if (colorDialog.ShowDialog(this) == DialogResult.OK)
            {
                echoColor = colorDialog.Color;
                UpdateEchoColorButton();
            }
        }

        private void UpdateEchoColorButton()
        {
            echoColorButton.Text = FormatColor(echoColor);
            echoColorButton.BackColor = echoColor;
            echoColorButton.ForeColor = GetReadableTextColor(echoColor);
        }

        private static string FormatColor(Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        private static Color GetReadableTextColor(Color backgroundColor)
        {
            double brightness = backgroundColor.R * 0.299 + backgroundColor.G * 0.587 + backgroundColor.B * 0.114;
            return brightness > 150 ? Color.Black : Color.White;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                camouflageSkin?.Dispose();
            }

            base.Dispose(disposing);
        }

        public string SelectedPort => portComboBox.Text;

        public void RefreshPorts(IEnumerable<string> ports)
        {
            string selectedPort = portComboBox.Text;
            string[] portList = ports.ToArray();

            portComboBox.Items.Clear();
            portComboBox.Items.AddRange(portList.Cast<object>().ToArray());

            if (!string.IsNullOrWhiteSpace(selectedPort) && portList.Contains(selectedPort))
            {
                portComboBox.Text = selectedPort;
            }
            else if (portList.Length > 0)
            {
                portComboBox.SelectedIndex = 0;
            }
            else
            {
                portComboBox.SelectedIndex = -1;
            }
        }

        public string SelectedBaudRate => baudComboBox.Text;

        public bool StayOnTop => topMostCheckBox.Checked;

        public int BufferLines => (int)bufferLinesUpDown.Value;

        public Color EchoColor => echoColor;
    }
}
