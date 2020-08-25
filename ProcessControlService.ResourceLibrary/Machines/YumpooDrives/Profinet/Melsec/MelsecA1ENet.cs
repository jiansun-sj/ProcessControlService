using System;
using System.Linq;
using YumpooDrive.Core;
using YumpooDrive.Core.IMessage;
using YumpooDrive.Core.Net;

namespace YumpooDrive.Profinet.Melsec
{
	/// <summary>
	/// 三菱PLC通讯协议，采用A兼容1E帧协议实现，使用二进制码通讯，请根据实际型号来进行选取
	/// </summary>
	/// <remarks>
	/// 本类适用于的PLC列表
	/// <list type="number">
	/// <item>FX3U(C) PLC   测试人sandy_liao</item>
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
	/// <note type="important">本通讯类由CKernal推送，感谢</note>
	/// </remarks>
	public class MelsecA1ENet : NetworkDeviceBase<MelsecA1EBinaryMessage, RegularByteTransform>
	{
		/// <summary>
		/// PLC编号
		/// </summary>
		public byte PLCNumber
		{
			get;
			set;
		} = byte.MaxValue;


		/// <summary>
		/// 实例化三菱的A兼容1E帧协议的通讯对象
		/// </summary>
		public MelsecA1ENet()
		{
			base.WordLength = 1;
		}

		/// <summary>
		/// 实例化一个三菱的A兼容1E帧协议的通讯对象
		/// </summary>
		/// <param name="ipAddress">PLC的Ip地址</param>
		/// <param name="port">PLC的端口</param>
		public MelsecA1ENet(string ipAddress, int port)
		{
			base.WordLength = 1;
			IpAddress = ipAddress;
			Port = port;
		}

		/// <summary>
		/// 从三菱PLC中读取想要的数据，返回读取结果
		/// </summary>
		/// <param name="address">读取地址，格式为"M100","D100","W1A0"</param>
		/// <param name="length">读取的数据长度，字最大值960，位最大值7168</param>
		/// <returns>带成功标志的结果数据对象</returns>
		public override OperateResult<byte[]> Read(string address, ushort length)
		{
			OperateResult<byte[]> operateResult = BuildReadCommand(address, length, isBit: false, PLCNumber);
			if (!operateResult.IsSuccess)
			{
				return OperateResult.CreateFailedResult<byte[]>(operateResult);
			}
			OperateResult<byte[]> operateResult2 = ReadFromCoreServer(operateResult.Content);
			if (!operateResult2.IsSuccess)
			{
				return OperateResult.CreateFailedResult<byte[]>(operateResult2);
			}
			if (operateResult2.Content[1] != 0)
			{
				return new OperateResult<byte[]>(operateResult2.Content[1], StringResources.Language.MelsecPleaseReferToManulDocument);
			}
			return ExtractActualData(operateResult2.Content, isBit: false);
		}

		/// <summary>
		/// 从三菱PLC中批量读取位软元件，返回读取结果
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <param name="length">读取的长度</param>
		/// <returns>带成功标志的结果数据对象</returns>
		public override OperateResult<bool[]> ReadBool(string address, ushort length)
		{
			OperateResult<byte[]> operateResult = BuildReadCommand(address, length, isBit: true, PLCNumber);
			if (!operateResult.IsSuccess)
			{
				return OperateResult.CreateFailedResult<bool[]>(operateResult);
			}
			OperateResult<byte[]> operateResult2 = ReadFromCoreServer(operateResult.Content);
			if (!operateResult2.IsSuccess)
			{
				return OperateResult.CreateFailedResult<bool[]>(operateResult2);
			}
			if (operateResult2.Content[1] != 0)
			{
				return new OperateResult<bool[]>(operateResult2.Content[1], StringResources.Language.MelsecPleaseReferToManulDocument);
			}
			OperateResult<byte[]> operateResult3 = ExtractActualData(operateResult2.Content, isBit: true);
			if (!operateResult3.IsSuccess)
			{
				return OperateResult.CreateFailedResult<bool[]>(operateResult3);
			}
			return OperateResult.CreateSuccessResult(operateResult3.Content.Select((byte m) => m == 1).Take(length).ToArray());
		}

		/// <summary>
		/// 向PLC写入数据，数据格式为原始的字节类型
		/// </summary>
		/// <param name="address">初始地址</param>
		/// <param name="value">原始的字节数据</param>
		/// <returns>返回写入结果</returns>
		public override OperateResult Write(string address, byte[] value)
		{
			OperateResult<byte[]> operateResult = BuildWriteCommand(address, value, PLCNumber);
			if (!operateResult.IsSuccess)
			{
				return operateResult;
			}
			OperateResult<byte[]> operateResult2 = ReadFromCoreServer(operateResult.Content);
			if (!operateResult2.IsSuccess)
			{
				return operateResult2;
			}
			if (operateResult2.Content[1] != 0)
			{
				return new OperateResult(operateResult2.Content[1], StringResources.Language.MelsecPleaseReferToManulDocument);
			}
			return OperateResult.CreateSuccessResult();
		}

		/// <summary>
		/// 向PLC中位软元件写入bool数组，返回值说明，比如你写入M100,values[0]对应M100
		/// </summary>
		/// <param name="address">要写入的数据地址</param>
		/// <param name="values">要写入的实际数据，可以指定任意的长度</param>
		/// <returns>返回写入结果</returns>
		public override OperateResult Write(string address, bool[] values)
		{
			return Write(address, values.Select((bool m) => (byte)(m ? 1 : 0)).ToArray());
		}

		/// <summary>                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      
		/// 返回表示当前对象的字符串
		/// </summary>
		/// <returns>字符串信息</returns>
		public override string ToString()
		{
			return $"MelsecA1ENet[{IpAddress}:{Port}]";
		}

		/// <summary>
		/// 根据类型地址长度确认需要读取的指令头
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <param name="length">长度</param>
		/// <param name="isBit">指示是否按照位成批的读出</param>
		/// <param name="plcNumber">PLC编号</param>
		/// <returns>带有成功标志的指令数据</returns>
		public static OperateResult<byte[]> BuildReadCommand(string address, ushort length, bool isBit, byte plcNumber)
		{
			OperateResult<MelsecA1EDataType, ushort> operateResult = MelsecHelper.McA1EAnalysisAddress(address);
			if (!operateResult.IsSuccess)
			{
				return OperateResult.CreateFailedResult<byte[]>(operateResult);
			}
			byte b = (byte)((!isBit) ? 1 : 0);
			return OperateResult.CreateSuccessResult(new byte[12]
			{
				b,
				plcNumber,
				10,
				0,
				(byte)((int)operateResult.Content2 % 256),
				(byte)((int)operateResult.Content2 / 256),
				0,
				0,
				operateResult.Content1.DataCode[1],
				operateResult.Content1.DataCode[0],
				(byte)((int)length % 256),
				0
			});
		}

		/// <summary>
		/// 根据类型地址以及需要写入的数据来生成指令头
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <param name="value">数据值</param>
		/// <param name="plcNumber">PLC编号</param>
		/// <returns>带有成功标志的指令数据</returns>
		public static OperateResult<byte[]> BuildWriteCommand(string address, byte[] value, byte plcNumber)
		{
			OperateResult<MelsecA1EDataType, ushort> operateResult = MelsecHelper.McA1EAnalysisAddress(address);
			if (!operateResult.IsSuccess)
			{
				return OperateResult.CreateFailedResult<byte[]>(operateResult);
			}
			int num = -1;
			if (operateResult.Content1.DataType == 1)
			{
				num = value.Length;
				value = MelsecHelper.TransBoolArrayToByteData(value);
			}
			byte b = (byte)((operateResult.Content1.DataType == 1) ? 2 : 3);
			byte[] array = new byte[12 + value.Length];
			array[0] = b;
			array[1] = plcNumber;
			array[2] = 10;
			array[3] = 0;
			array[4] = (byte)((int)operateResult.Content2 % 256);
			array[5] = (byte)((int)operateResult.Content2 / 256);
			array[6] = 0;
			array[7] = 0;
			array[8] = operateResult.Content1.DataCode[1];
			array[9] = operateResult.Content1.DataCode[0];
			array[10] = (byte)(num % 256);
			array[11] = 0;
			if (operateResult.Content1.DataType == 1)
			{
				if (num > 0)
				{
					array[10] = (byte)(num % 256);
				}
				else
				{
					array[10] = (byte)(value.Length * 2 % 256);
				}
			}
			else
			{
				array[10] = (byte)(value.Length / 2 % 256);
			}
			Array.Copy(value, 0, array, 12, value.Length);
			return OperateResult.CreateSuccessResult(array);
		}

		/// <summary>
		/// 从PLC反馈的数据中提取出实际的数据内容，需要传入反馈数据，是否位读取
		/// </summary>
		/// <param name="response">反馈的数据内容</param>
		/// <param name="isBit">是否位读取</param>
		/// <returns>解析后的结果对象</returns>
		public static OperateResult<byte[]> ExtractActualData(byte[] response, bool isBit)
		{
			if (isBit)
			{
				byte[] array = new byte[(response.Length - 2) * 2];
				for (int i = 2; i < response.Length; i++)
				{
					if ((response[i] & 0x10) == 16)
					{
						array[(i - 2) * 2] = 1;
					}
					if ((response[i] & 1) == 1)
					{
						array[(i - 2) * 2 + 1] = 1;
					}
				}
				return OperateResult.CreateSuccessResult(array);
			}
			byte[] array2 = new byte[response.Length - 2];
			Array.Copy(response, 2, array2, 0, array2.Length);
			return OperateResult.CreateSuccessResult(array2);
		}
	}
}
