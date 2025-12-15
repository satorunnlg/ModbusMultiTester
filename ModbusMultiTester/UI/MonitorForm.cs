using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace ModbusMultiTester.UI
{
	/// <summary>
	/// データ監視用の子ウィンドウクラス。
	/// データバインディングを使用して、高速更新時のカーソル飛びを防ぎます。
	/// </summary>
	public partial class MonitorForm : Form
	{
		// --- 公開イベント ---
		public event Action<ushort, ushort>? DataEdited;

		// --- 公開プロパティ ---
		public bool IsSettingsApplied { get; private set; } = false;

		public int RegisterTypeIndex => toolStripComboBoxKind.SelectedIndex;
		public string RegisterTypeName => toolStripComboBoxKind.SelectedItem?.ToString() ?? "保持レジスタ";

		public ushort CurrentStartAddress { get; private set; }
		public ushort CurrentCount { get; private set; }

		// --- データバインディング用 ---
		// BindingListは項目の追加削除やプロパティ変更を自動でGridに通知します
		private BindingList<MonitorItem> _dataSource;
		private BindingSource _bindingSource;

		// --- UI コントロール ---
		private ToolStripNumericUpDown _nudAddr;
		private ToolStripNumericUpDown _nudCount;

		public MonitorForm(int index)
		{
			InitializeComponent();
			this.Text = $"モニタ #{index} (未設定)";

			// 1. データソースの初期化
			_dataSource = new BindingList<MonitorItem>();
			_bindingSource = new BindingSource();
			_bindingSource.DataSource = _dataSource;

			// 2. DataGridViewの設定
			InitializeDataGridView();

			// 3. ツールバー設定
			SetupCustomControls();

			// 4. イベント登録
			// バインディング使用時は CellValueChanged ではなく、
			// バインド元のデータ変更イベント、または CellEndEdit を使うのが一般的ですが、
			// ここでは簡易的に CellEndEdit (入力確定時) を使用します。
			dataGridView1.CellEndEdit += DataGridView1_CellEndEdit;

			// データエラー（入力値不正など）の抑制
			dataGridView1.DataError += (s, e) => { e.Cancel = true; };

			dataGridView1.ReadOnly = true;
		}

		private void InitializeDataGridView()
		{
			dataGridView1.AutoGenerateColumns = false;
			dataGridView1.DataSource = _bindingSource;

			// --- 列定義 ---

			// 1. アドレス
			var colAddr = new DataGridViewTextBoxColumn();
			colAddr.DataPropertyName = "Address";
			colAddr.HeaderText = "アドレス";
			colAddr.Width = 70;
			colAddr.ReadOnly = true;
			colAddr.DefaultCellStyle.BackColor = Color.WhiteSmoke; // 編集不可色
			colAddr.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

			// 2. 値 (Decimal)
			var colVal = new DataGridViewTextBoxColumn();
			colVal.DataPropertyName = "Value";
			colVal.HeaderText = "10進"; // "値" から変更
			colVal.Width = 70;
			colVal.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

			// 3. Hex
			var colHex = new DataGridViewTextBoxColumn();
			colHex.DataPropertyName = "ValueHex"; // MonitorItemのプロパティ名
			colHex.HeaderText = "16進";
			colHex.Width = 60;
			colHex.DefaultCellStyle.Font = new Font("Consolas", 9); // 等幅フォント推奨
			colHex.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

			// 4. Binary
			var colBin = new DataGridViewTextBoxColumn();
			colBin.DataPropertyName = "ValueBin";
			colBin.HeaderText = "2進";
			colBin.Width = 120;
			colBin.DefaultCellStyle.Font = new Font("Consolas", 9);
			colBin.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

			// 5. ASCII
			var colAscii = new DataGridViewTextBoxColumn();
			colAscii.DataPropertyName = "ValueAscii";
			colAscii.HeaderText = "ASCII";
			colAscii.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill; // 残りを埋める
			colAscii.DefaultCellStyle.Font = new Font("Consolas", 9);

			dataGridView1.Columns.Clear();
			dataGridView1.Columns.AddRange(colAddr, colVal, colHex, colBin, colAscii);

			// 高速描画設定
			typeof(DataGridView).InvokeMember("DoubleBuffered",
				System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty,
				null, dataGridView1, new object[] { true });

			// セルの色変更イベント
			dataGridView1.CellFormatting += DataGridView1_CellFormatting;
		}

		private void DataGridView1_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
		{
			// 値列（10進, 16進, 2進, ASCII）のいずれかの場合に色変更対象
			// 列インデックス: 0=Address, 1=Value(10進), 2=Hex, 3=Bin, 4=ASCII
			if (e.ColumnIndex >= 1 && e.RowIndex >= 0)
			{
				if (dataGridView1.Rows[e.RowIndex].DataBoundItem is MonitorItem item)
				{
					if (item.HasChanged)
					{
						e.CellStyle.BackColor = Color.LightYellow;
						e.CellStyle.SelectionBackColor = Color.Gold;
					}
					else
					{
						// デフォルト色に戻す（アドレス列以外）
						if (e.ColumnIndex != 0)
						{
							e.CellStyle.BackColor = Color.White;
							e.CellStyle.SelectionBackColor = SystemColors.Highlight;
						}
					}
				}
			}
		}

		private void SetupCustomControls()
		{
			if (toolStripComboBoxKind.Items.Count == 0)
			{
				toolStripComboBoxKind.Items.AddRange(new object[] {
					"コイル",
					"離散入力",
					"入力レジスタ",
					"保持レジスタ"
				});
			}
			toolStripComboBoxKind.SelectedIndex = 3;

			_nudAddr = new ToolStripNumericUpDown();
			_nudAddr.NumericUpDownControl.Minimum = 0;
			_nudAddr.NumericUpDownControl.Maximum = 65535;
			_nudAddr.NumericUpDownControl.TextAlign = HorizontalAlignment.Right;
			_nudAddr.Width = 80;
			toolStrip1.Items.Insert(3, _nudAddr);

			_nudCount = new ToolStripNumericUpDown();
			_nudCount.NumericUpDownControl.Minimum = 1;
			_nudCount.NumericUpDownControl.Maximum = 2000;
			_nudCount.NumericUpDownControl.Value = 10;
			_nudCount.NumericUpDownControl.TextAlign = HorizontalAlignment.Right;
			_nudCount.Width = 60;
			toolStrip1.Items.Insert(5, _nudCount);

			toolStripButtonApply.BackColor = Color.LightSkyBlue;
		}

		private void BtnApply_Click(object? sender, EventArgs e)
		{
			ushort addr = (ushort)_nudAddr.NumericUpDownControl.Value;
			ushort count = (ushort)_nudCount.NumericUpDownControl.Value;

			CurrentStartAddress = addr;
			CurrentCount = count;
			IsSettingsApplied = true;

			// データリストの再構築
			InitializeDataSource(addr, count);

			this.Text = $"モニタ";
			toolStripStatusLabelSetting.Text = $"(先頭アドレス={addr}, 個数={count})";
			toolStripStatusLabelStatus.Text = "準備完了";
			toolStripStatusLabelStatus.ForeColor = Color.Black;
		}

		private void InitializeDataSource(ushort startAddr, ushort count)
		{
			// データ更新を一時停止（高速化）
			_dataSource.RaiseListChangedEvents = false;
			_dataSource.Clear();

			for (int i = 0; i < count; i++)
			{
				_dataSource.Add(new MonitorItem((ushort)(startAddr + i), 0));
			}

			_dataSource.RaiseListChangedEvents = true;
			_dataSource.ResetBindings(); // グリッドに反映
		}

		public void EnableGridEditing(bool enable, bool isMasterMode = false)
		{
			if (!IsSettingsApplied)
			{
				dataGridView1.ReadOnly = true;
				return;
			}

			// マスターモードの場合は、レジスタタイプによって編集可否を判断
			if (isMasterMode && enable)
			{
				int typeIdx = RegisterTypeIndex;
				// Coil (0x) と Holding Register (4x) のみ編集可能
				// Discrete Input (1x) と Input Register (3x) は読み取り専用
				bool isWritable = (typeIdx == 0 || typeIdx == 3);
				dataGridView1.ReadOnly = !isWritable;
			}
			else
			{
				// スレーブモードまたは無効化の場合はそのまま
				dataGridView1.ReadOnly = !enable;
			}
			// アドレス列は常にReadOnly (InitializeDataGridViewで設定済み)
		}

		// 入力確定時のイベント (Bindingを使用する場合、こちらの方が適切)
		private void DataGridView1_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
		{
			if (!IsSettingsApplied) return;
			if (e.RowIndex < 0) return;

			// バインドされているデータオブジェクトを取得
			if (dataGridView1.Rows[e.RowIndex].DataBoundItem is MonitorItem item)
			{
				// 値が変わったかどうかに関わらず、編集が終わったら書き込みリクエストを出す
				// (値が同じでも「書き込み」操作自体を行いたい場合があるため)
				DataEdited?.Invoke(item.Address, item.Value);
			}
		}

		public void UpdateResult(ushort[] data, bool isSuccess, string msg = "")
		{
			if (this.IsDisposed) return;

			if (!IsSettingsApplied)
			{
				toolStripStatusLabelStatus.Text = "設定未反映";
				toolStripStatusLabelStatus.ForeColor = Color.Gray;
				return;
			}

			if (isSuccess && data != null)
			{
				toolStripStatusLabelStatus.Text = "モニタ中";
				toolStripStatusLabelStatus.ForeColor = Color.Green;

				// 個数が一致しない場合は再構築
				if (_dataSource.Count != data.Length)
				{
					InitializeDataSource(CurrentStartAddress, (ushort)data.Length);
				}

				// --- 編集中のセルを判定 ---
				bool isEditing = dataGridView1.IsCurrentCellInEditMode;
				int editingRowIndex = -1;
				if (isEditing && dataGridView1.CurrentCell != null)
				{
					editingRowIndex = dataGridView1.CurrentCell.RowIndex;
				}
				// -----------------------

				// データを更新
				// ここでBindingListの中身を書き換えると、INotifyPropertyChanged経由で
				// Gridの該当セルだけが書き換わります（フォーカスは飛びません）。
				for (int i = 0; i < data.Length; i++)
				{
					// 編集中の行は更新しない（入力の邪魔をしない）
					if (isEditing && i == editingRowIndex) continue;

					if (i < _dataSource.Count)
					{
						// MonitorItemのプロパティを更新
						// 値が同じならNotifyされないので無駄な描画も起きない
						_dataSource[i].Value = data[i];

						// アドレスも念のため同期（ずれていなければ変更通知は飛ばない）
						_dataSource[i].Address = (ushort)(CurrentStartAddress + i);
					}
				}
			}
			else
			{
				toolStripStatusLabelStatus.Text = msg;
				toolStripStatusLabelStatus.ForeColor = Color.Red;
			}
		}
	}

	/// <summary>
	/// データバインディング用のデータクラス
	/// INotifyPropertyChangedを実装することで、プロパティ変更をGridに自動通知します。
	/// </summary>
	public class MonitorItem : INotifyPropertyChanged
	{
		private ushort _address;
		private ushort _value;
		private ushort _previousValue;
		private bool _hasChanged;

		public event PropertyChangedEventHandler? PropertyChanged;

		public MonitorItem(ushort address, ushort value)
		{
			_address = address;
			_value = value;
			_previousValue = value;
			_hasChanged = false;
		}

		// アドレス (読み取り専用想定だがBindingのためsetも用意)
		public ushort Address
		{
			get => _address;
			set
			{
				if (_address != value)
				{
					_address = value;
					OnPropertyChanged(nameof(Address));
				}
			}
		}

		// 元の値 (ushort)
		public ushort Value
		{
			get => _value;
			set
			{
				if (_value != value)
				{
					_previousValue = _value;
					_value = value;
					_hasChanged = true;
					// 全プロパティの変更を通知してグリッドを更新させる
					OnPropertyChanged(nameof(Value));
					OnPropertyChanged(nameof(ValueHex));
					OnPropertyChanged(nameof(ValueBin));
					OnPropertyChanged(nameof(ValueAscii));
					OnPropertyChanged(nameof(HasChanged));
				}
			}
		}

		// 16進数表示 (例: FFFF)
		public string ValueHex
		{
			get => _value.ToString("X4");
			set
			{
				// 入力された16進文字列をパースしてValueにセット
				if (ushort.TryParse(value, System.Globalization.NumberStyles.HexNumber, null, out ushort res))
				{
					Value = res;
				}
			}
		}

		// 2進数表示 (例: 1111000011110000)
		public string ValueBin
		{
			get => Convert.ToString(_value, 2).PadLeft(16, '0');
			set
			{
				try
				{
					// 入力された2進文字列をパース
					// 空白除去などのサニタイズを入れても良い
					string clean = value.Replace(" ", "");
					Value = Convert.ToUInt16(clean, 2);
				}
				catch { /* 変換失敗時は無視、またはエラー通知 */ }
			}
		}

		// ASCII表示 (2文字分)
		public string ValueAscii
		{
			get
			{
				// 上位バイト・下位バイトを文字に変換 (非表示文字は '.' に置換などの工夫も可)
				byte high = (byte)(_value >> 8);
				byte low = (byte)(_value & 0xFF);

				// 簡易的な表示 (制御文字対策等は必要に応じて追加)
				char c1 = (high >= 32 && high <= 126) ? (char)high : '.';
				char c2 = (low >= 32 && low <= 126) ? (char)low : '.';

				return $"{c1}{c2}";
			}
			set
			{
				if (string.IsNullOrEmpty(value)) return;

				// 入力文字から値を生成 (最大2文字まで有効とする)
				byte high = 0;
				byte low = 0;

				if (value.Length > 0) high = (byte)value[0];
				if (value.Length > 1) low = (byte)value[1];

				Value = (ushort)((high << 8) | low);
			}
		}

		public ushort PreviousValue
		{
			get => _previousValue;
		}

		public bool HasChanged
		{
			get => _hasChanged;
			set
			{
				if (_hasChanged != value)
				{
					_hasChanged = value;
					OnPropertyChanged(nameof(HasChanged));
				}
			}
		}

		public void ResetChanged()
		{
			if (_hasChanged)
			{
				_hasChanged = false;
				OnPropertyChanged(nameof(HasChanged));
			}
		}

		protected void OnPropertyChanged(string name)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
	}

	// (NumericUpDownの定義は変更なしのため省略。以前のまま記述してください)
	public class ToolStripNumericUpDown : ToolStripControlHost
	{
		public ToolStripNumericUpDown() : base(new NumericUpDown()) { }
		public NumericUpDown NumericUpDownControl => (NumericUpDown)Control;
	}
}