using System;
using System.Linq;
using System.Text;
using YumpooDrive.BasicFramework;
using YumpooDrive.Core;
using YumpooDrive.Core.Net;

namespace YumpooDrive.Profinet.Melsec
{
	/// <summary>
	/// 三菱计算机链接协议的网口版本
	/// </summary>
	public class MelsecFxLinksOverTcp : NetworkDeviceSoloBase<RegularByteTransform>
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
		public MelsecFxLinksOverTcp()
		{
			base.WordLength = 1;
		}

		/// <summary>
		/// 实例化默认的构造方法
		/// </summary>
		/// <param name="ipAddress">Ip地址信息</param>
		/// <param name="port">端口号</param>
		public MelsecFxLinksOverTcp(string ipAddress, int port)
		{
			base.WordLength = 1;
			IpAddress = ipAddress;
			Port = port;
		}

		/// <summary>
		/// 批量读取PLC的数据，以字为单位，支持读取X,Y,M,S,D,T,C，具体的地址范围需要根据PLC型号来确认
		/// </summary>
		/// <param name="address">地址信息</param>
		/// <param name="length">数据长度</param>
		/// <returns>读取结果信息</returns>
		public override OperateResult<byte[]> Read(string address, ushort length)
		{
			OperateResult<byte[]> operateResult = BuildReadCommand(station, address, length, isBool: false, sumCheck, watiingTime);
			if (!operateResult.IsSuccess)
			{
				return OperateResult.CreateFailedResult<byte[]>(operateResult);
			}
			OperateResult<byte[]> operateResult2 = ReadFromCoreServer(operateResult.Content);
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
			OperateResult<byte[]> operateResult = BuildWriteByteCommand(station, address, value, sumCheck, watiingTime);
			if (!operateResult.IsSuccess)
			{
				return operateResult;
			}
			OperateResult<byte[]> operateResult2 = ReadFromCoreServer(operateResult.Content);
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
			OperateResult<byte[]> operateResult = BuildReadCommand(station, address, length, isBool: true, sumCheck, watiingTime);
			if (!operateResult.IsSuccess)
			{
				return OperateResult.CreateFailedResult<bool[]>(operateResult);
			}
			OperateResult<byte[]> operateResult2 = ReadFromCoreServer(operateResult.Content);
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
			OperateResult<byte[]> operateResult = BuildWriteBoolCommand(station, address, value, sumCheck, watiingTime);
			if (!operateResult.IsSuccess)
			{
				return operateResult;
			}
			OperateResult<byte[]> operateResult2 = ReadFromCoreServer(operateResult.Content);
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
			OperateResult<byte[]> operateResult = BuildStart(station, sumCheck, watiingTime);
			if (!operateResult.IsSuccess)
			{
				return operateResult;
			}
			OperateResult<byte[]> operateResult2 = ReadFromCoreServer(operateResult.Content);
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
			OperateResult<byte[]> operateResult = BuildStop(station, sumCheck, watiingTime);
			if (!operateResult.IsSuccess)
			{
				return operateResult;
			}
			OperateResult<byte[]> operateResult2 = ReadFromCoreServer(operateResult.Content);
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

		/// <summary>
		/// 解析数据地址成不同的三菱地址类型
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <returns>地址结果对象</returns>
		private static OperateResult<string> FxAnalysisAddress(string address)
		{
			OperateResult<string> operateResult = new OperateResult<string>();
			try
			{
				switch (address[0])
				{
					case 'X':
					case 'x':
						{
							ushort num2 = Convert.ToUInt16(address.Substring(1), 8);
							operateResult.Content = "X" + Convert.ToUInt16(address.Substring(1), 10).ToString("D4");
							break;
						}
					case 'Y':
					case 'y':
						{
							ushort num = Convert.ToUInt16(address.Substring(1), 8);
							operateResult.Content = "Y" + Convert.ToUInt16(address.Substring(1), 10).ToString("D4");
							break;
						}
					case 'M':
					case 'm':
						operateResult.Content = "M" + Convert.ToUInt16(address.Substring(1), 10).ToString("D4");
						break;
					case 'S':
					case 's':
						operateResult.Content = "S" + Convert.ToUInt16(address.Substring(1), 10).ToString("D4");
						break;
					case 'T':
					case 't':
						if (address[1] == 'S' || address[1] == 's')
						{
							operateResult.Content = "TS" + Convert.ToUInt16(address.Substring(1), 10).ToString("D3");
						}
						else
						{
							if (address[1] != 'N' && address[1] != 'n')
							{
								throw new Exception(StringResources.Language.NotSupportedDataType);
							}
							operateResult.Content = "TN" + Convert.ToUInt16(address.Substring(1), 10).ToString("D3");
						}
						break;
					case 'C':
					case 'c':
						if (address[1] == 'S' || address[1] == 's')
						{
							operateResult.Content = "CS" + Convert.ToUInt16(address.Substring(1), 10).ToString("D3");
						}
						else
						{
							if (address[1] != 'N' && address[1] != 'n')
							{
								throw new Exception(StringResources.Language.NotSupportedDataType);
							}
							operateResult.Content = "CN" + Convert.ToUInt16(address.Substring(1), 10).ToString("D3");
						}
						break;
					case 'D':
					case 'd':
						operateResult.Content = "D" + Convert.ToUInt16(address.Substring(1), 10).ToString("D4");
						break;
					case 'R':
					case 'r':
						operateResult.Content = "R" + Convert.ToUInt16(address.Substring(1), 10).ToString("D4");
						break;
					default:
						throw new Exception(StringResources.Language.NotSupportedDataType);
				}
			}
			catch (Exception ex)
			{
				operateResult.Message = ex.Message;
				return operateResult;
			}
			operateResult.IsSuccess = true;
			return operateResult;
		}

		/// <summary>
		/// 计算指令的和校验码
		/// </summary>
		/// <param name="data">指令</param>
		/// <returns>校验之后的信息</returns>
		public static string CalculateAcc(string data)
		{
			byte[] bytes = Encoding.ASCII.GetBytes(data);
			int num = 0;
			for (int i = 0; i < bytes.Length; i++)
			{
				num += bytes[i];
			}
			return data + num.ToString("X4").Substring(2);
		}

		/// <summary>
		/// 创建一条读取的指令信息，需要指定一些参数
		/// </summary>
		/// <param name="station">PLCd的站号</param>
		/// <param name="address">地址信息</param>
		/// <param name="length">数据长度</param>
		/// <param name="isBool">是否位读取</param>
		/// <param name="sumCheck">是否和校验</param>
		/// <param name="waitTime">等待时间</param>
		/// <returns>是否成功的结果对象</returns>
		public static OperateResult<byte[]> BuildReadCommand(byte station, string address, ushort length, bool isBool, bool sumCheck = true, byte waitTime = 0)
		{
			OperateResult<string> operateResult = FxAnalysisAddress(address);
			if (!operateResult.IsSuccess)
			{
				return OperateResult.CreateFailedResult<byte[]>(operateResult);
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(station.ToString("D2"));
			stringBuilder.Append("FF");
			if (isBool)
			{
				stringBuilder.Append("BR");
			}
			else
			{
				stringBuilder.Append("WR");
			}
			stringBuilder.Append(waitTime.ToString("X"));
			stringBuilder.Append(operateResult.Content);
			stringBuilder.Append(length.ToString("D2"));
			byte[] array = null;
			array = SoftBasic.SpliceTwoByteArray(bytes2: (!sumCheck) ? Encoding.ASCII.GetBytes(stringBuilder.ToString()) : Encoding.ASCII.GetBytes(CalculateAcc(stringBuilder.ToString())), bytes1: new byte[1]
			{
				5
			});
			return OperateResult.CreateSuccessResult(array);
		}

		/// <summary>
		/// 创建一条别入bool数据的指令信息，需要指定一些参数
		/// </summary>
		/// <param name="station">站号</param>
		/// <param name="address">地址</param>
		/// <param name="value">数组值</param>
		/// <param name="sumCheck">是否和校验</param>
		/// <param name="waitTime">等待时间</param>
		/// <returns>是否创建成功</returns>
		public static OperateResult<byte[]> BuildWriteBoolCommand(byte station, string address, bool[] value, bool sumCheck = true, byte waitTime = 0)
		{
			OperateResult<string> operateResult = FxAnalysisAddress(address);
			if (!operateResult.IsSuccess)
			{
				return OperateResult.CreateFailedResult<byte[]>(operateResult);
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(station.ToString("D2"));
			stringBuilder.Append("FF");
			stringBuilder.Append("BW");
			stringBuilder.Append(waitTime.ToString("X"));
			stringBuilder.Append(operateResult.Content);
			stringBuilder.Append(value.Length.ToString("D2"));
			for (int i = 0; i < value.Length; i++)
			{
				stringBuilder.Append(value[i] ? "1" : "0");
			}
			byte[] array = null;
			array = SoftBasic.SpliceTwoByteArray(bytes2: (!sumCheck) ? Encoding.ASCII.GetBytes(stringBuilder.ToString()) : Encoding.ASCII.GetBytes(CalculateAcc(stringBuilder.ToString())), bytes1: new byte[1]
			{
				5
			});
			return OperateResult.CreateSuccessResult(array);
		}

		/// <summary>
		/// 创建一条别入byte数据的指令信息，需要指定一些参数，按照字单位
		/// </summary>
		/// <param name="station">站号</param>
		/// <param name="address">地址</param>
		/// <param name="value">数组值</param>
		/// <param name="sumCheck">是否和校验</param>
		/// <param name="waitTime">等待时间</param>
		/// <returns>是否创建成功</returns>
		public static OperateResult<byte[]> BuildWriteByteCommand(byte station, string address, byte[] value, bool sumCheck = true, byte waitTime = 0)
		{
			OperateResult<string> operateResult = FxAnalysisAddress(address);
			if (!operateResult.IsSuccess)
			{
				return OperateResult.CreateFailedResult<byte[]>(operateResult);
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(station.ToString("D2"));
			stringBuilder.Append("FF");
			stringBuilder.Append("WW");
			stringBuilder.Append(waitTime.ToString("X"));
			stringBuilder.Append(operateResult.Content);
			stringBuilder.Append((value.Length / 2).ToString("D2"));
			byte[] array = new byte[value.Length * 2];
			for (int i = 0; i < value.Length / 2; i++)
			{
				SoftBasic.BuildAsciiBytesFrom(BitConverter.ToUInt16(value, i * 2)).CopyTo(array, 4 * i);
			}
			stringBuilder.Append(Encoding.ASCII.GetString(array));
			byte[] array2 = null;
			array2 = SoftBasic.SpliceTwoByteArray(bytes2: (!sumCheck) ? Encoding.ASCII.GetBytes(stringBuilder.ToString()) : Encoding.ASCII.GetBytes(CalculateAcc(stringBuilder.ToString())), bytes1: new byte[1]
			{
				5
			});
			return OperateResult.CreateSuccessResult(array2);
		}

		/// <summary>
		/// 创建启动PLC的报文信息
		/// </summary>
		/// <param name="station">站号信息</param>
		/// <param name="sumCheck">是否和校验</param>
		/// <param name="waitTime">等待时间</param>
		/// <returns>是否创建成功</returns>
		public static OperateResult<byte[]> BuildStart(byte station, bool sumCheck = true, byte waitTime = 0)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(station.ToString("D2"));
			stringBuilder.Append("FF");
			stringBuilder.Append("RR");
			stringBuilder.Append(waitTime.ToString("X"));
			byte[] array = null;
			array = SoftBasic.SpliceTwoByteArray(bytes2: (!sumCheck) ? Encoding.ASCII.GetBytes(stringBuilder.ToString()) : Encoding.ASCII.GetBytes(CalculateAcc(stringBuilder.ToString())), bytes1: new byte[1]
			{
				5
			});
			return OperateResult.CreateSuccessResult(array);
		}

		/// <summary>
		/// 创建启动PLC的报文信息
		/// </summary>
		/// <param name="station">站号信息</param>
		/// <param name="sumCheck">是否和校验</param>
		/// <param name="waitTime">等待时间</param>
		/// <returns>是否创建成功</returns>
		public static OperateResult<byte[]> BuildStop(byte station, bool sumCheck = true, byte waitTime = 0)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(station.ToString("D2"));
			stringBuilder.Append("FF");
			stringBuilder.Append("RS");
			stringBuilder.Append(waitTime.ToString("X"));
			byte[] array = null;
			array = SoftBasic.SpliceTwoByteArray(bytes2: (!sumCheck) ? Encoding.ASCII.GetBytes(stringBuilder.ToString()) : Encoding.ASCII.GetBytes(CalculateAcc(stringBuilder.ToString())), bytes1: new byte[1]
			{
				5
			});
			return OperateResult.CreateSuccessResult(array);
		}
	}
}
