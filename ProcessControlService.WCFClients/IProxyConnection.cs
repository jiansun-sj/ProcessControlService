using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProcessControlService.WCFClients
{
    public interface IProxyConnection : IDisposable
    {
        bool Connected { get;}

        bool Connect();
        void Disconnect();
        bool SendHeartBeat();
        bool IsMaster();


    }
}
