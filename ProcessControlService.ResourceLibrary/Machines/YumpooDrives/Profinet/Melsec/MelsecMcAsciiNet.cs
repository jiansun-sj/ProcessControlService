using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YumpooDrive.BasicFramework;
using YumpooDrive.Core;
using YumpooDrive.Core.Address;
using YumpooDrive.Core.IMessage;
using YumpooDrive.Core.Net;

namespace YumpooDrive.Profinet.Melsec
{
	/// <summary>
	/// 三菱PLC通讯类，采用Qna兼容3E帧协议实现，需要在PLC侧先的以太网模块先进行配置，必须为ASCII通讯格式
	/// </summary>
	/// <remarks>
	/// 地址的输入的格式说明如下：
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
	///     <term>X100,X1A0</term>
	///     <term>16</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>输出继电器</term>
	///     <term>Y</term>
	///     <term>Y100,Y1A0</term>
	///     <term>16</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///    <item>
	///     <term>锁存继电器</term>
	///     <term>L</term>
	///     <term>L100,L200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>报警器</term>
	///     <term>F</term>
	///     <term>F100,F200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>边沿继电器</term>
	///     <term>V</term>
	///     <term>V100,V200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>链接继电器</term>
	///     <term>B</term>
	///     <term>B100,B1A0</term>
	///     <term>16</term>
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
	///     <term>链接寄存器</term>
	///     <term>W</term>
	///     <term>W100,W1A0</term>
	///     <term>16</term>
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
	///   <item>
	///     <term>ZR文件寄存器</term>
	///     <term>ZR</term>
	///     <term>ZR100,ZR2A0</term>
	///     <term>16</term>
	///     <term>√</term>
	///     <term>×</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>变址寄存器</term>
	///     <term>Z</term>
	///     <term>Z100,Z200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>×</term>
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
	///     <term>定时器的线圈</term>
	///     <term>TC</term>
	///     <term>TC100,TC200</term>
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
	///     <term>累计定时器的触点</term>
	///     <term>SS</term>
	///     <term>SS100,SS200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>累计定时器的线圈</term>
	///     <term>SC</term>
	///     <term>SC100,SC200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>累计定时器的当前值</term>
	///     <term>SN</term>
	///     <term>SN100,SN200</term>
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
	///     <term>计数器的线圈</term>
	///     <term>CC</term>
	///     <term>CC100,CC200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>√</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>计数器的当前值</term>
	///     <term>CN</term>
	///     <term>CN100,CN200</term>
	///     <term>10</term>
	///     <term>√</term>
	///     <term>×</term>
	///     <term></term>
	///   </item>
	/// </list>
	/// </remarks>
	/// <example>
	/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Profinet\MelsecAscii.cs" region="Usage" title="简单的短连接使用" />
	/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Profinet\MelsecAscii.cs" region="Usage2" title="简单的长连接使用" />
	/// </example>
	public class MelsecMcAsciiNet : NetworkDeviceBase<MelsecQnA3EAsciiMessage, RegularByteTransform>
	{
		/// <summary>
		/// 网络号
		/// </summary>
		public byte NetworkNumber
		{
			get;
			set;
		} = 0;


		/// <summary>
		/// 网络站号
		/// </summary>
		public byte NetworkStationNumber
		{
			get;
			set;
		} = 0;


		/// <summary>
		/// 实例化三菱的Qna兼容3E帧协议的通讯对象
		/// </summary>
		public MelsecMcAsciiNet()
		{
			base.WordLength = 1;
		}

		/// <summary>
		/// 实例化一个三菱的Qna兼容3E帧协议的通讯对象
		/// </summary>
		/// <param name="ipAddress">PLC的Ip地址</param>
		/// <param name="port">PLC的端口</param>
		public MelsecMcAsciiNet(string ipAddress, int port)
		{
			base.WordLength = 1;
			IpAddress = ipAddress;
			Port = port;
		}

		/// <summary>
		/// 分析地址的方法，允许派生类里进行重写操作
		/// </summary>
		/// <param name="address">地址信息</param>
		/// <param name="length">数据长度</param>
		/// <returns>解析后的数据信息</returns>
		protected virtual OperateResult<McAddressData> McAnalysisAddress(string address, ushort length)
		{
			return McAddressData.ParseMelsecFrom(address, length);
		}

		/// <summary>
		/// 从三菱PLC中读取想要的数据，返回读取结果，读取的单位为字
		/// </summary>
		/// <param name="address">读取地址，格式为"M100","D100","W1A0"</param>
		/// <param name="length">读取的数据长度，字最大值960，位最大值7168</param>
		/// <returns>带成功标志的结果数据对象</returns>
		/// <remarks>
		/// 地址支持的列表参考 <seealso cref="T:YumpooDrive.Profinet.Melsec.MelsecMcAsciiNet" /> 的备注说明
		/// </remarks>
		/// <example>
		/// 假设起始地址为D100，D100存储了温度，100.6℃值为1006，D101存储了压力，1.23Mpa值为123，D102，D103存储了产量计数，读取如下：
		/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Profinet\MelsecAscii.cs" region="ReadExample2" title="Read示例" />
		/// 以下是读取不同类型数据的示例
		/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Profinet\MelsecAscii.cs" region="ReadExample1" title="Read示例" />
		/// </example>
		public override OperateResult<byte[]> Read(string address, ushort length)
		{
			OperateResult<McAddressData> operateResult = McAnalysisAddress(address, length);
			if (!operateResult.IsSuccess)
			{
				return OperateResult.CreateFailedResult<byte[]>(operateResult);
			}
			List<byte> list = new List<byte>();
			ushort num = 0;
			while (num < length)
			{
				ushort num2 = (ushort)Math.Min(length - num, 450);
				operateResult.Content.Length = num2;
				OperateResult<byte[]> operateResult2 = ReadAddressData(operateResult.Content);
				if (!operateResult2.IsSuccess)
				{
					return operateResult2;
				}
				list.AddRange(operateResult2.Content);
				num = (ushort)(num + num2);
				if (operateResult.Content.McDataType.DataType == 0)
				{
					operateResult.Content.AddressStart += num2;
				}
				else
				{
					operateResult.Content.AddressStart += num2 * 16;
				}
			}
			return OperateResult.CreateSuccessResult(list.ToArray());
		}

		private OperateResult<byte[]> ReadAddressData(McAddressData addressData)
		{
			byte[] mcCore = MelsecHelper.BuildAsciiReadMcCoreCommand(addressData, isBit: false);
			OperateResult<byte[]> operateResult = ReadFromCoreServer(PackMcCommand(mcCore, NetworkNumber, NetworkStationNumber));
			if (!operateResult.IsSuccess)
			{
				return OperateResult.CreateFailedResult<byte[]>(operateResult);
			}
			ushort num = Convert.ToUInt16(Encoding.ASCII.GetString(operateResult.Content, 18, 4), 16);
			if (num != 0)
			{
				return new OperateResult<byte[]>(num, StringResources.Language.MelsecPleaseReferToManulDocument);
			}
			return ExtractActualData(operateResult.Content, isBit: false);
		}

		/// <summary>
		/// 向PLC写入数据，数据格式为原始的字节类型
		/// </summary>
		/// <param name="address">初始地址</param>
		/// <param name="value">原始的字节数据</param>
		/// <example>
		/// 假设起始地址为D100，D100存储了温度，100.6℃值为1006，D101存储了压力，1.23Mpa值为123，D102，D103存储了产量计数，写入如下：
		/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Profinet\MelsecAscii.cs" region="WriteExample2" title="Write示例" />
		/// 以下是读取不同类型数据的示例
		/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Profinet\MelsecAscii.cs" region="WriteExample1" title="Write示例" />
		/// </example>
		/// <returns>结果</returns>
		public override OperateResult Write(string address, byte[] value)
		{
			OperateResult<McAddressData> operateResult = McAnalysisAddress(address, 0);
			if (!operateResult.IsSuccess)
			{
				return OperateResult.CreateFailedResult<byte[]>(operateResult);
			}
			byte[] mcCore = MelsecHelper.BuildAsciiWriteWordCoreCommand(operateResult.Content, value);
			OperateResult<byte[]> operateResult2 = ReadFromCoreServer(PackMcCommand(mcCore, NetworkNumber, NetworkStationNumber));
			if (!operateResult2.IsSuccess)
			{
				return operateResult2;
			}
			ushort num = Convert.ToUInt16(Encoding.ASCII.GetString(operateResult2.Content, 18, 4), 16);
			if (num != 0)
			{
				return new OperateResult<byte[]>(num, StringResources.Language.MelsecPleaseReferToManulDocument);
			}
			return OperateResult.CreateSuccessResult();
		}

		/// <summary>
		/// 从三菱PLC中批量读取位软元件，返回读取结果
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <param name="length">读取的长度</param>
		/// <returns>带成功标志的结果数据对象</returns>
		/// <remarks>
		/// 地址支持的列表参考 <seealso cref="T:YumpooDrive.Profinet.Melsec.MelsecMcAsciiNet" /> 的备注说明
		/// </remarks>
		/// <example>
		/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Profinet\MelsecAscii.cs" region="ReadBool" title="Bool类型示例" />
		/// </example>
		public override OperateResult<bool[]> ReadBool(string address, ushort length)
		{
			OperateResult<McAddressData> operateResult = McAnalysisAddress(address, length);
			if (!operateResult.IsSuccess)
			{
				return OperateResult.CreateFailedResult<bool[]>(operateResult);
			}
			byte[] mcCore = MelsecHelper.BuildAsciiReadMcCoreCommand(operateResult.Content, isBit: true);
			OperateResult<byte[]> operateResult2 = ReadFromCoreServer(PackMcCommand(mcCore, NetworkNumber, NetworkStationNumber));
			if (!operateResult2.IsSuccess)
			{
				return OperateResult.CreateFailedResult<bool[]>(operateResult2);
			}
			ushort num = Convert.ToUInt16(Encoding.ASCII.GetString(operateResult2.Content, 18, 4), 16);
			if (num != 0)
			{
				return new OperateResult<bool[]>(num, StringResources.Language.MelsecPleaseReferToManulDocument);
			}
			OperateResult<byte[]> operateResult3 = ExtractActualData(operateResult2.Content, isBit: true);
			if (!operateResult3.IsSuccess)
			{
				return OperateResult.CreateFailedResult<bool[]>(operateResult3);
			}
			return OperateResult.CreateSuccessResult(operateResult3.Content.Select((byte m) => m == 1).Take(length).ToArray());
		}

		/// <summary>
		/// 向PLC中位软元件写入bool数组，返回值说明，比如你写入M100,values[0]对应M100
		/// </summary>
		/// <param name="address">要写入的数据地址</param>
		/// <param name="values">要写入的实际数据，可以指定任意的长度</param>
		/// <example>
		/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Profinet\MelsecAscii.cs" region="WriteBool" title="Write示例" />
		/// </example>
		/// <returns>返回写入结果</returns>
		public override OperateResult Write(string address, bool[] values)
		{
			OperateResult<McAddressData> operateResult = McAnalysisAddress(address, 0);
			if (!operateResult.IsSuccess)
			{
				return operateResult;
			}
			byte[] mcCore = MelsecHelper.BuildAsciiWriteBitCoreCommand(operateResult.Content, values);
			OperateResult<byte[]> operateResult2 = ReadFromCoreServer(PackMcCommand(mcCore, NetworkNumber, NetworkStationNumber));
			if (!operateResult2.IsSuccess)
			{
				return operateResult2;
			}
			ushort num = Convert.ToUInt16(Encoding.ASCII.GetString(operateResult2.Content, 18, 4), 16);
			if (num != 0)
			{
				return new OperateResult<byte[]>(num, StringResources.Language.MelsecPleaseReferToManulDocument);
			}
			return OperateResult.CreateSuccessResult();
		}

		/// <summary>
		/// 远程Run操作
		/// </summary>
		/// <returns>是否成功</returns>
		public OperateResult RemoteRun()
		{
			OperateResult<byte[]> operateResult = ReadFromCoreServer(PackMcCommand(Encoding.ASCII.GetBytes("1001000000010000"), NetworkNumber, NetworkStationNumber));
			if (!operateResult.IsSuccess)
			{
				return operateResult;
			}
			ushort num = Convert.ToUInt16(Encoding.ASCII.GetString(operateResult.Content, 18, 4), 16);
			if (num != 0)
			{
				return new OperateResult(num, StringResources.Language.MelsecPleaseReferToManulDocument);
			}
			return OperateResult.CreateSuccessResult();
		}

		/// <summary>
		/// 远程Stop操作
		/// </summary>
		/// <returns>是否成功</returns>
		public OperateResult RemoteStop()
		{
			OperateResult<byte[]> operateResult = ReadFromCoreServer(PackMcCommand(Encoding.ASCII.GetBytes("100200000001"), NetworkNumber, NetworkStationNumber));
			if (!operateResult.IsSuccess)
			{
				return operateResult;
			}
			ushort num = Convert.ToUInt16(Encoding.ASCII.GetString(operateResult.Content, 18, 4), 16);
			if (num != 0)
			{
				return new OperateResult(num, StringResources.Language.MelsecPleaseReferToManulDocument);
			}
			return OperateResult.CreateSuccessResult();
		}

		/// <summary>
		/// 读取PLC的型号信息
		/// </summary>
		/// <returns>返回型号的结果对象</returns>
		public OperateResult<string> ReadPlcType()
		{
			OperateResult<byte[]> operateResult = ReadFromCoreServer(PackMcCommand(Encoding.ASCII.GetBytes("01010000"), NetworkNumber, NetworkStationNumber));
			if (!operateResult.IsSuccess)
			{
				return OperateResult.CreateFailedResult<string>(operateResult);
			}
			ushort num = Convert.ToUInt16(Encoding.ASCII.GetString(operateResult.Content, 18, 4), 16);
			if (num != 0)
			{
				return new OperateResult<string>(num, StringResources.Language.MelsecPleaseReferToManulDocument);
			}
			return OperateResult.CreateSuccessResult(Encoding.ASCII.GetString(operateResult.Content, 22, 16).TrimEnd());
		}

		/// <summary>
		/// 获取当前对象的字符串标识形式
		/// </summary>
		/// <returns>字符串信息</returns>
		public override string ToString()
		{
			return $"MelsecMcAsciiNet[{IpAddress}:{Port}]";
		}

		/// <summary>
		/// 将MC协议的核心报文打包成一个可以直接对PLC进行发送的原始报文
		/// </summary>
		/// <param name="mcCore">MC协议的核心报文</param>
		/// <param name="networkNumber">网络号</param>
		/// <param name="networkStationNumber">网络站号</param>
		/// <returns>原始报文信息</returns>
		public static byte[] PackMcCommand(byte[] mcCore, byte networkNumber = 0, byte networkStationNumber = 0)
		{
			byte[] array = new byte[22 + mcCore.Length];
			array[0] = 53;
			array[1] = 48;
			array[2] = 48;
			array[3] = 48;
			array[4] = SoftBasic.BuildAsciiBytesFrom(networkNumber)[0];
			array[5] = SoftBasic.BuildAsciiBytesFrom(networkNumber)[1];
			array[6] = 70;
			array[7] = 70;
			array[8] = 48;
			array[9] = 51;
			array[10] = 70;
			array[11] = 70;
			array[12] = SoftBasic.BuildAsciiBytesFrom(networkStationNumber)[0];
			array[13] = SoftBasic.BuildAsciiBytesFrom(networkStationNumber)[1];
			array[14] = SoftBasic.BuildAsciiBytesFrom((ushort)(array.Length - 18))[0];
			array[15] = SoftBasic.BuildAsciiBytesFrom((ushort)(array.Length - 18))[1];
			array[16] = SoftBasic.BuildAsciiBytesFrom((ushort)(array.Length - 18))[2];
			array[17] = SoftBasic.BuildAsciiBytesFrom((ushort)(array.Length - 18))[3];
			array[18] = 48;
			array[19] = 48;
			array[20] = 49;
			array[21] = 48;
			mcCore.CopyTo(array, 22);
			return array;
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
				byte[] array = new byte[response.Length - 22];
				for (int i = 22; i < response.Length; i++)
				{
					array[i - 22] = (byte)((response[i] != 48) ? 1 : 0);
				}
				return OperateResult.CreateSuccessResult(array);
			}
			byte[] array2 = new byte[(response.Length - 22) / 2];
			for (int j = 0; j < array2.Length / 2; j++)
			{
				ushort value = Convert.ToUInt16(Encoding.ASCII.GetString(response, j * 4 + 22, 4), 16);
				BitConverter.GetBytes(value).CopyTo(array2, j * 2);
			}
			return OperateResult.CreateSuccessResult(array2);
		}
	}
}
