using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace YumpooDrive.Profinet.Toyopuc
{
    /// <summary>
    /// Toyopuc PLC的辅助类，用来辅助生成基本的指令信息
    /// </summary>
    public class ToyopucHelper
    {
        #region Command Type
        //顺序程序字符读取
        public const byte READ_PROGRAM_CHAR = 0x18;
        //顺序程序字符写入
        public const byte WRITE_PROGRAM_CHAR = 0x19;

        //字符读取
        public const byte READ_IO_REG_CHAR = 0x1C;
        //字符写入
        public const byte WRITE_IO_REG_CHAR = 0x1D;

        //字节读取
        public const byte READ_IO_REG_BYTE = 0x1E;
        //字节写入
        public const byte WRITE_IO_REG_BYTE = 0x1F;

        //位读取
        public const byte READ_IO_REG_BIT = 0x20;
        //位写入
        public const byte WRITE_IO_REG_BIT = 0x21;

        //多点字符读取
        public const byte READ_IO_REG_CHAR_ARRAY = 0x22;
        //多点字符写入
        public const byte WRITE_IO_REG_CHAR_ARRAY = 0x23;

        //多点字节读取
        public const byte READ_IO_REG_BYTE_ARRAY = 0x24;
        //多点字节写入
        public const byte WRITE_IO_REG_BYTE_ARRAY = 0x25;

        //多点位读取
        public const byte READ_IO_REG_BIT_ARRAY = 0x26;
        //多点位写入
        public const byte WRITE_IO_REG_BIT_ARRAY = 0x27;

        //读出参数
        public const byte READ_PARA = 0x30;
        //写入参数
        public const byte WRITE_PARA = 0x31;

        //拓展程序字符读取
        public const byte EX_READ_PROGRAM_CHAR = 0x90;
        //拓展程序字符写入
        public const byte EX_WRITE_PROGRAM_CHAR = 0x91;

        //拓展数据字符读取
        public const byte EX_READ_DATA_CHAR = 0x94;
        //拓展数据字符写入
        public const byte EX_WRITE_DATA_CHAR = 0x95;

        //拓展数据字节读取
        public const byte EX_READ_DATA_BYTE = 0x96;
        //拓展数据字节写入
        public const byte EX_WRITE_DATA_BYTE = 0x97;
        #endregion

        public static OperateResult<byte[]> BuildReadCommand(string Address, int Len, bool IsBool)
        {
            List<byte> cmd = new List<byte>();
            cmd.Add(0x00);//FT
            cmd.Add(0x00);

            cmd.Add(0x05);//LL
            cmd.Add(0x00);//LH

            byte c = GetCommandType(Address, out byte[] addArray, IsBool);
            cmd.Add(c);//CMD

            if (Address.Contains("-") && Address.StartsWith("p"))
            {
                string pno = Regex.Replace(Address.Split('-')[0], @"[^0-9]+", "");
                cmd.Add(Convert.ToByte(pno));//程序号
            }

            if (addArray == null)
            {
                return OperateResult.CreateFailedResult<byte[]>(new OperateResult());
            }

            cmd.Add(addArray[0]); //Address Low
            cmd.Add(addArray[1]); //Address High

            cmd.Add(BitConverter.GetBytes((ushort)Len)[0]);
            cmd.Add(BitConverter.GetBytes((ushort)Len)[1]);
            return OperateResult.CreateSuccessResult(cmd.ToArray());
        }

        public static OperateResult<byte[]> BuildWriteCommand(string Address, byte[] Data, bool IsBool)
        {
            List<byte> cmd = new List<byte>();
            cmd.Add(0x00);//FT
            cmd.Add(0x00);

            var frameLen = BitConverter.GetBytes(Data.Length + 1);
            cmd.Add(frameLen[0]);//LL
            cmd.Add(frameLen[1]);//LH

            byte c = GetCommandType(Address, out byte[] addArray, false, IsBool);
            cmd.Add(c);//CMD Type

            if (Address.Contains("-") && Address.StartsWith("p"))
            {
                string pno = Regex.Replace(Address.Split('-')[0], @"[^0-9]+", "");
                cmd.Add(Convert.ToByte(pno));//程序号
            }

            if (addArray == null)
            {
                return OperateResult.CreateFailedResult<byte[]>(new OperateResult());
            }

            cmd.Add(addArray[0]);
            cmd.Add(addArray[1]);

            foreach (var da in Data)
            {
                cmd.Add(da);
            }

            return OperateResult.CreateSuccessResult(cmd.ToArray());
        }

        private static byte GetCommandType(string Address, out byte[] AddressArray, bool IsRead = true, bool IsBool = false)
        {
            Address = Address.ToLower();
            if (Address.Contains("-"))
            {
                if (Address.StartsWith("p"))
                {
                    var add = Address.Split('-')[0];

                    AddressArray = GetRealAddress(add, IsBool ? AddressType.Bit : AddressType.Byte, out bool IsEx);
                    if (!IsEx)
                    {
                        return IsRead ? READ_PROGRAM_CHAR : WRITE_PROGRAM_CHAR;
                    }
                    else
                    {
                        return IsRead ? EX_READ_PROGRAM_CHAR : EX_WRITE_PROGRAM_CHAR;
                    }
                }
                else
                {
                    AddressArray = null;
                    return 0;
                }
            }
            else
            {
                AddressArray = GetRealAddress(Address, IsBool ? AddressType.Bit : AddressType.Byte, out bool IsEx);
                if (!IsEx)
                {
                    if (IsBool)
                    {
                        return IsRead ? READ_IO_REG_BIT : WRITE_IO_REG_BIT;
                    }
                    else
                    {
                        return IsRead ? READ_IO_REG_BYTE : WRITE_IO_REG_BYTE;
                    }
                }
                else
                {
                    return IsRead ? EX_READ_DATA_BYTE : EX_WRITE_DATA_BYTE;
                }

            }
        }

        private static byte[] GetRealAddress(string Address, AddressType type, out bool IsExAddr)
        {
            string result = Regex.Replace(Address, @"[^0-9]+", "");

            ushort offset = Convert.ToUInt16(result);
            ushort StartAddr = 0x00;
            #region Bit

            if (Address.StartsWith("k"))
            {
                StartAddr = 0x0020;
            }
            else if (Address.StartsWith("v"))
            {
                StartAddr = 0x0050;
            }
            else if (Address.StartsWith("t") || Address.StartsWith("c"))
            {
                StartAddr = 0x0060;
            }
            else if (Address.StartsWith("l"))
            {
                StartAddr = 0x0080;
            }
            else if (Address.StartsWith("x") || Address.StartsWith("y"))
            {
                StartAddr = 0x0100;
            }
            else if (Address.StartsWith("m"))
            {
                StartAddr = 0x0180;
            }
            #endregion
            #region Char

            else if (Address.StartsWith("s"))
            {
                StartAddr = 0x0200;
            }
            else if (Address.StartsWith("n"))
            {
                StartAddr = 0x0600;
            }
            else if (Address.StartsWith("r"))
            {
                StartAddr = 0x0800;
            }
            else if (Address.StartsWith("d"))
            {
                StartAddr = 0x1000;
            }
            else if (Address.StartsWith("b"))
            {
                StartAddr = 0x6000;
            }

            #endregion
            #region Ex Bit

            else if (Address.StartsWith("ek"))
            {

                StartAddr = 0x0100;
            }
            else if (Address.StartsWith("ev"))
            {
                StartAddr = 0x0200;
            }
            else if (Address.StartsWith("et") || Address.StartsWith("ec"))
            {
                StartAddr = 0x0300;
            }
            else if (Address.StartsWith("el"))
            {
                StartAddr = 0x0380;
            }
            else if (Address.StartsWith("ex") || Address.StartsWith("ey"))
            {
                StartAddr = 0x0580;
            }
            else if (Address.StartsWith("em"))
            {
                StartAddr = 0x0600;
            }
            #endregion
            #region Ex Char
            else if (Address.StartsWith("es"))
            {
                StartAddr = 0x0800;
            }
            else if (Address.StartsWith("en"))
            {
                StartAddr = 0x1000;
            }
            else if (Address.StartsWith("h"))
            {
                StartAddr = 0x1800;
            }
            else if (Address.StartsWith("u"))
            {
                StartAddr = 0x0000;
            }
            #endregion

            switch (Address[0])
            {
                case 'e':
                case 'h':
                case 'u':
                    IsExAddr = true;
                    break;
                default:
                    IsExAddr = false;
                    break;
            }

            switch (type)
            {
                case AddressType.Char:
                    return BitConverter.GetBytes((ushort)(StartAddr + offset));
                case AddressType.Byte:
                    return BitConverter.GetBytes((ushort)(StartAddr + offset * 2));
                case AddressType.Bit:
                    return BitConverter.GetBytes((ushort)(StartAddr + offset * 16));
                default:
                    return null;
            }
        }

        public static OperateResult<byte[]> ExtractActualData(byte[] response)
        {
            return OperateResult.CreateSuccessResult(response.Skip(5).Take(response.Length - 5).ToArray());
        }

        public enum AddressType
        {
            Char,
            Byte,
            Bit
        }
    }
}
