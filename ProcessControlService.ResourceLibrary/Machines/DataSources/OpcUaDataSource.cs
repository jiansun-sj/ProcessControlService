using log4net;
using Opc.Ua;
using OpcUaHelper;
using System;
using System.Xml;

namespace ProcessControlService.ResourceLibrary.Machines.DataSources
{
    /// <summary>
    /// add by zsy
    /// OPC UA客户端数据源
    /// 配置参考backup/OpcUaTest.xml
    /// </summary>
    public class OpcUaDataSource : DataSource
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(OpcUaDataSource));
        private OpcUaClient Client;
        private string ServerUrl = "";
        private string UserName = "";
        private string Password = "";
        public OpcUaDataSource(string Name, Machine machine)
            : base(Name, machine)
        {
        }

        public override void Disconnect()
        {
            Client?.Disconnect();
        }

        protected override bool Connect()
        {
            try
            {
                Client.ReconnectPeriod = 2;

                Client.ConnectServer(ServerUrl).GetAwaiter().GetResult();

                if (Client.Connected)
                {
                    LOG.Info($"Connect to OpcUaDataSource [{SourceName}] success.");
                }
                else
                {
                    LOG.Warn($"Connect to OpcUaDataSource [{SourceName}] failed.");
                }
                return Client.Connected;
            }
            catch (Exception ex)
            {
                LOG.Error($"Connect to [{SourceName}] failed. Message [{ex.Message}]");
                return false;
            }

        }

        protected override bool Connected => Client != null && Client.Connected;

        public override bool UpdateAllValue()
        {
            try
            {
                foreach (Tag tag in Tags.Values)
                {
                    var value = ReadTag(tag);
                    if (value != null)
                    {
                        tag.Quality = Quality.Good;
                    }
                    else
                    {
                        tag.Quality = Quality.Bad;
                    }
                    tag.TagValue = value;
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override object ReadTag(Tag tag)
        {
            if (tag.AccessType == TagAccessType.Read || tag.AccessType == TagAccessType.ReadWrite)
            {
                try
                {
                    switch (tag.TagType)
                    {
                        case "bool":
                            return Client.ReadNode<bool>(tag.Address);
                        case "byte":
                            return Client.ReadNode<byte>(tag.Address);
                        case "sbyte":
                            return Client.ReadNode<sbyte>(tag.Address);
                        case "short":
                        case "int16":
                            var val = Client.ReadNode<short>(tag.Address);
                            return val;
                        case "ushort":
                        case "uint16":
                            return Client.ReadNode<ushort>(tag.Address);
                        case "int":
                        case "int32":
                            return Client.ReadNode<int>(tag.Address);
                        case "uint":
                        case "uint32":
                            return Client.ReadNode<uint>(tag.Address);
                        case "long":
                        case "int64":
                            return Client.ReadNode<long>(tag.Address);
                        case "ulong":
                        case "uint64":
                            return Client.ReadNode<ulong>(tag.Address);
                        case "float":
                        case "single":
                            return Client.ReadNode<float>(tag.Address);
                        case "double":
                            return Client.ReadNode<double>(tag.Address);
                        case "string":
                            return Client.ReadNode<string>(tag.Address);
                        default:
                            throw new Exception("Unsupport tag type");
                    }
                }
                catch (Exception ex)
                {
                    LOG.Error($"Datasource[{SourceName}] read error. Tag[{tag.TagName}] Address[{tag.Address}] Message[{ex.Message}]");
                    tag.TagValue = null;
                    tag.Quality = Quality.Bad;
                    return tag.TagValue;
                }
            }
            else
            {
                return null;
            }
        }

        public override bool WriteTagToRealDevice(Tag tag, object value)
        {
            lock (this)
            {
                try
                {
                    bool writeRes = false;
                    switch (tag.TagType)
                    {
                        case "bool":
                            writeRes = Client.WriteNode(tag.Address, (bool)value); break;
                        case "byte":
                            writeRes = Client.WriteNode(tag.Address, (byte)value); break;
                        case "sbyte":
                            writeRes = Client.WriteNode(tag.Address, (sbyte)value); break;
                        case "short":
                        case "int16":
                            writeRes = Client.WriteNode(tag.Address, (short)value); break;
                        case "ushort":
                        case "uint16":
                            writeRes = Client.WriteNode(tag.Address, (ushort)value); break;
                        case "int":
                        case "int32":
                            writeRes = Client.WriteNode(tag.Address, (int)value); break;
                        case "uint":
                        case "uint32":
                            writeRes = Client.WriteNode(tag.Address, (uint)value); break;
                        case "long":
                        case "int64":
                            writeRes = Client.WriteNode(tag.Address, (long)value); break;
                        case "ulong":
                        case "uint64":
                            writeRes = Client.WriteNode(tag.Address, (ulong)value); break;
                        case "float":
                        case "single":
                            writeRes = Client.WriteNode(tag.Address, (float)value); break;
                        case "double":
                            writeRes = Client.WriteNode(tag.Address, (double)value); break;
                        case "string":
                            writeRes = Client.WriteNode(tag.Address, (string)value); break;
                        default:
                            throw new Exception("Unsupport tag type");
                    }

                    LOG.Info($"DataSource[{SourceName}] write tag. Tag[{tag.TagName}] Address[{tag.Address}] Value[{value.ToString()}] IsSuccess[{writeRes}]");
                    return true;

                }
                catch (Exception ex)
                {
                    LOG.Error($"DataSource[{SourceName}] write tag error. Tag[{tag.TagName}] Address[{tag.Address}] Value[{value.ToString()}] Message[{ex.Message}]");
                    return false;
                }
            }
        }

        public override bool LoadFromConfig(XmlNode node)
        {
            try
            {
                var xmlElement = (XmlElement)node;
                ServerUrl = xmlElement.GetAttribute("ServerUrl");
                if (xmlElement.HasAttribute("UserName"))
                {
                    UserName = xmlElement.GetAttribute("UserName");
                    Password = xmlElement.GetAttribute("Password");
                }

                Client = new OpcUaClient
                {
                    UserIdentity = !string.IsNullOrEmpty(UserName) ? new UserIdentity(UserName, Password) : new UserIdentity(new AnonymousIdentityToken())
                };
                return base.LoadFromConfig(xmlElement);
            }
            catch (Exception ex)
            {
                LOG.Error($"DataSource[{SourceName}] load error. Ex[{ex.Message}]");
                return false;
            }

        }

    }


}
