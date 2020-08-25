using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Runtime.Serialization;

namespace ProcessControlService.Contracts
{
    /// <summary>
    /// 系统管理服务接口
    /// Created by DongMin, 2019/11/9
    /// </summary>
    [ServiceContract(SessionMode = SessionMode.Allowed)]
    public interface IAdmin
    {

      
        [OperationContract(IsInitiating = true, IsTerminating = false)]
        void ConnectResourceHost(string ClientID);

        /// <summary>
        /// 客户端断开到资源服务
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        void DisconnectResourceHost(string ClientID);

        /// <summary>
        /// 客户端定期发送心跳信号
        /// </summary>
        [OperationContract]
        void HeartBeat(string ClientID);

        /// <summary>
        /// 获得运行模式
        /// </summary>
        [OperationContract]
        Int16 GetRedundancyMode();

        /// <summary>
        /// 改变运行模式
        /// </summary>
        [OperationContract]
        void ToggleRedundancyMode();

        

    }

   

    
}
