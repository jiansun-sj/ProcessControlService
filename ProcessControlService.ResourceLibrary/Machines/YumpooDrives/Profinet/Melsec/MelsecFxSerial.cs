using YumpooDrive.Core;
using YumpooDrive.Serial;

namespace YumpooDrive.Profinet.Melsec
{
	/// <summary>
	/// 三菱的串口通信的对象，适用于读取FX系列的串口数据，支持的类型参考文档说明
	/// </summary>
	/// <remarks>
	/// 字读写地址支持的列表如下：
	/// <list type="table">
	///   <listheader>
	///     <term>地址名称</term>
	///     <term>地址代号</term>
	///     <term>示例</term>
	///     <term>地址范围</term>
	///     <term>地址进制</term>
	///     <term>备注</term>
	///   </listheader>
	///   <item>
	///     <term>数据寄存器</term>
	///     <term>D</term>
	///     <term>D100,D200</term>
	///     <term>D0-D511,D8000-D8255</term>
	///     <term>10</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>定时器的值</term>
	///     <term>TN</term>
	///     <term>TN10,TN20</term>
	///     <term>TN0-TN255</term>
	///     <term>10</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>计数器的值</term>
	///     <term>CN</term>
	///     <term>CN10,CN20</term>
	///     <term>CN0-CN199,CN200-CN255</term>
	///     <term>10</term>
	///     <term></term>
	///   </item>
	/// </list>
	/// 位地址支持的列表如下：
	/// <list type="table">
	///   <listheader>
	///     <term>地址名称</term>
	///     <term>地址代号</term>
	///     <term>示例</term>
	///     <term>地址范围</term>
	///     <term>地址进制</term>
	///     <term>备注</term>
	///   </listheader>
	///   <item>
	///     <term>内部继电器</term>
	///     <term>M</term>
	///     <term>M100,M200</term>
	///     <term>M0-M1023,M8000-M8255</term>
	///     <term>10</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>输入继电器</term>
	///     <term>X</term>
	///     <term>X1,X20</term>
	///     <term>X0-X177</term>
	///     <term>8</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>输出继电器</term>
	///     <term>Y</term>
	///     <term>Y10,Y20</term>
	///     <term>Y0-Y177</term>
	///     <term>8</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>步进继电器</term>
	///     <term>S</term>
	///     <term>S100,S200</term>
	///     <term>S0-S999</term>
	///     <term>10</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>定时器触点</term>
	///     <term>TS</term>
	///     <term>TS10,TS20</term>
	///     <term>TS0-TS255</term>
	///     <term>10</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>定时器线圈</term>
	///     <term>TC</term>
	///     <term>TC10,TC20</term>
	///     <term>TC0-TC255</term>
	///     <term>10</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>计数器触点</term>
	///     <term>CS</term>
	///     <term>CS10,CS20</term>
	///     <term>CS0-CS255</term>
	///     <term>10</term>
	///     <term></term>
	///   </item>
	///   <item>
	///     <term>计数器线圈</term>
	///     <term>CC</term>
	///     <term>CC10,CC20</term>
	///     <term>CC0-CC255</term>
	///     <term>10</term>
	///     <term></term>
	///   </item>
	/// </list>
	/// </remarks>
	/// <example>
	/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Profinet\MelsecFxSerial.cs" region="Usage" title="简单的使用" />
	/// </example>
	public class MelsecFxSerial : SerialDeviceBase<RegularByteTransform>
	{
		/// <summary>
		/// 实例化三菱的串口协议的通讯对象
		/// </summary>
		public MelsecFxSerial()
		{
			base.WordLength = 1;
		}

		/// <summary>
		/// 从三菱PLC中读取想要的数据，返回读取结果
		/// </summary>
		/// <param name="address">读取地址，，支持的类型参考文档说明</param>
		/// <param name="length">读取的数据长度</param>
		/// <returns>带成功标志的结果数据对象</returns>
		/// <example>
		/// 假设起始地址为D100，D100存储了温度，100.6℃值为1006，D101存储了压力，1.23Mpa值为123，D102，D103存储了产量计数，读取如下：
		/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Profinet\MelsecFxSerial.cs" region="ReadExample2" title="Read示例" />
		/// 以下是读取不同类型数据的示例
		/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Profinet\MelsecFxSerial.cs" region="ReadExample1" title="Read示例" />
		/// </example>
		public override OperateResult<byte[]> Read(string address, ushort length)
		{
			return MelsecFxSerialOverTcp.ReadHelper(address, length, base.ReadBase);
		}

		/// <summary>
		/// 向PLC写入数据，数据格式为原始的字节类型
		/// </summary>
		/// <param name="address">初始地址，支持的类型参考文档说明</param>
		/// <param name="value">原始的字节数据</param>
		/// <example>
		/// 假设起始地址为D100，D100存储了温度，100.6℃值为1006，D101存储了压力，1.23Mpa值为123，D102，D103存储了产量计数，写入如下：
		/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Profinet\MelsecFxSerial.cs" region="WriteExample2" title="Write示例" />
		/// 以下是读取不同类型数据的示例
		/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Profinet\MelsecFxSerial.cs" region="WriteExample1" title="Write示例" />
		/// </example>
		/// <returns>是否写入成功的结果对象</returns>
		public override OperateResult Write(string address, byte[] value)
		{
			return MelsecFxSerialOverTcp.WriteHelper(address, value, base.ReadBase);
		}

		/// <summary>
		/// 从三菱PLC中批量读取位软元件，返回读取结果，该读取地址最好从0，16，32...等开始读取，这样可以读取比较长得数据数组
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <param name="length">读取的长度</param>
		/// <returns>带成功标志的结果数据对象</returns>
		/// <example>
		///  <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Profinet\MelsecFxSerial.cs" region="ReadBool" title="Bool类型示例" />
		/// </example>
		public override OperateResult<bool[]> ReadBool(string address, ushort length)
		{
			return MelsecFxSerialOverTcp.ReadBoolHelper(address, length, base.ReadBase);
		}

		/// <summary>
		/// 强制写入位数据的通断，支持的类型参考文档说明
		/// </summary>
		/// <param name="address">地址信息</param>
		/// <param name="value">是否为通</param>
		/// <returns>是否写入成功的结果对象</returns>
		public override OperateResult Write(string address, bool value)
		{
			return MelsecFxSerialOverTcp.WriteHelper(address, value, base.ReadBase);
		}

		/// <summary>
		/// 获取当前对象的字符串标识形式
		/// </summary>
		/// <returns>字符串信息</returns>
		public override string ToString()
		{
			return "MelsecFxSerial";
		}
	}
}
