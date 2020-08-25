using System;
using System.Collections.Generic;
using System.ServiceModel;
using ProcessControlService.Contracts.ProcessData;

namespace ProcessControlService.Contracts
{
    /// <summary>
    /// 带会话连接的过程服务接口
    /// Created by DongMin, 2017/05/11
    /// </summary>
    [ServiceContract(SessionMode = SessionMode.Allowed)]
    public interface IProcess
    {

        [OperationContract]
        bool IsMaster();
        /// <summary>
        /// 客户端连接到过程服务
        /// </summary>
        /// <returns></returns>
        [OperationContract(IsInitiating = true, IsTerminating = false)]
        void ConnectProcessHost(string clientId);

        /// <summary>
        /// 客户端断开到过程服务
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        void DisconnectProcessHost(string clientId);

        /// <summary>
        /// 客户端定期发送心跳信号
        /// </summary>
        [OperationContract]
        void HeartBeat(string clientId);

        /// <summary>
        /// 获取Process当前Step
        /// </summary>
        /// <param name="name">Process Name</param>
        /// <returns>当前Step Id</returns>
        [OperationContract]
        short GetProcessStep(string name);


        /// <summary>
        /// 手动启动Process
        /// </summary>
        /// <param name="name">Process Name</param>
        /// <param name="containers"></param>
        /// <param name="inParameters">Process参数：Key(参数名) Value(参数值)</param>
        [OperationContract]
        void StartProcess(string name, Dictionary<string,string> containers,Dictionary<string, string> inParameters);

        /// <summary>
        /// 停止Process
        /// </summary>
        /// <param name="name">Process Name</param>
        [OperationContract]
        void StopProcessInstance(string name);

        /// <summary>
        /// 读取所有非Process的资源名
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        List<string> ReadStaticResourceNames();

        /// <summary>
        /// 设置Process 手自动模式
        /// </summary>
        /// <param name="name">Process Name</param>
        /// <param name="autoRun">True:Auto; False:Maual</param>
        [OperationContract]
        void SetProcessAuto(string name, bool autoRun);


        /// <summary>
        /// 获取Process 手自动状态
        /// </summary>
        /// <param name="name">Process Name</param>
        /// <returns>True:自动；Fasle:手动</returns>
        [OperationContract]
        bool GetProcessAuto(string name);


        /// <summary>
        /// 获取所有可用Process
        /// </summary>
        /// <returns>所有可用Process Name 列表</returns>
        [OperationContract]
        List<string> ListProcessNames(); //获取可用过程列表


        /// <summary>
        /// 获取指定Process的基本信息
        /// </summary>
        /// <returns>
        /// </returns>
        [OperationContract]
        List<ProcessInfoModel> GetProcessInfos();

        /// <summary>
        /// 获取指定Process正在运行的所有ProcessInstance观测参数
        /// </summary>
        /// <param name="processName"></param>
        /// <returns></returns>
        [OperationContract]
        List<ProcessInstanceInfoModel> GetProcessInstanceInfoModels(string processName);

        [OperationContract]
        List<ProcessInstanceRecord> ReadProcessInstanceRecords(string processName, int pageSize, DateTime startDate,DateTime endDate,int searchPage);

        /// <summary>
        /// 获取Process 所有Step信息
        /// </summary>
        /// <param name="name">Process Name</param>
        /// <returns>
        /// Key:Step Id
        /// Value Array[0] Step Name
        /// Value Array[1] 下一Step Name
        /// Value Array[2] Step Check Step Id
        /// Value Array[..] Step Check Step Id
        /// </returns>
        [OperationContract]
        Dictionary<short, List<string>> GetProcessAllStepsIdName(string name);

        /// <summary>
        /// 获取过程输入参数列表
        /// </summary>
        /// <param name="processName">Process Name</param>
        /// <returns>Key:参数名;Value:默认值</returns>
        [OperationContract]
        Dictionary<string, string> ListProcessInParameters(string processName);
    }
}
