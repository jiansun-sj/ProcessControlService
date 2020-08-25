using System;
using System.Linq;
using System.Text;
using YumpooDrive.BasicFramework;
using YumpooDrive.Core;
using YumpooDrive.Core.Address;
using YumpooDrive.Core.Net;

namespace YumpooDrive.Profinet.Melsec
{
	/// <summary>
	/// 基于Qna 兼容3C帧的格式一的通讯，具体的地址需要参照三菱的基本地址，本类是基于tcp通讯的实现
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
	public class MelsecA3CNet1OverTcp : NetworkDeviceSoloBase<RegularByteTransform>
	{
		private byte station = 0;

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
		/// 实例化默认的构造方法
		/// </summary>
		public MelsecA3CNet1OverTcp()
		{
			base.WordLength = 1;
		}

		/// <summary>
		/// 实例化默认的构造方法
		/// </summary>
		/// <param name="ipAddress">Ip地址信息</param>
		/// <param name="port">端口号信息</param>
		public MelsecA3CNet1OverTcp(string ipAddress, int port)
		{
			base.WordLength = 1;
			IpAddress = ipAddress;
			Port = port;
		}

		private OperateResult<byte[]> ReadWithPackCommand(byte[] command)
		{
			return ReadFromCoreServer(PackCommand(command, station));
		}

		/// <summary>
		/// 批量读取PLC的数据，以字为单位，支持读取X,Y,M,S,D,T,C，具体的地址范围需要根据PLC型号来确认
		/// </summary>
		/// <param name="address">地址信息</param>
		/// <param name="length">数据长度</param>
		/// <returns>读取结果信息</returns>
		public override OperateResult<byte[]> Read(string address, ushort length)
		{
			return ReadHelper(address, length, ReadWithPackCommand);
		}

		/// <summary>
		/// 批量写入PLC的数据，以字为单位，也就是说最少2个字节信息，支持X,Y,M,S,D,T,C，具体的地址范围需要根据PLC型号来确认
		/// </summary>
		/// <param name="address">地址信息</param>
		/// <param name="value">数据值</param>
		/// <returns>是否写入成功</returns>
		public override OperateResult Write(string address, byte[] value)
		{
			return WriteHelper(address, value, ReadWithPackCommand);
		}

		/// <summary>
		/// 批量读取bool类型数据，支持的类型为X,Y,S,T,C，具体的地址范围取决于PLC的类型
		/// </summary>
		/// <param name="address">地址信息，比如X10,Y17，注意X，Y的地址是8进制的</param>
		/// <param name="length">读取的长度</param>
		/// <returns>读取结果信息</returns>
		public override OperateResult<bool[]> ReadBool(string address, ushort length)
		{
			return ReadBoolHelper(address, length, ReadWithPackCommand);
		}

		/// <summary>
		/// 批量写入bool类型的数组，支持的类型为X,Y,S,T,C，具体的地址范围取决于PLC的类型
		/// </summary>
		/// <param name="address">PLC的地址信息</param>
		/// <param name="value">数据信息</param>
		/// <returns>是否写入成功</returns>
		public override OperateResult Write(string address, bool[] value)
		{
			return WriteHelper(address, value, ReadWithPackCommand);
		}

		/// <summary>
		/// 远程Run操作
		/// </summary>
		/// <returns>是否成功</returns>
		public OperateResult RemoteRun()
		{
			return RemoteRunHelper(ReadWithPackCommand);
		}

		/// <summary>
		/// 远程Stop操作
		/// </summary>
		/// <returns>是否成功</returns>
		public OperateResult RemoteStop()
		{
			return RemoteStopHelper(ReadWithPackCommand);
		}

		/// <summary>
		/// 读取PLC的型号信息
		/// </summary>
		/// <returns>返回型号的结果对象</returns>
		public OperateResult<string> ReadPlcType()
		{
			return ReadPlcTypeHelper(ReadWithPackCommand);
		}

		/// <summary>
		/// 返回表示当前对象的字符串
		/// </summary>
		/// <returns>字符串信息</returns>
		public override string ToString()
		{
			return $"MelsecA3CNet1OverTcp[{IpAddress}:{Port}]";
		}

		/// <summary>
		/// 批量读取PLC的数据，以字为单位，支持读取X,Y,M,S,D,T,C，具体的地址范围需要根据PLC型号来确认
		/// </summary>
		/// <param name="address">地址信息</param>
		/// <param name="length">数据长度</param>
		/// <param name="readCore">通信的载体信息</param>
		/// <returns>读取结果信息</returns>
		public static OperateResult<byte[]> ReadHelper(string address, ushort length, Func<byte[], OperateResult<byte[]>> readCore)
		{
			OperateResult<McAddressData> operateResult = McAddressData.ParseMelsecFrom(address, length);
			if (!operateResult.IsSuccess)
			{
				return OperateResult.CreateFailedResult<byte[]>(operateResult);
			}
			byte[] arg = MelsecHelper.BuildAsciiReadMcCoreCommand(operateResult.Content, isBit: false);
			OperateResult<byte[]> operateResult2 = readCore(arg);
			if (!operateResult2.IsSuccess)
			{
				return operateResult2;
			}
			if (operateResult2.Content[0] != 2)
			{
				return new OperateResult<byte[]>(operateResult2.Content[0], "Read Faild:" + Encoding.ASCII.GetString(operateResult2.Content, 1, operateResult2.Content.Length - 1));
			}
			byte[] array = new byte[length * 2];
			for (int i = 0; i < array.Length / 2; i++)
			{
				ushort value = Convert.ToUInt16(Encoding.ASCII.GetString(operateResult2.Content, i * 4 + 11, 4), 16);
				BitConverter.GetBytes(value).CopyTo(array, i * 2);
			}
			return OperateResult.CreateSuccessResult(array);
		}

		/// <summary>
		/// 批量写入PLC的数据，以字为单位，也就是说最少2个字节信息，支持X,Y,M,S,D,T,C，具体的地址范围需要根据PLC型号来确认
		/// </summary>
		/// <param name="address">地址信息</param>
		/// <param name="value">数据值</param>
		/// <param name="readCore">通信的载体信息</param>
		/// <returns>是否写入成功</returns>
		public static OperateResult WriteHelper(string address, byte[] value, Func<byte[], OperateResult<byte[]>> readCore)
		{
			OperateResult<McAddressData> operateResult = McAddressData.ParseMelsecFrom(address, 0);
			if (!operateResult.IsSuccess)
			{
				return OperateResult.CreateFailedResult<byte[]>(operateResult);
			}
			byte[] arg = MelsecHelper.BuildAsciiWriteWordCoreCommand(operateResult.Content, value);
			OperateResult<byte[]> operateResult2 = readCore(arg);
			if (!operateResult2.IsSuccess)
			{
				return operateResult2;
			}
			if (operateResult2.Content[0] != 6)
			{
				return new OperateResult(operateResult2.Content[0], "Write Faild:" + Encoding.ASCII.GetString(operateResult2.Content, 1, operateResult2.Content.Length - 1));
			}
			return OperateResult.CreateSuccessResult();
		}

		/// <summary>
		/// 批量读取bool类型数据，支持的类型为X,Y,S,T,C，具体的地址范围取决于PLC的类型
		/// </summary>
		/// <param name="address">地址信息，比如X10,Y17，注意X，Y的地址是8进制的</param>
		/// <param name="length">读取的长度</param>
		/// <param name="readCore">通信的载体信息</param>
		/// <returns>读取结果信息</returns>
		public static OperateResult<bool[]> ReadBoolHelper(string address, ushort length, Func<byte[], OperateResult<byte[]>> readCore)
		{
			OperateResult<McAddressData> operateResult = McAddressData.ParseMelsecFrom(address, length);
			if (!operateResult.IsSuccess)
			{
				return OperateResult.CreateFailedResult<bool[]>(operateResult);
			}
			byte[] arg = MelsecHelper.BuildAsciiReadMcCoreCommand(operateResult.Content, isBit: true);
			OperateResult<byte[]> operateResult2 = readCore(arg);
			if (!operateResult2.IsSuccess)
			{
				return OperateResult.CreateFailedResult<bool[]>(operateResult2);
			}
			if (operateResult2.Content[0] != 2)
			{
				return new OperateResult<bool[]>(operateResult2.Content[0], "Read Faild:" + Encoding.ASCII.GetString(operateResult2.Content, 1, operateResult2.Content.Length - 1));
			}
			byte[] array = new byte[length];
			Array.Copy(operateResult2.Content, 11, array, 0, length);
			return OperateResult.CreateSuccessResult(array.Select((byte m) => m == 49).ToArray());
		}

		/// <summary>
		/// 批量写入bool类型的数组，支持的类型为X,Y,S,T,C，具体的地址范围取决于PLC的类型
		/// </summary>
		/// <param name="address">PLC的地址信息</param>
		/// <param name="value">数据信息</param>
		/// <param name="readCore">通信的载体信息</param>
		/// <returns>是否写入成功</returns>
		public static OperateResult WriteHelper(string address, bool[] value, Func<byte[], OperateResult<byte[]>> readCore)
		{
			OperateResult<McAddressData> operateResult = McAddressData.ParseMelsecFrom(address, 0);
			if (!operateResult.IsSuccess)
			{
				return OperateResult.CreateFailedResult<bool[]>(operateResult);
			}
			byte[] arg = MelsecHelper.BuildAsciiWriteBitCoreCommand(operateResult.Content, value);
			OperateResult<byte[]> operateResult2 = readCore(arg);
			if (!operateResult2.IsSuccess)
			{
				return operateResult2;
			}
			if (operateResult2.Content[0] != 6)
			{
				return new OperateResult(operateResult2.Content[0], "Write Faild:" + Encoding.ASCII.GetString(operateResult2.Content, 1, operateResult2.Content.Length - 1));
			}
			return OperateResult.CreateSuccessResult();
		}

		/// <summary>
		/// 远程Run操作
		/// </summary>
		/// <param name="readCore">通信的载体信息</param>
		/// <returns>是否成功</returns>
		public static OperateResult RemoteRunHelper(Func<byte[], OperateResult<byte[]>> readCore)
		{
			OperateResult<byte[]> operateResult = readCore(Encoding.ASCII.GetBytes("1001000000010000"));
			if (!operateResult.IsSuccess)
			{
				return operateResult;
			}
			if (operateResult.Content[0] != 6 && operateResult.Content[0] != 2)
			{
				return new OperateResult(operateResult.Content[0], "Faild:" + Encoding.ASCII.GetString(operateResult.Content, 1, operateResult.Content.Length - 1));
			}
			return OperateResult.CreateSuccessResult();
		}

		/// <summary>
		/// 远程Stop操作
		/// </summary>
		/// <param name="readCore">通信的载体信息</param>
		/// <returns>是否成功</returns>
		public static OperateResult RemoteStopHelper(Func<byte[], OperateResult<byte[]>> readCore)
		{
			OperateResult<byte[]> operateResult = readCore(Encoding.ASCII.GetBytes("100200000001"));
			if (!operateResult.IsSuccess)
			{
				return operateResult;
			}
			if (operateResult.Content[0] != 6 && operateResult.Content[0] != 2)
			{
				return new OperateResult(operateResult.Content[0], "Faild:" + Encoding.ASCII.GetString(operateResult.Content, 1, operateResult.Content.Length - 1));
			}
			return OperateResult.CreateSuccessResult();
		}

		/// <summary>
		/// 读取PLC的型号信息
		/// </summary>
		/// <param name="readCore">通信的载体信息</param>
		/// <returns>返回型号的结果对象</returns>
		public static OperateResult<string> ReadPlcTypeHelper(Func<byte[], OperateResult<byte[]>> readCore)
		{
			OperateResult<byte[]> operateResult = readCore(Encoding.ASCII.GetBytes("01010000"));
			if (!operateResult.IsSuccess)
			{
				return OperateResult.CreateFailedResult<string>(operateResult);
			}
			if (operateResult.Content[0] != 6 && operateResult.Content[0] != 2)
			{
				return new OperateResult<string>(operateResult.Content[0], "Faild:" + Encoding.ASCII.GetString(operateResult.Content, 1, operateResult.Content.Length - 1));
			}
			return OperateResult.CreateSuccessResult(Encoding.ASCII.GetString(operateResult.Content, 11, 16).TrimEnd());
		}

		/// <summary>
		/// 将命令进行打包传送
		/// </summary>
		/// <param name="mcCommand">mc协议的命令</param>
		/// <param name="station">PLC的站号</param>
		/// <returns>最终的原始报文信息</returns>
		public static byte[] PackCommand(byte[] mcCommand, byte station = 0)
		{
			byte[] array = new byte[13 + mcCommand.Length];
			array[0] = 5;
			array[1] = 70;
			array[2] = 57;
			array[3] = SoftBasic.BuildAsciiBytesFrom(station)[0];
			array[4] = SoftBasic.BuildAsciiBytesFrom(station)[1];
			array[5] = 48;
			array[6] = 48;
			array[7] = 70;
			array[8] = 70;
			array[9] = 48;
			array[10] = 48;
			mcCommand.CopyTo(array, 11);
			int num = 0;
			for (int i = 1; i < array.Length - 3; i++)
			{
				num += array[i];
			}
			array[array.Length - 2] = SoftBasic.BuildAsciiBytesFrom((byte)num)[0];
			array[array.Length - 1] = SoftBasic.BuildAsciiBytesFrom((byte)num)[1];
			return array;
		}
	}
}
