using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using YumpooDrive.Core.IMessage;

namespace YumpooDrive.Core.Net
{
	/// <summary>
	/// 基于单次无协议的网络交互的基类，通常是串口协议扩展成网口协议的基类
	/// </summary>
	/// <typeparam name="TTransform">指定了数据转换的规则</typeparam>
	public class NetworkDeviceSoloBase<TTransform> : NetworkDeviceBase<HslMessage, TTransform> where TTransform : IByteTransform, new()
	{
		private int sleepTime = 20;

		/// <summary>
		/// 连续串口缓冲数据检测的间隔时间，默认20ms
		/// </summary>
		public int SleepTime
		{
			get
			{
				return sleepTime;
			}
			set
			{
				if (value > 0)
				{
					sleepTime = value;
				}
			}
		}

		/// <summary>
		/// 实例化一个默认的对象
		/// </summary>
		public NetworkDeviceSoloBase()
		{
			base.ReceiveTimeOut = 5000;
		}

		/// <summary>
		/// 从串口接收一串数据信息，可以指定是否一定要接收到数据
		/// </summary>
		/// <param name="socket">串口对象</param>
		/// <param name="awaitData">是否必须要等待数据返回</param>
		/// <returns>结果数据对象</returns>
		protected OperateResult<byte[]> ReceiveSolo(Socket socket, bool awaitData)
		{
			//if (!Authorization.nzugaydgwadawdibbas())
			//{
			//	return new OperateResult<byte[]>(StringResources.Language.AuthorizationFailed);
			//}
			byte[] buffer = new byte[1024];
			MemoryStream memoryStream = new MemoryStream();
			DateTime now = DateTime.Now;
			TimeOut hslTimeOut = new TimeOut
			{
				DelayTime = base.ReceiveTimeOut,
				WorkSocket = socket
			};
			if (base.ReceiveTimeOut > 0)
			{
				ThreadPool.QueueUserWorkItem(base.ThreadPoolCheckTimeOut, hslTimeOut);
			}
			try
			{
				Thread.Sleep(sleepTime);
                socket.ReceiveTimeout = 500;
				int count = socket.Receive(buffer);
				hslTimeOut.IsSuccessful = true;
				memoryStream.Write(buffer, 0, count);
			}
			catch (Exception ex)
			{
				memoryStream.Dispose();
				return new OperateResult<byte[]>(ex.Message);
			}
			byte[] value = memoryStream.ToArray();
			memoryStream.Dispose();
			return OperateResult.CreateSuccessResult(value);
		}

		/// <summary>
		/// 重写读取服务器的数据信息
		/// </summary>
		/// <param name="socket">网络套接字</param>
		/// <param name="send">发送的数据内容</param>
		/// <returns>读取数据的结果</returns>
		public override OperateResult<byte[]> ReadFromCoreServer(Socket socket, byte[] send)
		{
			//base.LogNet?.WriteDebug(ToString(), StringResources.Language.Send + " : " + SoftBasic.ByteToHexString(send, ' '));
			OperateResult operateResult = Send(socket, send);
			if (!operateResult.IsSuccess)
			{
				socket?.Close();
				return OperateResult.CreateFailedResult<byte[]>(operateResult);
			}
			if (receiveTimeOut < 0)
			{
				return OperateResult.CreateSuccessResult(new byte[0]);
			}
			OperateResult<byte[]> operateResult2 = ReceiveSolo(socket, awaitData: false);
			if (!operateResult2.IsSuccess)
			{
				socket?.Close();
				return new OperateResult<byte[]>(StringResources.Language.ReceiveDataTimeout + receiveTimeOut);
			}
			//base.LogNet?.WriteDebug(ToString(), StringResources.Language.Receive + " : " + SoftBasic.ByteToHexString(operateResult2.Content, ' '));
			return OperateResult.CreateSuccessResult(operateResult2.Content);
		}

		/// <summary>
		/// 返回表示当前对象的字符串信息
		/// </summary>
		/// <returns>字符串信息</returns>
		public override string ToString()
		{
			return $"NetworkDeviceSoloBase<{typeof(TTransform)}>[{IpAddress}:{Port}]";
		}
	}
}
