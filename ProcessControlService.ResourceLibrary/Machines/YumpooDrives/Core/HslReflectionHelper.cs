using System;
using System.Linq.Expressions;
using System.Reflection;

namespace YumpooDrive.Core
{
	/// <summary>
	/// 反射的辅助类
	/// </summary>
	public class HslReflectionHelper
	{
		/// <summary>
		/// 从设备里读取支持Hsl特性的数据内容，该特性为<see cref="T:ProcessControlService.CommunicationStandard.HslDeviceAddressAttribute" />，详细参考论坛的操作说明。
		/// </summary>
		/// <typeparam name="T">自定义的数据类型对象</typeparam>
		/// <param name="readWrite">读写接口的实现</param>
		/// <returns>包含是否成功的结果对象</returns>
		public static OperateResult<T> Read<T>(IReadWriteNet readWrite) where T : class, new()
		{
			Type typeFromHandle = typeof(T);
			object obj = typeFromHandle.Assembly.CreateInstance(typeFromHandle.FullName);
			PropertyInfo[] properties = typeFromHandle.GetProperties(BindingFlags.Instance | BindingFlags.Public);
			PropertyInfo[] array = properties;
			foreach (PropertyInfo propertyInfo in array)
			{
				object[] customAttributes = propertyInfo.GetCustomAttributes(typeof(DeviceAddressAttribute), inherit: false);
				if (customAttributes == null)
				{
					continue;
				}
				DeviceAddressAttribute hslDeviceAddressAttribute = null;
				for (int j = 0; j < customAttributes.Length; j++)
				{
					DeviceAddressAttribute hslDeviceAddressAttribute2 = (DeviceAddressAttribute)customAttributes[j];
					if (hslDeviceAddressAttribute2.DeviceType != null && hslDeviceAddressAttribute2.DeviceType == readWrite.GetType())
					{
						hslDeviceAddressAttribute = hslDeviceAddressAttribute2;
						break;
					}
				}
				if (hslDeviceAddressAttribute == null)
				{
					for (int k = 0; k < customAttributes.Length; k++)
					{
						DeviceAddressAttribute hslDeviceAddressAttribute3 = (DeviceAddressAttribute)customAttributes[k];
						if (hslDeviceAddressAttribute3.DeviceType == null)
						{
							hslDeviceAddressAttribute = hslDeviceAddressAttribute3;
							break;
						}
					}
				}
				if (hslDeviceAddressAttribute == null)
				{
					continue;
				}
				Type propertyType = propertyInfo.PropertyType;
				if (propertyType == typeof(short))
				{
					OperateResult<short> operateResult = readWrite.ReadInt16(hslDeviceAddressAttribute.Address);
					if (!operateResult.IsSuccess)
					{
						return OperateResult.CreateFailedResult<T>(operateResult);
					}
					propertyInfo.SetValue(obj, operateResult.Content, null);
				}
				else if (propertyType == typeof(short[]))
				{
					OperateResult<short[]> operateResult2 = readWrite.ReadInt16(hslDeviceAddressAttribute.Address, (ushort)((hslDeviceAddressAttribute.Length <= 0) ? 1 : hslDeviceAddressAttribute.Length));
					if (!operateResult2.IsSuccess)
					{
						return OperateResult.CreateFailedResult<T>(operateResult2);
					}
					propertyInfo.SetValue(obj, operateResult2.Content, null);
				}
				else if (propertyType == typeof(ushort))
				{
					OperateResult<ushort> operateResult3 = readWrite.ReadUInt16(hslDeviceAddressAttribute.Address);
					if (!operateResult3.IsSuccess)
					{
						return OperateResult.CreateFailedResult<T>(operateResult3);
					}
					propertyInfo.SetValue(obj, operateResult3.Content, null);
				}
				else if (propertyType == typeof(ushort[]))
				{
					OperateResult<ushort[]> operateResult4 = readWrite.ReadUInt16(hslDeviceAddressAttribute.Address, (ushort)((hslDeviceAddressAttribute.Length <= 0) ? 1 : hslDeviceAddressAttribute.Length));
					if (!operateResult4.IsSuccess)
					{
						return OperateResult.CreateFailedResult<T>(operateResult4);
					}
					propertyInfo.SetValue(obj, operateResult4.Content, null);
				}
				else if (propertyType == typeof(int))
				{
					OperateResult<int> operateResult5 = readWrite.ReadInt32(hslDeviceAddressAttribute.Address);
					if (!operateResult5.IsSuccess)
					{
						return OperateResult.CreateFailedResult<T>(operateResult5);
					}
					propertyInfo.SetValue(obj, operateResult5.Content, null);
				}
				else if (propertyType == typeof(int[]))
				{
					OperateResult<int[]> operateResult6 = readWrite.ReadInt32(hslDeviceAddressAttribute.Address, (ushort)((hslDeviceAddressAttribute.Length <= 0) ? 1 : hslDeviceAddressAttribute.Length));
					if (!operateResult6.IsSuccess)
					{
						return OperateResult.CreateFailedResult<T>(operateResult6);
					}
					propertyInfo.SetValue(obj, operateResult6.Content, null);
				}
				else if (propertyType == typeof(uint))
				{
					OperateResult<uint> operateResult7 = readWrite.ReadUInt32(hslDeviceAddressAttribute.Address);
					if (!operateResult7.IsSuccess)
					{
						return OperateResult.CreateFailedResult<T>(operateResult7);
					}
					propertyInfo.SetValue(obj, operateResult7.Content, null);
				}
				else if (propertyType == typeof(uint[]))
				{
					OperateResult<uint[]> operateResult8 = readWrite.ReadUInt32(hslDeviceAddressAttribute.Address, (ushort)((hslDeviceAddressAttribute.Length <= 0) ? 1 : hslDeviceAddressAttribute.Length));
					if (!operateResult8.IsSuccess)
					{
						return OperateResult.CreateFailedResult<T>(operateResult8);
					}
					propertyInfo.SetValue(obj, operateResult8.Content, null);
				}
				else if (propertyType == typeof(long))
				{
					OperateResult<long> operateResult9 = readWrite.ReadInt64(hslDeviceAddressAttribute.Address);
					if (!operateResult9.IsSuccess)
					{
						return OperateResult.CreateFailedResult<T>(operateResult9);
					}
					propertyInfo.SetValue(obj, operateResult9.Content, null);
				}
				else if (propertyType == typeof(long[]))
				{
					OperateResult<long[]> operateResult10 = readWrite.ReadInt64(hslDeviceAddressAttribute.Address, (ushort)((hslDeviceAddressAttribute.Length <= 0) ? 1 : hslDeviceAddressAttribute.Length));
					if (!operateResult10.IsSuccess)
					{
						return OperateResult.CreateFailedResult<T>(operateResult10);
					}
					propertyInfo.SetValue(obj, operateResult10.Content, null);
				}
				else if (propertyType == typeof(ulong))
				{
					OperateResult<ulong> operateResult11 = readWrite.ReadUInt64(hslDeviceAddressAttribute.Address);
					if (!operateResult11.IsSuccess)
					{
						return OperateResult.CreateFailedResult<T>(operateResult11);
					}
					propertyInfo.SetValue(obj, operateResult11.Content, null);
				}
				else if (propertyType == typeof(ulong[]))
				{
					OperateResult<ulong[]> operateResult12 = readWrite.ReadUInt64(hslDeviceAddressAttribute.Address, (ushort)((hslDeviceAddressAttribute.Length <= 0) ? 1 : hslDeviceAddressAttribute.Length));
					if (!operateResult12.IsSuccess)
					{
						return OperateResult.CreateFailedResult<T>(operateResult12);
					}
					propertyInfo.SetValue(obj, operateResult12.Content, null);
				}
				else if (propertyType == typeof(float))
				{
					OperateResult<float> operateResult13 = readWrite.ReadFloat(hslDeviceAddressAttribute.Address);
					if (!operateResult13.IsSuccess)
					{
						return OperateResult.CreateFailedResult<T>(operateResult13);
					}
					propertyInfo.SetValue(obj, operateResult13.Content, null);
				}
				else if (propertyType == typeof(float[]))
				{
					OperateResult<float[]> operateResult14 = readWrite.ReadFloat(hslDeviceAddressAttribute.Address, (ushort)((hslDeviceAddressAttribute.Length <= 0) ? 1 : hslDeviceAddressAttribute.Length));
					if (!operateResult14.IsSuccess)
					{
						return OperateResult.CreateFailedResult<T>(operateResult14);
					}
					propertyInfo.SetValue(obj, operateResult14.Content, null);
				}
				else if (propertyType == typeof(double))
				{
					OperateResult<double> operateResult15 = readWrite.ReadDouble(hslDeviceAddressAttribute.Address);
					if (!operateResult15.IsSuccess)
					{
						return OperateResult.CreateFailedResult<T>(operateResult15);
					}
					propertyInfo.SetValue(obj, operateResult15.Content, null);
				}
				else if (propertyType == typeof(double[]))
				{
					OperateResult<double[]> operateResult16 = readWrite.ReadDouble(hslDeviceAddressAttribute.Address, (ushort)((hslDeviceAddressAttribute.Length <= 0) ? 1 : hslDeviceAddressAttribute.Length));
					if (!operateResult16.IsSuccess)
					{
						return OperateResult.CreateFailedResult<T>(operateResult16);
					}
					propertyInfo.SetValue(obj, operateResult16.Content, null);
				}
				else if (propertyType == typeof(string))
				{
					OperateResult<string> operateResult17 = readWrite.ReadString(hslDeviceAddressAttribute.Address, (ushort)((hslDeviceAddressAttribute.Length <= 0) ? 1 : hslDeviceAddressAttribute.Length));
					if (!operateResult17.IsSuccess)
					{
						return OperateResult.CreateFailedResult<T>(operateResult17);
					}
					propertyInfo.SetValue(obj, operateResult17.Content, null);
				}
				else if (propertyType == typeof(byte[]))
				{
					OperateResult<byte[]> operateResult18 = readWrite.Read(hslDeviceAddressAttribute.Address, (ushort)((hslDeviceAddressAttribute.Length <= 0) ? 1 : hslDeviceAddressAttribute.Length));
					if (!operateResult18.IsSuccess)
					{
						return OperateResult.CreateFailedResult<T>(operateResult18);
					}
					propertyInfo.SetValue(obj, operateResult18.Content, null);
				}
				else if (propertyType == typeof(bool))
				{
					OperateResult<bool> operateResult19 = readWrite.ReadBool(hslDeviceAddressAttribute.Address);
					if (!operateResult19.IsSuccess)
					{
						return OperateResult.CreateFailedResult<T>(operateResult19);
					}
					propertyInfo.SetValue(obj, operateResult19.Content, null);
				}
				else if (propertyType == typeof(bool[]))
				{
					OperateResult<bool[]> operateResult20 = readWrite.ReadBool(hslDeviceAddressAttribute.Address, (ushort)((hslDeviceAddressAttribute.Length <= 0) ? 1 : hslDeviceAddressAttribute.Length));
					if (!operateResult20.IsSuccess)
					{
						return OperateResult.CreateFailedResult<T>(operateResult20);
					}
					propertyInfo.SetValue(obj, operateResult20.Content, null);
				}
			}
			return OperateResult.CreateSuccessResult((T)obj);
		}

		/// <summary>
		/// 从设备里读取支持Hsl特性的数据内容，该特性为<see cref="T:ProcessControlService.CommunicationStandard.HslDeviceAddressAttribute" />，详细参考论坛的操作说明。
		/// </summary>
		/// <typeparam name="T">自定义的数据类型对象</typeparam>
		/// <param name="data">自定义的数据对象</param>
		/// <param name="readWrite">数据读写对象</param>
		/// <returns>包含是否成功的结果对象</returns>
		/// <exception cref="T:System.ArgumentNullException"></exception>
		public static OperateResult Write<T>(T data, IReadWriteNet readWrite) where T : class, new()
		{
			if (data == null)
			{
				throw new ArgumentNullException("data");
			}
			Type typeFromHandle = typeof(T);
			PropertyInfo[] properties = typeFromHandle.GetProperties(BindingFlags.Instance | BindingFlags.Public);
			PropertyInfo[] array = properties;
			foreach (PropertyInfo propertyInfo in array)
			{
				object[] customAttributes = propertyInfo.GetCustomAttributes(typeof(DeviceAddressAttribute), inherit: false);
				if (customAttributes == null)
				{
					continue;
				}
				DeviceAddressAttribute hslDeviceAddressAttribute = null;
				for (int j = 0; j < customAttributes.Length; j++)
				{
					DeviceAddressAttribute hslDeviceAddressAttribute2 = (DeviceAddressAttribute)customAttributes[j];
					if (hslDeviceAddressAttribute2.DeviceType != null && hslDeviceAddressAttribute2.DeviceType == readWrite.GetType())
					{
						hslDeviceAddressAttribute = hslDeviceAddressAttribute2;
						break;
					}
				}
				if (hslDeviceAddressAttribute == null)
				{
					for (int k = 0; k < customAttributes.Length; k++)
					{
						DeviceAddressAttribute hslDeviceAddressAttribute3 = (DeviceAddressAttribute)customAttributes[k];
						if (hslDeviceAddressAttribute3.DeviceType == null)
						{
							hslDeviceAddressAttribute = hslDeviceAddressAttribute3;
							break;
						}
					}
				}
				if (hslDeviceAddressAttribute == null)
				{
					continue;
				}
				Type propertyType = propertyInfo.PropertyType;
				if (propertyType == typeof(short))
				{
					short value = (short)propertyInfo.GetValue(data, null);
					OperateResult operateResult = readWrite.Write(hslDeviceAddressAttribute.Address, value);
					if (!operateResult.IsSuccess)
					{
						return operateResult;
					}
				}
				else if (propertyType == typeof(short[]))
				{
					short[] values = (short[])propertyInfo.GetValue(data, null);
					OperateResult operateResult2 = readWrite.Write(hslDeviceAddressAttribute.Address, values);
					if (!operateResult2.IsSuccess)
					{
						return operateResult2;
					}
				}
				else if (propertyType == typeof(ushort))
				{
					ushort value2 = (ushort)propertyInfo.GetValue(data, null);
					OperateResult operateResult3 = readWrite.Write(hslDeviceAddressAttribute.Address, value2);
					if (!operateResult3.IsSuccess)
					{
						return operateResult3;
					}
				}
				else if (propertyType == typeof(ushort[]))
				{
					ushort[] values2 = (ushort[])propertyInfo.GetValue(data, null);
					OperateResult operateResult4 = readWrite.Write(hslDeviceAddressAttribute.Address, values2);
					if (!operateResult4.IsSuccess)
					{
						return operateResult4;
					}
				}
				else if (propertyType == typeof(int))
				{
					int value3 = (int)propertyInfo.GetValue(data, null);
					OperateResult operateResult5 = readWrite.Write(hslDeviceAddressAttribute.Address, value3);
					if (!operateResult5.IsSuccess)
					{
						return operateResult5;
					}
				}
				else if (propertyType == typeof(int[]))
				{
					int[] values3 = (int[])propertyInfo.GetValue(data, null);
					OperateResult operateResult6 = readWrite.Write(hslDeviceAddressAttribute.Address, values3);
					if (!operateResult6.IsSuccess)
					{
						return operateResult6;
					}
				}
				else if (propertyType == typeof(uint))
				{
					uint value4 = (uint)propertyInfo.GetValue(data, null);
					OperateResult operateResult7 = readWrite.Write(hslDeviceAddressAttribute.Address, value4);
					if (!operateResult7.IsSuccess)
					{
						return operateResult7;
					}
				}
				else if (propertyType == typeof(uint[]))
				{
					uint[] values4 = (uint[])propertyInfo.GetValue(data, null);
					OperateResult operateResult8 = readWrite.Write(hslDeviceAddressAttribute.Address, values4);
					if (!operateResult8.IsSuccess)
					{
						return operateResult8;
					}
				}
				else if (propertyType == typeof(long))
				{
					long value5 = (long)propertyInfo.GetValue(data, null);
					OperateResult operateResult9 = readWrite.Write(hslDeviceAddressAttribute.Address, value5);
					if (!operateResult9.IsSuccess)
					{
						return operateResult9;
					}
				}
				else if (propertyType == typeof(long[]))
				{
					long[] values5 = (long[])propertyInfo.GetValue(data, null);
					OperateResult operateResult10 = readWrite.Write(hslDeviceAddressAttribute.Address, values5);
					if (!operateResult10.IsSuccess)
					{
						return operateResult10;
					}
				}
				else if (propertyType == typeof(ulong))
				{
					ulong value6 = (ulong)propertyInfo.GetValue(data, null);
					OperateResult operateResult11 = readWrite.Write(hslDeviceAddressAttribute.Address, value6);
					if (!operateResult11.IsSuccess)
					{
						return operateResult11;
					}
				}
				else if (propertyType == typeof(ulong[]))
				{
					ulong[] values6 = (ulong[])propertyInfo.GetValue(data, null);
					OperateResult operateResult12 = readWrite.Write(hslDeviceAddressAttribute.Address, values6);
					if (!operateResult12.IsSuccess)
					{
						return operateResult12;
					}
				}
				else if (propertyType == typeof(float))
				{
					float value7 = (float)propertyInfo.GetValue(data, null);
					OperateResult operateResult13 = readWrite.Write(hslDeviceAddressAttribute.Address, value7);
					if (!operateResult13.IsSuccess)
					{
						return operateResult13;
					}
				}
				else if (propertyType == typeof(float[]))
				{
					float[] values7 = (float[])propertyInfo.GetValue(data, null);
					OperateResult operateResult14 = readWrite.Write(hslDeviceAddressAttribute.Address, values7);
					if (!operateResult14.IsSuccess)
					{
						return operateResult14;
					}
				}
				else if (propertyType == typeof(double))
				{
					double value8 = (double)propertyInfo.GetValue(data, null);
					OperateResult operateResult15 = readWrite.Write(hslDeviceAddressAttribute.Address, value8);
					if (!operateResult15.IsSuccess)
					{
						return operateResult15;
					}
				}
				else if (propertyType == typeof(double[]))
				{
					double[] values8 = (double[])propertyInfo.GetValue(data, null);
					OperateResult operateResult16 = readWrite.Write(hslDeviceAddressAttribute.Address, values8);
					if (!operateResult16.IsSuccess)
					{
						return operateResult16;
					}
				}
				else if (propertyType == typeof(string))
				{
					string value9 = (string)propertyInfo.GetValue(data, null);
					OperateResult operateResult17 = readWrite.Write(hslDeviceAddressAttribute.Address, value9);
					if (!operateResult17.IsSuccess)
					{
						return operateResult17;
					}
				}
				else if (propertyType == typeof(byte[]))
				{
					byte[] value10 = (byte[])propertyInfo.GetValue(data, null);
					OperateResult operateResult18 = readWrite.Write(hslDeviceAddressAttribute.Address, value10);
					if (!operateResult18.IsSuccess)
					{
						return operateResult18;
					}
				}
				else if (propertyType == typeof(bool))
				{
					bool value11 = (bool)propertyInfo.GetValue(data, null);
					OperateResult operateResult19 = readWrite.Write(hslDeviceAddressAttribute.Address, value11);
					if (!operateResult19.IsSuccess)
					{
						return operateResult19;
					}
				}
				else if (propertyType == typeof(bool[]))
				{
					bool[] value12 = (bool[])propertyInfo.GetValue(data, null);
					OperateResult operateResult20 = readWrite.Write(hslDeviceAddressAttribute.Address, value12);
					if (!operateResult20.IsSuccess)
					{
						return operateResult20;
					}
				}
			}
			return OperateResult.CreateSuccessResult(data);
		}

		/// <summary>
		/// 使用表达式树的方式来给一个属性赋值
		/// </summary>
		/// <param name="propertyInfo">属性信息</param>
		/// <param name="obj">对象信息</param>
		/// <param name="objValue">实际的值</param>
		public static void SetPropertyExp<T, K>(PropertyInfo propertyInfo, T obj, K objValue)
		{
			ParameterExpression parameterExpression = Expression.Parameter(typeof(T), "obj");
			ParameterExpression parameterExpression2 = Expression.Parameter(propertyInfo.PropertyType, "objValue");
			MethodCallExpression body = Expression.Call(parameterExpression, propertyInfo.GetSetMethod(), parameterExpression2);
			Expression<Action<T, K>> expression = Expression.Lambda<Action<T, K>>(body, new ParameterExpression[2]
			{
				parameterExpression,
				parameterExpression2
			});
			expression.Compile()(obj, objValue);
		}
	}
}
