using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace YumpooDrive.Profinet.AllenBradley
{
    /// <summary>
    /// AB PLC的辅助类，用来辅助生成基本的指令信息
    /// </summary>
    public class AllenBradleyHelper
    {
        // Fields
        public const int CIP_MULTIREAD_DATA = 0x1000;
        public const byte CIP_READ_DATA = 0x4c;
        public const int CIP_READ_FRAGMENT = 0x52;
        public const int CIP_READ_WRITE_DATA = 0x4e;
        public const ushort CIP_Type_BitArray = 0xd3;
        public const ushort CIP_Type_Bool = 0xc1;
        public const ushort CIP_Type_Byte = 0xc2;
        public const ushort CIP_Type_Double = 0xcb;
        public const ushort CIP_Type_DWord = 0xc4;
        public const ushort CIP_Type_LInt = 0xc5;
        public const ushort CIP_Type_Real = 0xca;
        public const ushort CIP_Type_String = 0xd0;
        public const ushort CIP_Type_Struct = 0xcc;
        public const ushort CIP_Type_Word = 0xc3;
        public const int CIP_WRITE_DATA = 0x4d;
        public const int CIP_WRITE_FRAGMENT = 0x53;

        // Methods
        private static byte[] BuildRequestPathCommand(string address)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                char[] separator = new char[] { '.' };
                string[] strArray = address.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < strArray.Length; i++)
                {
                    string str = string.Empty;
                    int index = strArray[i].IndexOf('[');
                    int num3 = strArray[i].IndexOf(']');
                    if (((index > 0) && (num3 > 0)) && (num3 > index))
                    {
                        str = strArray[i].Substring(index + 1, (num3 - index) - 1);
                        strArray[i] = strArray[i].Substring(0, index);
                    }
                    stream.WriteByte(0x91);
                    stream.WriteByte((byte)strArray[i].Length);
                    byte[] bytes = Encoding.ASCII.GetBytes(strArray[i]);
                    stream.Write(bytes, 0, bytes.Length);
                    if ((bytes.Length % 2) == 1)
                    {
                        stream.WriteByte(0);
                    }
                    if (!string.IsNullOrEmpty(str))
                    {
                        char[] chArray2 = new char[] { ',' };
                        string[] strArray2 = str.Split(chArray2, StringSplitOptions.RemoveEmptyEntries);
                        for (int j = 0; j < strArray2.Length; j++)
                        {
                            int num5 = Convert.ToInt32(strArray2[j]);
                            if (num5 < 0x100)
                            {
                                stream.WriteByte(40);
                                stream.WriteByte((byte)num5);
                            }
                            else
                            {
                                stream.WriteByte(0x29);
                                stream.WriteByte(0);
                                stream.WriteByte(BitConverter.GetBytes(num5)[0]);
                                stream.WriteByte(BitConverter.GetBytes(num5)[1]);
                            }
                        }
                    }
                }
                return stream.ToArray();
            }
        }

        public static OperateResult<byte[]> ExtractActualData(byte[] response, bool isRead)
        {
            List<byte> list = new List<byte>();
            int startIndex = 0x26;
            ushort num2 = BitConverter.ToUInt16(response, 0x26);
            if (BitConverter.ToInt32(response, 40) == 0x8a)
            {
                startIndex = 0x2c;
                int num3 = BitConverter.ToUInt16(response, startIndex);
                for (int i = 0; i < num3; i++)
                {
                    int num5 = BitConverter.ToUInt16(response, (startIndex + 2) + (i * 2)) + startIndex;
                    int num6 = (i == (num3 - 1)) ? response.Length : (BitConverter.ToUInt16(response, (startIndex + 4) + (i * 2)) + startIndex);
                    ushort num7 = BitConverter.ToUInt16(response, num5 + 2);
                    switch (num7)
                    {
                        case 0:
                            break;

                        case 4:
                            return new OperateResult<byte[]>
                            {
                                ErrorCode = num7,
                                Message = StringResources.Language.AllenBradley04
                            };

                        case 5:
                            return new OperateResult<byte[]>
                            {
                                ErrorCode = num7,
                                Message = StringResources.Language.AllenBradley05
                            };

                        case 6:
                            if ((response[startIndex + 2] != 210) && (response[startIndex + 2] != 0xcc))
                            {
                                break;
                            }
                            return new OperateResult<byte[]>
                            {
                                ErrorCode = num7,
                                Message = StringResources.Language.AllenBradley06
                            };

                        case 10:
                            return new OperateResult<byte[]>
                            {
                                ErrorCode = num7,
                                Message = StringResources.Language.AllenBradley0A
                            };

                        case 0x13:
                            return new OperateResult<byte[]>
                            {
                                ErrorCode = num7,
                                Message = StringResources.Language.AllenBradley13
                            };

                        case 0x1c:
                            return new OperateResult<byte[]>
                            {
                                ErrorCode = num7,
                                Message = StringResources.Language.AllenBradley1C
                            };

                        case 30:
                            return new OperateResult<byte[]>
                            {
                                ErrorCode = num7,
                                Message = StringResources.Language.AllenBradley1E
                            };

                        case 0x26:
                            return new OperateResult<byte[]>
                            {
                                ErrorCode = num7,
                                Message = StringResources.Language.AllenBradley26
                            };

                        default:
                            return new OperateResult<byte[]>
                            {
                                ErrorCode = num7,
                                Message = StringResources.Language.UnknownError
                            };
                    }
                    if (isRead)
                    {
                        for (int j = num5 + 6; j < num6; j++)
                        {
                            list.Add(response[j]);
                        }
                    }
                }
            }
            else
            {
                byte num11 = response[startIndex + 4];
                switch (num11)
                {
                    case 0:
                        break;

                    case 4:
                        return new OperateResult<byte[]>
                        {
                            ErrorCode = num11,
                            Message = StringResources.Language.AllenBradley04
                        };

                    case 5:
                        return new OperateResult<byte[]>
                        {
                            ErrorCode = num11,
                            Message = StringResources.Language.AllenBradley05
                        };

                    case 6:
                        if ((response[startIndex + 2] != 210) && (response[startIndex + 2] != 0xcc))
                        {
                            break;
                        }
                        return new OperateResult<byte[]>
                        {
                            ErrorCode = num11,
                            Message = StringResources.Language.AllenBradley06
                        };

                    case 10:
                        return new OperateResult<byte[]>
                        {
                            ErrorCode = num11,
                            Message = StringResources.Language.AllenBradley0A
                        };

                    case 0x13:
                        return new OperateResult<byte[]>
                        {
                            ErrorCode = num11,
                            Message = StringResources.Language.AllenBradley13
                        };

                    case 0x1c:
                        return new OperateResult<byte[]>
                        {
                            ErrorCode = num11,
                            Message = StringResources.Language.AllenBradley1C
                        };

                    case 30:
                        return new OperateResult<byte[]>
                        {
                            ErrorCode = num11,
                            Message = StringResources.Language.AllenBradley1E
                        };

                    case 0x26:
                        return new OperateResult<byte[]>
                        {
                            ErrorCode = num11,
                            Message = StringResources.Language.AllenBradley26
                        };

                    default:
                        return new OperateResult<byte[]>
                        {
                            ErrorCode = num11,
                            Message = StringResources.Language.UnknownError
                        };
                }
                if (isRead)
                {
                    for (int k = startIndex + 8; k < ((startIndex + 2) + num2); k++)
                    {
                        list.Add(response[k]);
                    }
                }
            }
            return OperateResult.CreateSuccessResult<byte[]>(list.ToArray());
        }

        public static byte[] PackCommandSpecificData(byte slot, params byte[][] cips)
        {
            MemoryStream stream = new MemoryStream();
            stream.WriteByte(0);
            stream.WriteByte(0);
            stream.WriteByte(0);
            stream.WriteByte(0);
            stream.WriteByte(1);
            stream.WriteByte(0);
            stream.WriteByte(2);
            stream.WriteByte(0);
            stream.WriteByte(0);
            stream.WriteByte(0);
            stream.WriteByte(0);
            stream.WriteByte(0);
            stream.WriteByte(0xb2);
            stream.WriteByte(0);
            stream.WriteByte(0);
            stream.WriteByte(0);
            stream.WriteByte(0x52);
            stream.WriteByte(2);
            stream.WriteByte(0x20);
            stream.WriteByte(6);
            stream.WriteByte(0x24);
            stream.WriteByte(1);
            stream.WriteByte(10);
            stream.WriteByte(240);
            stream.WriteByte(0);
            stream.WriteByte(0);
            int num = 0;
            if (cips.Length == 1)
            {
                stream.Write(cips[0], 0, cips[0].Length);
                num += cips[0].Length;
            }
            else
            {
                stream.WriteByte(10);
                stream.WriteByte(2);
                stream.WriteByte(0x20);
                stream.WriteByte(2);
                stream.WriteByte(0x24);
                stream.WriteByte(1);
                num += 8;
                stream.Write(BitConverter.GetBytes((ushort)cips.Length), 0, 2);
                ushort num2 = (ushort)(2 + (2 * cips.Length));
                num += 2 * cips.Length;
                for (int i = 0; i < cips.Length; i++)
                {
                    stream.Write(BitConverter.GetBytes(num2), 0, 2);
                    num2 = (ushort)(num2 + cips[i].Length);
                }
                for (int j = 0; j < cips.Length; j++)
                {
                    stream.Write(cips[j], 0, cips[j].Length);
                    num += cips[j].Length;
                }
            }
            stream.WriteByte(1);
            stream.WriteByte(0);
            stream.WriteByte(1);
            stream.WriteByte(slot);
            byte[] array = stream.ToArray();
            stream.Dispose();
            BitConverter.GetBytes((short)num).CopyTo(array, 0x18);
            array[14] = BitConverter.GetBytes((short)(array.Length - 0x10))[0];
            array[15] = BitConverter.GetBytes((short)(array.Length - 0x10))[1];
            return array;
        }

        public static byte[] PackRequestHeader(ushort command, uint session, byte[] commandSpecificData)
        {
            byte[] destinationArray = new byte[commandSpecificData.Length + 0x18];
            Array.Copy(commandSpecificData, 0, destinationArray, 0x18, commandSpecificData.Length);
            BitConverter.GetBytes(command).CopyTo(destinationArray, 0);
            BitConverter.GetBytes(session).CopyTo(destinationArray, 4);
            BitConverter.GetBytes((ushort)commandSpecificData.Length).CopyTo(destinationArray, 2);
            return destinationArray;
        }

        public static byte[] PackRequestReadSegment(string address, int startIndex, int length)
        {
            byte[] array = new byte[0x400];
            int index = 0;
            array[index++] = 0x52;
            index++;
            byte[] buffer2 = BuildRequestPathCommand(address + (address.EndsWith("]") ? string.Empty : $"[{startIndex}]"));
            buffer2.CopyTo(array, index);
            index += buffer2.Length;
            array[1] = (byte)((index - 2) / 2);
            array[index++] = BitConverter.GetBytes(length)[0];
            array[index++] = BitConverter.GetBytes(length)[1];
            array[index++] = BitConverter.GetBytes(0)[0];
            array[index++] = BitConverter.GetBytes(0)[1];
            array[index++] = BitConverter.GetBytes(0)[2];
            array[index++] = BitConverter.GetBytes(0)[3];
            byte[] destinationArray = new byte[index];
            Array.Copy(array, 0, destinationArray, 0, index);
            return destinationArray;
        }

        public static byte[] PackRequestWrite(string address, ushort typeCode, byte[] value, int length = 1)
        {
            byte[] array = new byte[0x400];
            int index = 0;
            array[index++] = 0x4d;
            index++;
            byte[] buffer2 = BuildRequestPathCommand(address);
            buffer2.CopyTo(array, index);
            index += buffer2.Length;
            array[1] = (byte)((index - 2) / 2);
            array[index++] = BitConverter.GetBytes(typeCode)[0];
            array[index++] = BitConverter.GetBytes(typeCode)[1];
            array[index++] = BitConverter.GetBytes(length)[0];
            array[index++] = BitConverter.GetBytes(length)[1];
            value.CopyTo(array, index);
            index += value.Length;
            byte[] destinationArray = new byte[index];
            Array.Copy(array, 0, destinationArray, 0, index);
            return destinationArray;
        }

        public static byte[] PackRequsetRead(string address, int length)
        {
            byte[] array = new byte[0x400];
            int index = 0;
            array[index++] = 0x4c;
            index++;
            byte[] buffer2 = BuildRequestPathCommand(address);
            buffer2.CopyTo(array, index);
            index += buffer2.Length;
            array[1] = (byte)((index - 2) / 2);
            array[index++] = BitConverter.GetBytes(length)[0];
            array[index++] = BitConverter.GetBytes(length)[1];
            byte[] destinationArray = new byte[index];
            Array.Copy(array, 0, destinationArray, 0, index);
            return destinationArray;
        }

    }
}
