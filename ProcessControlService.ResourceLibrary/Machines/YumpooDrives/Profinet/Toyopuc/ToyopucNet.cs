using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using YumpooDrive.Core;

namespace YumpooDrive.Profinet.Toyopuc
{
    public static class TcpClientEx
    {
        public static bool IsOnline(this TcpClient c)
        {
            try
            {
                return !((c.Client.Poll(1000, SelectMode.SelectRead) && (c.Client.Available == 0)) || !c.Client.Connected);
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
    public class ToyopucNet //: NetworkDeviceBase<ToyopucMessage, RegularByteTransform>
    {
        public ToyopucNet()
        {
            WordLength = 2;
        }

        public ToyopucNet(string ipAddress, int port = 5000)
        {
            WordLength = 2;
            IpAddress = ipAddress;
            Port = port;
        }

        private readonly RegularByteTransform ByteTransform = new RegularByteTransform();

        public int WordLength { get; set; }

        public string IpAddress { get; set; }
        public int Port { get; set; } = 44818;

        public bool IsConnect => client != null && client.IsOnline();

        private TcpClient client;
        private NetworkStream Ns;


        public OperateResult ConnectServer()
        {
            try
            {
                client = new TcpClient();
                client.Client.ReceiveTimeout = 500;
                client.SendTimeout = 500;
                client.Connect(IpAddress, Port);
                Ns = client.GetStream();
                return new OperateResult() { IsSuccess = true };
            }
            catch (Exception)
            {
                return new OperateResult() { IsSuccess = false };
            }

        }

        public void ConnectClose()
        {
            try
            {
                client.Close();
            }
            catch { }
        }

        private OperateResult CheckResponse(byte[] response)
        {
            try
            {
                int err = response[4];
                if (response[1] == 0)
                {
                    return OperateResult.CreateSuccessResult();
                }
                string msg = string.Empty;
                switch (err)
                {
                    case 0x11:
                        msg = StringResources.Language.ToyopucError11;
                        break;
                    case 0x20:
                        msg = StringResources.Language.ToyopucError20;
                        break;
                    case 0x21:
                        msg = StringResources.Language.ToyopucError21;
                        break;
                    case 0x23:
                        msg = StringResources.Language.ToyopucError23;
                        break;
                    case 0x24:
                        msg = StringResources.Language.ToyopucError24;
                        break;
                    case 0x25:
                        msg = StringResources.Language.ToyopucError25;
                        break;
                    case 0x26:
                        msg = StringResources.Language.ToyopucError26;
                        break;
                    case 0x31:
                        msg = StringResources.Language.ToyopucError31;
                        break;
                    case 0x32:
                        msg = StringResources.Language.ToyopucError32;
                        break;
                    case 0x33:
                        msg = StringResources.Language.ToyopucError33;
                        break;
                    case 0x34:
                        msg = StringResources.Language.ToyopucError34;
                        break;
                    case 0x35:
                        msg = StringResources.Language.ToyopucError35;
                        break;
                    case 0x36:
                        msg = StringResources.Language.ToyopucError36;
                        break;
                    case 0x39:
                        msg = StringResources.Language.ToyopucError39;
                        break;
                    case 0x3C:
                        msg = StringResources.Language.ToyopucError3C;
                        break;
                    case 0x3D:
                        msg = StringResources.Language.ToyopucError3D;
                        break;
                    case 0x3E:
                        msg = StringResources.Language.ToyopucError3E;
                        break;
                    case 0x3F:
                        msg = StringResources.Language.ToyopucError3F;
                        break;
                    case 0x40:
                        msg = StringResources.Language.ToyopucError40;
                        break;
                    case 0x41:
                        msg = StringResources.Language.ToyopucError41;
                        break;
                    case 0x42:
                        msg = StringResources.Language.ToyopucError42;
                        break;
                    case 0x43:
                        msg = StringResources.Language.ToyopucError43;
                        break;
                    case 0x52:
                        msg = StringResources.Language.ToyopucError52;
                        break;
                    case 0x66:
                        msg = StringResources.Language.ToyopucError66;
                        break;
                    case 0x70:
                        msg = StringResources.Language.ToyopucError70;
                        break;
                    case 0x72:
                        msg = StringResources.Language.ToyopucError72;
                        break;
                    case 0x73:
                        msg = StringResources.Language.ToyopucError73;
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


        public virtual OperateResult<byte[]> ReadFromCoreServer(byte[] send)
        {
            try
            {
                lock (this)
                {
                    OperateResult<byte[]> res = new OperateResult<byte[]>(StringResources.Language.ReceiveDataTimeout);

                    if (IsConnect)
                    {
                        Ns.Write(send, 0, send.Length);
                        byte[] rcv = new byte[1024];

                        int rectime = 50;

                        while (--rectime > 0)
                        {
                            if (Ns.DataAvailable)
                            {
                                int len = Ns.Read(rcv, 0, rcv.Length);
                                byte[] recdata = new byte[len];
                                Array.Copy(rcv, 0, recdata, 0, len);
                                res = OperateResult.CreateSuccessResult(recdata);
                                break;
                            }
                            Thread.Sleep(10);
                        }
                    }

                    return res;
                }
            }
            catch (Exception ex)
            {
                ConnectClose();
                ConnectServer();
                return OperateResult.CreateFailedResult<byte[]>(new OperateResult<byte[]>(ex.Message));

            }

        }


        public OperateResult<byte[]> Read(string address, ushort length, bool IsBool = false)
        {
            var datas = ToyopucHelper.BuildReadCommand(address, length, IsBool);

            if (!datas.IsSuccess)
            {
                return datas;
            }

            var read = ReadFromCoreServer(datas.Content);

            if (!read.IsSuccess)
                return read;

            var check = CheckResponse(read.Content);
            if (!check.IsSuccess)
            {
                return OperateResult.CreateFailedResult<byte[]>(check);
            }
            return ToyopucHelper.ExtractActualData(read.Content);

        }

        public OperateResult<bool> ReadBool(string address)
        {
            OperateResult<byte[]> result = Read(address, 1, true);
            if (!result.IsSuccess)
            {
                return OperateResult.CreateFailedResult<bool>(result);
            }
            return OperateResult.CreateSuccessResult(ByteTransform.TransBool(result.Content, 0));
        }

        public OperateResult<bool[]> ReadBoolArray(string address)
        {
            OperateResult<byte[]> result = Read(address, 1);
            if (!result.IsSuccess)
            {
                return OperateResult.CreateFailedResult<bool[]>(result);
            }
            return OperateResult.CreateSuccessResult(ByteTransform.TransBool(result.Content, 0, result.Content.Length));
        }

        public OperateResult<byte> ReadByte(string address)
        {
            OperateResult<byte[]> result = Read(address, 1);
            if (!result.IsSuccess)
            {
                return OperateResult.CreateFailedResult<byte>(result);
            }
            return OperateResult.CreateSuccessResult(ByteTransform.TransByte(result.Content, 0));
        }


        public OperateResult<string> ReadString(string address, ushort length, Encoding encoding)
        {
            OperateResult<byte[]> result = Read(address, length);
            if (!result.IsSuccess)
            {
                return OperateResult.CreateFailedResult<string>(result);
            }
            return OperateResult.CreateSuccessResult(encoding.GetString(result.Content));
        }


        public override string ToString()
        {
            return $"ToyopucNet[{IpAddress}:{Port}]";
        }

        public OperateResult Write(string address, bool value)
        {
            return WriteTag(address, new byte[] { value ? (byte)1 : (byte)0 }, true);
        }

        public OperateResult Write(string address, byte value)
        {
            byte[] buffer1 = new byte[1];
            buffer1[0] = value;
            return WriteTag(address, buffer1);
        }

        public OperateResult Write(string address, byte[] value)
        {
            return WriteTag(address, value);
        }

        public OperateResult Write(string address, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                value = string.Empty;
            }
            byte[] bytes = Encoding.ASCII.GetBytes(value);

            return WriteTag(address, bytes);
        }

        public OperateResult WriteTag(string address, byte[] value, bool IsBool = false)
        {
            OperateResult<byte[]> command = ToyopucHelper.BuildWriteCommand(address, value, IsBool);
            if (!command.IsSuccess) return command;

            // 核心交互
            OperateResult<byte[]> read = ReadFromCoreServer(command.Content);
            if (!read.IsSuccess) return read;

            // 检查反馈
            OperateResult check = CheckResponse(read.Content);
            if (!check.IsSuccess) return OperateResult.CreateFailedResult<byte[]>(check);

            // 提取写入结果
            return read;
        }
    }



}
