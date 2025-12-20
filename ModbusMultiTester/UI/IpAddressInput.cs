using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Windows.Forms;

namespace ModbusMultiTester.UI
{
    /// <summary>
    /// Windows標準のようなIPアドレス入力専用コントロール
    /// TextBoxが持つ標準的なプロパティ（Font, ForeColor, ReadOnly等）をサポートします。
    /// </summary>
    [DefaultEvent("TextChanged")]
    [DefaultProperty("Text")]
    public partial class IpAddressInput : UserControl
    {
        private readonly TextBox[] _boxes = new TextBox[4];
        private readonly Label[] _dots = new Label[3];
        private readonly FlowLayoutPanel _panel;

        // 内部状態
        private bool _readOnly = false;
        private ContentAlignment _textAlignment = ContentAlignment.MiddleCenter;

        // イベント抑止フラグ
        private bool _isUpdating = false;

        public IpAddressInput()
        {
            // --- コンテナ設定 ---
            this.Size = new Size(130, 23); // 初期の推奨サイズ
            this.BackColor = SystemColors.Window;
            this.BorderStyle = BorderStyle.FixedSingle;
            this.Padding = new Padding(0);
            this.Cursor = Cursors.IBeam; // テキスト入力っぽいカーソル

            // --- レイアウトパネル ---
            _panel = new FlowLayoutPanel();
            _panel.Dock = DockStyle.Fill;
            _panel.FlowDirection = FlowDirection.LeftToRight;
            _panel.WrapContents = false;
            _panel.Padding = new Padding(0);
            _panel.Margin = new Padding(0);
            _panel.BackColor = Color.Transparent; // 親の色を透かす

            // パネルのどこをクリックしてもフォーカス移動
            _panel.Click += (s, e) => FocusFirstEditable();
            this.Controls.Add(_panel);

            // --- 内部コントロール生成 ---
            for (int i = 0; i < 4; i++)
            {
                var tb = new TextBox();
                tb.BorderStyle = BorderStyle.None;
                tb.TextAlign = HorizontalAlignment.Center;
                tb.MaxLength = 3;
                tb.Margin = new Padding(0);
                tb.Text = "0";

                // デザイン調整（初期フォント等は親に追従させるため後で設定）
                tb.Width = 24;

                // イベント登録
                int index = i;
                tb.KeyPress += (s, e) => OnBoxKeyPress(s, e, index);
                tb.KeyDown += (s, e) => OnBoxKeyDown(s, e, index);
                tb.TextChanged += (s, e) => OnBoxTextChanged(s, e, index);
                tb.Leave += (s, e) => OnBoxLeave(s, e);
                tb.Enter += (s, e) => tb.SelectAll();

                // 親のContextMenuStripを使うようにする
                tb.ContextMenuStripChanged += (s, e) => this.ContextMenuStrip = tb.ContextMenuStrip;

                _boxes[i] = tb;
                _panel.Controls.Add(tb);

                // ドット追加
                if (i < 3)
                {
                    var dot = new Label();
                    dot.Text = ".";
                    dot.AutoSize = true;
                    dot.Margin = new Padding(0);
                    dot.TextAlign = ContentAlignment.BottomCenter;

                    _dots[i] = dot;
                    _panel.Controls.Add(dot);
                }
            }

            // 背景パネルをクリックしたときに、最初のボックスにフォーカスを当てる
            _panel.Click += (s, e) =>
            {
                _boxes[0].Focus();
                _boxes[0].SelectAll();
            };

            // 初期化
            SetIpAddress("127.0.0.1");
            UpdateStyles(); // 色やフォントの適用
            RecalculateLayout(); // レイアウト計算
        }

        // ====================================================================
        //  標準プロパティの実装 (Override & New)
        // ====================================================================

        [Category("Appearance")]
        [Description("コントロール内のテキストの配置を指定します。")]
        [DefaultValue(typeof(ContentAlignment), "MiddleCenter")]
        public ContentAlignment TextAlignment
        {
            get => _textAlignment;
            set
            {
                _textAlignment = value;
                RecalculateLayout();
            }
        }

        [Category("Behavior")]
        [Description("テキストを変更できるかどうかを示します。")]
        [DefaultValue(false)]
        public bool ReadOnly
        {
            get => _readOnly;
            set
            {
                _readOnly = value;
                UpdateStyles(); // 背景色の変更などを反映
            }
        }

        [Category("Appearance")]
        public override Font Font
        {
            get => base.Font;
            set
            {
                base.Font = value;
                UpdateStyles();
                RecalculateLayout(); // フォントサイズが変わればレイアウトも変わる
            }
        }

        [Category("Appearance")]
        public override Color ForeColor
        {
            get => base.ForeColor;
            set
            {
                base.ForeColor = value;
                UpdateStyles();
            }
        }

        [Category("Appearance")]
        public override Color BackColor
        {
            get => base.BackColor;
            set
            {
                base.BackColor = value;
                UpdateStyles();
            }
        }

        [Category("Behavior")]
        public override ContextMenuStrip ContextMenuStrip
        {
            get => base.ContextMenuStrip;
            set
            {
                base.ContextMenuStrip = value;
                // 内部のTextBoxにも右クリックメニューを伝播
                if (_boxes != null)
                {
                    foreach (var box in _boxes) if (box != null) box.ContextMenuStrip = value;
                }
            }
        }

        // テキストプロパティ
        [Category("Appearance")]
        [Bindable(true)]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public override string Text
        {
            get
            {
                if (_boxes == null || _boxes[0] == null) return "0.0.0.0";
                return $"{ParseBox(_boxes[0])}.{ParseBox(_boxes[1])}.{ParseBox(_boxes[2])}.{ParseBox(_boxes[3])}";
            }
            set
            {
                SetIpAddress(value);
                // 値がセットされたらTextChangedを発火させる
                OnTextChanged(EventArgs.Empty);
            }
        }

        // ====================================================================
        //  ロジック & イベント
        // ====================================================================

        public IPAddress GetIpAddress()
        {
            if (IPAddress.TryParse(this.Text, out var ip)) return ip;
            return IPAddress.Any;
        }

        private void SetIpAddress(string ipStr)
        {
            if (_boxes == null || _boxes[0] == null) return;

            _isUpdating = true; // イベント発火抑制
            try
            {
                if (IPAddress.TryParse(ipStr, out var ip))
                {
                    byte[] bytes = ip.GetAddressBytes();
                    for (int i = 0; i < 4; i++) _boxes[i].Text = bytes[i].ToString();
                }
                else
                {
                    foreach (var box in _boxes) box.Text = "0";
                }
            }
            finally
            {
                _isUpdating = false;
            }
        }

        // --- スタイル適用とレイアウト ---

        /// <summary>
        /// ReadOnlyやEnabledの状態、Fontの設定に合わせて内部コントロールを一括更新します。
        /// </summary>
        private void UpdateStyles()
        {
            if (_boxes == null || _boxes[0] == null) return;

            // 背景色の決定 (Enabled > ReadOnly > UserSet)
            Color targetBackColor = this.BackColor;
            if (!this.Enabled) targetBackColor = SystemColors.Control;
            else if (_readOnly) targetBackColor = SystemColors.Control;

            Color targetForeColor = this.ForeColor;
            if (!this.Enabled) targetForeColor = SystemColors.GrayText;

            // 内部コントロールへの適用
            foreach (var tb in _boxes)
            {
                if (tb == null) continue;
                tb.Font = this.Font;
                tb.ForeColor = targetForeColor;
                tb.BackColor = targetBackColor;
                tb.ReadOnly = _readOnly;
                // ReadOnlyでもカーソルが入るようにするかはお好みで。
                // ここでは標準TextBoxに合わせてTabStopは残します。
            }

            foreach (var dot in _dots)
            {
                if (dot == null) continue;
                dot.Font = this.Font;
                dot.ForeColor = targetForeColor;
                dot.BackColor = targetBackColor;
            }

            // コンテナ自体の色も合わせる
            base.BackColor = targetBackColor;
        }

        /// <summary>
        /// フォントサイズ等に合わせて内部レイアウトを再計算します。
        /// </summary>
        private void RecalculateLayout()
        {
            if (_panel == null || _boxes == null || _boxes[0] == null) return;

            _panel.SuspendLayout();

            // 1. 高さの調整
            // TextBoxの高さはFontによって自動決定される(PreferredHeight)
            int preferredHeight = _boxes[0].PreferredHeight;

            // 全Boxの高さを統一 (Multiline=falseなのでHeight設定は無視されるが念のため)
            foreach (var b in _boxes) b.Height = preferredHeight;

            // コントロール自体の高さも合わせる (境界線分を足す)
            int borderSize = (this.BorderStyle == BorderStyle.None) ? 0 : 2;
            this.Height = preferredHeight + borderSize + this.Padding.Vertical;

            // 2. 幅とパディングの計算 (Alignment)
            // コンテンツ幅 = Box幅*4 + Dot幅*3
            // Dotの幅はFont依存なのでAutoSizeで計測
            int totalContentWidth = 0;
            foreach (var b in _boxes) totalContentWidth += b.Width;
            foreach (var d in _dots) totalContentWidth += d.Width;

            int clientW = this.ClientSize.Width;
            int clientH = this.ClientSize.Height;

            int padLeft = 0;
            // TextBoxはパネル内で上詰め配置されるため、垂直中央寄せのためにTopPaddingを計算
            // パネル高さとコンテンツ高さの差分
            int padTop = Math.Max(0, (clientH - preferredHeight) / 2);

            switch (_textAlignment)
            {
                case ContentAlignment.TopCenter:
                case ContentAlignment.MiddleCenter:
                case ContentAlignment.BottomCenter:
                    padLeft = Math.Max(0, (clientW - totalContentWidth) / 2);
                    break;
                case ContentAlignment.TopRight:
                case ContentAlignment.MiddleRight:
                case ContentAlignment.BottomRight:
                    padLeft = Math.Max(0, clientW - totalContentWidth);
                    break;
                default: // Left
                    padLeft = 0;
                    break;
            }

            // ドットの垂直位置微調整 (TextBoxの文字ベースラインに合わせる)
            // 簡易的に Padding.Top を TextBox より少し増やす
            foreach (var d in _dots) d.Padding = new Padding(0, 2, 0, 0);

            _panel.Padding = new Padding(padLeft, padTop, 0, 0);

            _panel.ResumeLayout();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            RecalculateLayout();
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            UpdateStyles();
        }

        // --- 内部イベントハンドラ ---

        private void OnBoxKeyPress(object? sender, KeyPressEventArgs e, int index)
        {
            if (_readOnly) return;

            // --- 4. ドット入力でのフォーカス移動 ---
            if (e.KeyChar == '.' || e.KeyChar == '。')
            {
                if (index < 3)
                {
                    _boxes[index + 1].Focus();
                    _boxes[index + 1].SelectAll();
                }
                e.Handled = true; // 文字としては入力させない
                return;
            }

            // 数字と制御文字以外は拒否
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void OnBoxTextChanged(object? sender, EventArgs e, int index)
        {
            if (_isUpdating) return;
            if (sender is not TextBox tb) return;

            // 入力制限ロジック
            string original = tb.Text;
            string digits = new string(original.Where(char.IsDigit).ToArray());

            if (int.TryParse(digits, out int val))
            {
                if (val > 255) digits = "255";
            }

            // 書き換えが発生する場合
            if (original != digits)
            {
                int cursor = tb.SelectionStart; // カーソル位置保存
                tb.Text = digits;
                tb.SelectionStart = Math.Min(cursor, tb.Text.Length);
            }

            // 親へ変更通知
            OnTextChanged(EventArgs.Empty);

            // オートフォーカス移動 (入力中でかつ3桁になった時)
            if (tb.Text.Length == 3 && index < 3 && tb.SelectionStart == 3)
            {
                _boxes[index + 1].Focus();
                _boxes[index + 1].SelectAll();
            }
        }

        private void OnBoxKeyDown(object? sender, KeyEventArgs e, int index)
        {
            if (sender is not TextBox tb) return;

            // --- 全選択 (Ctrl+A) ---
            if (e.Control && e.KeyCode == Keys.A)
            {
                tb.SelectAll();
                e.SuppressKeyPress = true; // ビープ音防止
                return;
            }

            // --- コピー (Ctrl+C) ---
            if (e.Control && e.KeyCode == Keys.C)
            {
                if (tb.SelectionLength > 0)
                {
                    // 選択範囲があれば標準のコピー
                    tb.Copy();
                }
                else
                {
                    // 【重要】選択範囲がなければ「IP全体」をコピーする
                    Clipboard.SetText(this.Text);
                }
                e.SuppressKeyPress = true;
                return;
            }

            // --- 貼り付け (Ctrl+V) ---
            if (e.Control && e.KeyCode == Keys.V)
            {
                PasteIpAddress();
                e.SuppressKeyPress = true;
                return;
            }

            // --- 矢印移動 (既存ロジック) ---
            if (e.KeyCode == Keys.Right && tb.SelectionStart == tb.TextLength && index < 3)
            {
                _boxes[index + 1].Focus();
                _boxes[index + 1].SelectAll(); // 移動時に全選択すると連続操作しやすい
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Left && tb.SelectionStart == 0 && index > 0)
            {
                _boxes[index - 1].Focus();
                _boxes[index - 1].SelectAll();
                e.Handled = true;
            }
            // バックスペースでの戻り
            else if (e.KeyCode == Keys.Back && tb.TextLength == 0 && index > 0)
            {
                _boxes[index - 1].Focus();
                _boxes[index - 1].SelectionStart = _boxes[index - 1].TextLength;
                e.Handled = true;
            }
        }

        private void OnBoxLeave(object? sender, EventArgs e)
        {
            if (sender is not TextBox tb) return;

            // ゼロ埋め処理 ( "001" -> "1", "" -> "0" )
            _isUpdating = true;
            if (string.IsNullOrWhiteSpace(tb.Text)) tb.Text = "0";
            else if (int.TryParse(tb.Text, out int val)) tb.Text = val.ToString();
            _isUpdating = false;

            // 変更があれば通知
            OnTextChanged(EventArgs.Empty);
        }

        // --- ヘルパー ---

        private void FocusFirstEditable()
        {
            if (_boxes != null && _boxes[0] != null)
            {
                _boxes[0].Focus();
            }
        }

        private string ParseBox(TextBox tb)
        {
            if (tb == null) return "0";
            return string.IsNullOrWhiteSpace(tb.Text) ? "0" : tb.Text;
        }

        // クリップボードからの貼り付け処理
        private void PasteIpAddress()
        {
            if (Clipboard.ContainsText())
            {
                string text = Clipboard.GetText().Trim();
                // ドット、カンマ、スペースなどで分割を試みる
                var parts = text.Split(new[] { '.', ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 4)
                {
                    // 4つの数字として解釈できるかチェック
                    bool valid = true;
                    byte[] bytes = new byte[4];
                    for (int i = 0; i < 4; i++)
                    {
                        if (!byte.TryParse(parts[i], out bytes[i])) valid = false;
                    }

                    if (valid)
                    {
                        for (int i = 0; i < 4; i++) _boxes[i].Text = bytes[i].ToString();
                        _boxes[3].Focus(); // 最後へフォーカス移動
                    }
                }
            }
        }
    }
}