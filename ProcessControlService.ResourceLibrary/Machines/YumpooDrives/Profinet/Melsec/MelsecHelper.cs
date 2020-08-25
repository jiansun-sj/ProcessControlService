using System;
using System.Linq;
using System.Text;
using YumpooDrive.BasicFramework;
using YumpooDrive.Core.Address;

namespace YumpooDrive.Profinet.Melsec
{
	/// <summary>
	/// 所有三菱通讯类的通用辅助工具类，包含了一些通用的静态方法，可以使用本类来获取一些原始的报文信息。详细的操作参见例子
	/// </summary>
	public class MelsecHelper
	{
		/// <summary>
		/// 解析A1E协议数据地址
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <returns></returns>
		public static OperateResult<MelsecA1EDataType, ushort> McA1EAnalysisAddress(string address)
		{
			OperateResult<MelsecA1EDataType, ushort> operateResult = new OperateResult<MelsecA1EDataType, ushort>();
			try
			{
				switch (address[0])
				{
					case 'X':
					case 'x':
						operateResult.Content1 = MelsecA1EDataType.X;
						operateResult.Content2 = Convert.ToUInt16(address.Substring(1), MelsecA1EDataType.X.FromBase);
						break;
					case 'Y':
					case 'y':
						operateResult.Content1 = MelsecA1EDataType.Y;
						operateResult.Content2 = Convert.ToUInt16(address.Substring(1), MelsecA1EDataType.Y.FromBase);
						break;
					case 'M':
					case 'm':
						operateResult.Content1 = MelsecA1EDataType.M;
						operateResult.Content2 = Convert.ToUInt16(address.Substring(1), MelsecA1EDataType.M.FromBase);
						break;
					case 'S':
					case 's':
						operateResult.Content1 = MelsecA1EDataType.S;
						operateResult.Content2 = Convert.ToUInt16(address.Substring(1), MelsecA1EDataType.S.FromBase);
						break;
					case 'D':
					case 'd':
						operateResult.Content1 = MelsecA1EDataType.D;
						operateResult.Content2 = Convert.ToUInt16(address.Substring(1), MelsecA1EDataType.D.FromBase);
						break;
					case 'R':
					case 'r':
						operateResult.Content1 = MelsecA1EDataType.R;
						operateResult.Content2 = Convert.ToUInt16(address.Substring(1), MelsecA1EDataType.R.FromBase);
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
		/// 从三菱地址，是否位读取进行创建读取的MC的核心报文
		/// </summary>
		/// <param name="isBit">是否进行了位读取操作</param>
		/// <param name="addressData">三菱Mc协议的数据地址</param>
		/// <returns>带有成功标识的报文对象</returns>
		public static byte[] BuildReadMcCoreCommand(McAddressData addressData, bool isBit)
		{
			return new byte[10]
			{
				1,
				4,
				(byte)(isBit ? 1 : 0),
				0,
				BitConverter.GetBytes(addressData.AddressStart)[0],
				BitConverter.GetBytes(addressData.AddressStart)[1],
				BitConverter.GetBytes(addressData.AddressStart)[2],
				addressData.McDataType.DataCode,
				(byte)((int)addressData.Length % 256),
				(byte)((int)addressData.Length / 256)
			};
		}

		/// <summary>
		/// 从三菱地址，是否位读取进行创建读取Ascii格式的MC的核心报文
		/// </summary>
		/// <param name="addressData">三菱Mc协议的数据地址</param>
		/// <param name="isBit">是否进行了位读取操作</param>
		/// <returns>带有成功标识的报文对象</returns>
		public static byte[] BuildAsciiReadMcCoreCommand(McAddressData addressData, bool isBit)
		{
			return new byte[20]
			{
				48,
				52,
				48,
				49,
				48,
				48,
				48,
				(byte)(isBit ? 49 : 48),
				Encoding.ASCII.GetBytes(addressData.McDataType.AsciiCode)[0],
				Encoding.ASCII.GetBytes(addressData.McDataType.AsciiCode)[1],
				BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[0],
				BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[1],
				BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[2],
				BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[3],
				BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[4],
				BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[5],
				SoftBasic.BuildAsciiBytesFrom(addressData.Length)[0],
				SoftBasic.BuildAsciiBytesFrom(addressData.Length)[1],
				SoftBasic.BuildAsciiBytesFrom(addressData.Length)[2],
				SoftBasic.BuildAsciiBytesFrom(addressData.Length)[3]
			};
		}

		/// <summary>
		/// 以字为单位，创建数据写入的核心报文
		/// </summary>
		/// <param name="addressData">三菱Mc协议的数据地址</param>
		/// <param name="value">实际的原始数据信息</param>
		/// <returns>带有成功标识的报文对象</returns>
		public static byte[] BuildWriteWordCoreCommand(McAddressData addressData, byte[] value)
		{
			if (value == null)
			{
				value = new byte[0];
			}
			byte[] array = new byte[10 + value.Length];
			array[0] = 1;
			array[1] = 20;
			array[2] = 0;
			array[3] = 0;
			array[4] = BitConverter.GetBytes(addressData.AddressStart)[0];
			array[5] = BitConverter.GetBytes(addressData.AddressStart)[1];
			array[6] = BitConverter.GetBytes(addressData.AddressStart)[2];
			array[7] = addressData.McDataType.DataCode;
			array[8] = (byte)(value.Length / 2 % 256);
			array[9] = (byte)(value.Length / 2 / 256);
			value.CopyTo(array, 10);
			return array;
		}

		/// <summary>
		/// 以字为单位，创建ASCII数据写入的核心报文
		/// </summary>
		/// <param name="addressData">三菱Mc协议的数据地址</param>
		/// <param name="value">实际的原始数据信息</param>
		/// <returns>带有成功标识的报文对象</returns>
		public static byte[] BuildAsciiWriteWordCoreCommand(McAddressData addressData, byte[] value)
		{
			if (value == null)
			{
				value = new byte[0];
			}
			byte[] array = new byte[value.Length * 2];
			for (int i = 0; i < value.Length / 2; i++)
			{
				SoftBasic.BuildAsciiBytesFrom(BitConverter.ToUInt16(value, i * 2)).CopyTo(array, 4 * i);
			}
			value = array;
			byte[] array2 = new byte[20 + value.Length];
			array2[0] = 49;
			array2[1] = 52;
			array2[2] = 48;
			array2[3] = 49;
			array2[4] = 48;
			array2[5] = 48;
			array2[6] = 48;
			array2[7] = 48;
			array2[8] = Encoding.ASCII.GetBytes(addressData.McDataType.AsciiCode)[0];
			array2[9] = Encoding.ASCII.GetBytes(addressData.McDataType.AsciiCode)[1];
			array2[10] = BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[0];
			array2[11] = BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[1];
			array2[12] = BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[2];
			array2[13] = BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[3];
			array2[14] = BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[4];
			array2[15] = BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[5];
			array2[16] = SoftBasic.BuildAsciiBytesFrom((ushort)(value.Length / 4))[0];
			array2[17] = SoftBasic.BuildAsciiBytesFrom((ushort)(value.Length / 4))[1];
			array2[18] = SoftBasic.BuildAsciiBytesFrom((ushort)(value.Length / 4))[2];
			array2[19] = SoftBasic.BuildAsciiBytesFrom((ushort)(value.Length / 4))[3];
			value.CopyTo(array2, 20);
			return array2;
		}

		/// <summary>
		/// 以位为单位，创建数据写入的核心报文
		/// </summary>
		/// <param name="addressData">三菱Mc协议的数据地址</param>
		/// <param name="value">原始的bool数组数据</param>
		/// <returns>带有成功标识的报文对象</returns>
		public static byte[] BuildWriteBitCoreCommand(McAddressData addressData, bool[] value)
		{
			if (value == null)
			{
				value = new bool[0];
			}
			byte[] array = TransBoolArrayToByteData(value);
			byte[] array2 = new byte[10 + array.Length];
			array2[0] = 1;
			array2[1] = 20;
			array2[2] = 1;
			array2[3] = 0;
			array2[4] = BitConverter.GetBytes(addressData.AddressStart)[0];
			array2[5] = BitConverter.GetBytes(addressData.AddressStart)[1];
			array2[6] = BitConverter.GetBytes(addressData.AddressStart)[2];
			array2[7] = addressData.McDataType.DataCode;
			array2[8] = (byte)(value.Length % 256);
			array2[9] = (byte)(value.Length / 256);
			array.CopyTo(array2, 10);
			return array2;
		}

		/// <summary>
		/// 以位为单位，创建ASCII数据写入的核心报文
		/// </summary>
		/// <param name="addressData">三菱Mc协议的数据地址</param>
		/// <param name="value">原始的bool数组数据</param>
		/// <returns>带有成功标识的报文对象</returns>
		public static byte[] BuildAsciiWriteBitCoreCommand(McAddressData addressData, bool[] value)
		{
			if (value == null)
			{
				value = new bool[0];
			}
			byte[] array = value.Select((bool m) => (byte)(m ? 49 : 48)).ToArray();
			byte[] array2 = new byte[20 + array.Length];
			array2[0] = 49;
			array2[1] = 52;
			array2[2] = 48;
			array2[3] = 49;
			array2[4] = 48;
			array2[5] = 48;
			array2[6] = 48;
			array2[7] = 49;
			array2[8] = Encoding.ASCII.GetBytes(addressData.McDataType.AsciiCode)[0];
			array2[9] = Encoding.ASCII.GetBytes(addressData.McDataType.AsciiCode)[1];
			array2[10] = BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[0];
			array2[11] = BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[1];
			array2[12] = BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[2];
			array2[13] = BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[3];
			array2[14] = BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[4];
			array2[15] = BuildBytesFromAddress(addressData.AddressStart, addressData.McDataType)[5];
			array2[16] = SoftBasic.BuildAsciiBytesFrom((ushort)value.Length)[0];
			array2[17] = SoftBasic.BuildAsciiBytesFrom((ushort)value.Length)[1];
			array2[18] = SoftBasic.BuildAsciiBytesFrom((ushort)value.Length)[2];
			array2[19] = SoftBasic.BuildAsciiBytesFrom((ushort)value.Length)[3];
			array.CopyTo(array2, 20);
			return array2;
		}

		/// <summary>
		/// 从三菱的地址中构建MC协议的6字节的ASCII格式的地址
		/// </summary>
		/// <param name="address">三菱地址</param>
		/// <param name="type">三菱的数据类型</param>
		/// <returns>6字节的ASCII格式的地址</returns>
		internal static byte[] BuildBytesFromAddress(int address, MelsecMcDataType type)
		{
			return Encoding.ASCII.GetBytes(address.ToString((type.FromBase == 10) ? "D6" : "X6"));
		}

		/// <summary>
		/// 将0，1，0，1的字节数组压缩成三菱格式的字节数组来表示开关量的
		/// </summary>
		/// <param name="value">原始的数据字节</param>
		/// <returns>压缩过后的数据字节</returns>
		internal static byte[] TransBoolArrayToByteData(byte[] value)
		{
			int num = (value.Length + 1) / 2;
			byte[] array = new byte[num];
			for (int i = 0; i < num; i++)
			{
				if (value[i * 2] != 0)
				{
					array[i] += 16;
				}
				if (i * 2 + 1 < value.Length && value[i * 2 + 1] != 0)
				{
					array[i]++;
				}
			}
			return array;
		}

		/// <summary>
		/// 将bool的组压缩成三菱格式的字节数组来表示开关量的
		/// </summary>
		/// <param name="value">原始的数据字节</param>
		/// <returns>压缩过后的数据字节</returns>
		internal static byte[] TransBoolArrayToByteData(bool[] value)
		{
			int num = (value.Length + 1) / 2;
			byte[] array = new byte[num];
			for (int i = 0; i < num; i++)
			{
				if (value[i * 2])
				{
					array[i] += 16;
				}
				if (i * 2 + 1 < value.Length && value[i * 2 + 1])
				{
					array[i]++;
				}
			}
			return array;
		}

		/// <summary>
		/// 计算Fx协议指令的和校验信息
		/// </summary>
		/// <param name="data">字节数据</param>
		/// <returns>校验之后的数据</returns>
		internal static byte[] FxCalculateCRC(byte[] data)
		{
			int num = 0;
			for (int i = 1; i < data.Length - 2; i++)
			{
				num += data[i];
			}
			return SoftBasic.BuildAsciiBytesFrom((byte)num);
		}

		/// <summary>
		/// 检查指定的和校验是否是正确的
		/// </summary>
		/// <param name="data">字节数据</param>
		/// <returns>是否成功</returns>
		internal static bool CheckCRC(byte[] data)
		{
			byte[] array = FxCalculateCRC(data);
			if (array[0] != data[data.Length - 2])
			{
				return false;
			}
			if (array[1] != data[data.Length - 1])
			{
				return false;
			}
			return true;
		}
	}
}
