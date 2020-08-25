using System;
using YumpooDrive.Profinet.Melsec;

namespace YumpooDrive.Core.Address
{
	/// <summary>
	/// 三菱的数据地址表示形式
	/// </summary>
	public class McAddressData : DeviceAddressDataBase
	{
		/// <summary>
		/// 三菱的数据地址信息
		/// </summary>
		public MelsecMcDataType McDataType
		{
			get;
			set;
		}

		/// <summary>
		/// 实例化一个默认的对象
		/// </summary>
		public McAddressData()
		{
			McDataType = MelsecMcDataType.D;
		}

		/// <summary>
		/// 从指定的地址信息解析成真正的设备地址信息，默认是三菱的地址
		/// </summary>
		/// <param name="address">地址信息</param>
		/// <param name="length">数据长度</param>
		public override void Parse(string address, ushort length)
		{
			OperateResult<McAddressData> operateResult = ParseMelsecFrom(address, length);
			if (operateResult.IsSuccess)
			{
				base.AddressStart = operateResult.Content.AddressStart;
				base.Length = operateResult.Content.Length;
				McDataType = operateResult.Content.McDataType;
			}
		}

		/// <summary>
		/// 从实际三菱的地址里面解析出
		/// </summary>
		/// <param name="address">三菱的地址数据信息</param>
		/// <param name="length">读取的数据长度</param>
		/// <returns>是否成功的结果对象</returns>
		public static OperateResult<McAddressData> ParseMelsecFrom(string address, ushort length)
		{
			McAddressData mcAddressData = new McAddressData();
			mcAddressData.Length = length;
			try
			{
				switch (address[0])
				{
					case 'M':
					case 'm':
						mcAddressData.McDataType = MelsecMcDataType.M;
						mcAddressData.AddressStart = Convert.ToInt32(address.Substring(1), MelsecMcDataType.M.FromBase);
						break;
					case 'X':
					case 'x':
						mcAddressData.McDataType = MelsecMcDataType.X;
						mcAddressData.AddressStart = Convert.ToInt32(address.Substring(1), MelsecMcDataType.X.FromBase);
						break;
					case 'Y':
					case 'y':
						mcAddressData.McDataType = MelsecMcDataType.Y;
						mcAddressData.AddressStart = Convert.ToInt32(address.Substring(1), MelsecMcDataType.Y.FromBase);
						break;
					case 'D':
					case 'd':
						mcAddressData.McDataType = MelsecMcDataType.D;
						mcAddressData.AddressStart = Convert.ToInt32(address.Substring(1), MelsecMcDataType.D.FromBase);
						break;
					case 'W':
					case 'w':
						mcAddressData.McDataType = MelsecMcDataType.W;
						mcAddressData.AddressStart = Convert.ToInt32(address.Substring(1), MelsecMcDataType.W.FromBase);
						break;
					case 'L':
					case 'l':
						mcAddressData.McDataType = MelsecMcDataType.L;
						mcAddressData.AddressStart = Convert.ToInt32(address.Substring(1), MelsecMcDataType.L.FromBase);
						break;
					case 'F':
					case 'f':
						mcAddressData.McDataType = MelsecMcDataType.F;
						mcAddressData.AddressStart = Convert.ToInt32(address.Substring(1), MelsecMcDataType.F.FromBase);
						break;
					case 'V':
					case 'v':
						mcAddressData.McDataType = MelsecMcDataType.V;
						mcAddressData.AddressStart = Convert.ToInt32(address.Substring(1), MelsecMcDataType.V.FromBase);
						break;
					case 'B':
					case 'b':
						mcAddressData.McDataType = MelsecMcDataType.B;
						mcAddressData.AddressStart = Convert.ToInt32(address.Substring(1), MelsecMcDataType.B.FromBase);
						break;
					case 'R':
					case 'r':
						mcAddressData.McDataType = MelsecMcDataType.R;
						mcAddressData.AddressStart = Convert.ToInt32(address.Substring(1), MelsecMcDataType.R.FromBase);
						break;
					case 'S':
					case 's':
						if (address[1] == 'N' || address[1] == 'n')
						{
							mcAddressData.McDataType = MelsecMcDataType.SN;
							mcAddressData.AddressStart = Convert.ToInt32(address.Substring(2), MelsecMcDataType.SN.FromBase);
						}
						else if (address[1] == 'S' || address[1] == 's')
						{
							mcAddressData.McDataType = MelsecMcDataType.SS;
							mcAddressData.AddressStart = Convert.ToInt32(address.Substring(2), MelsecMcDataType.SS.FromBase);
						}
						else if (address[1] == 'C' || address[1] == 'c')
						{
							mcAddressData.McDataType = MelsecMcDataType.SC;
							mcAddressData.AddressStart = Convert.ToInt32(address.Substring(2), MelsecMcDataType.SC.FromBase);
						}
						else
						{
							mcAddressData.McDataType = MelsecMcDataType.S;
							mcAddressData.AddressStart = Convert.ToInt32(address.Substring(1), MelsecMcDataType.S.FromBase);
						}
						break;
					case 'Z':
					case 'z':
						if (address.StartsWith("ZR") || address.StartsWith("zr"))
						{
							mcAddressData.McDataType = MelsecMcDataType.ZR;
							mcAddressData.AddressStart = Convert.ToInt32(address.Substring(2), MelsecMcDataType.ZR.FromBase);
						}
						else
						{
							mcAddressData.McDataType = MelsecMcDataType.Z;
							mcAddressData.AddressStart = Convert.ToInt32(address.Substring(1), MelsecMcDataType.Z.FromBase);
						}
						break;
					case 'T':
					case 't':
						if (address[1] == 'N' || address[1] == 'n')
						{
							mcAddressData.McDataType = MelsecMcDataType.TN;
							mcAddressData.AddressStart = Convert.ToInt32(address.Substring(2), MelsecMcDataType.TN.FromBase);
						}
						else if (address[1] == 'S' || address[1] == 's')
						{
							mcAddressData.McDataType = MelsecMcDataType.TS;
							mcAddressData.AddressStart = Convert.ToInt32(address.Substring(2), MelsecMcDataType.TS.FromBase);
						}
						else
						{
							if (address[1] != 'C' && address[1] != 'c')
							{
								throw new Exception(StringResources.Language.NotSupportedDataType);
							}
							mcAddressData.McDataType = MelsecMcDataType.TC;
							mcAddressData.AddressStart = Convert.ToInt32(address.Substring(2), MelsecMcDataType.TC.FromBase);
						}
						break;
					case 'C':
					case 'c':
						if (address[1] == 'N' || address[1] == 'n')
						{
							mcAddressData.McDataType = MelsecMcDataType.CN;
							mcAddressData.AddressStart = Convert.ToInt32(address.Substring(2), MelsecMcDataType.CN.FromBase);
						}
						else if (address[1] == 'S' || address[1] == 's')
						{
							mcAddressData.McDataType = MelsecMcDataType.CS;
							mcAddressData.AddressStart = Convert.ToInt32(address.Substring(2), MelsecMcDataType.CS.FromBase);
						}
						else
						{
							if (address[1] != 'C' && address[1] != 'c')
							{
								throw new Exception(StringResources.Language.NotSupportedDataType);
							}
							mcAddressData.McDataType = MelsecMcDataType.CC;
							mcAddressData.AddressStart = Convert.ToInt32(address.Substring(2), MelsecMcDataType.CC.FromBase);
						}
						break;
					default:
						throw new Exception(StringResources.Language.NotSupportedDataType);
				}
			}
			catch (Exception ex)
			{
				return new OperateResult<McAddressData>(ex.Message);
			}
			return OperateResult.CreateSuccessResult(mcAddressData);
		}

		/// <summary>
		/// 从实际基恩士的地址里面解析出
		/// </summary>
		/// <param name="address">基恩士的地址数据信息</param>
		/// <param name="length">读取的数据长度</param>
		/// <returns>是否成功的结果对象</returns>
		public static OperateResult<McAddressData> ParseKeyenceFrom(string address, ushort length)
		{
			McAddressData mcAddressData = new McAddressData();
			mcAddressData.Length = length;
			try
			{
				switch (address[0])
				{
					case 'M':
					case 'm':
						mcAddressData.McDataType = MelsecMcDataType.Keyence_M;
						mcAddressData.AddressStart = Convert.ToInt32(address.Substring(1), MelsecMcDataType.Keyence_M.FromBase);
						break;
					case 'X':
					case 'x':
						mcAddressData.McDataType = MelsecMcDataType.Keyence_X;
						mcAddressData.AddressStart = Convert.ToInt32(address.Substring(1), MelsecMcDataType.Keyence_X.FromBase);
						break;
					case 'Y':
					case 'y':
						mcAddressData.McDataType = MelsecMcDataType.Keyence_Y;
						mcAddressData.AddressStart = Convert.ToInt32(address.Substring(1), MelsecMcDataType.Keyence_Y.FromBase);
						break;
					case 'B':
					case 'b':
						mcAddressData.McDataType = MelsecMcDataType.Keyence_B;
						mcAddressData.AddressStart = Convert.ToInt32(address.Substring(1), MelsecMcDataType.Keyence_B.FromBase);
						break;
					case 'L':
					case 'l':
						mcAddressData.McDataType = MelsecMcDataType.Keyence_L;
						mcAddressData.AddressStart = Convert.ToInt32(address.Substring(1), MelsecMcDataType.Keyence_L.FromBase);
						break;
					case 'S':
					case 's':
						if (address[1] == 'M' || address[1] == 'm')
						{
							mcAddressData.McDataType = MelsecMcDataType.Keyence_SM;
							mcAddressData.AddressStart = Convert.ToInt32(address.Substring(2), MelsecMcDataType.Keyence_SM.FromBase);
						}
						else
						{
							if (address[1] != 'D' && address[1] != 'd')
							{
								throw new Exception(StringResources.Language.NotSupportedDataType);
							}
							mcAddressData.McDataType = MelsecMcDataType.Keyence_SD;
							mcAddressData.AddressStart = Convert.ToInt32(address.Substring(2), MelsecMcDataType.Keyence_SD.FromBase);
						}
						break;
					case 'D':
					case 'd':
						mcAddressData.McDataType = MelsecMcDataType.Keyence_D;
						mcAddressData.AddressStart = Convert.ToInt32(address.Substring(1), MelsecMcDataType.Keyence_D.FromBase);
						break;
					case 'R':
					case 'r':
						mcAddressData.McDataType = MelsecMcDataType.Keyence_R;
						mcAddressData.AddressStart = Convert.ToInt32(address.Substring(1), MelsecMcDataType.Keyence_R.FromBase);
						break;
					case 'Z':
					case 'z':
						if (address[1] != 'R' && address[1] != 'r')
						{
							throw new Exception(StringResources.Language.NotSupportedDataType);
						}
						mcAddressData.McDataType = MelsecMcDataType.Keyence_ZR;
						mcAddressData.AddressStart = Convert.ToInt32(address.Substring(2), MelsecMcDataType.Keyence_ZR.FromBase);
						break;
					case 'W':
					case 'w':
						mcAddressData.McDataType = MelsecMcDataType.Keyence_W;
						mcAddressData.AddressStart = Convert.ToInt32(address.Substring(1), MelsecMcDataType.Keyence_W.FromBase);
						break;
					case 'T':
					case 't':
						if (address[1] == 'N' || address[1] == 'n')
						{
							mcAddressData.McDataType = MelsecMcDataType.Keyence_TN;
							mcAddressData.AddressStart = Convert.ToInt32(address.Substring(2), MelsecMcDataType.Keyence_TN.FromBase);
						}
						else
						{
							if (address[1] != 'S' && address[1] != 's')
							{
								throw new Exception(StringResources.Language.NotSupportedDataType);
							}
							mcAddressData.McDataType = MelsecMcDataType.Keyence_TS;
							mcAddressData.AddressStart = Convert.ToInt32(address.Substring(2), MelsecMcDataType.Keyence_TS.FromBase);
						}
						break;
					case 'C':
					case 'c':
						if (address[1] == 'N' || address[1] == 'n')
						{
							mcAddressData.McDataType = MelsecMcDataType.Keyence_CN;
							mcAddressData.AddressStart = Convert.ToInt32(address.Substring(2), MelsecMcDataType.Keyence_CN.FromBase);
						}
						else
						{
							if (address[1] != 'S' && address[1] != 's')
							{
								throw new Exception(StringResources.Language.NotSupportedDataType);
							}
							mcAddressData.McDataType = MelsecMcDataType.Keyence_CS;
							mcAddressData.AddressStart = Convert.ToInt32(address.Substring(2), MelsecMcDataType.Keyence_CS.FromBase);
						}
						break;
					default:
						throw new Exception(StringResources.Language.NotSupportedDataType);
				}
			}
			catch (Exception ex)
			{
				return new OperateResult<McAddressData>(ex.Message);
			}
			return OperateResult.CreateSuccessResult(mcAddressData);
		}

		/// <summary>
		/// 计算松下的MC协议的偏移地址的机制
		/// </summary>
		/// <param name="address">字符串形式的地址</param>
		/// <returns>实际的偏移地址</returns>
		public static int GetPanasonicAddress(string address)
		{
			if (address.IndexOf('.') > 0)
			{
				string[] array = address.Split('.');
				return Convert.ToInt32(array[0]) * 16 + Convert.ToInt32(array[1]);
			}
			return Convert.ToInt32(address.Substring(0, address.Length - 1)) * 16 + Convert.ToInt32(address.Substring(address.Length - 1), 16);
		}

		/// <summary>
		/// 从实际松下的地址里面解析出
		/// </summary>
		/// <param name="address">松下的地址数据信息</param>
		/// <param name="length">读取的数据长度</param>
		/// <returns>是否成功的结果对象</returns>
		public static OperateResult<McAddressData> ParsePanasonicFrom(string address, ushort length)
		{
			McAddressData mcAddressData = new McAddressData();
			mcAddressData.Length = length;
			try
			{
				switch (address[0])
				{
					case 'R':
					case 'r':
						{
							int panasonicAddress = GetPanasonicAddress(address.Substring(1));
							if (panasonicAddress < 14400)
							{
								mcAddressData.McDataType = MelsecMcDataType.Panasonic_R;
								mcAddressData.AddressStart = panasonicAddress;
							}
							else
							{
								mcAddressData.McDataType = MelsecMcDataType.Panasonic_SM;
								mcAddressData.AddressStart = panasonicAddress - 14400;
							}
							break;
						}
					case 'X':
					case 'x':
						mcAddressData.McDataType = MelsecMcDataType.Panasonic_X;
						mcAddressData.AddressStart = GetPanasonicAddress(address.Substring(1));
						break;
					case 'Y':
					case 'y':
						mcAddressData.McDataType = MelsecMcDataType.Panasonic_Y;
						mcAddressData.AddressStart = GetPanasonicAddress(address.Substring(1));
						break;
					case 'L':
					case 'l':
						if (address[1] == 'D' || address[1] == 'd')
						{
							mcAddressData.McDataType = MelsecMcDataType.Panasonic_LD;
							mcAddressData.AddressStart = Convert.ToInt32(address.Substring(2));
						}
						else
						{
							mcAddressData.McDataType = MelsecMcDataType.Panasonic_L;
							mcAddressData.AddressStart = GetPanasonicAddress(address.Substring(1));
						}
						break;
					case 'D':
					case 'd':
						{
							int num = Convert.ToInt32(address.Substring(1));
							if (num < 90000)
							{
								mcAddressData.McDataType = MelsecMcDataType.Panasonic_DT;
								mcAddressData.AddressStart = Convert.ToInt32(address.Substring(1));
							}
							else
							{
								mcAddressData.McDataType = MelsecMcDataType.Panasonic_SD;
								mcAddressData.AddressStart = Convert.ToInt32(address.Substring(1)) - 90000;
							}
							break;
						}
					case 'T':
					case 't':
						if (address[1] == 'N' || address[1] == 'n')
						{
							mcAddressData.McDataType = MelsecMcDataType.Panasonic_TN;
							mcAddressData.AddressStart = Convert.ToInt32(address.Substring(2));
						}
						else
						{
							if (address[1] != 'S' && address[1] != 's')
							{
								throw new Exception(StringResources.Language.NotSupportedDataType);
							}
							mcAddressData.McDataType = MelsecMcDataType.Panasonic_TS;
							mcAddressData.AddressStart = Convert.ToInt32(address.Substring(2));
						}
						break;
					case 'C':
					case 'c':
						if (address[1] == 'N' || address[1] == 'n')
						{
							mcAddressData.McDataType = MelsecMcDataType.Panasonic_CN;
							mcAddressData.AddressStart = Convert.ToInt32(address.Substring(2));
						}
						else
						{
							if (address[1] != 'S' && address[1] != 's')
							{
								throw new Exception(StringResources.Language.NotSupportedDataType);
							}
							mcAddressData.McDataType = MelsecMcDataType.Panasonic_CS;
							mcAddressData.AddressStart = Convert.ToInt32(address.Substring(2));
						}
						break;
					default:
						throw new Exception(StringResources.Language.NotSupportedDataType);
				}
			}
			catch (Exception ex)
			{
				return new OperateResult<McAddressData>(ex.Message);
			}
			return OperateResult.CreateSuccessResult(mcAddressData);
		}
	}
}
