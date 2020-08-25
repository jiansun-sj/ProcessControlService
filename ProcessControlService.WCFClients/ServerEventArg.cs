using System;

namespace ProcessControlService.WCFClients
{
    /// <summary>
    /// 事件类型枚举
    /// </summary>
    public enum ServerEventType
    {
        //默认值是0
        None,
        //丢失心跳
        MissHeartBeat,
        //心跳事件
        HeartBeat,
        // 连上
        Connected,
        // 断开
        Disconnected,
        // 连接失败
        Fault,
    }
    /// <summary>
    /// 事件参数
    /// </summary>
    public class ServerEventArg : EventArgs
    {
        public ServerEventArg(string ClientID, ServerEventType eventType)
        {
            ServerID = ClientID;
            ServerEventType = eventType;
        }
        public string ServerID { get; }
        public ServerEventType ServerEventType { get; }

    }
}
