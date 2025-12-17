using ModbusMultiTester.Core;
using ModbusMultiTester.UI;
using NModbus;
using NModbus.Device;
using NModbus.IO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ModbusMultiTester
{
	/// <summary>
	/// メインフォームクラス。
	/// Modbus TCPのMaster/Slave機能の管理、通信設定、MDI子ウィンドウの登録を行います。
	/// </summary>
	public partial class MainForm : Form
	{
		// --- Master用メンバー ---
		private TcpClient? _masterClient;
		private IModbusMaster? _modbusMaster;
		private System.Windows.Forms.Timer _pollTimer;

		// --- Slave用メンバー ---
		private TcpListener? _slaveListener;
		private IModbusSlaveNetwork? _slaveNetwork;
		private IModbusSlave? _mySlave;

		// --- UIレイアウト用 ---
		private MdiClient? _mdiClient;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public MainForm()
		{
			InitializeComponent();

			// ロガー機能の開始
			AppLogger.Start();
			AppLogger.Info("Application Started.");

			// ネットワークの初期化
			InitializeNetworking();
			SetupTimer();
			PanelChange(); // UIの初期表示状態を設定

			// MDIコンテナの背景色設定
			foreach (Control ctrl in this.Controls)
			{
				if (ctrl is MdiClient mdiClient)
				{
					mdiClient.BackColor = Color.FromArgb(240, 240, 240);
					break;
				}
			}
		}

		/// <summary>
		/// フォームロード時の処理。
		/// MdiClientの取得とレイアウト調整、初期子ウィンドウの表示を行います。
		/// </summary>
		private void MainForm_Load(object sender, EventArgs e)
		{
			foreach (Control c in this.Controls)
			{
				if (c is MdiClient client)
				{
					_mdiClient = client;
					_mdiClient.BackColor = Color.FromArgb(240, 240, 240);
					break;
				}
			}

			PanelChange();
			AddMonitorWindow();
		}

		/// <summary>
		/// レイアウト変更イベントのオーバーライド。
		/// MDI領域の位置とサイズを動的に調整します。
		/// </summary>
		protected override void OnLayout(LayoutEventArgs levent)
		{
			base.OnLayout(levent);

			if (_mdiClient != null)
			{
				int topOffset = toolStrip1.Bottom;
				var newRect = new Rectangle(
					0,
					topOffset,
					this.ClientRectangle.Width,
					this.ClientRectangle.Height - topOffset
				);

				if (_mdiClient.Bounds != newRect)
				{
					_mdiClient.Bounds = newRect;
				}
			}
		}

		/// <summary>
		/// フォームリサイズ時の処理
		/// </summary>
		private void MainForm_Resize(object sender, EventArgs e)
		{
			AdjustMdiLayout();
		}

		private void AdjustMdiLayout()
		{
			if (_mdiClient == null) return;
			int topOffset = toolStrip1.Bottom;
			_mdiClient.Location = new Point(0, topOffset);
			_mdiClient.Width = this.ClientRectangle.Width;
			_mdiClient.Height = this.ClientRectangle.Height - topOffset;
		}

		/// <summary>
		/// NIC一覧の初期化
		/// </summary>
		private void InitializeNetworking()
		{
			comboBoxSrcIP.Items.Clear();
			comboBoxSrcIP.Items.Add(new NicOption
			{
				DisplayName = "指定なし(OS選択)",
				Ip = IPAddress.Any
			});

			try
			{
				foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
				{
					if (nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
					{
						var props = nic.GetIPProperties();
						foreach (var ip in props.UnicastAddresses)
						{
							if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
							{
								comboBoxSrcIP.Items.Add(new NicOption
								{
									DisplayName = $"{ip.Address} - {nic.Name}",
									Ip = ip.Address
								});
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				AppLogger.Error($"NIC Scan Error: {ex.Message}");
			}

			comboBoxSrcIP.Items.Add(new NicOption
			{
				DisplayName = "127.0.0.1 - Localhost",
				Ip = IPAddress.Loopback
			});

			if (comboBoxSrcIP.Items.Count > 0) comboBoxSrcIP.SelectedIndex = 0;
		}

		private void SetupTimer()
		{
			_pollTimer = new System.Windows.Forms.Timer();
			_pollTimer.Interval = (int)numericUpDownInterval.Value;
			_pollTimer.Tick += PollTimer_Tick;
		}

		// --- UIイベントハンドラー ---

		private void toolStripButtonAddPanel_Click(object sender, EventArgs e)
		{
			AddMonitorWindow();
		}

		private void AddMonitorWindow()
		{
			var child = new MonitorForm(this.MdiChildren.Length + 1);
			child.MdiParent = this;

			// データの編集(書き込み)イベント
			child.DataEdited += (addr, val) =>
			{
				// --- SLAVE MODE (受信) ---
				if (radioButtonSlave.Checked && _mySlave != null)
				{
					/* ... 省略：Slave側処理 ... */
					try
					{
						int typeIdx = child.RegisterTypeIndex;
						switch (typeIdx)
						{
							case 0: _mySlave.DataStore.CoilDiscretes.WritePoints(addr, new bool[] { val != 0 }); break;
							case 1: _mySlave.DataStore.CoilInputs.WritePoints(addr, new bool[] { val != 0 }); break;
							case 2: _mySlave.DataStore.InputRegisters.WritePoints(addr, new ushort[] { val }); break;
							case 3: _mySlave.DataStore.HoldingRegisters.WritePoints(addr, new ushort[] { val }); break;
						}
					}
					catch (Exception ex) { AppLogger.Error($"Slave Write Error: {ex.Message}"); }
				}
				// --- MASTER MODE ---
				else if (radioButtonMaster.Checked && _modbusMaster != null)
				{
					// 同期皁E��書き込みを実行（書き込み完亁E��確実にする�E�E
					try
					{
						byte slaveId = (byte)numericUpDownSlaveID.Value;
						int typeIdx = child.RegisterTypeIndex;

						switch (typeIdx)
						{
							case 0: // Coil (Write Single Coil 0x05)
								_modbusMaster.WriteSingleCoil(slaveId, addr, val != 0);
								AppLogger.Info($"Master Write: Coil Addr={addr}, Val={val}");
								break;
							case 3: // Holding Register (Write Single Register 0x06)
								_modbusMaster.WriteSingleRegister(slaveId, addr, val);
								AppLogger.Info($"Master Write: Register Addr={addr}, Val={val}");
								break;
							default:
								// Input(1x), InputReg(3x) は読み取り専用
								AppLogger.Error($"Write Error: Input types are read-only.");
								break;
						}
					}
					catch (Exception ex)
					{
						AppLogger.Error($"Master Write Error: {ex.Message}");
						MessageBox.Show($"書き込みエラー: {ex.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
			};

			child.Show();
		}

		private void mode_CheckedChanged(object sender, EventArgs e)
		{
			PanelChange();
			// モード切替時に万全のため通信を切断する
			DisconnectMaster();
			StopSlave();
		}

		private void PanelChange()
		{
			if (radioButtonMaster.Checked)
			{
				panelMaster.Visible = true;
				panelSlave.Visible = false;
				panelMaster.Dock = DockStyle.Fill;
			}
			else
			{
				panelMaster.Visible = false;
				panelSlave.Visible = true;
				panelSlave.Dock = DockStyle.Fill;
			}
		}

		// ====================================================================
		//  VALIDATION & UI LOCKING LOGIC
		// ====================================================================

		/// <summary>
		/// Masterモードの入力値を検証します。
		/// MonitorFormの設定状態もチェックします。
		/// </summary>
		private bool ValidateMasterSettings()
		{
			// 1.基本設定チェック
			// IpAddressInputコントロールは常に正しいIP形式(x.x.x.x)を返すため、
			// TryParseでの厳密なチェックはほぼ不要ですが、念のため "0.0.0.0" 等は除外します。

			// IPAddress型として取得できるか確認
			if (ipAddressInputDest.GetIpAddress().Equals(IPAddress.Any) && ipAddressInputDest.Text != "0.0.0.0")
			{
				// 基本的にここには来ない
				// 基本的にここには来ない
				MessageBox.Show("接続先IPアドレスが不正です。", "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
			if (numericUpDownPort.Value < 1 || numericUpDownPort.Value > 65535)
			{
				MessageBox.Show("ポート番号が不正です。", "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return false;
			}
			if (numericUpDownInterval.Value < 10)
			{
				MessageBox.Show("通信インターバルが短すぎます。", "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return false;
			}

			// 2. モニタ設定チェック (必須：追加)
			var monitors = this.MdiChildren.OfType<MonitorForm>().ToList();
			if (monitors.Count == 0)
			{
				MessageBox.Show("モニタ画面(パネル)が1つもありません。\n「＋パネル追加」ボタンで追加してください。", "設定エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return false;
			}

			// 「設定反映」済みのモニタが少なくとも1つあるか？
			bool anyApplied = monitors.Any(m => m.IsSettingsApplied);
			if (!anyApplied)
			{
				MessageBox.Show("設定が反映されているモニタがありません。\n各モニタの「設定反映」ボタンを押して、読取アドレスを確定させてください。", "設定エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return false;
			}

			return true;
		}
		/// <summary>
		/// Slaveモードの入力値を検証します。
		/// </summary>
		private bool ValidateSlaveSettings()
		{
			// ポート番号チェック (numericUpDown1 = Listen Port)
			if (numericUpDown1.Value < 1 || numericUpDown1.Value > 65535)
			{
				MessageBox.Show("待受ポート番号は 1 ～ 65535 の範囲で指定してください。", "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				numericUpDown1.Focus();
				return false;
			}

			return true;
		}

		/// <summary>
		/// Masterモード接続中のUIロック/解除を制御します。
		/// </summary>
		/// <param name="isConnecting">接続中ならtrue, 切断中ならfalse</param>
		private void SetMasterUiState(bool isConnecting)
		{
			// 設定項目のロック
			ipAddressInputDest.Enabled = !isConnecting;
			numericUpDownPort.Enabled = !isConnecting;
			numericUpDownSlaveID.Enabled = !isConnecting;
			numericUpDownInterval.Enabled = !isConnecting;
			numericUpDownTimeout.Enabled = !isConnecting;
			checkBoxOneShot.Enabled = !isConnecting;
			comboBoxSrcIP.Enabled = !isConnecting;

			// モード切替のロック
			groupBoxMode.Enabled = !isConnecting;

			// ボタン表示の切り替え
			if (isConnecting)
			{
				buttonConnect.Text = "切断";
				buttonConnect.BackColor = Color.LightGreen;
			}
			else
			{
				buttonConnect.Text = "接続";
				buttonConnect.BackColor = SystemColors.Control;
			}
		}

		/// <summary>
		/// Slaveモード待受中のUIロック/解除を制御します。
		/// </summary>
		/// <param name="isListening">待受中ならtrue, 停止中ならfalse</param>
		private void SetSlaveUiState(bool isListening)
		{
			// 設定項目のロック
			numericUpDown1.Enabled = !isListening; // Port
			numericUpDown2.Enabled = !isListening; // UnitID
			comboBoxSrcIP.Enabled = !isListening;

			// モード切替のロック
			groupBoxMode.Enabled = !isListening;

			// ボタン表示の切り替え
			if (isListening)
			{
				buttonListen.Text = "停止";
				buttonListen.BackColor = Color.LightGreen;
			}
			else
			{
				buttonListen.Text = "待受\r\n開始";
				buttonListen.BackColor = SystemColors.Control;
			}
		}


		// ====================================================================
		//  MASTER MODE LOGIC
		// ====================================================================

		private async void buttonConnect_Click(object sender, EventArgs e)
		{
			if (_masterClient != null)
			{
				DisconnectMaster();
				return;
			}

			// バリデーションを実行
			if (!ValidateMasterSettings()) return;

			try
			{
				// UIロック
				buttonConnect.Enabled = false;

				// IP/Port設定
				IPAddress sourceIp = IPAddress.Any;
				if (comboBoxSrcIP.SelectedItem is NicOption nic) sourceIp = nic.Ip;
				var localEndPoint = new IPEndPoint(sourceIp, 0);

				_masterClient = new TcpClient(localEndPoint);

				string targetIp = ipAddressInputDest.Text;
				int targetPort = (int)numericUpDownPort.Value;

				AppLogger.Info($"Connecting to {targetIp}:{targetPort}...");
				await _masterClient.ConnectAsync(targetIp, targetPort);

				// Modbus構築
				var adapter = new LoggingAdapter(_masterClient);
				var factory = new ModbusFactory();
				var transport = factory.CreateIpTransport(adapter);
				_modbusMaster = new ModbusIpMaster(transport);

				// タイムアウト値を設定
				int timeout = (int)numericUpDownTimeout.Value;
				_modbusMaster.Transport.ReadTimeout = timeout;
				_modbusMaster.Transport.WriteTimeout = timeout;

				AppLogger.Info("Connected.");
				SetMasterUiState(true);

				// ワンショットモードかチェック
				if (checkBoxOneShot.Checked)
				{
					// ワンショットモード: 1回だけポーリングして切断
					AppLogger.Info("One-shot mode: Polling once...");
					await ExecuteOneShotPoll();
					DisconnectMaster();
				}
				else
				{
					// 通常モード: 連続ポーリング
					_pollTimer.Interval = (int)numericUpDownInterval.Value;
					_pollTimer.Start();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"接続エラー: {ex.Message}", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				AppLogger.Error($"Connect Fail: {ex.Message}");
				DisconnectMaster();
			}
			finally
			{
				buttonConnect.Enabled = true;
			}
		}

		private void DisconnectMaster()
		{
			// �^�C�}�[停止
			_pollTimer.Stop();

			// リソース解放
			_modbusMaster?.Dispose();
			_masterClient?.Close();
			_modbusMaster = null;
			_masterClient = null;

			AppLogger.Info("Disconnected.");

			// UIロック解除
			SetMasterUiState(false);
		}

		/// <summary>
		/// ワンショットモード用: 1回だけポーリングを実行
		/// </summary>
		private async Task ExecuteOneShotPoll()
		{
			if (_modbusMaster == null) return;

			try
			{
				var monitors = this.MdiChildren.OfType<MonitorForm>().ToList();
				byte slaveId = (byte)numericUpDownSlaveID.Value;

				foreach (var monitor in monitors)
				{
					// 設定未反映のモニタはスキップ
					if (!monitor.IsSettingsApplied) continue;

					// マスターモードでも編集可能にする(レジスタタイプに応じて)
					monitor.EnableGridEditing(true, isMasterMode: true);

					// 確定済みのプロパティを使用
					ushort startAddr = monitor.CurrentStartAddress;
					ushort count = monitor.CurrentCount;
					int typeIdx = monitor.RegisterTypeIndex;

					try
					{
						ushort[]? data = null;
						bool[]? boolData = null;

						await Task.Run(() =>
						{
							switch (typeIdx)
							{
								case 0: boolData = _modbusMaster.ReadCoils(slaveId, startAddr, count); break;
								case 1: boolData = _modbusMaster.ReadInputs(slaveId, startAddr, count); break;
								case 2: data = _modbusMaster.ReadInputRegisters(slaveId, startAddr, count); break;
								case 3: data = _modbusMaster.ReadHoldingRegisters(slaveId, startAddr, count); break;
							}
						});

						if (boolData != null) data = boolData.Select(b => (ushort)(b ? 1 : 0)).ToArray();

						if (data != null) monitor.UpdateResult(data, true);
					}
					catch (Exception ex)
					{
						monitor.UpdateResult(new ushort[0], false, ex.Message);
					}
				}
			}
			catch (Exception ex)
			{
				AppLogger.Error($"One-shot poll error: {ex.Message}");
			}
		}

		private async void PollTimer_Tick(object sender, EventArgs e)
		{
			_pollTimer.Stop();

			if (radioButtonMaster.Checked)
			{
				// --- MASTER MODE ---
				if (_modbusMaster == null) return;

				try
				{
					var monitors = this.MdiChildren.OfType<MonitorForm>().ToList();
					byte slaveId = (byte)numericUpDownSlaveID.Value;

					foreach (var monitor in monitors)
					{
						// 設定未反映のモニタはスキップ
						if (!monitor.IsSettingsApplied) continue;

						// マスターモードでも編雁E��能にする�E�レジスタタイプに応じて�E�E
						monitor.EnableGridEditing(true, isMasterMode: true);

						// 確定済みのプロパティを使用
						ushort startAddr = monitor.CurrentStartAddress;
						ushort count = monitor.CurrentCount;
						int typeIdx = monitor.RegisterTypeIndex;

						try
						{
							ushort[]? data = null;
							bool[]? boolData = null;

							await Task.Run(() =>
							{
								switch (typeIdx)
								{
									case 0: boolData = _modbusMaster.ReadCoils(slaveId, startAddr, count); break;
									case 1: boolData = _modbusMaster.ReadInputs(slaveId, startAddr, count); break;
									case 2: data = _modbusMaster.ReadInputRegisters(slaveId, startAddr, count); break;
									case 3: data = _modbusMaster.ReadHoldingRegisters(slaveId, startAddr, count); break;
								}
							});

							if (boolData != null) data = boolData.Select(b => (ushort)(b ? 1 : 0)).ToArray();

							if (data != null) monitor.UpdateResult(data, true);
						}
						catch (Exception ex)
						{
							monitor.UpdateResult(new ushort[0], false, ex.Message);
						}
					}
				}
				finally
				{
					if (_masterClient != null && _masterClient.Connected) _pollTimer.Start();
					else DisconnectMaster();
				}
			}
			else
			{
				// --- SLAVE MODE ---
				if (_mySlave != null)
				{
					var monitors = this.MdiChildren.OfType<MonitorForm>().ToList();
					foreach (var monitor in monitors)
					{
						// 設定未反映なら何もしない(グリッド更新しない)
						if (!monitor.IsSettingsApplied) continue;

						monitor.EnableGridEditing(true);

						ushort startAddr = monitor.CurrentStartAddress;
						ushort count = monitor.CurrentCount;
						int typeIdx = monitor.RegisterTypeIndex;

						try
						{
							ushort[] data = new ushort[count];
							// DataStoreから読み出し
							switch (typeIdx)
							{
								case 0:
									var coils = _mySlave.DataStore.CoilDiscretes.ReadPoints(startAddr, count);
									data = coils.Select(b => (ushort)(b ? 1 : 0)).ToArray();
									break;
								case 1:
									var inputs = _mySlave.DataStore.CoilInputs.ReadPoints(startAddr, count);
									data = inputs.Select(b => (ushort)(b ? 1 : 0)).ToArray();
									break;
								case 2:
									data = _mySlave.DataStore.InputRegisters.ReadPoints(startAddr, count);
									break;
								case 3:
									data = _mySlave.DataStore.HoldingRegisters.ReadPoints(startAddr, count);
									break;
							}
							monitor.UpdateResult(data, true);
						}
						catch { }
					}
					_pollTimer.Start();
				}
			}
		}


		// ====================================================================
		//  SLAVE MODE LOGIC
		// ====================================================================

		private void buttonListen_Click(object sender, EventArgs e)
		{
			// 既にListen中の場合は停止する
			if (_slaveListener != null)
			{
				StopSlave();
				return;
			}

			// バリデーションを実行
			if (!ValidateSlaveSettings()) return;

			try
			{
				// 1. IP & Port 設定
				IPAddress listenIp = IPAddress.Any;
				if (comboBoxSrcIP.SelectedItem is NicOption nic)
				{
					listenIp = nic.Ip;
				}
				int port = (int)numericUpDown1.Value; // Port

				// 2. Listener作成
				_slaveListener = new TcpListener(listenIp, port);
				_slaveListener.Start();

				// 3. NModbus Slave機能構築
				var factory = new ModbusFactory();
				_slaveNetwork = factory.CreateSlaveNetwork(_slaveListener);

				byte unitId = (byte)numericUpDown2.Value; // UnitID
				_mySlave = factory.CreateSlave(unitId);
				_slaveNetwork.AddSlave(_mySlave);

				// 4. Listen開始
				_slaveNetwork.ListenAsync();

				AppLogger.Info($"Slave Started on {listenIp}:{port}, UnitID={unitId}");

				// 待受成功状態のUI設定
				SetSlaveUiState(true);

				// 画面更新用タイマー開始
				_pollTimer.Interval = (int)numericUpDownInterval.Value;
				_pollTimer.Start();
			}
			catch (Exception ex)
			{
				MessageBox.Show($"待受開始エラー: {ex.Message}", "Listen Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				AppLogger.Error($"Listen Error: {ex.Message}");
				StopSlave();
			}
		}

		private void StopSlave()
		{
			try
			{
				_pollTimer.Stop();
				_slaveNetwork?.Dispose();
				_slaveListener?.Stop();
			}
			catch { }

			_slaveNetwork = null;
			_slaveListener = null;
			_mySlave = null;

			AppLogger.Info("Slave Stopped.");

			// UIロック解除
			SetSlaveUiState(false);
		}

		// ====================================================================
		//  MDI LAYOUT LOGIC
		// ====================================================================

		private void toolStripButton2_Click(object sender, EventArgs e)
		{
			if (_mdiClient == null) return;
			var sortedForms = GetSortedWindows();
			if (sortedForms.Count == 0) return;

			int borderX = SystemInformation.Border3DSize.Width * 2;
			int borderY = SystemInformation.Border3DSize.Height * 2;
			int validWidth = _mdiClient.Width - borderX;
			int validHeight = _mdiClient.Height - borderY;

			int w = validWidth;
			int h = validHeight / sortedForms.Count;

			for (int i = 0; i < sortedForms.Count; i++)
			{
				var f = sortedForms[i];
				f.WindowState = FormWindowState.Normal;
				f.SetBounds(0, i * h, w, h);
			}
		}

		private void toolStripButton3_Click(object sender, EventArgs e)
		{
			if (_mdiClient == null) return;
			var sortedForms = GetSortedWindows();
			if (sortedForms.Count == 0) return;

			int borderX = SystemInformation.Border3DSize.Width * 2;
			int borderY = SystemInformation.Border3DSize.Height * 2;
			int validWidth = _mdiClient.Width - borderX;
			int validHeight = _mdiClient.Height - borderY;

			int w = validWidth / sortedForms.Count;
			int h = validHeight;

			for (int i = 0; i < sortedForms.Count; i++)
			{
				var f = sortedForms[i];
				f.WindowState = FormWindowState.Normal;
				f.SetBounds(i * w, 0, w, h);
			}
		}

		private List<Form> GetSortedWindows()
		{
			return this.MdiChildren
				.OfType<MonitorForm>()
				.OrderBy(c => c.Top)
				.ThenBy(c => c.Left)
				.Cast<Form>()
				.ToList();
		}

		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			_pollTimer?.Stop();
			DisconnectMaster();
			StopSlave();
			AppLogger.Stop();
			base.OnFormClosing(e);
		}

		private void tableLayoutPanel2_Paint(object sender, PaintEventArgs e)
		{

		}
	}
}