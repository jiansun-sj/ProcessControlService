using log4net;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;

namespace ProcessControlService.WCFClients
{

    /// <summary>
    /// 连接远程HOST管理池
    /// David 20191003
    /// </summary>
    public class HostConnectionManager
    {
        #region Private Fields
        private static readonly ILog LOG = LogManager.GetLogger(typeof(HostConnectionManager));

        //当前连接配置
        //private static readonlyDictionary<string, IHostConnection> HostConnections = new Dictionary<string, IHostConnection>();
        private static Dictionary<string, IHostConnection> CurrentHostConnections;
        private static string _currentHostConnetionsName = string.Empty;

        //连接配置集合
        private static readonly Dictionary<string, Dictionary<string, IHostConnection>> HostConnectionGroups = new Dictionary<string, Dictionary<string, IHostConnection>>();

        /// <summary>
        /// Key:HostName  Value.Item1:Main Connection  Value.Item2:Slave Connetion
        /// </summary>
        //private static readonly Dictionary<string, Tuple<HostConnection, HostConnection>> HostConnections = new Dictionary<string, Tuple<HostConnection, HostConnection>>();

        private static bool ConfigLoaded { get; set; } = false;
        #endregion

        #region Private Methods
        private static void LoadFromConfig()
        {
            try
            {
                if (ConfigLoaded)
                {
                    return;
                }
                string appPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
                string xmlPathfile = appPath + "WCFClient.xml";

                XmlDocument doc = new XmlDocument();
                doc.Load(xmlPathfile);

                XmlNodeList RemoteHosts = doc.SelectNodes("//WCFClientSetting//RemoteHost");

                foreach (XmlNode RemoteHost in RemoteHosts)
                {//遍历每一个配置组
                    string HostGroupName;

                    XmlElement RemoteHostItem = (XmlElement)RemoteHost;

                    HostGroupName = RemoteHostItem.HasAttribute("Name") ? RemoteHostItem.GetAttribute("Name").ToLower() : "default";

                    Dictionary<string, IHostConnection> connections = new Dictionary<string, IHostConnection>();

                    //装载连接
                    foreach (XmlNode item in RemoteHost)
                    {//装载连接配置
                        if (item.NodeType == XmlNodeType.Comment)
                        {
                            continue;
                        }
                        XmlElement host = (XmlElement)item;

                        string strName = host.GetAttribute("Name");
                        string strService = host.GetAttribute("Service");
                        string strAddress = host.GetAttribute("Address");

                        string partnerAddress = "";

                        if (host.HasAttribute("PartnerAddress"))
                        {//冗余连接
                            partnerAddress = host.GetAttribute("PartnerAddress");
                        }

                        HostConnectionType connectionType = HostConnectionType.Unknow;

                        if (strService == "MachineHost")
                        {
                            connectionType = HostConnectionType.Machine;
                        }
                        else if (strService == "ProcessHost")
                        {
                            connectionType = HostConnectionType.Process;
                        }
                        else if (strService == "ResourceHost") // add by dongmin 20180221
                        {
                            connectionType = HostConnectionType.Resource;

                        }
                        else if (strService == "PartnerHost") // add by dongmin 20180310
                        {
                            connectionType = HostConnectionType.Partner;
                        }
                        else if (strService == "AdminHost") // add by dongmin 20191109
                        {
                            connectionType = HostConnectionType.Admin;
                        }
                        else
                        {
                            throw new Exception("没有定义该连接" + strService);
                        }

                        if (string.IsNullOrEmpty(partnerAddress))
                        {
                            connections.Add(strName, new HostConnection(connectionType, strAddress));
                        }
                        else
                        {
                            connections.Add(strName, new RedundancyHostConnection(connectionType, strAddress, partnerAddress));
                        }
                    }

                    //放到连接组集合
                    if (!HostConnectionGroups.ContainsKey(HostGroupName))
                    {
                        HostConnectionGroups.Add(HostGroupName, connections);
                    }
                }

                SelectCurrentGroup("default");
              
                ConfigLoaded = true;

            }
            catch (Exception ex)
            {
                LOG.Error(string.Format("加载WCFClient配置文件出错." + ex));
            }

        }
        #endregion

        #region Public Methods
        public static IHostConnection CreateConnection(HostConnectionType ProxyType)
        {
            try
            {
                if (!ConfigLoaded)
                {
                    LoadFromConfig();
                }

                foreach (var host in CurrentHostConnections.Values)
                {
                    if (ProxyType == host.HostType)
                    {
                        return host;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                LOG.Error(string.Format("创建连接类型{0}失败:{1}", ProxyType.ToString(), ex));
                return null;
            }
        }
        public static IHostConnection CreateConnection(string HostName)
        {
            try
            {
                if (!ConfigLoaded)
                {
                    LoadFromConfig();
                }
                if (!CurrentHostConnections.ContainsKey(HostName))
                {
                    throw new InvalidOperationException($"Can`t find the host name [{HostName}]");
                }
                else
                {
                    return CurrentHostConnections[HostName];


                }
            }
            catch (Exception ex)
            {
                LOG.Error(string.Format("创建连接{0}失败:{1}", HostName, ex.Message));
                return null;
            }
        }

        public static void ReleaseConnection(HostConnectionType ProxyType)
        {
            try
            {
                foreach (var host in CurrentHostConnections.Values)
                {
                    host.StopConnect();
                }
            }
            catch (Exception ex)
            {
                LOG.Error(string.Format("断开连接类型{0}失败:{1}", ProxyType.ToString(), ex));
            }
        }

        public static void ReleaseConnection(string HostName)
        {
            try
            {
                if (CurrentHostConnections.ContainsKey(HostName))
                {
                    CurrentHostConnections[HostName].StopConnect();
                }
            }
            catch (Exception ex)
            {
                LOG.Error(string.Format("断开连接{0}失败:{1}", HostName, ex));
            }
        }

        public static void ReleaseConnection()
        {
            try
            {
                if(CurrentHostConnections!=null)
                {
                    foreach (var host in CurrentHostConnections.Values)
                    {
                        host.StopConnect();
                    }
                }
    
            }
            catch (Exception ex)
            {
                LOG.Error($"Release connection error.Ex:[{ex.Message}]");
            }
        }

        public static IHostConnection GetConnection(string ConnectionName)
        {
            if (CurrentHostConnections.ContainsKey(ConnectionName))
            {
                return CurrentHostConnections[ConnectionName];
            }
            else
            {
                LOG.Error(string.Format("未找到连接：{0}", ConnectionName));
                return null;
            }
        }

        public static void SelectCurrentGroup(string GroupName)
        {
            if (HostConnectionGroups.ContainsKey(GroupName))
            {
                if (GroupName != _currentHostConnetionsName)
                { //更换连接组

                    ReleaseConnection(); //释放所有连接

                    CurrentHostConnections = HostConnectionGroups[GroupName];
                    _currentHostConnetionsName = GroupName;

                }
            }
            
        }

        public static string CurrentGroup() => _currentHostConnetionsName;

        public static List<string> GetAllGroupNames()
        {
            List<string> groupNames = new List<string>();
            foreach (string item in HostConnectionGroups.Keys)
            {
                groupNames.Add(item);
            }
            return groupNames;
        }

        #endregion
    }

    public enum HostConnectionType
    {
        Machine = 0, //机器连接
        Process = 1, //过程连接
        Resource = 2, // 资源连接 
        Partner = 3,// 伙伴连接 
        Admin = 4,// 伙伴连接 
        Unknow = 5
    }
}
