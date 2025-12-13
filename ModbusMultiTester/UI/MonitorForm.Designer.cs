namespace ModbusMultiTester.UI
{
	partial class MonitorForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
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
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MonitorForm));
			toolStrip1 = new ToolStrip();
			toolStripLabel1 = new ToolStripLabel();
			toolStripComboBoxKind = new ToolStripComboBox();
			toolStripLabel2 = new ToolStripLabel();
			toolStripLabel3 = new ToolStripLabel();
			toolStripSeparator1 = new ToolStripSeparator();
			toolStripButtonApply = new ToolStripButton();
			dataGridView1 = new DataGridView();
			statusStrip1 = new StatusStrip();
			toolStripStatusLabelStatus = new ToolStripStatusLabel();
			toolStripStatusLabelSetting = new ToolStripStatusLabel();
			toolStrip1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
			statusStrip1.SuspendLayout();
			SuspendLayout();
			// 
			// toolStrip1
			// 
			toolStrip1.GripStyle = ToolStripGripStyle.Hidden;
			toolStrip1.Items.AddRange(new ToolStripItem[] { toolStripLabel1, toolStripComboBoxKind, toolStripLabel2, toolStripLabel3, toolStripSeparator1, toolStripButtonApply });
			toolStrip1.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
			toolStrip1.Location = new Point(0, 0);
			toolStrip1.Name = "toolStrip1";
			toolStrip1.Size = new Size(580, 25);
			toolStrip1.TabIndex = 0;
			toolStrip1.Text = "toolStrip1";
			// 
			// toolStripLabel1
			// 
			toolStripLabel1.Name = "toolStripLabel1";
			toolStripLabel1.Size = new Size(31, 22);
			toolStripLabel1.Text = "種別";
			// 
			// toolStripComboBoxKind
			// 
			toolStripComboBoxKind.DropDownStyle = ComboBoxStyle.DropDownList;
			toolStripComboBoxKind.Name = "toolStripComboBoxKind";
			toolStripComboBoxKind.Size = new Size(90, 25);
			// 
			// toolStripLabel2
			// 
			toolStripLabel2.Name = "toolStripLabel2";
			toolStripLabel2.Size = new Size(66, 22);
			toolStripLabel2.Text = "先頭アドレス";
			// 
			// toolStripLabel3
			// 
			toolStripLabel3.Name = "toolStripLabel3";
			toolStripLabel3.Size = new Size(31, 22);
			toolStripLabel3.Text = "個数";
			// 
			// toolStripSeparator1
			// 
			toolStripSeparator1.Name = "toolStripSeparator1";
			toolStripSeparator1.Size = new Size(6, 25);
			// 
			// toolStripButtonApply
			// 
			toolStripButtonApply.DisplayStyle = ToolStripItemDisplayStyle.Text;
			toolStripButtonApply.Image = (Image)resources.GetObject("toolStripButtonApply.Image");
			toolStripButtonApply.ImageTransparentColor = Color.Magenta;
			toolStripButtonApply.Name = "toolStripButtonApply";
			toolStripButtonApply.Size = new Size(35, 22);
			toolStripButtonApply.Text = "設定";
			toolStripButtonApply.Click += BtnApply_Click;
			// 
			// dataGridView1
			// 
			dataGridView1.BorderStyle = BorderStyle.None;
			dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			dataGridView1.Dock = DockStyle.Fill;
			dataGridView1.Location = new Point(0, 25);
			dataGridView1.Name = "dataGridView1";
			dataGridView1.RowHeadersVisible = false;
			dataGridView1.Size = new Size(580, 403);
			dataGridView1.TabIndex = 1;
			// 
			// statusStrip1
			// 
			statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatusLabelStatus, toolStripStatusLabelSetting });
			statusStrip1.Location = new Point(0, 428);
			statusStrip1.Name = "statusStrip1";
			statusStrip1.Size = new Size(580, 22);
			statusStrip1.TabIndex = 2;
			statusStrip1.Text = "statusStrip1";
			// 
			// toolStripStatusLabelStatus
			// 
			toolStripStatusLabelStatus.Name = "toolStripStatusLabelStatus";
			toolStripStatusLabelStatus.Size = new Size(43, 17);
			toolStripStatusLabelStatus.Text = "停止中";
			// 
			// toolStripStatusLabelSetting
			// 
			toolStripStatusLabelSetting.Name = "toolStripStatusLabelSetting";
			toolStripStatusLabelSetting.Size = new Size(0, 17);
			// 
			// MonitorForm
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(580, 450);
			Controls.Add(dataGridView1);
			Controls.Add(statusStrip1);
			Controls.Add(toolStrip1);
			Name = "MonitorForm";
			ShowIcon = false;
			Text = "MonitorForm";
			toolStrip1.ResumeLayout(false);
			toolStrip1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
			statusStrip1.ResumeLayout(false);
			statusStrip1.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}

		#endregion

		private ToolStrip toolStrip1;
		private ToolStripLabel toolStripLabel1;
		private ToolStripComboBox toolStripComboBoxKind;
		private ToolStripLabel toolStripLabel2;
		private ToolStripLabel toolStripLabel3;
		private DataGridView dataGridView1;
		private ToolStripSeparator toolStripSeparator1;
		private ToolStripButton toolStripButtonApply;
		private StatusStrip statusStrip1;
		private ToolStripStatusLabel toolStripStatusLabelStatus;
		private ToolStripStatusLabel toolStripStatusLabelSetting;
	}
}