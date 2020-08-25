using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace ProcessControlService.Services
{
    /// <summary>
    /// 事件类型枚举
    /// </summary>
    public enum ClientEventType
    {
        //默认值是0
        None,
        //丢失心跳
        MissHeartBeat,
        //心跳事件
        HeartBeat,
        //下线
        Disconnect,
    }
    /// <summary>
    /// 事件参数
    /// </summary>
    public class ClientEventArg:EventArgs
    {
        //客户端ID
        private string _clientID;
        //事件类型
        private ClientEventType _eventType;
        //病人状态，可为null
        //private PatientStatus _patientstatus;
        public ClientEventArg(string ClientID, ClientEventType eventType)
        {
            _clientID = ClientID;
            _eventType = eventType;
        }
        public string ClientID
        {
            get
            {
                return _clientID;
            }
        }
        public ClientEventType ClientEventType
        {
            get
            {
                return _eventType;
            }
        }
      
    }
}
