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
	/// ƒƒCƒ“‰æ–ÊƒNƒ‰ƒXB
	/// Modbus TCP‚ÌMaster/Slave‹@”\‚ÌŠÇ—A’ÊMİ’èAMDIqƒEƒBƒ“ƒhƒE‚Ì“Š‡‚ğs‚¢‚Ü‚·B
	/// </summary>
	public partial class MainForm : Form
	{
		// --- Master—pƒƒ“ƒo ---
		private TcpClient? _masterClient;
		private IModbusMaster? _modbusMaster;
		private System.Windows.Forms.Timer _pollTimer;

		// --- Slave—pƒƒ“ƒo ---
		private TcpListener? _slaveListener;
		private IModbusSlaveNetwork? _slaveNetwork;
		private IModbusSlave? _mySlave;

		// --- UIƒŒƒCƒAƒEƒg—p ---
		private MdiClient? _mdiClient;

		/// <summary>
		/// ƒRƒ“ƒXƒgƒ‰ƒNƒ^
		/// </summary>
		public MainForm()
		{
			InitializeComponent();

			// ƒƒK[‹@”\‚ÌŠJn
			AppLogger.Start();
			AppLogger.Info("Application Started.");

			// Šeí‰Šú‰»ˆ—
			InitializeNetworking();
			SetupTimer();
			PanelChange(); // UI‚Ì‰Šú•\¦ó‘Ô‚ğİ’è

			// MDIƒRƒ“ƒeƒi‚Ì”wŒiFİ’è
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
		/// ƒtƒH[ƒ€ƒ[ƒh‚Ìˆ—B
		/// MdiClient‚Ìæ“¾‚ÆƒŒƒCƒAƒEƒg’²®A‰ŠúqƒEƒBƒ“ƒhƒE‚Ì•\¦‚ğs‚¢‚Ü‚·B
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
		/// ƒŒƒCƒAƒEƒg•ÏXƒCƒxƒ“ƒg‚ÌƒI[ƒo[ƒ‰ƒCƒhB
		/// MDI—Ìˆæ‚ÌˆÊ’u‚ÆƒTƒCƒY‚ğ‹­§“I‚É§Œä‚µ‚Ü‚·B
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
		/// ƒtƒH[ƒ€ƒŠƒTƒCƒY‚Ìˆ—
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
		/// NICˆê——‚Ì‰Šú‰»
		/// </summary>
		private void InitializeNetworking()
		{
			comboBoxSrcIP.Items.Clear();
			comboBoxSrcIP.Items.Add(new NicOption
			{
				DisplayName = "w’è‚È‚µ(OS•W€)",
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

		// --- UIƒCƒxƒ“ƒgƒnƒ“ƒhƒ‰ ---

		private void toolStripButtonAddPanel_Click(object sender, EventArgs e)
		{
			AddMonitorWindow();
		}

		private void AddMonitorWindow()
		{
			var child = new MonitorForm(this.MdiChildren.Length + 1);
			child.MdiParent = this;

			// ƒf[ƒ^‚Ì•ÒW(‘‚«‚İ)ƒCƒxƒ“ƒg
			child.DataEdited += (addr, val) =>
			{
				// --- SLAVE MODE (Šù‘¶) ---
				if (radioButtonSlave.Checked && _mySlave != null)
				{
					/* ... Šù‘¶‚ÌSlaveˆ— ... */
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
					// åŒæœŸçš„ã«æ›¸ãè¾¼ã¿ã‚’å®Ÿè¡Œï¼ˆæ›¸ãè¾¼ã¿å®Œäº†ã‚’ç¢ºå®Ÿã«ã™ã‚‹ï¼‰
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
								// Input(1x), InputReg(3x) ã¯èª­ã¿å–ã‚Šå°‚ç”¨
								AppLogger.Error($"Write Error: Input types are read-only.");
								break;
						}
					}
					catch (Exception ex)
					{
						AppLogger.Error($"Master Write Error: {ex.Message}");
						MessageBox.Show($"æ›¸ãè¾¼ã¿ã‚¨ãƒ©ãƒ¼: {ex.Message}", "ã‚¨ãƒ©ãƒ¼", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
			};

			child.Show();
		}

		private void mode_CheckedChanged(object sender, EventArgs e)
		{
			PanelChange();
			// ƒ‚[ƒhØ‘Ö‚ÉˆÀ‘S‚Ì‚½‚ß’ÊM‚ğØ’f‚·‚é
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
		/// Masterƒ‚[ƒh‚Ì“ü—Í’l‚ğŒŸØ‚µ‚Ü‚·B
		/// MonitorForm‚Ìİ’èó‘Ô‚àƒ`ƒFƒbƒN‚µ‚Ü‚·B
		/// </summary>
		private bool ValidateMasterSettings()
		{
			// 1.Šî–{İ’èƒ`ƒFƒbƒN
			// IpAddressInputƒRƒ“ƒgƒ[ƒ‹‚Íí‚É³‚µ‚¢IPŒ`®(x.x.x.x)‚ğ•Ô‚·‚½‚ßA
			// TryParse‚Å‚ÌŒµ–§‚Èƒ`ƒFƒbƒN‚Í‚Ù‚Ú•s—v‚Å‚·‚ªA”O‚Ì‚½‚ß "0.0.0.0" ‚ğ’e‚­‚È‚Ç‚Í‚±‚±‚Ås‚¦‚Ü‚·B

			// IPAddressŒ^‚Æ‚µ‚Äæ“¾‚Å‚«‚é‚©Šm”F
			if (ipAddressInputDest.GetIpAddress().Equals(IPAddress.Any) && ipAddressInputDest.Text != "0.0.0.0")
			{
				// Šî–{“I‚É‚±‚±‚É‚Í—ˆ‚È‚¢
				MessageBox.Show("Ú‘±æIPƒAƒhƒŒƒX‚ª–³Œø‚Å‚·B", "“ü—ÍƒGƒ‰[", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return false;
			}
			if (numericUpDownPort.Value < 1 || numericUpDownPort.Value > 65535)
			{
				MessageBox.Show("ƒ|[ƒg”Ô†‚ª–³Œø‚Å‚·B", "“ü—ÍƒGƒ‰[", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return false;
			}
			if (numericUpDownInterval.Value < 10)
			{
				MessageBox.Show("’ÊMƒCƒ“ƒ^[ƒoƒ‹‚ª’Z‚·‚¬‚Ü‚·B", "“ü—ÍƒGƒ‰[", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return false;
			}

			// 2. ƒ‚ƒjƒ^İ’èƒ`ƒFƒbƒN (‚±‚±‚ğ’Ç‰Á)
			var monitors = this.MdiChildren.OfType<MonitorForm>().ToList();
			if (monitors.Count == 0)
			{
				MessageBox.Show("ƒ‚ƒjƒ^‰æ–Êiƒpƒlƒ‹j‚ª1‚Â‚à‚ ‚è‚Ü‚¹‚ñB\nu{ƒpƒlƒ‹’Ç‰Ávƒ{ƒ^ƒ“‚Å’Ç‰Á‚µ‚Ä‚­‚¾‚³‚¢B", "İ’èƒGƒ‰[", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return false;
			}

			// uİ’è”½‰fvÏ‚İ‚Ìƒ‚ƒjƒ^‚ª­‚È‚­‚Æ‚à1‚Â‚ ‚é‚©H
			bool anyApplied = monitors.Any(m => m.IsSettingsApplied);
			if (!anyApplied)
			{
				MessageBox.Show("İ’è‚ª”½‰f‚³‚ê‚Ä‚¢‚éƒ‚ƒjƒ^‚ª‚ ‚è‚Ü‚¹‚ñB\nŠeƒ‚ƒjƒ^‚Ìuİ’è”½‰fvƒ{ƒ^ƒ“‚ğ‰Ÿ‚µ‚ÄAŠÄ‹ƒAƒhƒŒƒX‚ğŠm’è‚³‚¹‚Ä‚­‚¾‚³‚¢B", "İ’èƒGƒ‰[", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return false;
			}

			return true;
		}
		/// <summary>
		/// Slaveƒ‚[ƒh‚Ì“ü—Í’l‚ğŒŸØ‚µ‚Ü‚·B
		/// </summary>
		private bool ValidateSlaveSettings()
		{
			// ƒ|[ƒg”Ô†ƒ`ƒFƒbƒN (numericUpDown1 = Listen Port)
			if (numericUpDown1.Value < 1 || numericUpDown1.Value > 65535)
			{
				MessageBox.Show("‘Òóƒ|[ƒg”Ô†‚Í 1 ` 65535 ‚Ì”ÍˆÍ‚Åw’è‚µ‚Ä‚­‚¾‚³‚¢B", "“ü—ÍƒGƒ‰[", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				numericUpDown1.Focus();
				return false;
			}

			return true;
		}

		/// <summary>
		/// Masterƒ‚[ƒhÚ‘±’†‚ÌUIƒƒbƒN/‰ğœ‚ğ§Œä‚µ‚Ü‚·B
		/// </summary>
		/// <param name="isConnecting">Ú‘±’†‚È‚çtrue, Ø’f’†‚È‚çfalse</param>
		private void SetMasterUiState(bool isConnecting)
		{
			// İ’è€–Ú‚ÌƒƒbƒN
			ipAddressInputDest.Enabled = !isConnecting;
			numericUpDownPort.Enabled = !isConnecting;
			numericUpDownSlaveID.Enabled = !isConnecting;
			numericUpDownInterval.Enabled = !isConnecting;
			comboBoxSrcIP.Enabled = !isConnecting;

			// ƒ‚[ƒhØ‘Ö‚ÌƒƒbƒN
			groupBoxMode.Enabled = !isConnecting;

			// ƒ{ƒ^ƒ“•\¦‚ÌØ‚è‘Ö‚¦
			if (isConnecting)
			{
				buttonConnect.Text = "Ø’f";
				buttonConnect.BackColor = Color.LightGreen;
			}
			else
			{
				buttonConnect.Text = "Ú‘±";
				buttonConnect.BackColor = SystemColors.Control;
			}
		}

		/// <summary>
		/// Slaveƒ‚[ƒh‘Òó’†‚ÌUIƒƒbƒN/‰ğœ‚ğ§Œä‚µ‚Ü‚·B
		/// </summary>
		/// <param name="isListening">‘Òó’†‚È‚çtrue, ’â~’†‚È‚çfalse</param>
		private void SetSlaveUiState(bool isListening)
		{
			// İ’è€–Ú‚ÌƒƒbƒN
			numericUpDown1.Enabled = !isListening; // Port
			numericUpDown2.Enabled = !isListening; // UnitID
			comboBoxSrcIP.Enabled = !isListening;

			// ƒ‚[ƒhØ‘Ö‚ÌƒƒbƒN
			groupBoxMode.Enabled = !isListening;

			// ƒ{ƒ^ƒ“•\¦‚ÌØ‚è‘Ö‚¦
			if (isListening)
			{
				buttonListen.Text = "’â~";
				buttonListen.BackColor = Color.LightGreen;
			}
			else
			{
				buttonListen.Text = "‘Òó\r\nŠJn";
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

			// ƒoƒŠƒf[ƒVƒ‡ƒ“Às
			if (!ValidateMasterSettings()) return;

			try
			{
				// UIƒƒbƒN
				buttonConnect.Enabled = false;

				// IP/Portİ’è
				IPAddress sourceIp = IPAddress.Any;
				if (comboBoxSrcIP.SelectedItem is NicOption nic) sourceIp = nic.Ip;
				var localEndPoint = new IPEndPoint(sourceIp, 0);

				_masterClient = new TcpClient(localEndPoint);

				string targetIp = ipAddressInputDest.Text;
				int targetPort = (int)numericUpDownPort.Value;

				AppLogger.Info($"Connecting to {targetIp}:{targetPort}...");
				await _masterClient.ConnectAsync(targetIp, targetPort);

				// Modbus\’z
				var adapter = new LoggingAdapter(_masterClient);
				var factory = new ModbusFactory();
				var transport = factory.CreateIpTransport(adapter);
				_modbusMaster = new ModbusIpMaster(transport);

				_modbusMaster.Transport.ReadTimeout = 1000;
				_modbusMaster.Transport.WriteTimeout = 1000;

				AppLogger.Info("Connected.");
				SetMasterUiState(true);

				_pollTimer.Interval = (int)numericUpDownInterval.Value;
				_pollTimer.Start();
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ú‘±ƒGƒ‰[: {ex.Message}", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
			// ƒ^ƒCƒ}[’â~
			_pollTimer.Stop();

			// ƒŠƒ\[ƒX‰ğ•ú
			_modbusMaster?.Dispose();
			_masterClient?.Close();
			_modbusMaster = null;
			_masterClient = null;

			AppLogger.Info("Disconnected.");

			// UIƒƒbƒN‰ğœ
			SetMasterUiState(false);
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
						// İ’è–¢”½‰f‚Ìƒ‚ƒjƒ^‚ÍƒXƒLƒbƒv
						if (!monitor.IsSettingsApplied) continue;

						// ãƒã‚¹ã‚¿ãƒ¼ãƒ¢ãƒ¼ãƒ‰ã§ã‚‚ç·¨é›†å¯èƒ½ã«ã™ã‚‹ï¼ˆãƒ¬ã‚¸ã‚¹ã‚¿ã‚¿ã‚¤ãƒ—ã«å¿œã˜ã¦ï¼‰
						monitor.EnableGridEditing(true, isMasterMode: true);

						// Šm’èÏ‚İ‚ÌƒvƒƒpƒeƒB‚ğg—p
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
						// İ’è–¢”½‰f‚È‚ç‰½‚à‚µ‚È‚¢iƒOƒŠƒbƒh‚àì‚ç‚ê‚È‚¢j
						if (!monitor.IsSettingsApplied) continue;

						monitor.EnableGridEditing(true);

						ushort startAddr = monitor.CurrentStartAddress;
						ushort count = monitor.CurrentCount;
						int typeIdx = monitor.RegisterTypeIndex;

						try
						{
							ushort[] data = new ushort[count];
							// DataStore‚©‚ç“Ç‚İo‚µ
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
			// Šù‚ÉListen’†‚Ìê‡‚Í’â~ˆ—‚Ö
			if (_slaveListener != null)
			{
				StopSlave();
				return;
			}

			// ƒoƒŠƒf[ƒVƒ‡ƒ“Às
			if (!ValidateSlaveSettings()) return;

			try
			{
				// 1. IP & Port İ’è
				IPAddress listenIp = IPAddress.Any;
				if (comboBoxSrcIP.SelectedItem is NicOption nic)
				{
					listenIp = nic.Ip;
				}
				int port = (int)numericUpDown1.Value; // Port

				// 2. Listener‹N“®
				_slaveListener = new TcpListener(listenIp, port);
				_slaveListener.Start();

				// 3. NModbus Slave‹@”\\’z
				var factory = new ModbusFactory();
				_slaveNetwork = factory.CreateSlaveNetwork(_slaveListener);

				byte unitId = (byte)numericUpDown2.Value; // UnitID
				_mySlave = factory.CreateSlave(unitId);
				_slaveNetwork.AddSlave(_mySlave);

				// 4. ListenŠJn
				_slaveNetwork.ListenAsync();

				AppLogger.Info($"Slave Started on {listenIp}:{port}, UnitID={unitId}");

				// ‘Òó¬Œ÷ó‘Ô‚ÌUIƒZƒbƒg
				SetSlaveUiState(true);

				// ‰æ–Ê“¯Šú—pƒ^ƒCƒ}[ŠJn
				_pollTimer.Interval = (int)numericUpDownInterval.Value;
				_pollTimer.Start();
			}
			catch (Exception ex)
			{
				MessageBox.Show($"‘ÒóŠJnƒGƒ‰[: {ex.Message}", "Listen Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

			// UIƒƒbƒN‰ğœ
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