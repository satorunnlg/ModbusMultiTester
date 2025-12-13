using NModbus.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ModbusMultiTester.Core;

namespace ModbusMultiTester.Core
{
	/// <summary>
	/// NModbusの通信ストリームをラップし、送受信データをログに記録するアダプター
	/// </summary>
	public class LoggingAdapter : IStreamResource
	{
		private readonly TcpClient _tcpClient;
		private readonly NetworkStream _networkStream;

		public LoggingAdapter(TcpClient tcpClient)
		{
			_tcpClient = tcpClient;
			_networkStream = tcpClient.GetStream();
		}

		// --- IStreamResourceの実装 ---

		public int InfiniteTimeout => Timeout.Infinite;

		public int ReadTimeout
		{
			get => _networkStream.ReadTimeout;
			set => _networkStream.ReadTimeout = value;
		}

		public int WriteTimeout
		{
			get => _networkStream.WriteTimeout;
			set => _networkStream.WriteTimeout = value;
		}

		// 送信(Write)時の割り込み処理
		public void Write(byte[] buffer, int offset, int count)
		{
			try
			{
				// 実際に送信
				_networkStream.Write(buffer, offset, count);

				// ログ記録用にデータをコピーして渡す
				byte[] logData = new byte[count];
				Array.Copy(buffer, offset, logData, 0, count);
				AppLogger.LogHex("TX", logData, count);
			}
			catch (Exception ex)
			{
				AppLogger.Error($"Write Error: {ex.Message}");
				throw;
			}
		}

		// 受信(Read)時の割り込み処理
		public int Read(byte[] buffer, int offset, int count)
		{
			try
			{
				// 実際に受信
				int bytesRead = _networkStream.Read(buffer, offset, count);

				if (bytesRead > 0)
				{
					// 受信した分だけログ記録
					byte[] logData = new byte[bytesRead];
					Array.Copy(buffer, offset, logData, 0, bytesRead);
					AppLogger.LogHex("RX", logData, bytesRead);
				}

				return bytesRead;
			}
			catch (Exception ex)
			{
				// タイムアウトはModbusでは日常茶飯事なので、ここではログレベルを考慮しても良い
				// ここではエラーとして記録
				if (!(ex is IOException))
				{
					AppLogger.Error($"Read Error: {ex.Message}");
				}
				throw;
			}
		}

		// IStreamResource の要件を満たすための追加メソッド
		public void DiscardInBuffer()
		{
			// TCP/IP (NetworkStream) ではバッファのクリア機能は提供されないため、
			// ここでは何もしない（空実装）で問題ありません。
		}

		public void Dispose()
		{
			_networkStream?.Dispose();
			_tcpClient?.Dispose();
		}
	}
}
