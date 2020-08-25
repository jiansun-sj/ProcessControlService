using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Runtime.Serialization;

namespace ProcessControlService.Contracts
{
    /// <summary>
    /// 冗余控制服务接口
    /// Created by DongMin, 2017/03/10
    /// </summary>
    [ServiceContract(SessionMode = SessionMode.Allowed)]
    public interface IPartner
    {

        [OperationContract]
        bool IsMaster();
        /// <summary>
        /// 客户端连接到资源服务
        /// </summary>
        /// <returns></returns>
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
        /// 获得对方的运行模式
        /// </summary>
        [OperationContract]
        Int16 GetPartnerMode();

        ///// <summary>
        ///// 改变对方的运行模式
        ///// </summary>
        //[OperationContract]
        //void TogglePartnerMode();

        /// <summary>
        /// 获得对方的运行时长
        /// </summary>
        [OperationContract]
        long GetPartnerRunTime();

        /// <summary>
        ///交换数据
        /// </summary>
        [OperationContract]
        void ExchangeData(string ResourceName, string ExchangeData);

    }

   

    
}
