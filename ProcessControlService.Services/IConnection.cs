using System;
using System.Collections.Generic;
using System.Threading;
using log4net;

namespace ProcessControlService.Services
{
    /// <summary>
    /// 连接管理
    /// </summary>
    public interface IConnection
    {
        bool Connected
        {
            get;
        }

        TimeSpan Duration
        {
            get;
        }

        string ConnectionID
        {
            get;
        }
    }
}
