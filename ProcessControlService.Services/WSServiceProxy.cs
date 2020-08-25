// ==================================================
// 文件名：WSServiceProxy.cs
// 创建时间：2020/01/02 10:09
// ==================================================
// 最后修改于：2020/06/03 10:09
// 修改人：jians
// ==================================================

using log4net;
using Newtonsoft.Json;
using System;
using WebSocketSharp;
using WebSocketSharp.Server;
using ErrorEventArgs = WebSocketSharp.ErrorEventArgs;

namespace ProcessControlService.Services
{
    public class WsServiceProxy : WebSocketBehavior, IConnection
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(WsServiceProxy));
        private readonly ResourceService _resourceService = new ResourceService();

        protected override void OnOpen()
        {
            LOG.InfoFormat("客户端{0}上线", Context.UserEndPoint.Address);
            base.OnOpen();
            ConnectionManager.AddConnection(ConnectionID, this);
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            var data = e.Data;
            //LOG.Info("接收数据：" + e.Data);
            var result = _resourceService.CallService(data);
            try
            {
                if (result != null && State == WebSocketState.Open)
                {
                    var res = JsonConvert.SerializeObject(new { req = e.Data, res = result });
                    //LOG.Info("数据返回：" + res);
                    Send(res);
                }
            }
            catch (Exception ex)
            {
                LOG.Error("发送WS数据出错：" + ex.Message);
            }
        }

        protected override void OnClose(CloseEventArgs e)
        {
            LOG.Info($"客户端下线:{ID}");
            base.OnClose(e);
            ConnectionManager.RemoveConnection(ConnectionID);
        }

        protected override void OnError(ErrorEventArgs e)
        {
            base.OnError(e);
        }

        #region IConnection

        public bool Connected => true;

        public TimeSpan Duration { get; }

        public string ConnectionID => $"WS_{ID}";

        #endregion
    }
}