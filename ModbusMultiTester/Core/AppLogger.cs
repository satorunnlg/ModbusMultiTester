using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusMultiTester.Core
{
    /// <summary>
    /// アプリケーションログおよび通信ログ(Hex)をファイルに記録するクラス
    /// </summary>
    public static class AppLogger
    {
        private static readonly ConcurrentQueue<string> _logQueue = new ConcurrentQueue<string>();
        private static readonly string _logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
        private static bool _isRunning = true;
        private static Task? _writeTask;

        /// <summary>
        /// ログ機能の開始（アプリ起動時に呼ぶ）
        /// </summary>
        public static void Start()
        {
            if (!Directory.Exists(_logPath))
            {
                Directory.CreateDirectory(_logPath);
            }
            _isRunning = true;
            _writeTask = Task.Run(ProcessQueue);
        }

        /// <summary>
        /// ログ機能の停止（アプリ終了時に呼ぶ）
        /// </summary>
        public static void Stop()
        {
            _isRunning = false;
            // キューの残りを確実に処理するため、書き込みタスクの完了を待つ
            try
            {
                _writeTask?.Wait(TimeSpan.FromSeconds(5));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AppLogger.Stop: {ex.Message}");
            }
        }

        // 通常の操作ログ
        public static void Info(string message) => Enqueue("INFO", message);
        public static void Error(string message) => Enqueue("ERROR", message);

        /// <summary>
        /// バイナリデータのHexダンプ記録
        /// </summary>
        /// <param name="direction">TX(送信) or RX(受信)</param>
        /// <param name="buffer">バイト配列</param>
        /// <param name="count">有効なバイト数</param>
        public static void LogHex(string direction, byte[] buffer, int count)
        {
            if (count <= 0) return;

            // バイト配列を "01 03 00 00 ..." 形式の文字列に変換
            StringBuilder hex = new StringBuilder(count * 3);
            for (int i = 0; i < count; i++)
            {
                hex.Append(buffer[i].ToString("X2") + " ");
            }

            Enqueue("RAW", $"{direction} Len={count}: {hex.ToString().Trim()}");
        }

        private static void Enqueue(string level, string message)
        {
            // フォーマット: YYYY-MM-DDTHH:mm:ss.fff [LEVEL] Message
            string timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff");
            _logQueue.Enqueue($"{timestamp} [{level}] {message}");
        }

        private static async Task ProcessQueue()
        {
            StreamWriter? currentWriter = null;
            string? currentDate = null;

            try
            {
                while (_isRunning || !_logQueue.IsEmpty)
                {
                    if (_logQueue.TryDequeue(out string? logLine))
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(logLine)) continue;

                            // 日付ごとにファイルを変える (例: modbus_20251213.log)
                            string today = DateTime.Now.ToString("yyyyMMdd");

                            // 日付が変わった場合は新しいファイルに切り替え
                            if (currentDate != today)
                            {
                                currentWriter?.Dispose();

                                string fileName = $"modbus_{today}.log";
                                string filePath = Path.Combine(_logPath, fileName);

                                currentWriter = new StreamWriter(filePath, append: true, Encoding.UTF8)
                                {
                                    AutoFlush = true
                                };
                                currentDate = today;
                            }

                            // StreamWriterに書き込み
                            await currentWriter!.WriteLineAsync(logLine);
                        }
                        catch (Exception ex)
                        {
                            // ログ書き込み失敗時はアプリを止めずにデバッグ出力
                            System.Diagnostics.Debug.WriteLine($"AppLogger.ProcessQueue: {ex.Message}");
                        }
                    }
                    else
                    {
                        // キューが空なら少し待機してCPU負荷を下げる
                        await Task.Delay(50);
                    }
                }
            }
            finally
            {
                // タスク終了時にStreamWriterを確実にクローズ
                currentWriter?.Dispose();
            }
        }
    }
}
