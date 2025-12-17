namespace ModbusMultiTester
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

		#region Windows Form Designer generated code

		/// <summary>
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			tableLayoutPanel1 = new TableLayoutPanel();
			groupBoxMode = new GroupBox();
			tableLayoutPanel4 = new TableLayoutPanel();
			radioButtonMaster = new RadioButton();
			radioButtonSlave = new RadioButton();
			groupBox2 = new GroupBox();
			tableLayoutPanel5 = new TableLayoutPanel();
			comboBoxSrcIP = new ComboBox();
			buttonNicRefresh = new Button();
			groupBoxSetting = new GroupBox();
			panelSlave = new Panel();
			tableLayoutPanel3 = new TableLayoutPanel();
			label5 = new Label();
			label6 = new Label();
			numericUpDown1 = new NumericUpDown();
			numericUpDown2 = new NumericUpDown();
			buttonListen = new Button();
			panelMaster = new Panel();
			tableLayoutPanel2 = new TableLayoutPanel();
			label1 = new Label();
			label2 = new Label();
			label3 = new Label();
			label4 = new Label();
			numericUpDownPort = new NumericUpDown();
			numericUpDownSlaveID = new NumericUpDown();
			numericUpDownInterval = new NumericUpDown();
			buttonConnect = new Button();
			ipAddressInputDest = new ModbusMultiTester.UI.IpAddressInput();
			label7 = new Label();
			numericUpDownTimeout = new NumericUpDown();
			checkBoxOneShot = new CheckBox();
			toolStrip1 = new ToolStrip();
			toolStripButtonAddPanel = new ToolStripButton();
			toolStripLabel1 = new ToolStripLabel();
			toolStripButtonInfo = new ToolStripButton();
			toolStripSeparator1 = new ToolStripSeparator();
			toolStripButton2 = new ToolStripButton();
			toolStripButton3 = new ToolStripButton();
			statusStrip1 = new StatusStrip();
			toolStripProgressBarStatus = new ToolStripProgressBar();
			tableLayoutPanel1.SuspendLayout();
			groupBoxMode.SuspendLayout();
			tableLayoutPanel4.SuspendLayout();
			groupBox2.SuspendLayout();
			tableLayoutPanel5.SuspendLayout();
			groupBoxSetting.SuspendLayout();
			panelSlave.SuspendLayout();
			tableLayoutPanel3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)numericUpDown1).BeginInit();
			((System.ComponentModel.ISupportInitialize)numericUpDown2).BeginInit();
			panelMaster.SuspendLayout();
			tableLayoutPanel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)numericUpDownPort).BeginInit();
			((System.ComponentModel.ISupportInitialize)numericUpDownSlaveID).BeginInit();
			((System.ComponentModel.ISupportInitialize)numericUpDownInterval).BeginInit();
			((System.ComponentModel.ISupportInitialize)numericUpDownTimeout).BeginInit();
			toolStrip1.SuspendLayout();
			statusStrip1.SuspendLayout();
			SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			tableLayoutPanel1.ColumnCount = 2;
			tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
			tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
			tableLayoutPanel1.Controls.Add(groupBoxMode, 0, 0);
			tableLayoutPanel1.Controls.Add(groupBox2, 1, 0);
			tableLayoutPanel1.Controls.Add(groupBoxSetting, 0, 1);
			tableLayoutPanel1.Dock = DockStyle.Top;
			tableLayoutPanel1.Location = new Point(0, 0);
			tableLayoutPanel1.Margin = new Padding(0);
			tableLayoutPanel1.Name = "tableLayoutPanel1";
			tableLayoutPanel1.RowCount = 2;
			tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 55F));
			tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			tableLayoutPanel1.Size = new Size(1099, 173);
			tableLayoutPanel1.TabIndex = 0;
			// 
			// groupBoxMode
			// 
			groupBoxMode.Controls.Add(tableLayoutPanel4);
			groupBoxMode.Dock = DockStyle.Fill;
			groupBoxMode.Location = new Point(3, 3);
			groupBoxMode.Name = "groupBoxMode";
			groupBoxMode.Size = new Size(543, 49);
			groupBoxMode.TabIndex = 2;
			groupBoxMode.TabStop = false;
			groupBoxMode.Text = "モード";
			// 
			// tableLayoutPanel4
			// 
			tableLayoutPanel4.ColumnCount = 2;
			tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
			tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
			tableLayoutPanel4.Controls.Add(radioButtonMaster, 0, 0);
			tableLayoutPanel4.Controls.Add(radioButtonSlave, 1, 0);
			tableLayoutPanel4.Dock = DockStyle.Fill;
			tableLayoutPanel4.Location = new Point(3, 19);
			tableLayoutPanel4.Name = "tableLayoutPanel4";
			tableLayoutPanel4.RowCount = 1;
			tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
			tableLayoutPanel4.Size = new Size(537, 27);
			tableLayoutPanel4.TabIndex = 2;
			// 
			// radioButtonMaster
			// 
			radioButtonMaster.AutoSize = true;
			radioButtonMaster.Checked = true;
			radioButtonMaster.Dock = DockStyle.Fill;
			radioButtonMaster.Location = new Point(3, 3);
			radioButtonMaster.Name = "radioButtonMaster";
			radioButtonMaster.Padding = new Padding(100, 0, 0, 0);
			radioButtonMaster.Size = new Size(262, 21);
			radioButtonMaster.TabIndex = 0;
			radioButtonMaster.TabStop = true;
			radioButtonMaster.Text = "マスター";
			radioButtonMaster.UseVisualStyleBackColor = true;
			radioButtonMaster.CheckedChanged += mode_CheckedChanged;
			// 
			// radioButtonSlave
			// 
			radioButtonSlave.AutoSize = true;
			radioButtonSlave.Dock = DockStyle.Fill;
			radioButtonSlave.Location = new Point(271, 3);
			radioButtonSlave.Name = "radioButtonSlave";
			radioButtonSlave.Padding = new Padding(100, 0, 0, 0);
			radioButtonSlave.Size = new Size(263, 21);
			radioButtonSlave.TabIndex = 1;
			radioButtonSlave.Text = "スレーブ";
			radioButtonSlave.UseVisualStyleBackColor = true;
			radioButtonSlave.CheckedChanged += mode_CheckedChanged;
			// 
			// groupBox2
			// 
			groupBox2.Controls.Add(tableLayoutPanel5);
			groupBox2.Dock = DockStyle.Fill;
			groupBox2.Location = new Point(552, 3);
			groupBox2.Name = "groupBox2";
			groupBox2.Size = new Size(544, 49);
			groupBox2.TabIndex = 4;
			groupBox2.TabStop = false;
			groupBox2.Text = "ソースIP";
			// 
			// tableLayoutPanel5
			// 
			tableLayoutPanel5.ColumnCount = 2;
			tableLayoutPanel5.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			tableLayoutPanel5.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60F));
			tableLayoutPanel5.Controls.Add(comboBoxSrcIP, 0, 0);
			tableLayoutPanel5.Controls.Add(buttonNicRefresh, 1, 0);
			tableLayoutPanel5.Dock = DockStyle.Fill;
			tableLayoutPanel5.Location = new Point(3, 19);
			tableLayoutPanel5.Name = "tableLayoutPanel5";
			tableLayoutPanel5.RowCount = 1;
			tableLayoutPanel5.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			tableLayoutPanel5.Size = new Size(538, 27);
			tableLayoutPanel5.TabIndex = 1;
			// 
			// comboBoxSrcIP
			// 
			comboBoxSrcIP.Dock = DockStyle.Fill;
			comboBoxSrcIP.DropDownStyle = ComboBoxStyle.DropDownList;
			comboBoxSrcIP.FormattingEnabled = true;
			comboBoxSrcIP.Location = new Point(3, 2);
			comboBoxSrcIP.Margin = new Padding(3, 2, 3, 3);
			comboBoxSrcIP.Name = "comboBoxSrcIP";
			comboBoxSrcIP.Size = new Size(472, 23);
			comboBoxSrcIP.TabIndex = 0;
			// 
			// buttonNicRefresh
			// 
			buttonNicRefresh.Dock = DockStyle.Fill;
			buttonNicRefresh.Location = new Point(481, 2);
			buttonNicRefresh.Margin = new Padding(3, 2, 3, 2);
			buttonNicRefresh.Name = "buttonNicRefresh";
			buttonNicRefresh.Size = new Size(54, 23);
			buttonNicRefresh.TabIndex = 1;
			buttonNicRefresh.Text = "更新";
			buttonNicRefresh.UseVisualStyleBackColor = true;
			buttonNicRefresh.Click += buttonNicRefresh_Click;
			// 
			// groupBoxSetting
			// 
			tableLayoutPanel1.SetColumnSpan(groupBoxSetting, 2);
			groupBoxSetting.Controls.Add(panelSlave);
			groupBoxSetting.Controls.Add(panelMaster);
			groupBoxSetting.Dock = DockStyle.Fill;
			groupBoxSetting.Location = new Point(3, 58);
			groupBoxSetting.Name = "groupBoxSetting";
			groupBoxSetting.Size = new Size(1093, 112);
			groupBoxSetting.TabIndex = 5;
			groupBoxSetting.TabStop = false;
			groupBoxSetting.Text = "設定";
			// 
			// panelSlave
			// 
			panelSlave.Controls.Add(tableLayoutPanel3);
			panelSlave.Location = new Point(642, 22);
			panelSlave.Name = "panelSlave";
			panelSlave.Size = new Size(445, 90);
			panelSlave.TabIndex = 1;
			// 
			// tableLayoutPanel3
			// 
			tableLayoutPanel3.ColumnCount = 5;
			tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
			tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
			tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
			tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
			tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90F));
			tableLayoutPanel3.Controls.Add(label5, 0, 0);
			tableLayoutPanel3.Controls.Add(label6, 2, 0);
			tableLayoutPanel3.Controls.Add(numericUpDown1, 1, 0);
			tableLayoutPanel3.Controls.Add(numericUpDown2, 3, 0);
			tableLayoutPanel3.Controls.Add(buttonListen, 4, 0);
			tableLayoutPanel3.Dock = DockStyle.Fill;
			tableLayoutPanel3.Location = new Point(0, 0);
			tableLayoutPanel3.Margin = new Padding(0);
			tableLayoutPanel3.Name = "tableLayoutPanel3";
			tableLayoutPanel3.RowCount = 3;
			tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
			tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
			tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			tableLayoutPanel3.Size = new Size(445, 90);
			tableLayoutPanel3.TabIndex = 0;
			// 
			// label5
			// 
			label5.AutoSize = true;
			label5.Dock = DockStyle.Fill;
			label5.Location = new Point(3, 0);
			label5.Name = "label5";
			label5.Size = new Size(94, 30);
			label5.TabIndex = 1;
			label5.Text = "ポート";
			label5.TextAlign = ContentAlignment.MiddleRight;
			// 
			// label6
			// 
			label6.AutoSize = true;
			label6.Dock = DockStyle.Fill;
			label6.Location = new Point(180, 0);
			label6.Name = "label6";
			label6.Size = new Size(94, 30);
			label6.TabIndex = 1;
			label6.Text = "ユニットID";
			label6.TextAlign = ContentAlignment.MiddleRight;
			// 
			// numericUpDown1
			// 
			numericUpDown1.BorderStyle = BorderStyle.FixedSingle;
			numericUpDown1.Dock = DockStyle.Fill;
			numericUpDown1.Location = new Point(103, 3);
			numericUpDown1.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
			numericUpDown1.Name = "numericUpDown1";
			numericUpDown1.Size = new Size(71, 23);
			numericUpDown1.TabIndex = 3;
			numericUpDown1.TextAlign = HorizontalAlignment.Right;
			numericUpDown1.Value = new decimal(new int[] { 502, 0, 0, 0 });
			// 
			// numericUpDown2
			// 
			numericUpDown2.BorderStyle = BorderStyle.FixedSingle;
			numericUpDown2.Dock = DockStyle.Fill;
			numericUpDown2.Location = new Point(280, 3);
			numericUpDown2.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
			numericUpDown2.Name = "numericUpDown2";
			numericUpDown2.Size = new Size(71, 23);
			numericUpDown2.TabIndex = 3;
			numericUpDown2.TextAlign = HorizontalAlignment.Right;
			numericUpDown2.Value = new decimal(new int[] { 1, 0, 0, 0 });
			// 
			// buttonListen
			// 
			buttonListen.Dock = DockStyle.Fill;
			buttonListen.Location = new Point(357, 3);
			buttonListen.Name = "buttonListen";
			tableLayoutPanel3.SetRowSpan(buttonListen, 3);
			buttonListen.Size = new Size(85, 84);
			buttonListen.TabIndex = 4;
			buttonListen.Text = "待受\r\n開始";
			buttonListen.UseVisualStyleBackColor = true;
			buttonListen.Click += buttonListen_Click;
			// 
			// panelMaster
			// 
			panelMaster.Controls.Add(tableLayoutPanel2);
			panelMaster.Location = new Point(9, 22);
			panelMaster.Name = "panelMaster";
			panelMaster.Size = new Size(627, 90);
			panelMaster.TabIndex = 0;
			// 
			// tableLayoutPanel2
			// 
			tableLayoutPanel2.ColumnCount = 5;
			tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
			tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
			tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
			tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
			tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90F));
			tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
			tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20F));
			tableLayoutPanel2.Controls.Add(label1, 0, 0);
			tableLayoutPanel2.Controls.Add(label2, 2, 0);
			tableLayoutPanel2.Controls.Add(label3, 2, 1);
			tableLayoutPanel2.Controls.Add(label4, 0, 1);
			tableLayoutPanel2.Controls.Add(numericUpDownPort, 3, 0);
			tableLayoutPanel2.Controls.Add(numericUpDownSlaveID, 1, 1);
			tableLayoutPanel2.Controls.Add(numericUpDownInterval, 3, 1);
			tableLayoutPanel2.Controls.Add(buttonConnect, 4, 0);
			tableLayoutPanel2.Controls.Add(ipAddressInputDest, 1, 0);
			tableLayoutPanel2.Controls.Add(label7, 0, 2);
			tableLayoutPanel2.Controls.Add(numericUpDownTimeout, 1, 2);
			tableLayoutPanel2.Controls.Add(checkBoxOneShot, 3, 2);
			tableLayoutPanel2.Dock = DockStyle.Fill;
			tableLayoutPanel2.Location = new Point(0, 0);
			tableLayoutPanel2.Margin = new Padding(0);
			tableLayoutPanel2.Name = "tableLayoutPanel2";
			tableLayoutPanel2.RowCount = 3;
			tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
			tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
			tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
			tableLayoutPanel2.Size = new Size(627, 90);
			tableLayoutPanel2.TabIndex = 0;
			// 
			// label1
			// 
			label1.AutoSize = true;
			label1.Dock = DockStyle.Fill;
			label1.Location = new Point(3, 0);
			label1.Name = "label1";
			label1.Size = new Size(94, 30);
			label1.TabIndex = 0;
			label1.Text = "接続先IP";
			label1.TextAlign = ContentAlignment.MiddleRight;
			// 
			// label2
			// 
			label2.AutoSize = true;
			label2.Dock = DockStyle.Fill;
			label2.Location = new Point(271, 0);
			label2.Name = "label2";
			label2.Size = new Size(94, 30);
			label2.TabIndex = 1;
			label2.Text = "ポート";
			label2.TextAlign = ContentAlignment.MiddleRight;
			// 
			// label3
			// 
			label3.AutoSize = true;
			label3.Dock = DockStyle.Fill;
			label3.Location = new Point(271, 30);
			label3.Name = "label3";
			label3.Size = new Size(94, 30);
			label3.TabIndex = 1;
			label3.Text = "インターバル";
			label3.TextAlign = ContentAlignment.MiddleRight;
			// 
			// label4
			// 
			label4.AutoSize = true;
			label4.Dock = DockStyle.Fill;
			label4.Location = new Point(3, 30);
			label4.Name = "label4";
			label4.Size = new Size(94, 30);
			label4.TabIndex = 1;
			label4.Text = "スレーブID";
			label4.TextAlign = ContentAlignment.MiddleRight;
			// 
			// numericUpDownPort
			// 
			numericUpDownPort.BorderStyle = BorderStyle.FixedSingle;
			numericUpDownPort.Dock = DockStyle.Fill;
			numericUpDownPort.Location = new Point(371, 3);
			numericUpDownPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
			numericUpDownPort.Name = "numericUpDownPort";
			numericUpDownPort.Size = new Size(162, 23);
			numericUpDownPort.TabIndex = 3;
			numericUpDownPort.TextAlign = HorizontalAlignment.Right;
			numericUpDownPort.Value = new decimal(new int[] { 502, 0, 0, 0 });
			// 
			// numericUpDownSlaveID
			// 
			numericUpDownSlaveID.BorderStyle = BorderStyle.FixedSingle;
			numericUpDownSlaveID.Dock = DockStyle.Fill;
			numericUpDownSlaveID.Location = new Point(103, 33);
			numericUpDownSlaveID.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
			numericUpDownSlaveID.Name = "numericUpDownSlaveID";
			numericUpDownSlaveID.Size = new Size(162, 23);
			numericUpDownSlaveID.TabIndex = 4;
			numericUpDownSlaveID.TextAlign = HorizontalAlignment.Right;
			numericUpDownSlaveID.Value = new decimal(new int[] { 1, 0, 0, 0 });
			// 
			// numericUpDownInterval
			// 
			numericUpDownInterval.BorderStyle = BorderStyle.FixedSingle;
			numericUpDownInterval.Dock = DockStyle.Fill;
			numericUpDownInterval.Location = new Point(371, 33);
			numericUpDownInterval.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
			numericUpDownInterval.Name = "numericUpDownInterval";
			numericUpDownInterval.Size = new Size(162, 23);
			numericUpDownInterval.TabIndex = 5;
			numericUpDownInterval.TextAlign = HorizontalAlignment.Right;
			numericUpDownInterval.Value = new decimal(new int[] { 100, 0, 0, 0 });
			// 
			// buttonConnect
			// 
			buttonConnect.Dock = DockStyle.Fill;
			buttonConnect.Location = new Point(539, 3);
			buttonConnect.Name = "buttonConnect";
			tableLayoutPanel2.SetRowSpan(buttonConnect, 3);
			buttonConnect.Size = new Size(85, 84);
			buttonConnect.TabIndex = 6;
			buttonConnect.Text = "接続";
			buttonConnect.UseVisualStyleBackColor = true;
			buttonConnect.Click += buttonConnect_Click;
			// 
			// ipAddressInputDest
			// 
			ipAddressInputDest.BackColor = SystemColors.Window;
			ipAddressInputDest.BorderStyle = BorderStyle.FixedSingle;
			ipAddressInputDest.Dock = DockStyle.Fill;
			ipAddressInputDest.Location = new Point(103, 3);
			ipAddressInputDest.Name = "ipAddressInputDest";
			ipAddressInputDest.Padding = new Padding(1);
			ipAddressInputDest.Size = new Size(162, 20);
			ipAddressInputDest.TabIndex = 7;
			ipAddressInputDest.Text = "127.0.0.1";
			// 
			// label7
			// 
			label7.AutoSize = true;
			label7.Dock = DockStyle.Fill;
			label7.Location = new Point(3, 60);
			label7.Name = "label7";
			label7.Size = new Size(94, 30);
			label7.TabIndex = 1;
			label7.Text = "タイムアウト";
			label7.TextAlign = ContentAlignment.MiddleRight;
			// 
			// numericUpDownTimeout
			// 
			numericUpDownTimeout.BorderStyle = BorderStyle.FixedSingle;
			numericUpDownTimeout.Dock = DockStyle.Fill;
			numericUpDownTimeout.Location = new Point(103, 63);
			numericUpDownTimeout.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
			numericUpDownTimeout.Name = "numericUpDownTimeout";
			numericUpDownTimeout.Size = new Size(162, 23);
			numericUpDownTimeout.TabIndex = 4;
			numericUpDownTimeout.TextAlign = HorizontalAlignment.Right;
			numericUpDownTimeout.Value = new decimal(new int[] { 5000, 0, 0, 0 });
			// 
			// checkBoxOneShot
			// 
			checkBoxOneShot.AutoSize = true;
			checkBoxOneShot.Checked = true;
			checkBoxOneShot.CheckState = CheckState.Checked;
			checkBoxOneShot.Dock = DockStyle.Fill;
			checkBoxOneShot.Location = new Point(371, 63);
			checkBoxOneShot.Name = "checkBoxOneShot";
			checkBoxOneShot.Size = new Size(162, 24);
			checkBoxOneShot.TabIndex = 8;
			checkBoxOneShot.Text = "ワンショットモード";
			checkBoxOneShot.UseVisualStyleBackColor = true;
			// 
			// toolStrip1
			// 
			toolStrip1.CanOverflow = false;
			toolStrip1.GripMargin = new Padding(0);
			toolStrip1.GripStyle = ToolStripGripStyle.Hidden;
			toolStrip1.Items.AddRange(new ToolStripItem[] { toolStripButtonAddPanel, toolStripLabel1, toolStripButtonInfo, toolStripSeparator1, toolStripButton2, toolStripButton3 });
			toolStrip1.Location = new Point(0, 173);
			toolStrip1.Name = "toolStrip1";
			toolStrip1.RenderMode = ToolStripRenderMode.System;
			toolStrip1.Size = new Size(1099, 25);
			toolStrip1.TabIndex = 2;
			toolStrip1.Text = "toolStrip1";
			// 
			// toolStripButtonAddPanel
			// 
			toolStripButtonAddPanel.DisplayStyle = ToolStripItemDisplayStyle.Text;
			toolStripButtonAddPanel.Image = (Image)resources.GetObject("toolStripButtonAddPanel.Image");
			toolStripButtonAddPanel.ImageTransparentColor = Color.Magenta;
			toolStripButtonAddPanel.Name = "toolStripButtonAddPanel";
			toolStripButtonAddPanel.Size = new Size(104, 22);
			toolStripButtonAddPanel.Text = "＋モニタパネル追加";
			toolStripButtonAddPanel.Click += toolStripButtonAddPanel_Click;
			// 
			// toolStripLabel1
			// 
			toolStripLabel1.Name = "toolStripLabel1";
			toolStripLabel1.Size = new Size(0, 22);
			// 
			// toolStripButtonInfo
			// 
			toolStripButtonInfo.Alignment = ToolStripItemAlignment.Right;
			toolStripButtonInfo.DisplayStyle = ToolStripItemDisplayStyle.Text;
			toolStripButtonInfo.Image = (Image)resources.GetObject("toolStripButtonInfo.Image");
			toolStripButtonInfo.ImageTransparentColor = Color.Magenta;
			toolStripButtonInfo.Name = "toolStripButtonInfo";
			toolStripButtonInfo.Size = new Size(60, 22);
			toolStripButtonInfo.Text = "ソフト情報";
			toolStripButtonInfo.Click += toolStripButtonInfo_Click;
			// 
			// toolStripSeparator1
			// 
			toolStripSeparator1.Alignment = ToolStripItemAlignment.Right;
			toolStripSeparator1.Name = "toolStripSeparator1";
			toolStripSeparator1.Size = new Size(6, 25);
			// 
			// toolStripButton2
			// 
			toolStripButton2.Alignment = ToolStripItemAlignment.Right;
			toolStripButton2.DisplayStyle = ToolStripItemDisplayStyle.Text;
			toolStripButton2.Image = (Image)resources.GetObject("toolStripButton2.Image");
			toolStripButton2.ImageTransparentColor = Color.Magenta;
			toolStripButton2.Name = "toolStripButton2";
			toolStripButton2.Size = new Size(56, 22);
			toolStripButton2.Text = "横に整列";
			toolStripButton2.Click += toolStripButton2_Click;
			// 
			// toolStripButton3
			// 
			toolStripButton3.Alignment = ToolStripItemAlignment.Right;
			toolStripButton3.DisplayStyle = ToolStripItemDisplayStyle.Text;
			toolStripButton3.Image = (Image)resources.GetObject("toolStripButton3.Image");
			toolStripButton3.ImageTransparentColor = Color.Magenta;
			toolStripButton3.Name = "toolStripButton3";
			toolStripButton3.Size = new Size(56, 22);
			toolStripButton3.Text = "縦に整列";
			toolStripButton3.Click += toolStripButton3_Click;
			// 
			// statusStrip1
			// 
			statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripProgressBarStatus });
			statusStrip1.Location = new Point(0, 737);
			statusStrip1.Name = "statusStrip1";
			statusStrip1.Size = new Size(1099, 22);
			statusStrip1.TabIndex = 4;
			statusStrip1.Text = "statusStrip1";
			// 
			// toolStripProgressBarStatus
			// 
			toolStripProgressBarStatus.Name = "toolStripProgressBarStatus";
			toolStripProgressBarStatus.Size = new Size(100, 16);
			// 
			// MainForm
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(1099, 759);
			Controls.Add(statusStrip1);
			Controls.Add(toolStrip1);
			Controls.Add(tableLayoutPanel1);
			IsMdiContainer = true;
			Name = "MainForm";
			Text = "ModbusMultiTester";
			Load += MainForm_Load;
			Resize += MainForm_Resize;
			tableLayoutPanel1.ResumeLayout(false);
			groupBoxMode.ResumeLayout(false);
			tableLayoutPanel4.ResumeLayout(false);
			tableLayoutPanel4.PerformLayout();
			groupBox2.ResumeLayout(false);
			tableLayoutPanel5.ResumeLayout(false);
			groupBoxSetting.ResumeLayout(false);
			panelSlave.ResumeLayout(false);
			tableLayoutPanel3.ResumeLayout(false);
			tableLayoutPanel3.PerformLayout();
			((System.ComponentModel.ISupportInitialize)numericUpDown1).EndInit();
			((System.ComponentModel.ISupportInitialize)numericUpDown2).EndInit();
			panelMaster.ResumeLayout(false);
			tableLayoutPanel2.ResumeLayout(false);
			tableLayoutPanel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)numericUpDownPort).EndInit();
			((System.ComponentModel.ISupportInitialize)numericUpDownSlaveID).EndInit();
			((System.ComponentModel.ISupportInitialize)numericUpDownInterval).EndInit();
			((System.ComponentModel.ISupportInitialize)numericUpDownTimeout).EndInit();
			toolStrip1.ResumeLayout(false);
			toolStrip1.PerformLayout();
			statusStrip1.ResumeLayout(false);
			statusStrip1.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private TableLayoutPanel tableLayoutPanel1;
		private GroupBox groupBoxMode;
		private RadioButton radioButtonSlave;
		private RadioButton radioButtonMaster;
		private GroupBox groupBox2;
		private ComboBox comboBoxSrcIP;
		private GroupBox groupBoxSetting;
		private Panel panelSlave;
		private Panel panelMaster;
		private TableLayoutPanel tableLayoutPanel2;
		private Label label1;
		private Label label5;
		private Label label2;
		private Label label3;
		private Label label4;
		private NumericUpDown numericUpDownPort;
		private NumericUpDown numericUpDownSlaveID;
		private NumericUpDown numericUpDownInterval;
		private TableLayoutPanel tableLayoutPanel3;
		private Label label6;
		private NumericUpDown numericUpDown1;
		private NumericUpDown numericUpDown2;
		private Button buttonConnect;
		private Button buttonListen;
		private ToolStrip toolStrip1;
		private ToolStripButton toolStripButtonAddPanel;
		private ToolStripLabel toolStripLabel1;
		private ToolStripButton toolStripButton2;
		private ToolStripButton toolStripButton3;
		private TableLayoutPanel tableLayoutPanel4;
		private UI.IpAddressInput ipAddressInputDest;
		private Label label7;
		private NumericUpDown numericUpDownTimeout;
		private Button button1;
		private CheckBox checkBoxOneShot;
		private TableLayoutPanel tableLayoutPanel5;
		private Button buttonNicRefresh;
		private ToolStripButton toolStripButtonInfo;
		private ToolStripSeparator toolStripSeparator1;
		private StatusStrip statusStrip1;
		private ToolStripProgressBar toolStripProgressBarStatus;
	}
}
