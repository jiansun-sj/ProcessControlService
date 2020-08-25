﻿using YumpooDrive.BasicFramework;
using YumpooDrive.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YumpooDrive.Serial
{
	/// <summary>
	/// 基于串口的设备交互类的对象，需要从本类继承，然后实现不同的设备读写操作。
	/// </summary>
	/// <typeparam name="TTransform">数据解析的规则泛型</typeparam>
	public class SerialDeviceBase<TTransform> : SerialBase, IReadWriteNet where TTransform : IByteTransform, new()
	{
		private TTransform byteTransform;

		private string connectionId = string.Empty;

		/// <summary>
		/// 单个数据字节的长度，西门子为2，三菱，欧姆龙，modbusTcp就为1
		/// </summary>
		/// <remarks>对设备来说，一个地址的数据对应的字节数，或是1个字节或是2个字节</remarks>
		protected ushort WordLength
		{
			get;
			set;
		} = 1;


		/// <summary>
		/// 当前客户端的数据变换机制，当你需要从字节数据转换类型数据的时候需要。
		/// </summary>
		/// <example>
		/// 主要是用来转换数据类型的，下面仅仅演示了2个方法，其他的类型转换，类似处理。
		/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Core\NetworkDoubleBase.cs" region="ByteTransform" title="ByteTransform示例" />
		/// </example>
		public TTransform ByteTransform
		{
			get
			{
				return byteTransform;
			}
			set
			{
				byteTransform = value;
			}
		}

		/// <summary>
		/// 当前连接的唯一ID号，默认为长度20的guid码加随机数组成，方便列表管理，也可以自己指定
		/// </summary>
		/// <remarks>
		/// Current Connection ID, conclude guid and random data, also, you can spcified
		/// </remarks>
		public string ConnectionId
		{
			get
			{
				return connectionId;
			}
			set
			{
				connectionId = value;
			}
		}

		/// <summary>
		/// 默认的构造方法实现的设备信息
		/// </summary>
		public SerialDeviceBase()
		{
			byteTransform = new TTransform();
		}

		/// <summary>
		/// 从设备读取原始数据
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <param name="length">地址长度</param>
		/// <returns>带有成功标识的结果对象</returns>
		/// <remarks>需要在继承类中重写实现，并且实现地址解析操作</remarks>
		public virtual OperateResult<byte[]> Read(string address, ushort length)
		{
			return new OperateResult<byte[]>();
		}

		/// <summary>
		/// 将原始数据写入设备
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <param name="value">原始数据</param>
		/// <returns>带有成功标识的结果对象</returns>
		/// <remarks>需要在继承类中重写实现，并且实现地址解析操作</remarks>
		public virtual OperateResult Write(string address, byte[] value)
		{
			return new OperateResult();
		}

		/// <summary>
		/// 读取自定义类型的数据，需要规定解析规则
		/// </summary>
		/// <typeparam name="T">类型名称</typeparam>
		/// <param name="address">起始地址</param>
		/// <returns>带有成功标识的结果对象</returns>
		/// <remarks>
		/// 需要是定义一个类，选择好相对于的ByteTransform实例，才能调用该方法。
		/// </remarks>
		public OperateResult<T> ReadCustomer<T>(string address) where T : IDataTransfer, new()
		{
			OperateResult<T> operateResult = new OperateResult<T>();
			T content = new T();
			OperateResult<byte[]> operateResult2 = Read(address, content.ReadCount);
			if (operateResult2.IsSuccess)
			{
				content.ParseSource(operateResult2.Content);
				operateResult.Content = content;
				operateResult.IsSuccess = true;
			}
			else
			{
				operateResult.ErrorCode = operateResult2.ErrorCode;
				operateResult.Message = operateResult2.Message;
			}
			return operateResult;
		}

		/// <summary>
		/// 写入自定义类型的数据到设备去，需要规定生成字节的方法
		/// </summary>
		/// <typeparam name="T">自定义类型</typeparam>
		/// <param name="address">起始地址</param>
		/// <param name="data">实例对象</param>
		/// <returns>带有成功标识的结果对象</returns>
		/// <remarks>
		/// 需要是定义一个类，选择好相对于的<see cref="T:ProcessControlService.CommunicationStandard.IDataTransfer" />实例，才能调用该方法。
		/// </remarks>
		public OperateResult WriteCustomer<T>(string address, T data) where T : IDataTransfer, new()
		{
			return Write(address, data.ToSource());
		}

		/// <summary>
		/// 从设备里读取支持Hsl特性的数据内容，该特性为<see cref="T:ProcessControlService.CommunicationStandard.HslDeviceAddressAttribute" />，详细参考论坛的操作说明。
		/// </summary>
		/// <typeparam name="T">自定义的数据类型对象</typeparam>
		/// <returns>包含是否成功的结果对象</returns>
		public OperateResult<T> Read<T>() where T : class, new()
		{
			return HslReflectionHelper.Read<T>(this);
		}

		/// <summary>
		/// 从设备里读取支持Hsl特性的数据内容，该特性为<see cref="T:ProcessControlService.CommunicationStandard.HslDeviceAddressAttribute" />，详细参考论坛的操作说明。
		/// </summary>
		/// <typeparam name="T">自定义的数据类型对象</typeparam>
		/// <returns>包含是否成功的结果对象</returns>
		/// <exception cref="T:System.ArgumentNullException"></exception>
		public OperateResult Write<T>(T data) where T : class, new()
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			return HslReflectionHelper.Write(data, this);
		}

		/// <summary>
		/// 读取设备的short类型的数据
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <returns>带成功标志的结果数据对象</returns>
		public OperateResult<short> ReadInt16(string address)
		{
			return ByteTransformHelper.GetResultFromArray(ReadInt16(address, 1));
		}

		/// <summary>
		/// 读取设备的short类型的数组
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <param name="length">数组长度</param>
		/// <returns>带成功标志的结果数据对象</returns>
		public OperateResult<short[]> ReadInt16(string address, ushort length)
		{
			return ByteTransformHelper.GetResultFromBytes(Read(address, (ushort)(length * WordLength)), (byte[] m) => ByteTransform.TransInt16(m, 0, length));
		}

		/// <summary>
		/// 读取设备的ushort数据类型的数据
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <returns>带成功标志的结果数据对象</returns>
		public OperateResult<ushort> ReadUInt16(string address)
		{
			return ByteTransformHelper.GetResultFromArray(ReadUInt16(address, 1));
		}

		/// <summary>
		/// 读取设备的ushort类型的数组
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <param name="length">数组长度</param>
		/// <returns>带成功标志的结果数据对象</returns>
		public OperateResult<ushort[]> ReadUInt16(string address, ushort length)
		{
			return ByteTransformHelper.GetResultFromBytes(Read(address, (ushort)(length * WordLength)), (byte[] m) => ByteTransform.TransUInt16(m, 0, length));
		}

		/// <summary>
		/// 读取设备的int类型的数据
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <returns>带成功标志的结果数据对象</returns>
		public OperateResult<int> ReadInt32(string address)
		{
			return ByteTransformHelper.GetResultFromArray(ReadInt32(address, 1));
		}

		/// <summary>
		/// 读取设备的int类型的数组
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <param name="length">数组长度</param>
		/// <returns>带成功标志的结果数据对象</returns>
		public OperateResult<int[]> ReadInt32(string address, ushort length)
		{
			return ByteTransformHelper.GetResultFromBytes(Read(address, (ushort)(length * WordLength * 2)), (byte[] m) => ByteTransform.TransInt32(m, 0, length));
		}

		/// <summary>
		/// 读取设备的uint类型的数据
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <returns>带成功标志的结果数据对象</returns>
		public OperateResult<uint> ReadUInt32(string address)
		{
			return ByteTransformHelper.GetResultFromArray(ReadUInt32(address, 1));
		}

		/// <summary>
		/// 读取设备的uint类型的数组
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <param name="length">数组长度</param>
		/// <returns>带成功标志的结果数据对象</returns>
		public OperateResult<uint[]> ReadUInt32(string address, ushort length)
		{
			return ByteTransformHelper.GetResultFromBytes(Read(address, (ushort)(length * WordLength * 2)), (byte[] m) => ByteTransform.TransUInt32(m, 0, length));
		}

		/// <summary>
		/// 读取设备的float类型的数据
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <returns>带成功标志的结果数据对象</returns>
		public OperateResult<float> ReadFloat(string address)
		{
			return ByteTransformHelper.GetResultFromArray(ReadFloat(address, 1));
		}

		/// <summary>
		/// 读取设备的float类型的数组
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <param name="length">数组长度</param>
		/// <returns>带成功标志的结果数据对象</returns>
		public OperateResult<float[]> ReadFloat(string address, ushort length)
		{
			return ByteTransformHelper.GetResultFromBytes(Read(address, (ushort)(length * WordLength * 2)), (byte[] m) => ByteTransform.TransSingle(m, 0, length));
		}

		/// <summary>
		/// 读取设备的long类型的数据
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <returns>带成功标志的结果数据对象</returns>
		public OperateResult<long> ReadInt64(string address)
		{
			return ByteTransformHelper.GetResultFromArray(ReadInt64(address, 1));
		}

		/// <summary>
		/// 读取设备的long类型的数组
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <param name="length">数组长度</param>
		/// <returns>带成功标志的结果数据对象</returns>
		public OperateResult<long[]> ReadInt64(string address, ushort length)
		{
			return ByteTransformHelper.GetResultFromBytes(Read(address, (ushort)(length * WordLength * 4)), (byte[] m) => ByteTransform.TransInt64(m, 0, length));
		}

		/// <summary>
		/// 读取设备的ulong类型的数据
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <returns>带成功标志的结果数据对象</returns>
		public OperateResult<ulong> ReadUInt64(string address)
		{
			return ByteTransformHelper.GetResultFromArray(ReadUInt64(address, 1));
		}

		/// <summary>
		/// 读取设备的ulong类型的数组
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <param name="length">数组长度</param>
		/// <returns>带成功标志的结果数据对象</returns>
		public OperateResult<ulong[]> ReadUInt64(string address, ushort length)
		{
			return ByteTransformHelper.GetResultFromBytes(Read(address, (ushort)(length * WordLength * 4)), (byte[] m) => ByteTransform.TransUInt64(m, 0, length));
		}

		/// <summary>
		/// 读取设备的double类型的数据
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <returns>带成功标志的结果数据对象</returns>
		public OperateResult<double> ReadDouble(string address)
		{
			return ByteTransformHelper.GetResultFromArray(ReadDouble(address, 1));
		}

		/// <summary>
		/// 读取设备的double类型的数组
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <param name="length">数组长度</param>
		/// <returns>带成功标志的结果数据对象</returns>
		public OperateResult<double[]> ReadDouble(string address, ushort length)
		{
			return ByteTransformHelper.GetResultFromBytes(Read(address, (ushort)(length * WordLength * 4)), (byte[] m) => ByteTransform.TransDouble(m, 0, length));
		}

		/// <summary>
		/// 读取设备的字符串数据，编码为ASCII
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <param name="length">地址长度</param>
		/// <returns>带成功标志的结果数据对象</returns>
		public OperateResult<string> ReadString(string address, ushort length)
		{
			return ByteTransformHelper.GetResultFromBytes(Read(address, length), (byte[] m) => ByteTransform.TransString(m, 0, m.Length, Encoding.ASCII));
		}

		/// <summary>
		/// 批量读取底层的数据信息，需要指定地址和长度，具体的结果取决于实现
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <param name="length">数据长度</param>
		/// <returns>带有成功标识的bool[]数组</returns>
		public virtual OperateResult<bool[]> ReadBool(string address, ushort length)
		{
			return new OperateResult<bool[]>(StringResources.Language.NotSupportedFunction);
		}

		/// <summary>
		/// 读取底层的bool数据信息，具体的结果取决于实现
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <returns>带有成功标识的bool数组</returns>
		public virtual OperateResult<bool> ReadBool(string address)
		{
			OperateResult<bool[]> operateResult = ReadBool(address, 1);
			if (!operateResult.IsSuccess)
			{
				return OperateResult.CreateFailedResult<bool>(operateResult);
			}
			return OperateResult.CreateSuccessResult(operateResult.Content[0]);
		}

		/// <summary>
		/// 写入bool数组数据
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <param name="value">写入值</param>
		/// <returns>带有成功标识的结果类对象</returns>
		public virtual OperateResult Write(string address, bool[] value)
		{
			return new OperateResult(StringResources.Language.NotSupportedFunction);
		}

		/// <summary>
		/// 写入bool数据
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <param name="value">写入值</param>
		/// <returns>带有成功标识的结果类对象</returns>
		public virtual OperateResult Write(string address, bool value)
		{
			return Write(address, new bool[1]
			{
				value
			});
		}

		/// <summary>
		/// 向设备中写入short数组，返回是否写入成功
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <param name="values">实际数据</param>
		/// <returns>返回写入结果</returns>
		public virtual OperateResult Write(string address, short[] values)
		{
			return Write(address, ByteTransform.TransByte(values));
		}

		/// <summary>
		/// 向设备中写入short数据，返回是否写入成功
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <param name="value">实际数据</param>
		/// <returns>返回写入结果</returns>
		public virtual OperateResult Write(string address, short value)
		{
			return Write(address, new short[1]
			{
				value
			});
		}

		/// <summary>
		/// 向设备中写入ushort数组，返回是否写入成功
		/// </summary>
		/// <param name="address">要写入的数据地址</param>
		/// <param name="values">要写入的实际数据</param>
		/// <returns>返回写入结果</returns>
		public virtual OperateResult Write(string address, ushort[] values)
		{
			return Write(address, ByteTransform.TransByte(values));
		}

		/// <summary>
		/// 向设备中写入ushort数据，返回是否写入成功
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <param name="value">实际数据</param>
		/// <returns>返回写入结果</returns>
		public virtual OperateResult Write(string address, ushort value)
		{
			return Write(address, new ushort[1]
			{
				value
			});
		}

		/// <summary>
		/// 向设备中写入int数组，返回是否写入成功
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <param name="values">实际数据</param>
		/// <returns>返回写入结果</returns>
		public virtual OperateResult Write(string address, int[] values)
		{
			return Write(address, ByteTransform.TransByte(values));
		}

		/// <summary>
		/// 向设备中写入int数据，返回是否写入成功
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <param name="value">实际数据</param>
		/// <returns>返回写入结果</returns>
		public virtual OperateResult Write(string address, int value)
		{
			return Write(address, new int[1]
			{
				value
			});
		}

		/// <summary>
		/// 向设备中写入uint数组，返回是否写入成功
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <param name="values">实际数据</param>
		/// <returns>返回写入结果</returns>
		public virtual OperateResult Write(string address, uint[] values)
		{
			return Write(address, ByteTransform.TransByte(values));
		}

		/// <summary>
		/// 向设备中写入uint数据，返回是否写入成功
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <param name="value">实际数据</param>
		/// <returns>返回写入结果</returns>
		public virtual OperateResult Write(string address, uint value)
		{
			return Write(address, new uint[1]
			{
				value
			});
		}

		/// <summary>
		/// 向设备中写入float数组，返回是否写入成功
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <param name="values">实际数据</param>
		/// <returns>返回写入结果</returns>
		public virtual OperateResult Write(string address, float[] values)
		{
			return Write(address, ByteTransform.TransByte(values));
		}

		/// <summary>
		/// 向设备中写入float数据，返回是否写入成功
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <param name="value">实际数据</param>
		/// <returns>返回写入结果</returns>
		public virtual OperateResult Write(string address, float value)
		{
			return Write(address, new float[1]
			{
				value
			});
		}

		/// <summary>
		/// 向设备中写入long数组，返回是否写入成功
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <param name="values">实际数据</param>
		/// <returns>返回写入结果</returns>
		public virtual OperateResult Write(string address, long[] values)
		{
			return Write(address, ByteTransform.TransByte(values));
		}

		/// <summary>
		/// 向设备中写入long数据，返回是否写入成功
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <param name="value">实际数据</param>
		/// <returns>返回写入结果</returns>
		public virtual OperateResult Write(string address, long value)
		{
			return Write(address, new long[1]
			{
				value
			});
		}

		/// <summary>
		/// 向P设备中写入ulong数组，返回是否写入成功
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <param name="values">实际数据</param>
		/// <returns>返回写入结果</returns>
		public virtual OperateResult Write(string address, ulong[] values)
		{
			return Write(address, ByteTransform.TransByte(values));
		}

		/// <summary>
		/// 向设备中写入ulong数据，返回是否写入成功
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <param name="value">实际数据</param>
		/// <returns>返回写入结果</returns>
		public virtual OperateResult Write(string address, ulong value)
		{
			return Write(address, new ulong[1]
			{
				value
			});
		}

		/// <summary>
		/// 向设备中写入double数组，返回是否写入成功
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <param name="values">实际数据</param>
		/// <returns>返回写入结果</returns>
		public virtual OperateResult Write(string address, double[] values)
		{
			return Write(address, ByteTransform.TransByte(values));
		}

		/// <summary>
		/// 向设备中写入double数据，返回是否写入成功
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <param name="value">实际数据</param>
		/// <returns>返回写入结果</returns>
		public virtual OperateResult Write(string address, double value)
		{
			return Write(address, new double[1]
			{
				value
			});
		}

		/// <summary>
		/// 向设备中写入字符串，编码格式为ASCII
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <param name="value">字符串数据</param>
		/// <returns>是否写入成功的结果对象</returns>
		/// <example>
		/// 以下为三菱的连接对象示例，其他的设备读写情况参照下面的代码：
		/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Core\NetworkDeviceBase.cs" region="WriteString" title="String类型示例" />
		/// </example>
		public virtual OperateResult Write(string address, string value)
		{
			byte[] array = ByteTransform.TransByte(value, Encoding.ASCII);
			if (WordLength == 1)
			{
				array = SoftBasic.ArrayExpandToLengthEven(array);
			}
			return Write(address, array);
		}

		/// <summary>
		/// 向设备中写入指定长度的字符串,超出截断，不够补0，编码格式为ASCII
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <param name="value">字符串数据</param>
		/// <param name="length">指定的字符串长度，必须大于0</param>
		/// <returns>是否写入成功的结果对象 -&gt; Whether to write a successful result object</returns>
		public virtual OperateResult Write(string address, string value, int length)
		{
			byte[] data = ByteTransform.TransByte(value, Encoding.ASCII);
			if (WordLength == 1)
			{
				data = SoftBasic.ArrayExpandToLengthEven(data);
			}
			data = SoftBasic.ArrayExpandToLength(data, length);
			return Write(address, data);
		}

		/// <summary>
		/// 向设备中写入字符串，编码格式为Unicode
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <param name="value">字符串数据</param>
		/// <returns>是否写入成功的结果对象</returns>
		public virtual OperateResult WriteUnicodeString(string address, string value)
		{
			byte[] value2 = ByteTransform.TransByte(value, Encoding.Unicode);
			return Write(address, value2);
		}

		/// <summary>
		/// 向设备中写入指定长度的字符串,超出截断，不够补0，编码格式为Unicode
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <param name="value">字符串数据</param>
		/// <param name="length">指定的字符串长度，必须大于0</param>
		/// <returns>是否写入成功的结果对象 -&gt; Whether to write a successful result object</returns>
		public virtual OperateResult WriteUnicodeString(string address, string value, int length)
		{
			byte[] data = ByteTransform.TransByte(value, Encoding.Unicode);
			data = SoftBasic.ArrayExpandToLength(data, length * 2);
			return Write(address, data);
		}

		/// <summary>
		/// 批量读取底层的数据信息，需要指定地址和长度，具体的结果取决于实现
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <param name="length">数据长度</param>
		/// <returns>带有成功标识的bool[]数组</returns>
		public Task<OperateResult<bool[]>> ReadBoolAsync(string address, ushort length)
		{
			return Task.Run(() => new OperateResult<bool[]>(StringResources.Language.NotSupportedFunction));
		}

		/// <summary>
		/// 读取底层的bool数据信息，具体的结果取决于实现
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <returns>带有成功标识的bool数组</returns>
		public Task<OperateResult<bool>> ReadBoolAsync(string address)
		{
			return Task.Run(() => new OperateResult<bool>(StringResources.Language.NotSupportedFunction));
		}

		/// <summary>
		/// 写入bool数组数据
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <param name="value">写入值</param>
		/// <returns>带有成功标识的结果类对象</returns>
		public Task<OperateResult> WriteAsync(string address, bool[] value)
		{
			return Task.Run(() => new OperateResult(StringResources.Language.NotSupportedFunction));
		}

		/// <summary>
		/// 写入bool数据
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <param name="value">写入值</param>
		/// <returns>带有成功标识的结果类对象</returns>
		public Task<OperateResult> WriteAsync(string address, bool value)
		{
			return Task.Run(() => new OperateResult(StringResources.Language.NotSupportedFunction));
		}

		/// <summary>
		/// 使用异步的操作从原始的设备中读取数据信息
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <param name="length">地址长度</param>
		/// <returns>带有成功标识的结果对象</returns>
		public Task<OperateResult<byte[]>> ReadAsync(string address, ushort length)
		{
			return Task.Run(() => Read(address, length));
		}

		/// <summary>
		/// 异步读取设备的short类型的数据
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <returns>带成功标志的结果数据对象</returns>
		/// <example>
		/// 以下为三菱的连接对象示例，其他的设备读写情况参照下面的代码：
		/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Core\NetworkDeviceBase.cs" region="ReadInt16Async" title="Int16类型示例" />
		/// </example>
		public Task<OperateResult<short>> ReadInt16Async(string address)
		{
			return Task.Run(() => ReadInt16(address));
		}

		/// <summary>
		/// 异步读取设备的ushort类型的数组
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <param name="length">数组长度</param>
		/// <returns>带成功标志的结果数据对象</returns>
		/// <example>
		/// 以下为三菱的连接对象示例，其他的设备读写情况参照下面的代码：
		/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Core\NetworkDeviceBase.cs" region="ReadInt16ArrayAsync" title="Int16类型示例" />
		/// </example>
		public Task<OperateResult<short[]>> ReadInt16Async(string address, ushort length)
		{
			return Task.Run(() => ReadInt16(address, length));
		}

		/// <summary>
		/// 异步读取设备的ushort数据类型的数据
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <returns>带成功标志的结果数据对象</returns>
		/// <example>
		/// 以下为三菱的连接对象示例，其他的设备读写情况参照下面的代码：
		/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Core\NetworkDeviceBase.cs" region="ReadUInt16Async" title="UInt16类型示例" />
		/// </example>
		public Task<OperateResult<ushort>> ReadUInt16Async(string address)
		{
			return Task.Run(() => ReadUInt16(address));
		}

		/// <summary>
		/// 异步读取设备的ushort类型的数组
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <param name="length">数组长度</param>
		/// <returns>带成功标志的结果数据对象</returns>
		/// <example>
		/// 以下为三菱的连接对象示例，其他的设备读写情况参照下面的代码：
		/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Core\NetworkDeviceBase.cs" region="ReadUInt16ArrayAsync" title="UInt16类型示例" />
		/// </example>
		public Task<OperateResult<ushort[]>> ReadUInt16Async(string address, ushort length)
		{
			return Task.Run(() => ReadUInt16(address, length));
		}

		/// <summary>
		/// 异步读取设备的int类型的数据
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <returns>带成功标志的结果数据对象</returns>
		/// <example>
		/// 以下为三菱的连接对象示例，其他的设备读写情况参照下面的代码：
		/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Core\NetworkDeviceBase.cs" region="ReadInt32Async" title="Int32类型示例" />
		/// </example>
		public Task<OperateResult<int>> ReadInt32Async(string address)
		{
			return Task.Run(() => ReadInt32(address));
		}

		/// <summary>
		/// 异步读取设备的int类型的数组
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <param name="length">数组长度</param>
		/// <returns>带成功标志的结果数据对象</returns>
		/// <example>
		/// 以下为三菱的连接对象示例，其他的设备读写情况参照下面的代码：
		/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Core\NetworkDeviceBase.cs" region="ReadInt32ArrayAsync" title="Int32类型示例" />
		/// </example>
		public Task<OperateResult<int[]>> ReadInt32Async(string address, ushort length)
		{
			return Task.Run(() => ReadInt32(address, length));
		}

		/// <summary>
		/// 异步读取设备的uint类型的数据
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <returns>带成功标志的结果数据对象</returns>
		/// <example>
		/// 以下为三菱的连接对象示例，其他的设备读写情况参照下面的代码：
		/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Core\NetworkDeviceBase.cs" region="ReadUInt32Async" title="UInt32类型示例" />
		/// </example>
		public Task<OperateResult<uint>> ReadUInt32Async(string address)
		{
			return Task.Run(() => ReadUInt32(address));
		}

		/// <summary>
		/// 异步读取设备的uint类型的数组
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <param name="length">数组长度</param>
		/// <returns>带成功标志的结果数据对象</returns>
		/// <example>
		/// 以下为三菱的连接对象示例，其他的设备读写情况参照下面的代码：
		/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Core\NetworkDeviceBase.cs" region="ReadUInt32ArrayAsync" title="UInt32类型示例" />
		/// </example>
		public Task<OperateResult<uint[]>> ReadUInt32Async(string address, ushort length)
		{
			return Task.Run(() => ReadUInt32(address, length));
		}

		/// <summary>
		/// 异步读取设备的float类型的数据
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <returns>带成功标志的结果数据对象</returns>
		/// <example>
		/// 以下为三菱的连接对象示例，其他的设备读写情况参照下面的代码：
		/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Core\NetworkDeviceBase.cs" region="ReadFloatAsync" title="Float类型示例" />
		/// </example>
		public Task<OperateResult<float>> ReadFloatAsync(string address)
		{
			return Task.Run(() => ReadFloat(address));
		}

		/// <summary>
		/// 异步读取设备的float类型的数组
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <param name="length">数组长度</param>
		/// <returns>带成功标志的结果数据对象</returns>
		/// <example>
		/// 以下为三菱的连接对象示例，其他的设备读写情况参照下面的代码：
		/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Core\NetworkDeviceBase.cs" region="ReadFloatArrayAsync" title="Float类型示例" />
		/// </example>
		public Task<OperateResult<float[]>> ReadFloatAsync(string address, ushort length)
		{
			return Task.Run(() => ReadFloat(address, length));
		}

		/// <summary>
		/// 异步读取设备的long类型的数据
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <returns>带成功标志的结果数据对象</returns>
		/// <example>
		/// 以下为三菱的连接对象示例，其他的设备读写情况参照下面的代码：
		/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Core\NetworkDeviceBase.cs" region="ReadInt64Async" title="Int64类型示例" />
		/// </example>
		public Task<OperateResult<long>> ReadInt64Async(string address)
		{
			return Task.Run(() => ReadInt64(address));
		}

		/// <summary>
		/// 异步读取设备的long类型的数组
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <param name="length">数组长度</param>
		/// <returns>带成功标志的结果数据对象</returns>
		/// <example>
		/// 以下为三菱的连接对象示例，其他的设备读写情况参照下面的代码：
		/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Core\NetworkDeviceBase.cs" region="ReadInt64ArrayAsync" title="Int64类型示例" />
		/// </example>
		public Task<OperateResult<long[]>> ReadInt64Async(string address, ushort length)
		{
			return Task.Run(() => ReadInt64(address, length));
		}

		/// <summary>
		/// 异步读取设备的ulong类型的数据
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <returns>带成功标志的结果数据对象</returns>
		/// <example>
		/// 以下为三菱的连接对象示例，其他的设备读写情况参照下面的代码：
		/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Core\NetworkDeviceBase.cs" region="ReadUInt64Async" title="UInt64类型示例" />
		/// </example>
		public Task<OperateResult<ulong>> ReadUInt64Async(string address)
		{
			return Task.Run(() => ReadUInt64(address));
		}

		/// <summary>
		/// 异步读取设备的ulong类型的数组
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <param name="length">数组长度</param>
		/// <returns>带成功标志的结果数据对象</returns>
		/// <example>
		/// 以下为三菱的连接对象示例，其他的设备读写情况参照下面的代码：
		/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Core\NetworkDeviceBase.cs" region="ReadUInt64ArrayAsync" title="UInt64类型示例" />
		/// </example>
		public Task<OperateResult<ulong[]>> ReadUInt64Async(string address, ushort length)
		{
			return Task.Run(() => ReadUInt64(address, length));
		}

		/// <summary>
		/// 异步读取设备的double类型的数据
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <returns>带成功标志的结果数据对象</returns>
		/// <example>
		/// 以下为三菱的连接对象示例，其他的设备读写情况参照下面的代码：
		/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Core\NetworkDeviceBase.cs" region="ReadDoubleAsync" title="Double类型示例" />
		/// </example>
		public Task<OperateResult<double>> ReadDoubleAsync(string address)
		{
			return Task.Run(() => ReadDouble(address));
		}

		/// <summary>
		/// 异步读取设备的double类型的数组
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <param name="length">数组长度</param>
		/// <returns>带成功标志的结果数据对象</returns>
		/// <example>
		/// 以下为三菱的连接对象示例，其他的设备读写情况参照下面的代码：
		/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Core\NetworkDeviceBase.cs" region="ReadDoubleArrayAsync" title="Double类型示例" />
		/// </example>
		public Task<OperateResult<double[]>> ReadDoubleAsync(string address, ushort length)
		{
			return Task.Run(() => ReadDouble(address, length));
		}

		/// <summary>
		/// 异步读取设备的字符串数据，编码为ASCII
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <param name="length">地址长度</param>
		/// <returns>带成功标志的结果数据对象</returns>
		/// <example>
		/// 以下为三菱的连接对象示例，其他的设备读写情况参照下面的代码：
		/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Core\NetworkDeviceBase.cs" region="ReadStringAsync" title="String类型示例" />
		/// </example>
		public Task<OperateResult<string>> ReadStringAsync(string address, ushort length)
		{
			return Task.Run(() => ReadString(address, length));
		}

		/// <summary>
		/// 异步将原始数据写入设备
		/// </summary>
		/// <param name="address">起始地址</param>
		/// <param name="value">原始数据</param>
		/// <returns>带有成功标识的结果对象</returns>
		/// <example>
		/// 以下为三菱的连接对象示例，其他的设备读写情况参照下面的代码：
		/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Core\NetworkDeviceBase.cs" region="WriteAsync" title="bytes类型示例" />
		/// </example>
		public Task<OperateResult> WriteAsync(string address, byte[] value)
		{
			return Task.Run(() => Write(address, value));
		}

		/// <summary>
		/// 异步向设备中写入short数组，返回是否写入成功
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <param name="values">实际数据</param>
		/// <returns>是否写入成功的结果对象</returns>
		/// <example>
		/// 以下为三菱的连接对象示例，其他的设备读写情况参照下面的代码：
		/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Core\NetworkDeviceBase.cs" region="WriteInt16ArrayAsync" title="Int16类型示例" />
		/// </example>
		public Task<OperateResult> WriteAsync(string address, short[] values)
		{
			return Task.Run(() => Write(address, values));
		}

		/// <summary>
		/// 异步向设备中写入short数据，返回是否写入成功
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <param name="value">实际数据</param>
		/// <returns>是否写入成功的结果对象</returns>
		/// <example>
		/// 以下为三菱的连接对象示例，其他的设备读写情况参照下面的代码：
		/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Core\NetworkDeviceBase.cs" region="WriteInt16Async" title="Int16类型示例" />
		/// </example>
		public Task<OperateResult> WriteAsync(string address, short value)
		{
			return Task.Run(() => Write(address, value));
		}

		/// <summary>
		/// 异步向设备中写入ushort数组，返回是否写入成功
		/// </summary>
		/// <param name="address">要写入的数据地址</param>
		/// <param name="values">要写入的实际数据</param>
		/// <returns>是否写入成功的结果对象</returns>
		/// <example>
		/// 以下为三菱的连接对象示例，其他的设备读写情况参照下面的代码：
		/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Core\NetworkDeviceBase.cs" region="WriteUInt16ArrayAsync" title="UInt16类型示例" />
		/// </example>
		public Task<OperateResult> WriteAsync(string address, ushort[] values)
		{
			return Task.Run(() => Write(address, values));
		}

		/// <summary>
		/// 异步向设备中写入ushort数据，返回是否写入成功
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <param name="value">实际数据</param>
		/// <returns>是否写入成功的结果对象</returns>
		/// <example>
		/// 以下为三菱的连接对象示例，其他的设备读写情况参照下面的代码：
		/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Core\NetworkDeviceBase.cs" region="WriteUInt16Async" title="UInt16类型示例" />
		/// </example>
		public Task<OperateResult> WriteAsync(string address, ushort value)
		{
			return Task.Run(() => Write(address, value));
		}

		/// <summary>
		/// 异步向设备中写入int数组，返回是否写入成功
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <param name="values">实际数据</param>
		/// <returns>是否写入成功的结果对象</returns>
		/// <example>
		/// 以下为三菱的连接对象示例，其他的设备读写情况参照下面的代码：
		/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Core\NetworkDeviceBase.cs" region="WriteInt32ArrayAsync" title="Int32类型示例" />
		/// </example>
		public Task<OperateResult> WriteAsync(string address, int[] values)
		{
			return Task.Run(() => Write(address, values));
		}

		/// <summary>
		/// 异步向设备中写入int数据，返回是否写入成功
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <param name="value">实际数据</param>
		/// <returns>是否写入成功的结果对象</returns>
		/// <example>
		/// 以下为三菱的连接对象示例，其他的设备读写情况参照下面的代码：
		/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Core\NetworkDeviceBase.cs" region="WriteInt32Async" title="Int32类型示例" />
		/// </example>
		public Task<OperateResult> WriteAsync(string address, int value)
		{
			return Task.Run(() => Write(address, value));
		}

		/// <summary>
		/// 异步向设备中写入uint数组，返回是否写入成功
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <param name="values">实际数据</param>
		/// <returns>是否写入成功的结果对象</returns>
		/// <example>
		/// 以下为三菱的连接对象示例，其他的设备读写情况参照下面的代码：
		/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Core\NetworkDeviceBase.cs" region="WriteUInt32ArrayAsync" title="UInt32类型示例" />
		/// </example>
		public Task<OperateResult> WriteAsync(string address, uint[] values)
		{
			return Task.Run(() => Write(address, values));
		}

		/// <summary>
		/// 异步向设备中写入uint数据，返回是否写入成功
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <param name="value">实际数据</param>
		/// <returns>是否写入成功的结果对象</returns>
		/// <example>
		/// 以下为三菱的连接对象示例，其他的设备读写情况参照下面的代码：
		/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Core\NetworkDeviceBase.cs" region="WriteUInt32Async" title="UInt32类型示例" />
		/// </example>
		public Task<OperateResult> WriteAsync(string address, uint value)
		{
			return Task.Run(() => Write(address, value));
		}

		/// <summary>
		/// 异步向设备中写入float数组，返回是否写入成功
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <param name="values">实际数据</param>
		/// <returns>返回写入结果</returns>
		/// <example>
		/// 以下为三菱的连接对象示例，其他的设备读写情况参照下面的代码：
		/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Core\NetworkDeviceBase.cs" region="WriteFloatArrayAsync" title="Float类型示例" />
		/// </example>
		public Task<OperateResult> WriteAsync(string address, float[] values)
		{
			return Task.Run(() => Write(address, values));
		}

		/// <summary>
		/// 异步向设备中写入float数据，返回是否写入成功
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <param name="value">实际数据</param>
		/// <returns>返回写入结果</returns>
		/// <example>
		/// 以下为三菱的连接对象示例，其他的设备读写情况参照下面的代码：
		/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Core\NetworkDeviceBase.cs" region="WriteFloatAsync" title="Float类型示例" />
		/// </example>
		public Task<OperateResult> WriteAsync(string address, float value)
		{
			return Task.Run(() => Write(address, value));
		}

		/// <summary>
		/// 异步向设备中写入long数组，返回是否写入成功
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <param name="values">实际数据</param>
		/// <returns>是否写入成功的结果对象</returns>
		/// <example>
		/// 以下为三菱的连接对象示例，其他的设备读写情况参照下面的代码：
		/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Core\NetworkDeviceBase.cs" region="WriteInt64ArrayAsync" title="Int64类型示例" />
		/// </example>
		public Task<OperateResult> WriteAsync(string address, long[] values)
		{
			return Task.Run(() => Write(address, values));
		}

		/// <summary>
		/// 异步向设备中写入long数据，返回是否写入成功
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <param name="value">实际数据</param>
		/// <returns>是否写入成功的结果对象</returns>
		/// <example>
		/// 以下为三菱的连接对象示例，其他的设备读写情况参照下面的代码：
		/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Core\NetworkDeviceBase.cs" region="WriteInt64Async" title="Int64类型示例" />
		/// </example>
		public Task<OperateResult> WriteAsync(string address, long value)
		{
			return Task.Run(() => Write(address, value));
		}

		/// <summary>
		/// 异步向P设备中写入ulong数组，返回是否写入成功
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <param name="values">实际数据</param>
		/// <returns>是否写入成功的结果对象</returns>
		/// <example>
		/// 以下为三菱的连接对象示例，其他的设备读写情况参照下面的代码：
		/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Core\NetworkDeviceBase.cs" region="WriteUInt64ArrayAsync" title="UInt64类型示例" />
		/// </example>
		public Task<OperateResult> WriteAsync(string address, ulong[] values)
		{
			return Task.Run(() => Write(address, values));
		}

		/// <summary>
		/// 异步向设备中写入ulong数据，返回是否写入成功
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <param name="value">实际数据</param>
		/// <returns>是否写入成功的结果对象</returns>
		/// <example>
		/// 以下为三菱的连接对象示例，其他的设备读写情况参照下面的代码：
		/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Core\NetworkDeviceBase.cs" region="WriteUInt64Async" title="UInt64类型示例" />
		/// </example>
		public Task<OperateResult> WriteAsync(string address, ulong value)
		{
			return Task.Run(() => Write(address, value));
		}

		/// <summary>
		/// 异步向设备中写入double数组，返回是否写入成功
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <param name="values">实际数据</param>
		/// <returns>是否写入成功的结果对象</returns>
		/// <example>
		/// 以下为三菱的连接对象示例，其他的设备读写情况参照下面的代码：
		/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Core\NetworkDeviceBase.cs" region="WriteDoubleArrayAsync" title="Double类型示例" />
		/// </example>
		public Task<OperateResult> WriteAsync(string address, double[] values)
		{
			return Task.Run(() => Write(address, values));
		}

		/// <summary>
		/// 异步向设备中写入double数据，返回是否写入成功
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <param name="value">实际数据</param>
		/// <returns>是否写入成功的结果对象</returns>
		/// <example>
		/// 以下为三菱的连接对象示例，其他的设备读写情况参照下面的代码：
		/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Core\NetworkDeviceBase.cs" region="WriteDoubleAsync" title="Double类型示例" />
		/// </example>
		public Task<OperateResult> WriteAsync(string address, double value)
		{
			return Task.Run(() => Write(address, value));
		}

		/// <summary>
		/// 异步向设备中写入字符串，编码格式为ASCII
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <param name="value">字符串数据</param>
		/// <returns>是否写入成功的结果对象</returns>
		/// <example>
		/// 以下为三菱的连接对象示例，其他的设备读写情况参照下面的代码：
		/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Core\NetworkDeviceBase.cs" region="WriteStringAsync" title="String类型示例" />
		/// </example>
		public Task<OperateResult> WriteAsync(string address, string value)
		{
			return Task.Run(() => Write(address, value));
		}

		/// <summary>
		/// 异步向设备中写入指定长度的字符串,超出截断，不够补0，编码格式为ASCII
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <param name="value">字符串数据</param>
		/// <param name="length">指定的字符串长度，必须大于0</param>
		/// <returns>是否写入成功的结果对象 -&gt; Whether to write a successful result object</returns>
		/// <example>
		/// 以下为三菱的连接对象示例，其他的设备读写情况参照下面的代码：
		/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Core\NetworkDeviceBase.cs" region="WriteString2Async" title="String类型示例" />
		/// </example>
		public Task<OperateResult> WriteAsync(string address, string value, int length)
		{
			return Task.Run(() => Write(address, value, length));
		}

		/// <summary>
		/// 异步向设备中写入字符串，编码格式为Unicode
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <param name="value">字符串数据</param>
		/// <returns>是否写入成功的结果对象</returns>
		public Task<OperateResult> WriteUnicodeStringAsync(string address, string value)
		{
			return Task.Run(() => WriteUnicodeString(address, value));
		}

		/// <summary>
		/// 异步向设备中写入指定长度的字符串,超出截断，不够补0，编码格式为Unicode
		/// </summary>
		/// <param name="address">数据地址</param>
		/// <param name="value">字符串数据</param>
		/// <param name="length">指定的字符串长度，必须大于0</param>
		/// <returns>是否写入成功的结果对象 -&gt; Whether to write a successful result object</returns>
		public Task<OperateResult> WriteUnicodeStringAsync(string address, string value, int length)
		{
			return Task.Run(() => WriteUnicodeString(address, value, length));
		}

		/// <summary>
		/// 异步读取自定义类型的数据，需要规定解析规则
		/// </summary>
		/// <typeparam name="T">类型名称</typeparam>
		/// <param name="address">起始地址</param>
		/// <returns>带有成功标识的结果对象</returns>
		/// <remarks>
		/// 需要是定义一个类，选择好相对于的ByteTransform实例，才能调用该方法。
		/// </remarks>
		/// <example>
		/// 此处演示三菱的读取示例，先定义一个类，实现<see cref="T:ProcessControlService.CommunicationStandard.IDataTransfer" />接口
		/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Core\NetworkDeviceBase.cs" region="IDataTransfer Example" title="DataMy示例" />
		/// 接下来就可以实现数据的读取了
		/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Core\NetworkDeviceBase.cs" region="ReadCustomerAsyncExample" title="ReadCustomerAsync示例" />
		/// </example>
		public Task<OperateResult<T>> ReadCustomerAsync<T>(string address) where T : IDataTransfer, new()
		{
			return Task.Run(() => ReadCustomer<T>(address));
		}

		/// <summary>
		/// 异步写入自定义类型的数据到设备去，需要规定生成字节的方法
		/// </summary>
		/// <typeparam name="T">自定义类型</typeparam>
		/// <param name="address">起始地址</param>
		/// <param name="data">实例对象</param>
		/// <returns>带有成功标识的结果对象</returns>
		/// <remarks>
		/// 需要是定义一个类，选择好相对于的<see cref="T:ProcessControlService.CommunicationStandard.IDataTransfer" />实例，才能调用该方法。
		/// </remarks>
		/// <example>
		/// 此处演示三菱的读取示例，先定义一个类，实现<see cref="T:ProcessControlService.CommunicationStandard.IDataTransfer" />接口
		/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Core\NetworkDeviceBase.cs" region="IDataTransfer Example" title="DataMy示例" />
		/// 接下来就可以实现数据的读取了
		/// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\Core\NetworkDeviceBase.cs" region="WriteCustomerAsyncExample" title="WriteCustomerAsync示例" />
		/// </example>
		public Task<OperateResult> WriteCustomerAsync<T>(string address, T data) where T : IDataTransfer, new()
		{
			return Task.Run(() => WriteCustomer(address, data));
		}

		/// <summary>
		/// 异步从设备里读取支持Hsl特性的数据内容，该特性为<see cref="T:ProcessControlService.CommunicationStandard.HslDeviceAddressAttribute" />，详细参考论坛的操作说明。
		/// </summary>
		/// <typeparam name="T">自定义的数据类型对象</typeparam>
		/// <returns>包含是否成功的结果对象</returns>
		public Task<OperateResult<T>> ReadAsync<T>() where T : class, new()
		{
			return Task.Run(() => HslReflectionHelper.Read<T>(this));
		}

		/// <summary>
		/// 异步从设备里读取支持Hsl特性的数据内容，该特性为<see cref="T:ProcessControlService.CommunicationStandard.HslDeviceAddressAttribute" />，详细参考论坛的操作说明。
		/// </summary>
		/// <typeparam name="T">自定义的数据类型对象</typeparam>
		/// <returns>包含是否成功的结果对象</returns>
		/// <exception cref="T:System.ArgumentNullException"></exception>
		public Task<OperateResult> WriteAsync<T>(T data) where T : class, new()
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			return Task.Run(() => HslReflectionHelper.Write(data, this));
		}

		/// <summary>
		/// 返回表示当前对象的字符串
		/// </summary>
		/// <returns>字符串数据</returns>
		public override string ToString()
		{
			return "SerialDeviceBase<TTransform>";
		}
	}
}