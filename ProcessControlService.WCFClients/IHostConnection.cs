using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace ProcessControlService.WCFClients
{

    public delegate void OnConnected(ServerEventArg e);
    public delegate void OnDisonnected(ServerEventArg e);
    public delegate void OnConnectFault(ServerEventArg e);
    public interface IHostConnection : IDisposable
    {
       void StartConnect();

        void StopConnect();

        string ConnectionAddress { get; }

        bool Connected { get; }

        OnConnected OnConnectedHander { get; set; }
        OnDisonnected OnDisconnectedHander { get; set; }
        OnConnectFault OnConnectFaultHander { get; set; }


        IProxyConnection GetProxy();

        HostConnectionType HostType { get; }

        //bool LoadFromConfig(XmlNode node);

        /// <summary>
        /// 订阅连接事件
        /// </summary>
        /// <param name="handler">事件处理方法</param>
        void AddConnectedHandler(OnConnected handler);

        /// <summary>
        /// 订阅断开事件
        /// </summary>
        /// <param name="handler">事件处理方法</param>
        void AddDisconnectedHandler(OnDisonnected handler);


        /// <summary>
        /// 订阅连接故障事件
        /// </summary>
        /// <param name="handler">事件处理方法</param>
        void AddConnectFaultHandler(OnConnectFault handler);
       

    }
}
