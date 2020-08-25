using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using YumpooDrive.BasicFramework;
using YumpooDrive.Core;
using YumpooDrive.Core.IMessage;
using YumpooDrive.Core.Net;

namespace YumpooDrive.Profinet.AllenBradley
{
    public class AllenBradleyNetNew : NetworkDeviceBase<AllenBradleyMessage, RegularByteTransform>
    {

        public int ArraySegment { get; set; }

        public uint SessionHandle { get; private set; }

        public byte Slot { get; set; }

        public AllenBradleyNetNew()
        {
            Slot = 0;
            ArraySegment = 100;
            base.WordLength = 2;
        }

        public AllenBradleyNetNew(string ipAddress, int port = 0xaf12)
        {
            Slot = 0;
            ArraySegment = 100;
            base.WordLength = 2;
            IpAddress = ipAddress;
            Port = port;
        }

        public OperateResult<byte[]> BuildReadCommand(string[] address)
        {
            if (address == null)
            {
                return new OperateResult<byte[]>("address or length is null");
            }
            int[] length = new int[address.Length];
            for (int i = 0; i < address.Length; i++)
            {
                length[i] = 1;
            }
            return BuildReadCommand(address, length);
        }

        public OperateResult<byte[]> BuildReadCommand(string[] address, int[] length)
        {
            if ((address == null) || (length == null))
            {
                return new OperateResult<byte[]>("address or length is null");
            }
            if (address.Length != length.Length)
            {
                return new OperateResult<byte[]>("address and length is not same array");
            }
            try
            {
                List<byte[]> list = new List<byte[]>();
                for (int i = 0; i < address.Length; i++)
                {
                    list.Add(AllenBradleyHelper.PackRequsetRead(address[i], length[i]));
                }
                byte[] commandSpecificData = AllenBradleyHelper.PackCommandSpecificData(Slot, list.ToArray());
                return OperateResult.CreateSuccessResult<byte[]>(AllenBradleyHelper.PackRequestHeader(0x6f, SessionHandle, commandSpecificData));
            }
            catch (Exception exception)
            {
                return new OperateResult<byte[]>("Address Wrong:" + exception.Message);
            }
        }

        public OperateResult<byte[]> BuildWriteCommand(string address, ushort typeCode, byte[] data, int length = 1)
        {
            try
            {
                byte[] buffer = AllenBradleyHelper.PackRequestWrite(address, typeCode, data, length);
                byte[][] cips = new byte[][] { buffer };
                byte[] commandSpecificData = AllenBradleyHelper.PackCommandSpecificData(Slot, cips);
                return OperateResult.CreateSuccessResult<byte[]>(AllenBradleyHelper.PackRequestHeader(0x6f, SessionHandle, commandSpecificData));
            }
            catch (Exception exception)
            {
                return new OperateResult<byte[]>("Address Wrong:" + exception.Message);
            }
        }

        private OperateResult CheckResponse(byte[] response)
        {
            try
            {
                int err = base.ByteTransform.TransInt32(response, 8);
                if (err == 0)
                {
                    return OperateResult.CreateSuccessResult();
                }
                string msg = string.Empty;
                switch (err)
                {
                    case 0x65:
                        msg = StringResources.Language.AllenBradleySessionStatus65;
                        break;

                    case 0x69:
                        msg = StringResources.Language.AllenBradleySessionStatus69;
                        break;

                    case 1:
                        msg = StringResources.Language.AllenBradleySessionStatus01;
                        break;

                    case 2:
                        msg = StringResources.Language.AllenBradleySessionStatus02;
                        break;

                    case 3:
                        msg = StringResources.Language.AllenBradleySessionStatus03;
                        break;

                    case 100:
                        msg = StringResources.Language.AllenBradleySessionStatus64;
                        break;

                    default:
                        msg = StringResources.Language.UnknownError;
                        break;
                }
                return new OperateResult(err, msg);
            }
            catch (Exception exception)
            {
                return new OperateResult(exception.Message);
            }
        }

        protected override OperateResult ExtraOnDisconnect(Socket socket)
        {
            OperateResult<byte[]> result = ReadFromCoreServer(socket, UnRegisterSessionHandle());
            if (!result.IsSuccess)
            {
                return result;
            }
            return OperateResult.CreateSuccessResult();
        }

        protected override OperateResult InitializationOnConnect(Socket socket)
        {
            OperateResult<byte[]> result = ReadFromCoreServer(socket, RegisterSessionHandle());
            if (!result.IsSuccess)
            {
                return result;
            }
            OperateResult result2 = CheckResponse(result.Content);
            if (!result2.IsSuccess)
            {
                return result2;
            }
            SessionHandle = base.ByteTransform.TransUInt32(result.Content, 4);
            return OperateResult.CreateSuccessResult();
        }

        public OperateResult<byte[]> Read(string[] address)
        {
            if (address == null)
            {
                return new OperateResult<byte[]>("address can not be null");
            }
            int[] length = new int[address.Length];
            for (int i = 0; i < length.Length; i++)
            {
                length[i] = 1;
            }
            return Read(address, length);
        }

        public override OperateResult<byte[]> Read(string address, ushort length)
        {
            if (length > 1)
            {
                return ReadSegment(address, 0, length);
            }
            string[] textArray1 = new string[] { address };
            int[] numArray1 = new int[] { length };
            return Read(textArray1, numArray1);
                
        }

        public OperateResult<byte[]> Read(string[] address, int[] length)
        {
            OperateResult<byte[]> result = BuildReadCommand(address, length);
            if (!result.IsSuccess)
            {
                return result;
            }
            OperateResult<byte[]> result2 = base.ReadFromCoreServer(result.Content);
            if (!result2.IsSuccess)
            {
                return result2;
            }
            OperateResult result3 = CheckResponse(result2.Content);
            if (!result3.IsSuccess)
            {
                return OperateResult.CreateFailedResult<byte[]>(result3);
            }
            return AllenBradleyHelper.ExtractActualData(result2.Content, true);
        }

        public override OperateResult<bool> ReadBool(string address)
        {
            OperateResult<byte[]> result = Read(address, 1);
            if (!result.IsSuccess)
            {
                return OperateResult.CreateFailedResult<bool>(result);
            }
            return OperateResult.CreateSuccessResult<bool>(base.ByteTransform.TransBool(result.Content, 0));
        }

        public OperateResult<bool[]> ReadBoolArray(string address)
        {
            OperateResult<byte[]> result = Read(address, 1);
            if (!result.IsSuccess)
            {
                return OperateResult.CreateFailedResult<bool[]>(result);
            }
            return OperateResult.CreateSuccessResult<bool[]>(base.ByteTransform.TransBool(result.Content, 0, result.Content.Length));
        }

        private OperateResult<byte[]> ReadByCips(params byte[][] cips)
        {
            OperateResult<byte[]> result = ReadCipFromServer(cips);
            if (!result.IsSuccess)
            {
                return result;
            }
            return AllenBradleyHelper.ExtractActualData(result.Content, true);
        }

        public OperateResult<byte> ReadByte(string address)
        {
            OperateResult<byte[]> result = Read(address, 1);
            if (!result.IsSuccess)
            {
                return OperateResult.CreateFailedResult<byte>(result);
            }
            return OperateResult.CreateSuccessResult<byte>(base.ByteTransform.TransByte(result.Content, 0));
        }

        public OperateResult<byte[]> ReadCipFromServer(params byte[][] cips)
        {
            byte[] commandSpecificData = AllenBradleyHelper.PackCommandSpecificData(Slot, cips);
            byte[] send = AllenBradleyHelper.PackRequestHeader(0x6f, SessionHandle, commandSpecificData);
            OperateResult<byte[]> result = base.ReadFromCoreServer(send);
            if (!result.IsSuccess)
            {
                return result;
            }
            OperateResult result2 = CheckResponse(result.Content);
            if (!result2.IsSuccess)
            {
                return OperateResult.CreateFailedResult<byte[]>(result2);
            }
            return OperateResult.CreateSuccessResult<byte[]>(result.Content);
        }

        public override OperateResult<double[]> ReadDouble(string address, ushort length) =>
            ByteTransformHelper.GetResultFromBytes<double[]>(Read(address, length), m => ByteTransform.TransDouble(m, 0, length));

        public override OperateResult<float[]> ReadFloat(string address, ushort length) =>
            ByteTransformHelper.GetResultFromBytes<float[]>(Read(address, length), m => ByteTransform.TransSingle(m, 0, length));

        public override OperateResult<short[]> ReadInt16(string address, ushort length) =>
            ByteTransformHelper.GetResultFromBytes<short[]>(Read(address, length), m => ByteTransform.TransInt16(m, 0, length));

        public override OperateResult<int[]> ReadInt32(string address, ushort length) =>
            ByteTransformHelper.GetResultFromBytes<int[]>(Read(address, length), m => ByteTransform.TransInt32(m, 0, length));

        public override OperateResult<long[]> ReadInt64(string address, ushort length) =>
            ByteTransformHelper.GetResultFromBytes<long[]>(Read(address, length), m => ByteTransform.TransInt64(m, 0, length));

        public OperateResult<byte[]> ReadSegment(string address, int startIndex, int length)
        {
            try
            {
                ushort num2;
                List<byte> list = new List<byte>();
                for (ushort i = 0; i < length; i = (ushort)(i + num2))
                {
                    num2 = (ushort)Math.Min(length - i, 100);
                    byte[][] cips = new byte[][] { AllenBradleyHelper.PackRequestReadSegment(address, startIndex + i, num2) };
                    OperateResult<byte[]> result = ReadByCips(cips);
                    if (!result.IsSuccess)
                    {
                        return result;
                    }
                    list.AddRange(result.Content);
                }
                return OperateResult.CreateSuccessResult<byte[]>(list.ToArray());
            }
            catch (Exception exception)
            {
                return new OperateResult<byte[]>("Address Wrong:" + exception.Message);
            }
        }

        public OperateResult<string> ReadString(string address) =>
            ReadString(address, 1, Encoding.ASCII);

        public override OperateResult<string> ReadString(string address, ushort length, Encoding encoding)
        {
            OperateResult<byte[]> result = Read(address, length);
            if (!result.IsSuccess)
            {
                return OperateResult.CreateFailedResult<string>(result);
            }
            if (result.Content.Length >= 6)
            {
                int count = BitConverter.ToInt32(result.Content, 2);
                return OperateResult.CreateSuccessResult<string>(encoding.GetString(result.Content, 6, count));
            }
            return OperateResult.CreateSuccessResult<string>(encoding.GetString(result.Content));
        }

        public override OperateResult<ushort[]> ReadUInt16(string address, ushort length) =>
            ByteTransformHelper.GetResultFromBytes<ushort[]>(Read(address, length), m => ByteTransform.TransUInt16(m, 0, length));

        public override OperateResult<uint[]> ReadUInt32(string address, ushort length) =>
            ByteTransformHelper.GetResultFromBytes<uint[]>(Read(address, length), m => ByteTransform.TransUInt32(m, 0, length));

        public override OperateResult<ulong[]> ReadUInt64(string address, ushort length) =>
            ByteTransformHelper.GetResultFromBytes<ulong[]>(Read(address, length), m => ByteTransform.TransUInt64(m, 0, length));

        public byte[] RegisterSessionHandle()
        {
            byte[] buffer1 = new byte[4];
            buffer1[0] = 1;
            byte[] commandSpecificData = buffer1;
            return AllenBradleyHelper.PackRequestHeader(0x65, 0, commandSpecificData);
        }

        public override string ToString() =>
            $"AllenBradleyNet[{IpAddress}:{Port}]";

        public byte[] UnRegisterSessionHandle() =>
            AllenBradleyHelper.PackRequestHeader(0x66, SessionHandle, new byte[0]);

        public override OperateResult Write(string address, double[] values) =>
            WriteTag(address, 0xcb, base.ByteTransform.TransByte(values), values.Length);

        public override OperateResult Write(string address, short[] values) =>
            WriteTag(address, 0xc3, base.ByteTransform.TransByte(values), values.Length);

        public override OperateResult Write(string address, int[] values) =>
            WriteTag(address, 0xc4, base.ByteTransform.TransByte(values), values.Length);

        public override OperateResult Write(string address, long[] values) =>
            WriteTag(address, 0xc5, base.ByteTransform.TransByte(values), values.Length);

        public override OperateResult Write(string address, float[] values) =>
            WriteTag(address, 0xca, base.ByteTransform.TransByte(values), values.Length);

        public override OperateResult Write(string address, ushort[] values) =>
            WriteTag(address, 0xc3, base.ByteTransform.TransByte(values), values.Length);

        public override OperateResult Write(string address, uint[] values) =>
            WriteTag(address, 0xc4, base.ByteTransform.TransByte(values), values.Length);

        public override OperateResult Write(string address, ulong[] values) =>
            WriteTag(address, 0xc5, base.ByteTransform.TransByte(values), values.Length);

        public override OperateResult Write(string address, bool value) =>
            WriteTag(address, 0xc1, value ? new byte[] { 0xff, 0xff } : new byte[2], 1);

        public OperateResult Write(string address, byte value)
        {
            byte[] buffer1 = new byte[2];
            buffer1[0] = value;
            return WriteTag(address, 0xc2, buffer1, 1);
        }

        public override OperateResult Write(string address, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                value = string.Empty;
            }
            byte[] bytes = Encoding.ASCII.GetBytes(value);
            OperateResult result = base.Write(address + ".LEN", bytes.Length);
            if (!result.IsSuccess)
            {
                return result;
            }
            byte[] buffer2 = SoftBasic.ArrayExpandToLengthEven<byte>(bytes);
            return WriteTag(address + ".DATA[0]", 0xc2, buffer2, bytes.Length);
        }

        public OperateResult WriteTag(string address, ushort typeCode, byte[] value, int length = 1)
        {
            OperateResult<byte[]> result = BuildWriteCommand(address, typeCode, value, length);
            if (!result.IsSuccess)
            {
                return result;
            }
            OperateResult<byte[]> result2 = ReadFromCoreServer(result.Content);
            if (!result2.IsSuccess)
            {
                return result2;
            }
            OperateResult result3 = CheckResponse(result2.Content);
            if (!result3.IsSuccess)
            {
                return OperateResult.CreateFailedResult<byte[]>(result3);
            }
            return AllenBradleyHelper.ExtractActualData(result2.Content, false);
        }


    }



}
