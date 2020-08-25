// ==================================================
// 文件名：IMachine.cs
// 创建时间：2020/01/04 10:22
// 上海芸浦信息技术有限公司
// copyright@yumpoo
// ==================================================
// 最后修改于：2020/03/13 10:22
// 修改人：jians
// ==================================================

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Threading.Tasks;

namespace ProcessControlService.Contracts
{
    /// <summary>
    ///     带会话连接的机器服务接口
    ///     Created by DongMin, 2017/05/11
    /// </summary>
    [ServiceContract(SessionMode = SessionMode.Allowed)]
    public interface IMachine
    {
        /// <summary>
        ///     冗余模式下获取服务端主从状态
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        bool IsMaster();

        /// <summary>
        ///     客户端连接到机器服务
        /// </summary>
        /// <returns></returns>
        [OperationContract(IsInitiating = true, IsTerminating = false)]
        void ConnectMachineHost(string clientId);

        /// <summary>
        ///     客户端断开到机器服务
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        void DisconnectMachineHost(string clientId);

        /// <summary>
        ///     客户端定期发送心跳信号
        /// </summary>
        [OperationContract]
        void HeartBeat(string clientId);

        /// <summary>
        ///     获得机器的状态列表
        /// </summary>
        /// <param name="machineName">机器名</param>
        /// <returns>
        ///     <see cref="MachineStatusModel" />
        /// </returns>
        [OperationContract]
        MachineStatusModel GetMachineStatus(string machineName);

        /// <summary>
        ///     读取机器标签
        /// </summary>
        /// <param name="machineName">机器名</param>
        /// <param name="tagName">标签名</param>
        /// <returns></returns>
        [OperationContract]
        string GetMachineTag(string machineName, string tagName);

        /// <summary>
        ///     获取标签值
        /// </summary>
        [OperationContract]
        List<string[]> GetTagsValue(List<string[]> machineTag);

        /// <summary>
        ///     写入标签值
        /// </summary>
        [OperationContract]
        List<string[]> SetTagsValue(List<string[]> machineTag);

        /// <summary>
        ///     获取机器数据源连接状态
        /// </summary>
        [OperationContract]
        List<DataSourceConnectionModel> GetAllDataSourceConn();

        /// <summary>
        ///     获取设备数据源列表
        /// </summary>
        /// <param name="machineName">设备名</param>
        /// <returns>数据源列表</returns>
        [OperationContract]
        List<string> GetMachineDataSourceName(string machineName);

        /// <summary>
        ///     设置机器标签值
        /// </summary>
        /// <param name="machineName">机器名</param>
        /// <param name="tagName">标签名</param>
        /// <param name="tagValue">标签值</param>
        [OperationContract]
        void SetMachineTag(string machineName, string tagName, string tagValue);

        /// <summary>
        ///     手动启动Action
        /// </summary>
        /// <param name="machineName">Machine Name</param>
        /// <param name="actionName">Action Name</param>
        /// <param name="inParameters">输入参数列表Key:参数名，Value:参数值</param>
        /// <returns></returns>
        [OperationContract]
        Dictionary<string, string> RunMachineAction(string machineName, string actionName,
            Dictionary<string, string> inParameters);

        /// <summary>
        ///     获取机器报警
        /// </summary>
        /// <param name="machineName"></param>
        /// <returns>
        ///     <see cref="MachineAlarmModel" />
        /// </returns>
        [OperationContract]
        List<MachineAlarmModel> GetMachineAlarms(string machineName);

        /// <summary>
        ///     获取可用机器列表
        /// </summary>
        /// <returns>Machine Name 列表</returns>
        [OperationContract]
        List<string> ListMachineNames(); //获取可用机器列表

        /// <summary>
        ///     获取机器连接数据源状态
        /// </summary>
        /// <param name="machineName">设备名</param>
        /// <returns></returns>
        [OperationContract]
        List<DataSourceConnectionModel> ListMachineConnections(string machineName);

        /// <summary>
        ///     获取可用的标签列表
        /// </summary>
        /// <param name="machineName">Machine Name</param>
        /// <returns>标签名 列表</returns>
        [OperationContract]
        List<string> ListMachineTags(string machineName); //获取可用的Tag列表

        // Add by David 20170603
        /// <summary>
        ///     获取Action输入参数列表
        /// </summary>
        /// <param name="machineName">Machine Name</param>
        /// <param name="actionName">Action Name</param>
        /// <returns>
        ///     Dict[0]:参数1名，
        ///     Dict[1]:参数1类型，
        ///     Dict[1]:参数1当前值，
        /// </returns>
        [OperationContract]
        List<string> ListActionInParameters(string machineName, string actionName);

        // Add by David 20170603
        /// <summary>
        ///     获取Action输出参数列表
        /// </summary>
        /// <param name="machineName">Machine Name</param>
        /// <param name="actionName">Action Name</param>
        /// <returns>
        ///     Dict[0]:参数1名，
        ///     Dict[1]:参数1类型，
        ///     Dict[1]:参数1当前值，
        /// </returns>
        [OperationContract]
        List<string> ListActionOutParameters(string machineName, string actionName);
    }

    /// <summary>
    ///     设备状态列表模型
    /// </summary>
    [DataContract]
    public class MachineStatusModel
    {
        /// <summary>
        ///     设备名
        /// </summary>
        [DataMember] public readonly string MachineName;

        /// <summary>
        ///     状态列表
        /// </summary>
        [DataMember] public Dictionary<string, string> StatusList = new Dictionary<string, string>();

        public MachineStatusModel(string name)
        {
            MachineName = name;
        }


        /// <summary>
        ///     获取状态
        /// </summary>
        /// <param name="statusName"></param>
        /// <returns></returns>
        public string GetStatus(string statusName)
        {
            return StatusList[statusName];
        }

        /// <summary>
        ///     增加状态值
        /// </summary>
        /// <param name="statusName"></param>
        /// <param name="strValue"></param>
        public void SetStatus(string statusName, string strValue)
        {
            StatusList[statusName] = strValue;
        }
    }

    /// <summary>
    ///     数据源模型
    /// </summary>
    [DataContract]
    public class DataSourceConnectionModel
    {
        /// <summary>
        ///     数据源名称
        /// </summary>
        [DataMember] public string Name;

        /// <summary>
        ///     数据源状态
        /// </summary>
        [DataMember] public string Status;

        /// <summary>
        ///     数据源类型
        /// </summary>
        [DataMember] public string Type;

        /// <summary>
        ///     数据源所属Machine
        /// </summary>
        [DataMember]
        public string MachineName { get; set; }
    }

    //[DataContract]
    //public class MachineCommandModel
    //{
    //    public MachineCommandModel(string Name)
    //    {
    //        MachineName = Name;
    //    }

    //    [DataMember]
    //    public readonly string MachineName;

    //    [DataMember]
    //    public Dictionary<string, string> CommandList = new Dictionary<string, string>();

    //    public void AddCommand(string CommandName, string strCommandValue)
    //    {
    //        CommandList.Add(CommandName, strCommandValue);
    //    }

    //    public void GetCommand(string CommandName)
    //    {

    //    }
    //}


    /// <summary>
    ///     设备报警模型
    /// </summary>
    [DataContract]
    public class MachineAlarmModel
    {
        public enum StatusType
        {
            UnTriggered = 0,
            Triggered = 1,
            Confirmed = 2,
            Reset = 3
        }

        /// <summary>
        ///     报警唯一ID
        /// </summary>
        [DataMember] public string UniqueId;

        /// <summary>
        ///     报警ID
        /// </summary>
        [DataMember] public string AlarmId;

        /// <summary>
        ///     报警组
        /// </summary>
        [DataMember] public string Group;

        /// <summary>
        ///     报警内容
        /// </summary>
        [DataMember] public string Message;

        /// <summary>
        ///     报警状态
        /// </summary>
        [DataMember] public StatusType Status;

        /// <summary>
        ///     触发时间
        /// </summary>
        [DataMember] public DateTime TrigTime;
    }
}