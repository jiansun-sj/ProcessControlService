using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YumpooDrive;

namespace ProcessControlService.ResourceLibrary.Machines.DataSources.Utils
{
    public static class ConvertUtils
    {
        public static byte[] GetBytes(Tag tag, object value)
        {
            switch (tag.TagType)
            {
                case "byte":
                    return BitConverter.GetBytes(Convert.ToByte(value));
                case "sbyte":
                    return BitConverter.GetBytes(Convert.ToSByte(value));
                case "uint16":
                case "ushort":
                    return BitConverter.GetBytes(Convert.ToUInt16(value));
                case "int16":
                case "short":
                    return BitConverter.GetBytes(Convert.ToInt16(value));
                case "int32":
                case "int":
                    return BitConverter.GetBytes(Convert.ToInt32(value));
                case "uint32":
                case "uint":
                    return BitConverter.GetBytes(Convert.ToUInt32(value));
                case "long":
                case "int64":
                    return BitConverter.GetBytes(Convert.ToInt64(value));
                case "ulong":
                case "uint64":
                    return BitConverter.GetBytes(Convert.ToUInt64(value));
                case "float":
                case "single":
                    return BitConverter.GetBytes(Convert.ToSingle(value));
                case "double":
                    return BitConverter.GetBytes(Convert.ToDouble(value));
                case "string":
                    return Encoding.ASCII.GetBytes(value.ToString());
                default:
                    return null;
            }
        }

        public static ushort GetLength(Tag tag)
        {
            switch (tag.TagType)
            {
                case "byte":
                case "sbyte":
                    return 1;
                case "short":
                case "ushort":
                case "int16":
                case "uint16":
                    return 2;
                case "int":
                case "int32":
                case "uint":
                case "uint32":
                case "single":
                case "float":
                    return 4;
                case "double":
                case "long":
                case "int64":
                    return 8;
                default:
                    throw new Exception("Unknown tag type");
            }
        }

        public static void DecodeTagValue(Tag tag, OperateResult<byte[]> res, bool IsStep7 = false)
        {
            if (res.IsSuccess)
            {
                if (IsStep7)
                {
                    res.Content = res.Content.Reverse().ToArray();
                }
                switch (tag.TagType)
                {
                    case "byte":
                        tag.TagValue = res.Content.Length > 0 ? res.Content[0] : 0;
                        break;
                    case "int16":
                    case "short":
                        tag.TagValue = BitConverter.ToInt16(res.Content, 0);
                        break;
                    case "uint16":
                    case "ushort":
                        tag.TagValue = BitConverter.ToUInt16(res.Content, 0);
                        break;
                    case "int32":
                    case "int":
                        tag.TagValue = BitConverter.ToInt32(res.Content, 0);
                        break;
                    case "uint32":
                    case "uint":
                        tag.TagValue = BitConverter.ToUInt32(res.Content, 0);
                        break;
                    case "float":
                    case "single":
                        tag.TagValue = BitConverter.ToSingle(res.Content, 0);
                        break;
                    case "long":
                    case "int64":
                        tag.TagValue = BitConverter.ToInt64(res.Content, 0);
                        break;
                    case "double":
                        tag.TagValue = BitConverter.ToDouble(res.Content, 0);
                        break;
                    case "string":
                        tag.TagValue = Encoding.ASCII.GetString(res.Content).Replace("\0", "");
                        break;
                    default:
                        break;
                }
                tag.Quality = Quality.Good;
            }
            else
            {
                tag.TagValue = null;
                tag.Quality = Quality.Bad;
            }

        }

        public static byte[] ReverseBits(byte[] datas)
        {
            List<byte> bts = new List<byte>();
            foreach (byte data in datas)
            {
                int temp;
                temp = (data << 4) | (data >> 4);
                temp = ((temp << 2) & 0xcc) | ((temp >> 2) & 0x33);
                temp = ((temp << 1) & 0xaa) | ((temp >> 1) & 0x55);
                bts.Add((byte)temp);
            }
            return bts.ToArray().Reverse().ToArray();

        }

    }
}
