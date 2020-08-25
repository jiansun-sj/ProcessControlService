namespace YumpooDrive.Profinet.Melsec
{
	/// <summary>
	/// 三菱PLC的数据类型，此处包含了几个常用的类型
	/// </summary>
	public class MelsecA1EDataType
	{
		/// <summary>
		/// X输入寄存器
		/// </summary>
		public static readonly MelsecA1EDataType X = new MelsecA1EDataType(new byte[2]
		{
			88,
			32
		}, 1, "X*", 8);

		/// <summary>
		/// Y输出寄存器
		/// </summary>
		public static readonly MelsecA1EDataType Y = new MelsecA1EDataType(new byte[2]
		{
			89,
			32
		}, 1, "Y*", 8);

		/// <summary>
		/// M中间寄存器
		/// </summary>
		public static readonly MelsecA1EDataType M = new MelsecA1EDataType(new byte[2]
		{
			77,
			32
		}, 1, "M*", 10);

		/// <summary>
		/// S状态寄存器
		/// </summary>
		public static readonly MelsecA1EDataType S = new MelsecA1EDataType(new byte[2]
		{
			83,
			32
		}, 1, "S*", 10);

		/// <summary>
		/// D数据寄存器
		/// </summary>
		public static readonly MelsecA1EDataType D = new MelsecA1EDataType(new byte[2]
		{
			68,
			32
		}, 0, "D*", 10);

		/// <summary>
		/// R文件寄存器
		/// </summary>
		public static readonly MelsecA1EDataType R = new MelsecA1EDataType(new byte[2]
		{
			82,
			32
		}, 0, "R*", 10);

		/// <summary>
		/// 类型的代号值（软元件代码，用于区分软元件类型，如：D，R）
		/// </summary>
		public byte[] DataCode
		{
			get;
			private set;
		} = new byte[2];


		/// <summary>
		/// 数据的类型，0代表按字，1代表按位
		/// </summary>
		public byte DataType
		{
			get;
			private set;
		} = 0;


		/// <summary>
		/// 当以ASCII格式通讯时的类型描述
		/// </summary>
		public string AsciiCode
		{
			get;
			private set;
		}

		/// <summary>
		/// 指示地址是10进制，还是16进制的
		/// </summary>
		public int FromBase
		{
			get;
			private set;
		}

		/// <summary>
		/// 如果您清楚类型代号，可以根据值进行扩展
		/// </summary>
		/// <param name="code">数据类型的代号</param>
		/// <param name="type">0或1，默认为0</param>
		/// <param name="asciiCode">ASCII格式的类型信息</param>
		/// <param name="fromBase">指示地址的多少进制的，10或是16</param>
		public MelsecA1EDataType(byte[] code, byte type, string asciiCode, int fromBase)
		{
			DataCode = code;
			AsciiCode = asciiCode;
			FromBase = fromBase;
			if (type < 2)
			{
				DataType = type;
			}
		}
	}
}
