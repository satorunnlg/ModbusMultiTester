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
	/// ���C����ʃN���X�B
	/// Modbus TCP��Master/Slave�@�\�̊Ǘ��A�ʐM�ݒ�AMDI�q�E�B���h�E�̓������s���܂��B
	/// </summary>
	public partial class MainForm : Form
	{
		// --- Master�p�����o ---
		private TcpClient? _masterClient;
		private IModbusMaster? _modbusMaster;
		private System.Windows.Forms.Timer _pollTimer;

		// --- Slave�p�����o ---
		private TcpListener? _slaveListener;
		private IModbusSlaveNetwork? _slaveNetwork;
		private IModbusSlave? _mySlave;

		// --- UI���C�A�E�g�p ---
		private MdiClient? _mdiClient;

		/// <summary>
		/// �R���X�g���N�^
		/// </summary>
		public MainForm()
		{
			InitializeComponent();

			// ���K�[�@�\�̊J�n
			AppLogger.Start();
			AppLogger.Info("Application Started.");

			// �e�평��������
			InitializeNetworking();
			SetupTimer();
			PanelChange(); // UI�̏����\����Ԃ�ݒ�

			// MDI�R���e�i�̔w�i�F�ݒ�
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
		/// �t�H�[�����[�h���̏����B
		/// MdiClient�̎擾�ƃ��C�A�E�g�����A�����q�E�B���h�E�̕\�����s���܂��B
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
		/// ���C�A�E�g�ύX�C�x���g�̃I�[�o�[���C�h�B
		/// MDI�̈�̈ʒu�ƃT�C�Y�������I�ɐ��䂵�܂��B
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
		/// �t�H�[�����T�C�Y���̏���
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
		/// NIC�ꗗ�̏�����
		/// </summary>
		private void InitializeNetworking()
		{
			comboBoxSrcIP.Items.Clear();
			comboBoxSrcIP.Items.Add(new NicOption
			{
				DisplayName = "�w��Ȃ�(OS�W��)",
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

		// --- UI�C�x���g�n���h�� ---

		private void toolStripButtonAddPanel_Click(object sender, EventArgs e)
		{
			AddMonitorWindow();
		}

		private void AddMonitorWindow()
		{
			var child = new MonitorForm(this.MdiChildren.Length + 1);
			child.MdiParent = this;

			// �f�[�^�̕ҏW(��������)�C�x���g
			child.DataEdited += (addr, val) =>
			{
				// --- SLAVE MODE (����) ---
				if (radioButtonSlave.Checked && _mySlave != null)
				{
					/* ... ������Slave���� ... */
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
			// ���[�h�ؑ֎��Ɉ��S�̂��ߒʐM��ؒf����
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
		/// Master���[�h�̓��͒l�����؂��܂��B
		/// MonitorForm�̐ݒ��Ԃ��`�F�b�N���܂��B
		/// </summary>
		private bool ValidateMasterSettings()
		{
			// 1.��{�ݒ�`�F�b�N
			// IpAddressInput�R���g���[���͏�ɐ�����IP�`��(x.x.x.x)��Ԃ����߁A
			// TryParse�ł̌����ȃ`�F�b�N�͂قڕs�v�ł����A�O�̂��� "0.0.0.0" ��e���Ȃǂ͂����ōs���܂��B

			// IPAddress�^�Ƃ��Ď擾�ł��邩�m�F
			if (ipAddressInputDest.GetIpAddress().Equals(IPAddress.Any) && ipAddressInputDest.Text != "0.0.0.0")
			{
				// ��{�I�ɂ����ɂ͗��Ȃ�
				MessageBox.Show("�ڑ���IP�A�h���X�������ł��B", "���̓G���[", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return false;
			}
			if (numericUpDownPort.Value < 1 || numericUpDownPort.Value > 65535)
			{
				MessageBox.Show("�|�[�g�ԍ��������ł��B", "���̓G���[", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return false;
			}
			if (numericUpDownInterval.Value < 10)
			{
				MessageBox.Show("�ʐM�C���^�[�o�����Z�����܂��B", "���̓G���[", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return false;
			}

			// 2. ���j�^�ݒ�`�F�b�N (������ǉ�)
			var monitors = this.MdiChildren.OfType<MonitorForm>().ToList();
			if (monitors.Count == 0)
			{
				MessageBox.Show("���j�^��ʁi�p�l���j��1������܂���B\n�u�{�p�l���ǉ��v�{�^���Œǉ����Ă��������B", "�ݒ�G���[", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return false;
			}

			// �u�ݒ蔽�f�v�ς݂̃��j�^�����Ȃ��Ƃ�1���邩�H
			bool anyApplied = monitors.Any(m => m.IsSettingsApplied);
			if (!anyApplied)
			{
				MessageBox.Show("�ݒ肪���f����Ă��郂�j�^������܂���B\n�e���j�^�́u�ݒ蔽�f�v�{�^���������āA�Ď��A�h���X���m�肳���Ă��������B", "�ݒ�G���[", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return false;
			}

			return true;
		}
		/// <summary>
		/// Slave���[�h�̓��͒l�����؂��܂��B
		/// </summary>
		private bool ValidateSlaveSettings()
		{
			// �|�[�g�ԍ��`�F�b�N (numericUpDown1 = Listen Port)
			if (numericUpDown1.Value < 1 || numericUpDown1.Value > 65535)
			{
				MessageBox.Show("�Ҏ�|�[�g�ԍ��� 1 �` 65535 �͈̔͂Ŏw�肵�Ă��������B", "���̓G���[", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				numericUpDown1.Focus();
				return false;
			}

			return true;
		}

		/// <summary>
		/// Master���[�h�ڑ�����UI���b�N/�����𐧌䂵�܂��B
		/// </summary>
		/// <param name="isConnecting">�ڑ����Ȃ�true, �ؒf���Ȃ�false</param>
		private void SetMasterUiState(bool isConnecting)
		{
			// �ݒ荀�ڂ̃��b�N
			ipAddressInputDest.Enabled = !isConnecting;
			numericUpDownPort.Enabled = !isConnecting;
			numericUpDownSlaveID.Enabled = !isConnecting;
			numericUpDownInterval.Enabled = !isConnecting;
			numericUpDownTimeout.Enabled = !isConnecting;
			checkBoxOneShot.Enabled = !isConnecting;
			comboBoxSrcIP.Enabled = !isConnecting;

			// ���[�h�ؑւ̃��b�N
			groupBoxMode.Enabled = !isConnecting;

			// �{�^���\���̐؂�ւ�
			if (isConnecting)
			{
				buttonConnect.Text = "�ؒf";
				buttonConnect.BackColor = Color.LightGreen;
			}
			else
			{
				buttonConnect.Text = "�ڑ�";
				buttonConnect.BackColor = SystemColors.Control;
			}
		}

		/// <summary>
		/// Slave���[�h�Ҏ󒆂�UI���b�N/�����𐧌䂵�܂��B
		/// </summary>
		/// <param name="isListening">�Ҏ󒆂Ȃ�true, ��~���Ȃ�false</param>
		private void SetSlaveUiState(bool isListening)
		{
			// �ݒ荀�ڂ̃��b�N
			numericUpDown1.Enabled = !isListening; // Port
			numericUpDown2.Enabled = !isListening; // UnitID
			comboBoxSrcIP.Enabled = !isListening;

			// ���[�h�ؑւ̃��b�N
			groupBoxMode.Enabled = !isListening;

			// �{�^���\���̐؂�ւ�
			if (isListening)
			{
				buttonListen.Text = "��~";
				buttonListen.BackColor = Color.LightGreen;
			}
			else
			{
				buttonListen.Text = "�Ҏ�\r\n�J�n";
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

			// �o���f�[�V�������s
			if (!ValidateMasterSettings()) return;

			try
			{
				// UI���b�N
				buttonConnect.Enabled = false;

				// IP/Port�ݒ�
				IPAddress sourceIp = IPAddress.Any;
				if (comboBoxSrcIP.SelectedItem is NicOption nic) sourceIp = nic.Ip;
				var localEndPoint = new IPEndPoint(sourceIp, 0);

				_masterClient = new TcpClient(localEndPoint);

				string targetIp = ipAddressInputDest.Text;
				int targetPort = (int)numericUpDownPort.Value;

				AppLogger.Info($"Connecting to {targetIp}:{targetPort}...");
				await _masterClient.ConnectAsync(targetIp, targetPort);

				// Modbus�\�z
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
				MessageBox.Show($"�ڑ��G���[: {ex.Message}", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
			// �^�C�}�[��~
			_pollTimer.Stop();

			// ���\�[�X���
			_modbusMaster?.Dispose();
			_masterClient?.Close();
			_modbusMaster = null;
			_masterClient = null;

			AppLogger.Info("Disconnected.");

			// UI���b�N����
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
						// �ݒ薢���f�̃��j�^�̓X�L�b�v
						if (!monitor.IsSettingsApplied) continue;

						// マスターモードでも編雁E��能にする�E�レジスタタイプに応じて�E�E
						monitor.EnableGridEditing(true, isMasterMode: true);

						// �m��ς݂̃v���p�e�B���g�p
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
						// �ݒ薢���f�Ȃ牽�����Ȃ��i�O���b�h������Ȃ��j
						if (!monitor.IsSettingsApplied) continue;

						monitor.EnableGridEditing(true);

						ushort startAddr = monitor.CurrentStartAddress;
						ushort count = monitor.CurrentCount;
						int typeIdx = monitor.RegisterTypeIndex;

						try
						{
							ushort[] data = new ushort[count];
							// DataStore����ǂݏo��
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
			// ����Listen���̏ꍇ�͒�~������
			if (_slaveListener != null)
			{
				StopSlave();
				return;
			}

			// �o���f�[�V�������s
			if (!ValidateSlaveSettings()) return;

			try
			{
				// 1. IP & Port �ݒ�
				IPAddress listenIp = IPAddress.Any;
				if (comboBoxSrcIP.SelectedItem is NicOption nic)
				{
					listenIp = nic.Ip;
				}
				int port = (int)numericUpDown1.Value; // Port

				// 2. Listener�N��
				_slaveListener = new TcpListener(listenIp, port);
				_slaveListener.Start();

				// 3. NModbus Slave�@�\�\�z
				var factory = new ModbusFactory();
				_slaveNetwork = factory.CreateSlaveNetwork(_slaveListener);

				byte unitId = (byte)numericUpDown2.Value; // UnitID
				_mySlave = factory.CreateSlave(unitId);
				_slaveNetwork.AddSlave(_mySlave);

				// 4. Listen�J�n
				_slaveNetwork.ListenAsync();

				AppLogger.Info($"Slave Started on {listenIp}:{port}, UnitID={unitId}");

				// �Ҏ󐬌���Ԃ�UI�Z�b�g
				SetSlaveUiState(true);

				// ��ʓ����p�^�C�}�[�J�n
				_pollTimer.Interval = (int)numericUpDownInterval.Value;
				_pollTimer.Start();
			}
			catch (Exception ex)
			{
				MessageBox.Show($"�Ҏ�J�n�G���[: {ex.Message}", "Listen Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

			// UI���b�N����
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