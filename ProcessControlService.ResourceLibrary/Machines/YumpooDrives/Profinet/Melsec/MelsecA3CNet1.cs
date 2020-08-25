using YumpooDrive.Core;
using YumpooDrive.Serial;

namespace YumpooDrive.Profinet.Melsec
{
	/// <summary>
	/// 基于Qna 兼容3C帧的格式一的通讯，具体的地址需要参照三菱的基本地址
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
	public class MelsecA3CNet1 : SerialDeviceBase<RegularByteTransform>
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
		public MelsecA3CNet1()
		{
			base.WordLength = 1;
		}

		private OperateResult<byte[]> ReadWithPackCommand(byte[] command)
		{
			return ReadBase(MelsecA3CNet1OverTcp.PackCommand(command, station));
		}

		/// <summary>
		/// 批量读取PLC的数据，以字为单位，支持读取X,Y,M,S,D,T,C，具体的地址范围需要根据PLC型号来确认
		/// </summary>
		/// <param name="address">地址信息</param>
		/// <param name="length">数据长度</param>
		/// <returns>读取结果信息</returns>
		public override OperateResult<byte[]> Read(string address, ushort length)
		{
			return MelsecA3CNet1OverTcp.ReadHelper(address, length, ReadWithPackCommand);
		}

		/// <summary>
		/// 批量写入PLC的数据，以字为单位，也就是说最少2个字节信息，支持X,Y,M,S,D,T,C，具体的地址范围需要根据PLC型号来确认
		/// </summary>
		/// <param name="address">地址信息</param>
		/// <param name="value">数据值</param>
		/// <returns>是否写入成功</returns>
		public override OperateResult Write(string address, byte[] value)
		{
			return MelsecA3CNet1OverTcp.WriteHelper(address, value, ReadWithPackCommand);
		}

		/// <summary>
		/// 批量读取bool类型数据，支持的类型为X,Y,S,T,C，具体的地址范围取决于PLC的类型
		/// </summary>
		/// <param name="address">地址信息，比如X10,Y17，注意X，Y的地址是8进制的</param>
		/// <param name="length">读取的长度</param>
		/// <returns>读取结果信息</returns>
		public override OperateResult<bool[]> ReadBool(string address, ushort length)
		{
			return MelsecA3CNet1OverTcp.ReadBoolHelper(address, length, ReadWithPackCommand);
		}

		/// <summary>
		/// 批量写入bool类型的数组，支持的类型为X,Y,S,T,C，具体的地址范围取决于PLC的类型
		/// </summary>
		/// <param name="address">PLC的地址信息</param>
		/// <param name="value">数据信息</param>
		/// <returns>是否写入成功</returns>
		public override OperateResult Write(string address, bool[] value)
		{
			return MelsecA3CNet1OverTcp.WriteHelper(address, value, ReadWithPackCommand);
		}

		/// <summary>
		/// 远程Run操作
		/// </summary>
		/// <returns>是否成功</returns>
		public OperateResult RemoteRun()
		{
			return MelsecA3CNet1OverTcp.RemoteRunHelper(ReadWithPackCommand);
		}

		/// <summary>
		/// 远程Stop操作
		/// </summary>
		/// <returns>是否成功</returns>
		public OperateResult RemoteStop()
		{
			return MelsecA3CNet1OverTcp.RemoteStopHelper(ReadWithPackCommand);
		}

		/// <summary>
		/// 读取PLC的型号信息
		/// </summary>
		/// <returns>返回型号的结果对象</returns>
		public OperateResult<string> ReadPlcType()
		{
			return MelsecA3CNet1OverTcp.ReadPlcTypeHelper(ReadWithPackCommand);
		}

		/// <summary>
		/// 返回表示当前对象的字符串
		/// </summary>
		/// <returns>字符串信息</returns>
		public override string ToString()
		{
			return "MelsecA3CNet1";
		}
	}
}
