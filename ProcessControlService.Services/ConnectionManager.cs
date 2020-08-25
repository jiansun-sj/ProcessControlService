using System.Collections.Generic;

namespace ProcessControlService.Services
{
    /// <summary>
    /// 连接管理
    /// </summary>
    public class ConnectionManager
    {
        private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(ConnectionManager));

        // 连接池
        private static Dictionary<string, IConnection> _connectionCollections = new Dictionary<string, IConnection>();


        /// <summary>
        /// 构造方法
        /// </summary>
        public ConnectionManager()
        {

        }


        // 增加连接
        public static void AddConnection(string ID, IConnection Connect)
        {
            _connectionCollections.Add(ID, Connect);
        }

        // 移出连接
        public static void RemoveConnection(string ID)
        {
            _connectionCollections.Remove(ID);
        }

        // 获得连接
        public static IConnection GetConnection(string ID)
        {
            return _connectionCollections[ID];
        }

        //获取所有连接
        public Dictionary<string, IConnection> ConnectionCollections => _connectionCollections;

        //获取连接数量
        public static int GetConnectionCount()
        {
            return _connectionCollections.Count;
        }

        /// <summary>
        /// 关闭WebSocket链接
        /// </summary>
        public static void CloseWebSocketConnection()
        {
            foreach (var connectionCollection in _connectionCollections)
            {
                if (!(connectionCollection.Value is WsServiceProxy wsServiceProxy))
                {
                    continue;
                }

                wsServiceProxy.Context.WebSocket.Close();
                LOG.Info($"冗余模式主从切换，关闭WebSocket连接,链接ID：{connectionCollection.Key}。");
                RemoveConnection(connectionCollection.Key);
            }
        }






    }
}
