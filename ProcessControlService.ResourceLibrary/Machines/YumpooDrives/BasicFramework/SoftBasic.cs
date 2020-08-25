using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
#if !NETSTANDARD2_0
//using System.Windows.Forms;
#endif
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Drawing;
using System.Runtime.Serialization;

namespace YumpooDrive.BasicFramework
{
    /// <summary>
    /// 一个软件基础类，提供常用的一些静态方法 ->
    /// A software-based class that provides some common static methods
    /// </summary>
    public class SoftBasic
    {
        private static List<char> hexCharList = new List<char>
        {
            '0',
            '1',
            '2',
            '3',
            '4',
            '5',
            '6',
            '7',
            '8',
            '9',
            'A',
            'B',
            'C',
            'D',
            'E',
            'F'
        };

        /// <summary>
        /// 设置或获取系统框架的版本号 -&gt;
        /// Set or get the version number of the system framework
        /// </summary>
        /// <remarks>
        /// 当你要显示本组件框架的版本号的时候，就可以用这个属性来显示
        /// </remarks>
        public static SystemVersion FrameworkVersion
        {
            get;
            set;
        } = new SystemVersion("8.0.2");


        /// <summary>
        /// 获取文件的md5码 -&gt; Get the MD5 code of the file
        /// </summary>
        /// <param name="filePath">文件的路径，既可以是完整的路径，也可以是相对的路径 -&gt; The path to the file</param>
        /// <returns>Md5字符串</returns>
        /// <example>
        /// 下面举例实现获取一个文件的md5码
        /// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\BasicFramework\SoftBasicExample.cs" region="CalculateFileMD5Example" title="CalculateFileMD5示例" />
        /// </example>
        public static string CalculateFileMD5(string filePath)
        {
            string result = string.Empty;
            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                result = CalculateStreamMD5(stream);
            }
            return result;
        }

        /// <summary>
        /// 获取数据流的md5码 -&gt; Get the MD5 code for the data stream
        /// </summary>
        /// <param name="stream">数据流，可以是内存流，也可以是文件流</param>
        /// <returns>Md5字符串</returns>
        /// <example>
        /// 下面举例实现获取一个流的md5码
        /// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\BasicFramework\SoftBasicExample.cs" region="CalculateStreamMD5Example1" title="CalculateStreamMD5示例" />
        /// </example>
        public static string CalculateStreamMD5(Stream stream)
        {
            byte[] value = null;
            using (MD5 mD = new MD5CryptoServiceProvider())
            {
                value = mD.ComputeHash(stream);
            }
            return BitConverter.ToString(value).Replace("-", "");
        }

        /// <summary>
        /// 获取文本字符串信息的Md5码，编码为UTF8
        /// </summary>
        /// <param name="data">文本数据信息</param>
        /// <returns>Md5字符串</returns>
        public static string CalculateStreamMD5(string data)
        {
            return CalculateStreamMD5(data, Encoding.UTF8);
        }

        /// <summary>
        /// 获取文本字符串信息的Md5码，使用指定的编码
        /// </summary>
        /// <param name="data">文本数据信息</param>
        /// <param name="encode">编码信息</param>
        /// <returns>Md5字符串</returns>
        public static string CalculateStreamMD5(string data, Encoding encode)
        {
            string result = string.Empty;
            using (MD5 mD = new MD5CryptoServiceProvider())
            {
                byte[] value = mD.ComputeHash(encode.GetBytes(data));
                result = BitConverter.ToString(value).Replace("-", "");
            }
            return result;
        }

        /// <summary>
        /// 从一个字节大小返回带单位的描述
        /// </summary>
        /// <param name="size">实际的大小值</param>
        /// <returns>最终的字符串值</returns>
        /// <example>
        /// 比如说我们获取了文件的长度，这个长度可以来自于本地，也可以来自于数据库查询
        /// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\BasicFramework\SoftBasicExample.cs" region="GetSizeDescriptionExample" title="GetSizeDescription示例" />
        /// </example>
        public static string GetSizeDescription(long size)
        {
            if (size < 1000)
            {
                return size + " B";
            }
            if (size < 1000000)
            {
                return (size / 1024f).ToString("F2") + " Kb";
            }
            if (size < 1000000000)
            {
                return (size / 1024f / 1024f).ToString("F2") + " Mb";
            }
            return (size / 1024f / 1024f / 1024f).ToString("F2") + " Gb";
        }

        /// <summary>
        /// 从一个时间差返回带单位的描述
        /// </summary>
        /// <param name="ts">实际的时间差</param>
        /// <returns>最终的字符串值</returns>
        /// <example>
        /// 比如说我们获取了一个时间差信息
        /// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\BasicFramework\SoftBasicExample.cs" region="GetTimeSpanDescriptionExample" title="GetTimeSpanDescription示例" />
        /// </example>
        public static string GetTimeSpanDescription(TimeSpan ts)
        {
            if (ts.TotalSeconds <= 60.0)
            {
                return (int)ts.TotalSeconds + " 秒";
            }
            if (ts.TotalMinutes <= 60.0)
            {
                return ts.TotalMinutes.ToString("F1") + " 分钟";
            }
            if (ts.TotalHours <= 24.0)
            {
                return ts.TotalHours.ToString("F1") + " 小时";
            }
            return ts.TotalDays.ToString("F1") + " 天";
        }

        /// <summary>
        /// 一个通用的数组新增个数方法，会自动判断越界情况，越界的情况下，会自动的截断或是填充 -&gt; 
        /// A common array of new methods, will automatically determine the cross-border situation, in the case of cross-border, will be automatically truncated or filled
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="array">原数据</param>
        /// <param name="data">等待新增的数据</param>
        /// <param name="max">原数据的最大值</param>
        /// <example>
        /// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\BasicFramework\SoftBasicExample.cs" region="AddArrayDataExample" title="AddArrayData示例" />
        /// </example>
        public static void AddArrayData<T>(ref T[] array, T[] data, int max)
        {
            if (data == null || data.Length == 0)
            {
                return;
            }
            if (array.Length == max)
            {
                Array.Copy(array, data.Length, array, 0, array.Length - data.Length);
                Array.Copy(data, 0, array, array.Length - data.Length, data.Length);
            }
            else if (array.Length + data.Length > max)
            {
                T[] array2 = new T[max];
                for (int i = 0; i < max - data.Length; i++)
                {
                    array2[i] = array[i + (array.Length - max + data.Length)];
                }
                for (int j = 0; j < data.Length; j++)
                {
                    array2[array2.Length - data.Length + j] = data[j];
                }
                array = array2;
            }
            else
            {
                T[] array3 = new T[array.Length + data.Length];
                for (int k = 0; k < array.Length; k++)
                {
                    array3[k] = array[k];
                }
                for (int l = 0; l < data.Length; l++)
                {
                    array3[array3.Length - data.Length + l] = data[l];
                }
                array = array3;
            }
        }

        /// <summary>
        /// 将一个数组进行扩充到指定长度，或是缩短到指定长度 -&gt;
        /// Extend an array to a specified length, or shorten to a specified length or fill
        /// </summary>
        /// <typeparam name="T">数组的类型</typeparam>
        /// <param name="data">原先数据的数据</param>
        /// <param name="length">新数组的长度</param>
        /// <returns>新数组长度信息</returns>
        /// <example>
        /// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\BasicFramework\SoftBasicExample.cs" region="ArrayExpandToLengthExample" title="ArrayExpandToLength示例" />
        /// </example>
        public static T[] ArrayExpandToLength<T>(T[] data, int length)
        {
            if (data == null)
            {
                return new T[length];
            }
            if (data.Length == length)
            {
                return data;
            }
            T[] array = new T[length];
            Array.Copy(data, array, Math.Min(data.Length, array.Length));
            return array;
        }

        /// <summary>
        /// 将一个数组进行扩充到偶数长度 -&gt;
        /// Extend an array to even lengths
        /// </summary>
        /// <typeparam name="T">数组的类型</typeparam>
        /// <param name="data">原先数据的数据</param>
        /// <returns>新数组长度信息</returns>
        /// <example>
        /// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\BasicFramework\SoftBasicExample.cs" region="ArrayExpandToLengthEvenExample" title="ArrayExpandToLengthEven示例" />
        /// </example>
        public static T[] ArrayExpandToLengthEven<T>(T[] data)
        {
            if (data == null)
            {
                return new T[0];
            }
            if (data.Length % 2 == 1)
            {
                return ArrayExpandToLength(data, data.Length + 1);
            }
            return data;
        }

        /// <summary>
        /// 将指定的数据按照指定长度进行分割，例如int[10]，指定长度4，就分割成int[4],int[4],int[2]，然后拼接list
        /// </summary>
        /// <typeparam name="T">数组的类型</typeparam>
        /// <param name="array">等待分割的数组</param>
        /// <param name="length">指定的长度信息</param>
        /// <returns>分割后结果内容</returns>
        /// <example>
        /// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\BasicFramework\SoftBasicExample.cs" region="ArraySplitByLengthExample" title="ArraySplitByLength示例" />
        /// </example>
        public static List<T[]> ArraySplitByLength<T>(T[] array, int length)
        {
            if (array == null)
            {
                return new List<T[]>();
            }
            List<T[]> list = new List<T[]>();
            int num = 0;
            while (num < array.Length)
            {
                if (num + length < array.Length)
                {
                    T[] array2 = new T[length];
                    Array.Copy(array, num, array2, 0, length);
                    num += length;
                    list.Add(array2);
                }
                else
                {
                    T[] array3 = new T[array.Length - num];
                    Array.Copy(array, num, array3, 0, array3.Length);
                    num += length;
                    list.Add(array3);
                }
            }
            return list;
        }

        /// <summary>
        /// 将整数进行有效的拆分成数组
        /// </summary>
        /// <param name="integer">整数信息</param>
        /// <param name="everyLength">单个的数组长度</param>
        /// <returns>拆分后的数组长度</returns>
        /// <example>
        /// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\BasicFramework\SoftBasicExample.cs" region="SplitIntegerToArrayExample" title="SplitIntegerToArray示例" />
        /// </example>
        public static int[] SplitIntegerToArray(int integer, int everyLength)
        {
            int[] array = new int[integer / everyLength + ((integer % everyLength != 0) ? 1 : 0)];
            for (int i = 0; i < array.Length; i++)
            {
                if (i == array.Length - 1)
                {
                    array[i] = ((integer % everyLength == 0) ? everyLength : (integer % everyLength));
                }
                else
                {
                    array[i] = everyLength;
                }
            }
            return array;
        }

        /// <summary>
        /// 判断两个字节的指定部分是否相同 -&gt;
        /// Determines whether the specified portion of a two-byte is the same
        /// </summary>
        /// <param name="b1">第一个字节</param>
        /// <param name="start1">第一个字节的起始位置</param>
        /// <param name="b2">第二个字节</param>
        /// <param name="start2">第二个字节的起始位置</param>
        /// <param name="length">校验的长度</param>
        /// <returns>返回是否相等</returns>
        /// <exception cref="T:System.IndexOutOfRangeException"></exception>
        /// <example>
        /// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\BasicFramework\SoftBasicExample.cs" region="IsTwoBytesEquelExample1" title="IsTwoBytesEquel示例" />
        /// </example>
        public static bool IsTwoBytesEquel(byte[] b1, int start1, byte[] b2, int start2, int length)
        {
            if (b1 == null || b2 == null)
            {
                return false;
            }
            for (int i = 0; i < length; i++)
            {
                if (b1[i + start1] != b2[i + start2])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 判断两个字节的指定部分是否相同 -&gt;
        /// Determines whether the specified portion of a two-byte is the same
        /// </summary>
        /// <param name="b1">第一个字节</param>
        /// <param name="b2">第二个字节</param>
        /// <returns>返回是否相等</returns>
        /// <example>
        /// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\BasicFramework\SoftBasicExample.cs" region="IsTwoBytesEquelExample2" title="IsTwoBytesEquel示例" />
        /// </example>
        public static bool IsTwoBytesEquel(byte[] b1, byte[] b2)
        {
            if (b1 == null || b2 == null)
            {
                return false;
            }
            if (b1.Length != b2.Length)
            {
                return false;
            }
            return IsTwoBytesEquel(b1, 0, b2, 0, b1.Length);
        }

        /// <summary>
        /// 判断两个数据的令牌是否相等 -&gt;
        /// Determines whether the tokens of two data are equal
        /// </summary>
        /// <param name="head">字节数据</param>
        /// <param name="token">GUID数据</param>
        /// <returns>返回是否相等</returns>
        /// <example>
        /// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\BasicFramework\SoftBasicExample.cs" region="IsTwoTokenEquelExample" title="IsByteTokenEquel示例" />
        /// </example>
        public static bool IsByteTokenEquel(byte[] head, Guid token)
        {
            return IsTwoBytesEquel(head, 12, token.ToByteArray(), 0, 16);
        }

        /// <summary>
        /// 判断两个数据的令牌是否相等 -&gt;
        /// Determines whether the tokens of two data are equal
        /// </summary>
        /// <param name="token1">第一个令牌</param>
        /// <param name="token2">第二个令牌</param>
        /// <returns>返回是否相等</returns>
        public static bool IsTwoTokenEquel(Guid token1, Guid token2)
        {
            return IsTwoBytesEquel(token1.ToByteArray(), 0, token2.ToByteArray(), 0, 16);
        }

        /// <summary>
        /// 获取一个枚举类型的所有枚举值，可直接应用于组合框数据 -&gt;
        /// Gets all the enumeration values of an enumeration type that can be applied directly to the combo box data
        /// </summary>
        /// <typeparam name="TEnum">枚举的类型值</typeparam>
        /// <returns>枚举值数组</returns>
        /// <example>
        /// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\BasicFramework\SoftBasicExample.cs" region="GetEnumValuesExample" title="GetEnumValues示例" />
        /// </example>
        public static TEnum[] GetEnumValues<TEnum>() where TEnum : struct
        {
            return (TEnum[])Enum.GetValues(typeof(TEnum));
        }

        /// <summary>
        /// 从字符串的枚举值数据转换成真实的枚举值数据 -&gt;
        /// Convert enumeration value data from strings to real enumeration value data
        /// </summary>
        /// <typeparam name="TEnum">枚举的类型值</typeparam>
        /// <param name="value">枚举的字符串的数据值</param>
        /// <returns>真实的枚举值</returns>
        /// <example>
        /// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\BasicFramework\SoftBasicExample.cs" region="GetEnumFromStringExample" title="GetEnumFromString示例" />
        /// </example>
        public static TEnum GetEnumFromString<TEnum>(string value) where TEnum : struct
        {
            return (TEnum)Enum.Parse(typeof(TEnum), value);
        }

        /// <summary>
        /// 一个泛型方法，提供json对象的数据读取 -&gt;
        /// A generic method that provides data read for a JSON object
        /// </summary>
        /// <typeparam name="T">读取的泛型</typeparam>
        /// <param name="json">json对象</param>
        /// <param name="value_name">值名称</param>
        /// <param name="default_value">默认值</param>
        /// <returns>值对象</returns>
        /// <example>
        /// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\BasicFramework\SoftBasicExample.cs" region="GetValueFromJsonObjectExample" title="GetValueFromJsonObject示例" />
        /// </example>
        //public static T GetValueFromJsonObject<T>(JObject json, string value_name, T default_value)
        //{
        //    if (json.Property(value_name) != null)
        //    {
        //        return json.Property(value_name).Value.Value<T>();
        //    }
        //    return default_value;
        //}

        /// <summary>
        /// 一个泛型方法，提供json对象的数据写入 -&gt;
        /// A generic method that provides data writing to a JSON object
        /// </summary>
        /// <typeparam name="T">写入的泛型</typeparam>
        /// <param name="json">json对象</param>
        /// <param name="property">值名称</param>
        /// <param name="value">值数据</param>
        /// <example>
        /// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\BasicFramework\SoftBasicExample.cs" region="JsonSetValueExample" title="JsonSetValue示例" />
        /// </example>
        //public static void JsonSetValue<T>(JObject json, string property, T value)
        //{
        //    if (json.Property(property) != null)
        //    {
        //        json.Property(property).Value = new JValue(value);
        //    }
        //    else
        //    {
        //        json.Add(property, new JValue(value));
        //    }
        //}

        /// <summary>
        /// 获取一个异常的完整错误信息 -&gt;
        /// Gets the complete error message for an exception
        /// </summary>
        /// <param name="ex">异常对象</param>
        /// <returns>完整的字符串数据</returns>
        /// <remarks>获取异常的完整信息</remarks>
        /// <exception cref="T:System.NullReferenceException">ex不能为空</exception>
        /// <example>
        /// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\BasicFramework\SoftBasicExample.cs" region="GetExceptionMessageExample1" title="GetExceptionMessage示例" />
        /// </example>
        public static string GetExceptionMessage(Exception ex)
        {
            return StringResources.Language.ExceptionMessage + ex.Message + Environment.NewLine + StringResources.Language.ExceptionStackTrace + ex.StackTrace + Environment.NewLine + StringResources.Language.ExceptopnTargetSite + ex.TargetSite;
        }

        /// <summary>
        /// 获取一个异常的完整错误信息，和额外的字符串描述信息 -&gt;
        /// Gets the complete error message for an exception, and additional string description information
        /// </summary>
        /// <param name="extraMsg">额外的信息</param>
        /// <param name="ex">异常对象</param>
        /// <returns>完整的字符串数据</returns>
        /// <exception cref="T:System.NullReferenceException"></exception>
        /// <example>
        /// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\BasicFramework\SoftBasicExample.cs" region="GetExceptionMessageExample2" title="GetExceptionMessage示例" />
        /// </example>
        public static string GetExceptionMessage(string extraMsg, Exception ex)
        {
            if (string.IsNullOrEmpty(extraMsg))
            {
                return GetExceptionMessage(ex);
            }
            return extraMsg + Environment.NewLine + GetExceptionMessage(ex);
        }

        /// <summary>
        /// 字节数据转化成16进制表示的字符串 -&gt;
        /// Byte data into a string of 16 binary representations
        /// </summary>
        /// <param name="InBytes">字节数组</param>
        /// <returns>返回的字符串</returns>
        /// <exception cref="T:System.NullReferenceException"></exception>
        /// <example>
        /// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\BasicFramework\SoftBasicExample.cs" region="ByteToHexStringExample1" title="ByteToHexString示例" />
        /// </example>
        public static string ByteToHexString(byte[] InBytes)
        {
            return ByteToHexString(InBytes, '\0');
        }

        /// <summary>
        /// 字节数据转化成16进制表示的字符串 -&gt;
        /// Byte data into a string of 16 binary representations
        /// </summary>
        /// <param name="InBytes">字节数组</param>
        /// <param name="segment">分割符</param>
        /// <returns>返回的字符串</returns>
        /// <exception cref="T:System.NullReferenceException"></exception>
        /// <example>
        /// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\BasicFramework\SoftBasicExample.cs" region="ByteToHexStringExample2" title="ByteToHexString示例" />
        /// </example>
        public static string ByteToHexString(byte[] InBytes, char segment)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (byte b in InBytes)
            {
                if (segment == '\0')
                {
                    stringBuilder.Append($"{b:X2}");
                }
                else
                {
                    stringBuilder.Append($"{b:X2}{segment}");
                }
            }
            if (segment != 0 && stringBuilder.Length > 1 && stringBuilder[stringBuilder.Length - 1] == segment)
            {
                stringBuilder.Remove(stringBuilder.Length - 1, 1);
            }
            return stringBuilder.ToString();
        }

        /// <summary>
        /// 字符串数据转化成16进制表示的字符串 -&gt;
        /// String data into a string of 16 binary representations
        /// </summary>
        /// <param name="InString">输入的字符串数据</param>
        /// <returns>返回的字符串</returns>
        /// <exception cref="T:System.NullReferenceException"></exception>
        public static string ByteToHexString(string InString)
        {
            return ByteToHexString(Encoding.Unicode.GetBytes(InString));
        }

        /// <summary>
        /// 将16进制的字符串转化成Byte数据，将检测每2个字符转化，也就是说，中间可以是任意字符 -&gt;
        /// Converts a 16-character string into byte data, which will detect every 2 characters converted, that is, the middle can be any character
        /// </summary>
        /// <param name="hex">十六进制的字符串，中间可以是任意的分隔符</param>
        /// <returns>转换后的字节数组</returns>
        /// <remarks>参数举例：AA 01 34 A8</remarks>
        /// <example>
        /// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\BasicFramework\SoftBasicExample.cs" region="HexStringToBytesExample" title="HexStringToBytes示例" />
        /// </example>
        public static byte[] HexStringToBytes(string hex)
        {
            hex = hex.ToUpper();
            MemoryStream memoryStream = new MemoryStream();
            for (int i = 0; i < hex.Length; i++)
            {
                if (i + 1 < hex.Length && hexCharList.Contains(hex[i]) && hexCharList.Contains(hex[i + 1]))
                {
                    memoryStream.WriteByte((byte)(hexCharList.IndexOf(hex[i]) * 16 + hexCharList.IndexOf(hex[i + 1])));
                    i++;
                }
            }
            byte[] result = memoryStream.ToArray();
            memoryStream.Dispose();
            return result;
        }

        /// <summary>
        /// 将byte数组按照双字节进行反转，如果为单数的情况，则自动补齐 -&gt;
        /// Reverses the byte array by double byte, or if the singular is the case, automatically
        /// </summary>
        /// <param name="inBytes">输入的字节信息</param>
        /// <returns>反转后的数据</returns>
        /// <example>
        /// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\BasicFramework\SoftBasicExample.cs" region="BytesReverseByWord" title="BytesReverseByWord示例" />
        /// </example>
        public static byte[] BytesReverseByWord(byte[] inBytes)
        {
            if (inBytes == null)
            {
                return null;
            }
            byte[] array = ArrayExpandToLengthEven(inBytes);
            for (int i = 0; i < array.Length / 2; i++)
            {
                byte b = array[i * 2];
                array[i * 2] = array[i * 2 + 1];
                array[i * 2 + 1] = b;
            }
            return array;
        }

        /// <summary>
        /// 将原始的byte数组转换成ascii格式的byte数组 -&gt;
        /// Converts the original byte array to an ASCII-formatted byte array
        /// </summary>
        /// <param name="inBytes">等待转换的byte数组</param>
        /// <returns>转换后的数组</returns>
        public static byte[] BytesToAsciiBytes(byte[] inBytes)
        {
            return Encoding.ASCII.GetBytes(ByteToHexString(inBytes));
        }

        /// <summary>
        /// 将ascii格式的byte数组转换成原始的byte数组 -&gt;
        /// Converts an ASCII-formatted byte array to the original byte array
        /// </summary>
        /// <param name="inBytes">等待转换的byte数组</param>
        /// <returns>转换后的数组</returns>
        public static byte[] AsciiBytesToBytes(byte[] inBytes)
        {
            return HexStringToBytes(Encoding.ASCII.GetString(inBytes));
        }

        /// <summary>
        /// 从字节构建一个ASCII格式的数据内容
        /// </summary>
        /// <param name="value">数据</param>
        /// <returns>ASCII格式的字节数组</returns>
        public static byte[] BuildAsciiBytesFrom(byte value)
        {
            return Encoding.ASCII.GetBytes(value.ToString("X2"));
        }

        /// <summary>
        /// 从short构建一个ASCII格式的数据内容
        /// </summary>
        /// <param name="value">数据</param>
        /// <returns>ASCII格式的字节数组</returns>
        public static byte[] BuildAsciiBytesFrom(short value)
        {
            return Encoding.ASCII.GetBytes(value.ToString("X4"));
        }

        /// <summary>
        /// 从ushort构建一个ASCII格式的数据内容
        /// </summary>
        /// <param name="value">数据</param>
        /// <returns>ASCII格式的字节数组</returns>
        public static byte[] BuildAsciiBytesFrom(ushort value)
        {
            return Encoding.ASCII.GetBytes(value.ToString("X4"));
        }

        /// <summary>
        /// 从字节数组构建一个ASCII格式的数据内容
        /// </summary>
        /// <param name="value">字节信息</param>
        /// <returns>ASCII格式的地址</returns>
        public static byte[] BuildAsciiBytesFrom(byte[] value)
        {
            byte[] array = new byte[value.Length * 2];
            for (int i = 0; i < value.Length; i++)
            {
                BuildAsciiBytesFrom(value[i]).CopyTo(array, 2 * i);
            }
            return array;
        }

        /// <summary>
        /// 将bool数组转换到byte数组 -&gt;
        /// Converting a bool array to a byte array
        /// </summary>
        /// <param name="array">bool数组</param>
        /// <returns>转换后的字节数组</returns>
        /// <example>
        /// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\BasicFramework\SoftBasicExample.cs" region="BoolArrayToByte" title="BoolArrayToByte示例" />
        /// </example>
        public static byte[] BoolArrayToByte(bool[] array)
        {
            if (array == null)
            {
                return null;
            }
            int num = (array.Length % 8 == 0) ? (array.Length / 8) : (array.Length / 8 + 1);
            byte[] array2 = new byte[num];
            for (int i = 0; i < array.Length; i++)
            {
                int num2 = i / 8;
                int num3 = i % 8;
                byte b = 0;
                switch (num3)
                {
                    case 0:
                        b = 1;
                        break;
                    case 1:
                        b = 2;
                        break;
                    case 2:
                        b = 4;
                        break;
                    case 3:
                        b = 8;
                        break;
                    case 4:
                        b = 16;
                        break;
                    case 5:
                        b = 32;
                        break;
                    case 6:
                        b = 64;
                        break;
                    case 7:
                        b = 128;
                        break;
                }
                if (array[i])
                {
                    array2[num2] += b;
                }
            }
            return array2;
        }

        /// <summary>
        /// 从Byte数组中提取位数组，length代表位数 -&gt;
        /// Extracts a bit array from a byte array, length represents the number of digits
        /// </summary>
        /// <param name="InBytes">原先的字节数组</param>
        /// <param name="length">想要转换的长度，如果超出自动会缩小到数组最大长度</param>
        /// <returns>转换后的bool数组</returns>
        /// <example>
        /// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\BasicFramework\SoftBasicExample.cs" region="ByteToBoolArray" title="ByteToBoolArray示例" />
        /// </example> 
        public static bool[] ByteToBoolArray(byte[] InBytes, int length)
        {
            if (InBytes == null)
            {
                return null;
            }
            if (length > InBytes.Length * 8)
            {
                length = InBytes.Length * 8;
            }
            bool[] array = new bool[length];
            for (int i = 0; i < length; i++)
            {
                int num = i / 8;
                int num2 = i % 8;
                byte b = 0;
                switch (num2)
                {
                    case 0:
                        b = 1;
                        break;
                    case 1:
                        b = 2;
                        break;
                    case 2:
                        b = 4;
                        break;
                    case 3:
                        b = 8;
                        break;
                    case 4:
                        b = 16;
                        break;
                    case 5:
                        b = 32;
                        break;
                    case 6:
                        b = 64;
                        break;
                    case 7:
                        b = 128;
                        break;
                }
                if ((InBytes[num] & b) == b)
                {
                    array[i] = true;
                }
            }
            return array;
        }

        /// <summary>
        /// 从Byte数组中提取所有的位数组 -&gt;
        /// Extracts a bit array from a byte array, length represents the number of digits
        /// </summary>
        /// <param name="InBytes">原先的字节数组</param>
        /// <returns>转换后的bool数组</returns>
        /// <example>
        /// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\BasicFramework\SoftBasicExample.cs" region="ByteToBoolArray" title="ByteToBoolArray示例" />
        /// </example> 
        public static bool[] ByteToBoolArray(byte[] InBytes)
        {
            if (InBytes == null)
            {
                return null;
            }
            return ByteToBoolArray(InBytes, InBytes.Length * 8);
        }

        /// <summary>
        /// 拼接2个字节数组成一个数组 -&gt;
        /// Splicing 2 bytes to to an array
        /// </summary>
        /// <param name="bytes1">数组一</param>
        /// <param name="bytes2">数组二</param>
        /// <returns>拼接后的数组</returns>
        /// <example>
        /// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\BasicFramework\SoftBasicExample.cs" region="SpliceTwoByteArray" title="SpliceTwoByteArray示例" />
        /// </example> 
        public static byte[] SpliceTwoByteArray(byte[] bytes1, byte[] bytes2)
        {
            if (bytes1 == null && bytes2 == null)
            {
                return null;
            }
            if (bytes1 == null)
            {
                return bytes2;
            }
            if (bytes2 == null)
            {
                return bytes1;
            }
            byte[] array = new byte[bytes1.Length + bytes2.Length];
            bytes1.CopyTo(array, 0);
            bytes2.CopyTo(array, bytes1.Length);
            return array;
        }

        /// <summary>
        /// 选择一个byte数组的前面的几个byte数据信息
        /// </summary>
        /// <param name="value">原始的数据信息</param>
        /// <param name="length">数据的长度</param>
        /// <returns>选择的前面的几个数据信息</returns>
        public static byte[] BytesArraySelectBegin(byte[] value, int length)
        {
            byte[] array = new byte[Math.Min(value.Length, length)];
            Array.Copy(value, 0, array, 0, array.Length);
            return array;
        }

        /// <summary>
        /// 将一个byte数组的前面指定位数移除，返回新的一个数组 -&gt;
        /// Removes the preceding specified number of bits in a byte array, returning a new array
        /// </summary>
        /// <param name="value">字节数组</param>
        /// <param name="length">等待移除的长度</param>
        /// <returns>新的数据</returns>
        /// <example>
        /// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\BasicFramework\SoftBasicExample.cs" region="BytesArrayRemoveBegin" title="BytesArrayRemoveBegin示例" />
        /// </example> 
        public static byte[] BytesArrayRemoveBegin(byte[] value, int length)
        {
            return BytesArrayRemoveDouble(value, length, 0);
        }

        /// <summary>
        /// 将一个byte数组的后面指定位数移除，返回新的一个数组 -&gt;
        /// Removes the specified number of digits after a byte array, returning a new array
        /// </summary>
        /// <param name="value">字节数组</param>
        /// <param name="length">等待移除的长度</param>
        /// <returns>新的数据</returns>
        /// <example>
        /// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\BasicFramework\SoftBasicExample.cs" region="BytesArrayRemoveLast" title="BytesArrayRemoveLast示例" />
        /// </example> 
        public static byte[] BytesArrayRemoveLast(byte[] value, int length)
        {
            return BytesArrayRemoveDouble(value, 0, length);
        }

        /// <summary>
        /// 将一个byte数组的前后移除指定位数，返回新的一个数组 -&gt;
        /// Removes a byte array before and after the specified number of bits, returning a new array
        /// </summary>
        /// <param name="value">字节数组</param>
        /// <param name="leftLength">前面的位数</param>
        /// <param name="rightLength">后面的位数</param>
        /// <returns>新的数据</returns>
        /// <example>
        /// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\BasicFramework\SoftBasicExample.cs" region="BytesArrayRemoveDouble" title="BytesArrayRemoveDouble示例" />
        /// </example> 
        public static byte[] BytesArrayRemoveDouble(byte[] value, int leftLength, int rightLength)
        {
            if (value == null)
            {
                return null;
            }
            if (value.Length <= leftLength + rightLength)
            {
                return new byte[0];
            }
            byte[] array = new byte[value.Length - leftLength - rightLength];
            Array.Copy(value, leftLength, array, 0, array.Length);
            return array;
        }

        /// <summary>
        /// 使用序列化反序列化深度克隆一个对象，该对象需要支持序列化特性 -&gt;
        /// Cloning an object with serialization deserialization depth that requires support for serialization attributes
        /// </summary>
        /// <param name="oringinal">源对象，支持序列化</param>
        /// <returns>新的一个实例化的对象</returns>
        /// <exception cref="T:System.NullReferenceException"></exception>
        /// <exception cref="T:System.NonSerializedAttribute"></exception>
        /// <remarks>
        /// <note type="warning">
        /// <paramref name="oringinal" /> 参数必须实现序列化的特性
        /// </note>
        /// </remarks>
        /// <example>
        /// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\BasicFramework\SoftBasicExample.cs" region="DeepClone" title="DeepClone示例" />
        /// </example>
        public static object DeepClone(object oringinal)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter
                {
                    Context = new StreamingContext(StreamingContextStates.Clone)
                };
                binaryFormatter.Serialize(memoryStream, oringinal);
                memoryStream.Position = 0L;
                return binaryFormatter.Deserialize(memoryStream);
            }
        }

        /// <summary>
        /// 获取一串唯一的随机字符串，长度为20，由Guid码和4位数的随机数组成，保证字符串的唯一性 -&gt;
        /// Gets a string of unique random strings with a length of 20, consisting of a GUID code and a 4-digit random number to guarantee the uniqueness of the string
        /// </summary>
        /// <returns>随机字符串数据</returns>
        /// <example>
        /// <code lang="cs" source="ProcessControlService.CommunicationStandard_Net45.Test\Documentation\Samples\BasicFramework\SoftBasicExample.cs" region="GetUniqueStringByGuidAndRandom" title="GetUniqueStringByGuidAndRandom示例" />
        /// </example>
        public static string GetUniqueStringByGuidAndRandom()
        {
            Random random = new Random();
            return Guid.NewGuid().ToString("N") + random.Next(1000, 10000);
        }
    }

}
