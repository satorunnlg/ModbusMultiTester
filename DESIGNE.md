# Modbus TCP Multi-Tester 開発設計書

**バージョン:** 0.0.1
**プラットフォーム:** .NET 8.0 (Windows Forms)
**開発言語:** C\#
**外部ライブラリ:** NModbus (v3.x系)

## 1\. プロジェクト概要

本ソフトウェアは、産業用通信プロトコル Modbus TCP の通信テストを行うためのエンジニア向けユーティリティである。
**MDI (Multiple Document Interface)** 形式を採用し、複数のメモリ領域（コイル、レジスタ等）を個別のウィンドウで同時に監視・操作することを可能にする。
単一のアプリケーションで **Master (Client)** と **Slave (Server)** の両機能を持ち、**送信元IPアドレス（NIC）の指定** や **通信フレームのHexログ保存** を備え、現場でのトラブルシューティング能力を最大化することを目的とする。

-----

## 2\. ディレクトリ構造 (ソリューション構成)

UIコンポーネントの独立性を高め、保守性を確保するための構成とする。

```text
ModbusMultiTester.sln        ... ソリューションファイル
│
└─ ModbusMultiTester         ... プロジェクトフォルダ
   │
   ├─ Properties/            ... アセンブリ情報など
   ├─ bin/                   ... ビルド出力
   ├─ logs/                  ... 実行時に生成されるログ保存先
   │
   ├─ Core/                  ... 通信・ロジックの中核
   │  ├─ AppLogger.cs        ... ログ出力クラス (非同期ファイル書き込み)
   │  ├─ LoggingAdapter.cs   ... NModbus通信傍受用アダプター (IStreamResource)
   │  └─ NicOption.cs        ... ネットワークアダプタ情報保持用クラス
   │
   ├─ UI/                    ... 画面・カスタムコントロール
   │  ├─ MainForm.cs         ... MDI親画面 (通信統括・全体設定)
   │  ├─ MonitorForm.cs      ... MDI子画面 (データグリッド・個別設定)
   │  │  └─ MonitorItem.cs   ... データバインディング用モデルクラス
   │  ├─ IpAddressInput.cs   ... IPアドレス入力専用カスタムコントロール
   │  └─ (Designer/ResX)     ... 各フォームのリソース
   │
   ├─ Program.cs             ... エントリーポイント
   └─ ModbusMultiTester.csproj
```

-----

## 3\. 機能要件

### 3.1 アプリケーション基本構造 (MDI)

  * **MDIアーキテクチャ:** `MainForm` を親とし、複数の `MonitorForm` を子ウィンドウとして動的に追加・削除可能とする。
  * **整列機能:** 子ウィンドウの「横に整列（縦積み）」「縦に整列（横並び）」をサポートし、スクロールバーや枠線を考慮したピクセル単位の正確な配置を行う。
  * **モード切替:** ラジオボタンにより「Master」と「Slave」を排他的に切り替え、設定パネルの表示制御および通信ロジックを切り替える。

### 3.2 ネットワーク機能 & UI制御

  * **NIC選択:** PC上の有効なIPv4インターフェースを列挙し、通信に使用する送信元IPアドレスを指定可能とする。
  * **IP入力支援:** `IpAddressInput` コントロールにより、0-255制限、ドット自動移動、空欄防止などを備えたWindows標準ライクな入力インターフェースを提供する。
  * **UIロック:** 接続中・待受中は設定項目を編集不可（Disable）にし、誤操作を防止する。

### 3.3 Master モード (Client)

  * **接続:** 指定された Target IP, Port への TCP 接続および Modbus Master の構築。
  * **一括ポーリング:** 定周期タイマーにより、**「設定反映済み」の全MonitorForm** に対して順次読み出し要求を行う。
  * **非同期通信:** 通信処理は別タスクで実行し、UIのフリーズを回避する。

### 3.4 Slave モード (Server)

  * **待受:** 指定された Local IP, Port での TCP Listener 起動および Modbus Slave の構築。
  * **メモリ共有:** NModbus の `DataStore` を単一のメモリ空間として保持する。
  * **双方向同期:**
      * **Masterからの書き込み:** `DataStore` が更新された際、次回のUI更新周期で `MonitorForm` のグリッドに反映する。
      * **UIからの書き込み:** `MonitorForm` 上での値変更を即座に検知し、`DataStore` へ書き戻す。

### 3.5 ロギング機能

  * **内容:** 送受信した Modbus フレームの生データ (Hex Dump) を時系列で記録。
  * **保存:** `logs/` フォルダへ日次ファイル (`modbus_yyyyMMdd.log`) として保存。

-----

## 4\. クラス詳細設計

### 4.1 `UI/MainForm.cs` (MDI Parent)

アプリケーションの主要な制御を行う。

  * **レイアウト制御:** `OnLayout` をオーバーライドし、`MdiClient` コントロール（背景領域）の位置をツールバー等の下部に強制的に合わせることで、描画崩れを防ぐ。
  * **バリデーション:** 通信開始前に IPアドレス形式、ポート範囲、および「有効なモニタ画面が存在するか」をチェックする。
  * **通信ループ (`PollTimer_Tick`):**
      * **Master時:** 各モニタの設定（アドレス・個数）に基づき `Read` 系メソッドを実行。結果をモニタへ通知。
      * **Slave時:** `DataStore` から現在の値を読み出し、各モニタへ通知（表示更新）。

### 4.2 `UI/MonitorForm.cs` (MDI Child)

個別のデータ監視範囲を担当するウィンドウ。

  * **設定管理:** レジスタ種別、開始アドレス、個数を管理。「設定反映」ボタン押下時のみグリッドを生成し、通信対象とするフラグ (`IsSettingsApplied`) を立てる。
  * **データバインディング:** `BindingList<MonitorItem>` を使用して `DataGridView` と接続。
      * 短周期更新時でもスクロール位置や選択セルがリセットされないようにする。
      * `UpdateResult` メソッドでデータの差分更新を行う。
  * **編集制御:**
      * Masterモード時: 原則ReadOnly（書き込み機能未実装のため）。
      * Slaveモード時: 編集可能。入力確定時に `MainForm` へイベント通知し、内部メモリを書き換える。
      * 編集中保護: 通信更新とユーザー入力が衝突した場合、編集中のセルは更新をスキップする。

### 4.3 `UI/IpAddressInput.cs` (UserControl)

4つの `TextBox` とラベルを組み合わせたカスタムコントロール。

  * **機能:** 数値のみ入力許可、範囲制限(0-255)、オートフォーカス移動、プロパティ（Font, ReadOnly等）の親コントロールへの伝播。

### 4.4 `Core/LoggingAdapter.cs` & `AppLogger.cs`

  * **LoggingAdapter:** `NModbus` の `IStreamResource` を実装し、`NetworkStream` への読み書きをフックして `AppLogger` へデータを流す。
  * **AppLogger:** `ConcurrentQueue` を用いた非同期書き込みにより、通信パフォーマンスへの影響を最小化する。

-----

## 5\. 画面UI設計

### 5.1 MainForm (親画面)

| エリア | コントロール | 説明 |
| :--- | :--- | :--- |
| **Header (上部)** | `RadioButton` | Master / Slave モード切替 |
| | `ComboBox` | Source IP (NIC) 選択 |
| **Settings (パネル)** | **[Master Panel]** | Masterモード時のみ表示 |
| | `IpAddressInput` | 接続先IPアドレス (例: 192.168.1.1) |
| | `NumericUpDown` | Port (Default 502), SlaveID, Interval |
| | `Button` | 接続 / 切断 |
| | **[Slave Panel]** | Slaveモード時のみ表示 |
| | `NumericUpDown` | Listen Port (Default 502), Unit ID |
| | `Button` | 待受開始 / 停止 |
| **Toolbar (下部)** | `ToolStrip` | [＋パネル追加], [横に整列], [縦に整列] |
| **Client Area** | `MdiClient` | 子ウィンドウが表示される領域 (背景色: 240, 240, 240) |

### 5.2 MonitorForm (子画面)

| エリア | コントロール | 説明 |
| :--- | :--- | :--- |
| **Toolbar** | `ComboBox` | レジスタ種別 (Coil, Discrete, Input, Holding) |
| | `ToolStripNumericUpDown` | 開始アドレス (0-65535) |
| | `ToolStripNumericUpDown` | データ個数 (1-2000) |
| | `Button` | 設定反映 (グリッド生成・通信有効化) |
| **Grid** | `DataGridView` | [アドレス], [値] の2列構成。データバインディング使用。 |
| **Status** | `StatusStrip` | 現在の状態 (停止中/モニタ中/設定未反映) |
