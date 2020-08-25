using System;

namespace YumpooDrive
{
	/// <summary>
	/// 应用于Hsl组件库读取的动态地址解析
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class DeviceAddressAttribute : Attribute
	{
		/// <summary>
		/// 设备的类似，这将决定是否使用当前的PLC地址
		/// </summary>
		public Type DeviceType
		{
			get;
			set;
		}

		/// <summary>
		/// 数据的地址信息
		/// </summary>
		public string Address
		{
			get;
		}

		/// <summary>
		/// 数据长度
		/// </summary>
		public int Length
		{
			get;
		}

		/// <summary>
		/// 实例化一个地址特性，指定地址信息
		/// </summary>
		/// <param name="address">真实的地址信息</param>
		public DeviceAddressAttribute(string address)
		{
			this.Address = address;
			Length = -1;
			DeviceType = null;
		}

		/// <summary>
		/// 实例化一个地址特性，指定地址信息
		/// </summary>
		/// <param name="address">真实的地址信息</param>
		/// <param name="deviceType">设备的地址信息</param>
		public DeviceAddressAttribute(string address, Type deviceType)
		{
			this.Address = address;
			Length = -1;
			this.DeviceType = deviceType;
		}

		/// <summary>
		/// 实例化一个地址特性，指定地址信息和数据长度，通常应用于数组的批量读取
		/// </summary>
		/// <param name="address">真实的地址信息</param>
		/// <param name="length">读取的数据长度</param>
		public DeviceAddressAttribute(string address, int length)
		{
			this.Address = address;
			this.Length = length;
			DeviceType = null;
		}

		/// <summary>
		/// 实例化一个地址特性，指定地址信息和数据长度，通常应用于数组的批量读取
		/// </summary>
		/// <param name="address">真实的地址信息</param>
		/// <param name="length">读取的数据长度</param>
		/// <param name="deviceType">设备类型</param>
		public DeviceAddressAttribute(string address, int length, Type deviceType)
		{
			this.Address = address;
			this.Length = length;
			this.DeviceType = deviceType;
		}
	}
}
