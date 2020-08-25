using System;
using System.Linq;
using System.Text;
using YumpooDrive.BasicFramework;
using YumpooDrive.Core;
using YumpooDrive.Serial;

namespace YumpooDrive.Profinet.Melsec
{
	/// <summary>
	/// 三菱PLC的计算机链接协议，适用的PLC型号参考备注
	/// </summary>
	/// <remarks>
	/// 支持的通讯的系列如下参考
	/// <list type="table">
	///     <listheader>
	///         <term>系列</term>
	///         <term>是否支持</term>
	///         <term>备注</term>
	///     </listheader>
	///     <item>
	///         <description>FX3UC系列</description>
	///         <description>支持</description>
	///         <description></description>
	///     </item>
	///     <item>
	///         <description>FX3U系列</description>
	///         <description>支持</description>
	///         <description></description>
	///     </item>
	///     <item>
	///         <description>FX3GC系列</description>
	///         <description>支持</description>
	///         <description></description>
	///     </item>
	///     <item>
	///         <description>FX3G系列</description>
	///         <description>支持</description>
	///         <description></description>
	///     </item>
	///     <item>
	///         <description>FX3S系列</description>
	///         <description>支持</description>
	///         <description></description>
	///     </item>
	///     <item>
	///         <description>FX2NC系列</description>
	///         <description>支持</description>
	///         <description></description>
	///     </item>
	///     <item>
	///         <description>FX2N系列</description>
	///         <description>部分支持(v1.06+)</description>
	///         <description>通过监控D8001来确认版本号</description>
	///     </item>
	///     <item>
	///         <description>FX1NC系列</description>
	///         <description>支持</description>
	///         <description></description>
	///     </item>
	///     <item>
	///         <description>FX1N系列</description>
	///         <description>支持</description>
	///         <description></description>
	///     </item>
	///     <item>
	///         <description>FX1S系列</description>
	///         <description>支持</description>
	///         <description></description>
	///     </item>
	///     <item>
	///         <description>FX0N系列</description>
	///         <description>部分支持(v1.20+)</description>
	///         <description></description>
	///     </item>
	///     <item>
	///         <description>FX0S系列</description>
	///         <description>不支持</description>
	///         <description></description>
	///     </item>
	///     <item>
	///         <description>FX0系列</description>
	///         <description>不支持</description>
	///         <description></description>
	///     </item>
	///     <item>
	///         <description>FX2C系列</description>
	///         <description>部分支持(v3.30+)</description>
	///         <description></description>
	///     </item>
	///     <item>
	///         <description>FX2(FX)系列</description>
	///         <description>部分支持(v3.30+)</description>
	///         <description></description>
	///     </item>
	///     <item>
	///         <description>FX1系列</description>
	///         <description>不支持</description>
	///         <description></description>
	///     </item>
	/// </list>
	/// 数据地址支持的格式如下：
	/// <list type="table">
	///   <listheader>
	///     <term>地址名称</term>
	///     <term>地址代号</term>
	///     <term>示例</term>
	///     <term>地址进制</term>
	///     <term>字操作</term>
	///     <term>位操作</term>
	///     <term>备注</term>
	///   </listheader>
	///   <item>
	///     <term>内部继电器</term>
	///     <term>M</term>
	///     <term>M100,M200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>输入继电器</term>
	///     <term>X</term>
	///     <term>X10,X20</term>
	///     <term>8</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>输出继电器</term>
	///     <term>Y</term>
	///     <term>Y10,Y20</term>
	///     <term>8</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>步进继电器</term>
	///     <term>S</term>
	///     <term>S100,S200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>定时器的触点</term>
	///     <term>TS</term>
	///     <term>TS100,TS200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>定时器的当前值</term>
	///     <term>TN</term>
	///     <term>TN100,TN200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>×</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>计数器的触点</term>
	///     <term>CS</term>
	///     <term>CS100,CS200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>计数器的当前</term>
	///     <term>CN</term>
	///     <term>CN100,CN200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>×</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>数据寄存器</term>
	///     <term>D</term>
	///     <term>D1000,D2000</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>×</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>文件寄存器</term>
	///     <term>R</term>
	///     <term>R100,R200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>×</term>
	///     <term></term>
	///   </item>
	/// </list>
	/// </remarks>
	public class MelsecFxLinks : SerialDeviceBase<RegularByteTransform>
	{
		private byte station = 0;

		private byte watiingTime = 0;

		private bool sumCheck = true;

		/// <summary>
		/// PLC的站号信息
		/// </summary>
		public byte Station
		{
			get
			{
				return station;
			}
			set
			{
				station = value;
			}
		}

		/// <summary>
		/// 报文等待时间，单位10ms，设置范围为0-15
		/// </summary>
		public byte WaittingTime
		{
			get
			{
				return watiingTime;
			}
			set
			{
				if (watiingTime > 15)
				{
					watiingTime = 15;
				}
				else
				{
					watiingTime = value;
				}
			}
		}

		/// <summary>
		/// 是否启动和校验
		/// </summary>
		public bool SumCheck
		{
			get
			{
				return sumCheck;
			}
			set
			{
				sumCheck = value;
			}
		}

		/// <summary>
		/// 实例化默认的构造方法
		/// </summary>
		public MelsecFxLinks()
		{
			base.WordLength = 1;
		}

		/// <summary>
		/// 批量读取PLC的数据，以字为单位，支持读取X,Y,M,S,D,T,C，具体的地址范围需要根据PLC型号来确认
		/// </summary>
		/// <param name="address">地址信息</param>
		/// <param name="length">数据长度</param>
		/// <returns>读取结果信息</returns>
		public override OperateResult<byte[]> Read(string address, ushort length)
		{
			OperateResult<byte[]> operateResult = MelsecFxLinksOverTcp.BuildReadCommand(station, address, length, isBool: false, sumCheck, watiingTime);
			if (!operateResult.IsSuccess)
			{
				return OperateResult.CreateFailedResult<byte[]>(operateResult);
			}
			OperateResult<byte[]> operateResult2 = ReadBase(operateResult.Content);
			if (!operateResult2.IsSuccess)
			{
				return OperateResult.CreateFailedResult<byte[]>(operateResult2);
			}
			if (operateResult2.Content[0] != 2)
			{
				return new OperateResult<byte[]>(operateResult2.Content[0], "Read Faild:" + SoftBasic.ByteToHexString(operateResult2.Content, ' '));
			}
			byte[] array = new byte[length * 2];
			for (int i = 0; i < array.Length / 2; i++)
			{
				ushort value = Convert.ToUInt16(Encoding.ASCII.GetString(operateResult2.Content, i * 4 + 5, 4), 16);
				BitConverter.GetBytes(value).CopyTo(array, i * 2);
			}
			return OperateResult.CreateSuccessResult(array);
		}

		/// <summary>
		/// 批量写入PLC的数据，以字为单位，也就是说最少2个字节信息，支持X,Y,M,S,D,T,C，具体的地址范围需要根据PLC型号来确认
		/// </summary>
		/// <param name="address">地址信息</param>
		/// <param name="value">数据值</param>
		/// <returns>是否写入成功</returns>
		public override OperateResult Write(string address, byte[] value)
		{
			OperateResult<byte[]> operateResult = MelsecFxLinksOverTcp.BuildWriteByteCommand(station, address, value, sumCheck, watiingTime);
			if (!operateResult.IsSuccess)
			{
				return operateResult;
			}
			OperateResult<byte[]> operateResult2 = ReadBase(operateResult.Content);
			if (!operateResult2.IsSuccess)
			{
				return operateResult2;
			}
			if (operateResult2.Content[0] != 6)
			{
				return new OperateResult(operateResult2.Content[0], "Write Faild:" + SoftBasic.ByteToHexString(operateResult2.Content, ' '));
			}
			return OperateResult.CreateSuccessResult();
		}

		/// <summary>
		/// 批量读取bool类型数据，支持的类型为X,Y,S,T,C，具体的地址范围取决于PLC的类型
		/// </summary>
		/// <param name="address">地址信息，比如X10,Y17，注意X，Y的地址是8进制的</param>
		/// <param name="length">读取的长度</param>
		/// <returns>读取结果信息</returns>
		public override OperateResult<bool[]> ReadBool(string address, ushort length)
		{
			OperateResult<byte[]> operateResult = MelsecFxLinksOverTcp.BuildReadCommand(station, address, length, isBool: true, sumCheck, watiingTime);
			if (!operateResult.IsSuccess)
			{
				return OperateResult.CreateFailedResult<bool[]>(operateResult);
			}
			OperateResult<byte[]> operateResult2 = ReadBase(operateResult.Content);
			if (!operateResult2.IsSuccess)
			{
				return OperateResult.CreateFailedResult<bool[]>(operateResult2);
			}
			if (operateResult2.Content[0] != 2)
			{
				return new OperateResult<bool[]>(operateResult2.Content[0], "Read Faild:" + SoftBasic.ByteToHexString(operateResult2.Content, ' '));
			}
			byte[] array = new byte[length];
			Array.Copy(operateResult2.Content, 5, array, 0, length);
			return OperateResult.CreateSuccessResult(array.Select((byte m) => m == 49).ToArray());
		}

		/// <summary>
		/// 批量写入bool类型的数组，支持的类型为X,Y,S,T,C，具体的地址范围取决于PLC的类型
		/// </summary>
		/// <param name="address">PLC的地址信息</param>
		/// <param name="value">数据信息</param>
		/// <returns>是否写入成功</returns>
		public override OperateResult Write(string address, bool[] value)
		{
			OperateResult<byte[]> operateResult = MelsecFxLinksOverTcp.BuildWriteBoolCommand(station, address, value, sumCheck, watiingTime);
			if (!operateResult.IsSuccess)
			{
				return operateResult;
			}
			OperateResult<byte[]> operateResult2 = ReadBase(operateResult.Content);
			if (!operateResult2.IsSuccess)
			{
				return operateResult2;
			}
			if (operateResult2.Content[0] != 6)
			{
				return new OperateResult(operateResult2.Content[0], "Write Faild:" + SoftBasic.ByteToHexString(operateResult2.Content, ' '));
			}
			return OperateResult.CreateSuccessResult();
		}

		/// <summary>
		/// 启动PLC
		/// </summary>
		/// <returns>是否启动成功</returns>
		public OperateResult StartPLC()
		{
			OperateResult<byte[]> operateResult = MelsecFxLinksOverTcp.BuildStart(station, sumCheck, watiingTime);
			if (!operateResult.IsSuccess)
			{
				return operateResult;
			}
			OperateResult<byte[]> operateResult2 = ReadBase(operateResult.Content);
			if (!operateResult2.IsSuccess)
			{
				return operateResult2;
			}
			if (operateResult2.Content[0] != 6)
			{
				return new OperateResult(operateResult2.Content[0], "Start Faild:" + SoftBasic.ByteToHexString(operateResult2.Content, ' '));
			}
			return OperateResult.CreateSuccessResult();
		}

		/// <summary>
		/// 停止PLC
		/// </summary>
		/// <returns>是否停止成功</returns>
		public OperateResult StopPLC()
		{
			OperateResult<byte[]> operateResult = MelsecFxLinksOverTcp.BuildStop(station, sumCheck, watiingTime);
			if (!operateResult.IsSuccess)
			{
				return operateResult;
			}
			OperateResult<byte[]> operateResult2 = ReadBase(operateResult.Content);
			if (!operateResult2.IsSuccess)
			{
				return operateResult2;
			}
			if (operateResult2.Content[0] != 6)
			{
				return new OperateResult(operateResult2.Content[0], "Stop Faild:" + SoftBasic.ByteToHexString(operateResult2.Content, ' '));
			}
			return OperateResult.CreateSuccessResult();
		}
	}
}
