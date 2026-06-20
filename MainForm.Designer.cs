namespace CPortTerminal
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private Panel topPanel;
        private ComboBox portComboBox;
        private ComboBox baudComboBox;
        private Button openButton;
        private Button closeButton;
        private Button exitButton;
        private CheckBox dtrCheckBox;
        private CheckBox rtsCheckBox;
        private CheckBox holdCheckBox;
        private CheckBox hexCheckBox;
        private CheckBox topMostCheckBox;
        private TextBox terminalTextBox;
        private Panel bottomPanel;
        private TextBox sendTextBox;
        private Button sendButton;
        private CheckBox crLfCheckBox;
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;
        private ToolStripMenuItem traySettingsItem;
        private ToolStripMenuItem trayTopMostItem;
        private ToolStripMenuItem trayExitItem;
        private ContextMenuStrip terminalMenu;
        private ToolStripMenuItem terminalOpenCloseItem;
        private ToolStripMenuItem terminalSettingsItem;
        private ToolStripMenuItem terminalCopyItem;
        private ToolStripMenuItem terminalClearItem;
        private ToolStripMenuItem terminalTopMostItem;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel portStatusLabel;
        private ToolStripStatusLabel messageStatusLabel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            topPanel = new Panel();
            openButton = new Button();
            exitButton = new Button();
            closeButton = new Button();
            dtrCheckBox = new CheckBox();
            rtsCheckBox = new CheckBox();
            holdCheckBox = new CheckBox();
            hexCheckBox = new CheckBox();
            portComboBox = new ComboBox();
            baudComboBox = new ComboBox();
            topMostCheckBox = new CheckBox();
            terminalMenu = new ContextMenuStrip(components);
            terminalOpenCloseItem = new ToolStripMenuItem();
            terminalSettingsItem = new ToolStripMenuItem();
            terminalCopyItem = new ToolStripMenuItem();
            terminalClearItem = new ToolStripMenuItem();
            terminalTopMostItem = new ToolStripMenuItem();
            terminalTextBox = new TextBox();
            bottomPanel = new Panel();
            sendTextBox = new TextBox();
            sendButton = new Button();
            crLfCheckBox = new CheckBox();
            trayMenu = new ContextMenuStrip(components);
            traySettingsItem = new ToolStripMenuItem();
            trayTopMostItem = new ToolStripMenuItem();
            trayExitItem = new ToolStripMenuItem();
            statusStrip = new StatusStrip();
            portStatusLabel = new ToolStripStatusLabel();
            messageStatusLabel = new ToolStripStatusLabel();
            trayIcon = new NotifyIcon(components);
            topPanel.SuspendLayout();
            terminalMenu.SuspendLayout();
            bottomPanel.SuspendLayout();
            trayMenu.SuspendLayout();
            statusStrip.SuspendLayout();
            SuspendLayout();
            // 
            // topPanel
            // 
            topPanel.Controls.Add(openButton);
            topPanel.Controls.Add(dtrCheckBox);
            topPanel.Controls.Add(rtsCheckBox);
            topPanel.Controls.Add(holdCheckBox);
            topPanel.Controls.Add(hexCheckBox);
            topPanel.Controls.Add(exitButton);
            topPanel.Dock = DockStyle.Top;
            topPanel.Location = new Point(0, 0);
            topPanel.Name = "topPanel";
            topPanel.Padding = new Padding(10, 8, 10, 8);
            topPanel.Size = new Size(1017, 48);
            topPanel.TabIndex = 0;
            // 
            // openButton
            // 
            openButton.Location = new Point(12, 11);
            openButton.Name = "openButton";
            openButton.Size = new Size(92, 25);
            openButton.TabIndex = 0;
            openButton.Text = "Open";
            openButton.UseVisualStyleBackColor = true;
            openButton.Click += OpenButton_Click;
            // 
            // exitButton
            // 
            exitButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            exitButton.Location = new Point(970, 11);
            exitButton.Name = "exitButton";
            exitButton.Size = new Size(35, 25);
            exitButton.TabIndex = 1;
            exitButton.Text = "X";
            exitButton.UseVisualStyleBackColor = true;
            exitButton.Click += ExitButton_Click;
            // 
            // closeButton
            // 
            closeButton.Location = new Point(0, 0);
            closeButton.Name = "closeButton";
            closeButton.Size = new Size(80, 25);
            closeButton.TabIndex = 1;
            closeButton.Text = "Close";
            closeButton.UseVisualStyleBackColor = true;
            closeButton.Visible = false;
            closeButton.Click += CloseButton_Click;
            // 
            // dtrCheckBox
            // 
            dtrCheckBox.AutoSize = true;
            dtrCheckBox.Checked = true;
            dtrCheckBox.CheckState = CheckState.Checked;
            dtrCheckBox.Location = new Point(116, 14);
            dtrCheckBox.Name = "dtrCheckBox";
            dtrCheckBox.Size = new Size(47, 19);
            dtrCheckBox.TabIndex = 2;
            dtrCheckBox.Text = "DTR";
            dtrCheckBox.UseVisualStyleBackColor = true;
            //
            // rtsCheckBox
            //
            rtsCheckBox.AutoSize = true;
            rtsCheckBox.Location = new Point(174, 14);
            rtsCheckBox.Name = "rtsCheckBox";
            rtsCheckBox.Size = new Size(47, 19);
            rtsCheckBox.TabIndex = 3;
            rtsCheckBox.Text = "RTS";
            rtsCheckBox.UseVisualStyleBackColor = true;
            //
            // holdCheckBox
            // 
            holdCheckBox.AutoSize = true;
            holdCheckBox.Location = new Point(232, 14);
            holdCheckBox.Name = "holdCheckBox";
            holdCheckBox.Size = new Size(58, 19);
            holdCheckBox.TabIndex = 4;
            holdCheckBox.Text = "HOLD";
            holdCheckBox.UseVisualStyleBackColor = true;
            holdCheckBox.CheckedChanged += HoldCheckBox_CheckedChanged;
            // 
            // hexCheckBox
            // 
            hexCheckBox.AutoSize = true;
            hexCheckBox.Location = new Point(302, 14);
            hexCheckBox.Name = "hexCheckBox";
            hexCheckBox.Size = new Size(48, 19);
            hexCheckBox.TabIndex = 5;
            hexCheckBox.Text = "HEX";
            hexCheckBox.UseVisualStyleBackColor = true;
            hexCheckBox.CheckedChanged += HexCheckBox_CheckedChanged;
            // 
            // portComboBox
            // 
            portComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            portComboBox.FormattingEnabled = true;
            portComboBox.Location = new Point(0, 0);
            portComboBox.Name = "portComboBox";
            portComboBox.Size = new Size(86, 23);
            portComboBox.TabIndex = 3;
            portComboBox.Visible = false;
            portComboBox.DropDown += PortComboBox_DropDown;
            // 
            // baudComboBox
            // 
            baudComboBox.FormattingEnabled = true;
            baudComboBox.Location = new Point(0, 0);
            baudComboBox.Name = "baudComboBox";
            baudComboBox.Size = new Size(94, 23);
            baudComboBox.TabIndex = 4;
            baudComboBox.Visible = false;
            // 
            // topMostCheckBox
            // 
            topMostCheckBox.AutoSize = true;
            topMostCheckBox.Location = new Point(0, 0);
            topMostCheckBox.Name = "topMostCheckBox";
            topMostCheckBox.Size = new Size(101, 19);
            topMostCheckBox.TabIndex = 5;
            topMostCheckBox.Text = "Always on top";
            topMostCheckBox.UseVisualStyleBackColor = true;
            topMostCheckBox.Visible = false;
            topMostCheckBox.CheckedChanged += TopMostCheckBox_CheckedChanged;
            // 
            // terminalMenu
            // 
            terminalMenu.Items.AddRange(new ToolStripItem[] { terminalOpenCloseItem, terminalSettingsItem, terminalCopyItem, terminalClearItem, terminalTopMostItem });
            terminalMenu.Name = "terminalMenu";
            terminalMenu.Size = new Size(150, 114);
            terminalMenu.Opening += TerminalMenu_Opening;
            // 
            // terminalOpenCloseItem
            // 
            terminalOpenCloseItem.Name = "terminalOpenCloseItem";
            terminalOpenCloseItem.Size = new Size(149, 22);
            terminalOpenCloseItem.Text = "Open";
            terminalOpenCloseItem.Click += OpenButton_Click;
            // 
            // terminalSettingsItem
            // 
            terminalSettingsItem.Name = "terminalSettingsItem";
            terminalSettingsItem.Size = new Size(149, 22);
            terminalSettingsItem.Text = "Settings";
            terminalSettingsItem.Click += SettingsButton_Click;
            // 
            // terminalCopyItem
            // 
            terminalCopyItem.Name = "terminalCopyItem";
            terminalCopyItem.Size = new Size(149, 22);
            terminalCopyItem.Text = "Copy";
            terminalCopyItem.Click += CopyTerminalItem_Click;
            // 
            // terminalClearItem
            // 
            terminalClearItem.Name = "terminalClearItem";
            terminalClearItem.Size = new Size(149, 22);
            terminalClearItem.Text = "Clear";
            terminalClearItem.Click += ClearTerminalItem_Click;
            // 
            // terminalTopMostItem
            // 
            terminalTopMostItem.CheckOnClick = true;
            terminalTopMostItem.Name = "terminalTopMostItem";
            terminalTopMostItem.Size = new Size(149, 22);
            terminalTopMostItem.Text = "Always on top";
            terminalTopMostItem.CheckedChanged += TrayTopMostItem_CheckedChanged;
            // 
            // terminalTextBox
            // 
            terminalTextBox.BackColor = Color.Black;
            terminalTextBox.ContextMenuStrip = terminalMenu;
            terminalTextBox.Dock = DockStyle.Fill;
            terminalTextBox.Font = new Font("Consolas", 10F);
            terminalTextBox.ForeColor = Color.Lime;
            terminalTextBox.Location = new Point(0, 48);
            terminalTextBox.Multiline = true;
            terminalTextBox.Name = "terminalTextBox";
            terminalTextBox.ReadOnly = true;
            terminalTextBox.ScrollBars = ScrollBars.Both;
            terminalTextBox.Size = new Size(1017, 407);
            terminalTextBox.TabIndex = 1;
            terminalTextBox.WordWrap = false;
            // 
            // bottomPanel
            // 
            bottomPanel.Controls.Add(sendTextBox);
            bottomPanel.Controls.Add(sendButton);
            bottomPanel.Controls.Add(crLfCheckBox);
            bottomPanel.Dock = DockStyle.Bottom;
            bottomPanel.Location = new Point(0, 455);
            bottomPanel.Name = "bottomPanel";
            bottomPanel.Padding = new Padding(10, 8, 10, 8);
            bottomPanel.Size = new Size(1017, 45);
            bottomPanel.TabIndex = 2;
            // 
            // sendTextBox
            // 
            sendTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            sendTextBox.Location = new Point(12, 11);
            sendTextBox.Name = "sendTextBox";
            sendTextBox.Size = new Size(807, 23);
            sendTextBox.TabIndex = 0;
            sendTextBox.KeyDown += SendTextBox_KeyDown;
            // 
            // sendButton
            // 
            sendButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            sendButton.Location = new Point(831, 10);
            sendButton.Name = "sendButton";
            sendButton.Size = new Size(90, 25);
            sendButton.TabIndex = 1;
            sendButton.Text = "Send";
            sendButton.UseVisualStyleBackColor = true;
            sendButton.Click += SendButton_Click;
            // 
            // crLfCheckBox
            // 
            crLfCheckBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            crLfCheckBox.AutoSize = true;
            crLfCheckBox.Checked = true;
            crLfCheckBox.CheckState = CheckState.Checked;
            crLfCheckBox.Location = new Point(939, 14);
            crLfCheckBox.Name = "crLfCheckBox";
            crLfCheckBox.Size = new Size(58, 19);
            crLfCheckBox.TabIndex = 2;
            crLfCheckBox.Text = "CR/LF";
            crLfCheckBox.UseVisualStyleBackColor = true;
            // 
            // trayMenu
            // 
            trayMenu.Items.AddRange(new ToolStripItem[] { traySettingsItem, trayTopMostItem, trayExitItem });
            trayMenu.Name = "trayMenu";
            trayMenu.Size = new Size(150, 70);
            // 
            // traySettingsItem
            // 
            traySettingsItem.Name = "traySettingsItem";
            traySettingsItem.Size = new Size(149, 22);
            traySettingsItem.Text = "Settings";
            traySettingsItem.Click += SettingsButton_Click;
            // 
            // trayTopMostItem
            // 
            trayTopMostItem.CheckOnClick = true;
            trayTopMostItem.Name = "trayTopMostItem";
            trayTopMostItem.Size = new Size(149, 22);
            trayTopMostItem.Text = "Always on top";
            trayTopMostItem.CheckedChanged += TrayTopMostItem_CheckedChanged;
            // 
            // trayExitItem
            // 
            trayExitItem.Name = "trayExitItem";
            trayExitItem.Size = new Size(149, 22);
            trayExitItem.Text = "Exit";
            trayExitItem.Click += ExitButton_Click;
            // 
            // statusStrip
            // 
            statusStrip.Items.AddRange(new ToolStripItem[] { portStatusLabel, messageStatusLabel });
            statusStrip.Location = new Point(0, 500);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(1017, 24);
            statusStrip.TabIndex = 6;
            statusStrip.Text = "statusStrip";
            // 
            // portStatusLabel
            // 
            portStatusLabel.BorderSides = ToolStripStatusLabelBorderSides.Right;
            portStatusLabel.Name = "portStatusLabel";
            portStatusLabel.Size = new Size(70, 19);
            portStatusLabel.Text = "Port closed";
            // 
            // messageStatusLabel
            // 
            messageStatusLabel.Name = "messageStatusLabel";
            messageStatusLabel.Size = new Size(932, 19);
            messageStatusLabel.Spring = true;
            messageStatusLabel.Text = "Ready";
            messageStatusLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // trayIcon
            // 
            trayIcon.ContextMenuStrip = trayMenu;
            trayIcon.Text = "COM Port Terminal";
            trayIcon.Visible = true;
            trayIcon.DoubleClick += TrayIcon_DoubleClick;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1017, 524);
            ContextMenuStrip = terminalMenu;
            Controls.Add(terminalTextBox);
            Controls.Add(bottomPanel);
            Controls.Add(statusStrip);
            Controls.Add(topPanel);
            Controls.Add(portComboBox);
            Controls.Add(baudComboBox);
            Controls.Add(topMostCheckBox);
            Controls.Add(closeButton);
            MinimumSize = new Size(320, 240);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "COM Port Terminal";
            FormClosing += MainForm_FormClosing;
            Load += MainForm_Load;
            topPanel.ResumeLayout(false);
            terminalMenu.ResumeLayout(false);
            bottomPanel.ResumeLayout(false);
            bottomPanel.PerformLayout();
            trayMenu.ResumeLayout(false);
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
