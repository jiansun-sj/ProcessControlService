﻿// ==================================================
// 文件名：IResourceService.cs
// 创建时间：2020/07/15 14:03
// ==================================================
// 最后修改于：2020/08/05 14:03
// 修改人：jians
// ==================================================

using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ProcessControlService.Contracts.ProcessData;

namespace ProcessControlService.Contracts
{
    /// <summary>
    /// 带会话连接的资源服务接口
    /// Created by DongMin, 2017/05/11
    /// </summary>
    //[ServiceContract(SessionMode = SessionMode.Required)]
    [ServiceContract(Namespace = "http://abc.com/enterpriseservices")]
    public interface IResourceService
    {
        [OperationContract]
        bool IsMaster();

        /// <summary>
        /// 客户端连接到资源服务
        /// </summary>
        /// <returns></returns>
        [OperationContract(IsInitiating = true, IsTerminating = false)]
        void ConnectResourceHost(string clientId);

        /// <summary>
        /// 客户端断开到资源服务
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        void DisconnectResourceHost(string clientId);

        /// <summary>
        /// 客户端定期发送心跳信号
        /// </summary>
        [OperationContract]
        void HeartBeat(string clientId);

        /// <summary>
        /// 获得资源的调用接口
        /// </summary>
        /// <param name="resourceName">资源名</param>
        /// <returns></returns>
        [OperationContract]
        List<ResourceServiceModel> GetResourceService(string resourceName);

        /// <summary>
        /// 获得资源的调用接口
        /// </summary>
        /// <param name="resourceType">资源类型</param>
        /// <returns>资源名列表</returns>
        [OperationContract]
        List<string> GetResources(string resourceType);

        /// <summary>
        /// 获得资源的调用接口
        /// </summary>
        /// <returns>所有资源名列表</returns>
        [OperationContract]
        List<string> GetAllResources();

        /// <summary>
        /// 调用资源的调用接口，以JSON方式返回结果
        /// </summary>
        /// 
        [OperationContract]
        string CallResourceService(string resourceName,string serviceName,string strParameter);

        /// <summary>
        /// 异步调用CallResourceService
        /// </summary>
        /// <param name="resourceName"></param>
        /// <param name="serviceName"></param>
        /// <param name="strParameter"></param>
        /// <returns></returns>
        [OperationContract]
        Task<string> CallResourceServiceAsc(string resourceName, string serviceName, string strParameter);

        /// <summary>
        /// 调用CallResourceService直接写入参数值
        /// </summary>
        /// <param name="resourceName"></param>
        /// <param name="serviceName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [OperationContract]
        string CallResourceServiceParams(string resourceName, string serviceName, params object[] parameters);

        
        /// <summary>
        /// 异步调用CallResourceServiceParams
        /// </summary>
        /// <param name="resourceName"></param>
        /// <param name="serviceName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [OperationContract]
        Task<string> CallResourceServiceParamsAsc(string resourceName, string serviceName, params object[] parameters);
        
        /// <summary>
        /// 有返回值的sql查询
        /// </summary>
        /// <param name="cnnName">数据库连接名</param>
        /// <param name="ct">Command 类型</param>
        /// <param name="sql">sql 语句</param>
        /// <returns></returns>
        [OperationContract]
        DataSet GetDataSet(string cnnName, CommandType ct, string sql);

        /// <summary>
        /// 无返回值的sql查询
        /// </summary>
        /// <param name="cnnName">数据库连接名</param>
        /// <param name="ct">Command 类型</param>
        /// <param name="sql">sql 语句</param>
        /// <returns>执行受影响行数</returns>
        [OperationContract]
        int ExecuteNonQuery(string cnnName, CommandType ct, string sql);

        /// <summary>
        /// 单个值的sql查询
        /// </summary>
        /// <param name="conStr">数据库连接名</param>
        /// <param name="sql">sql 语句</param>
        /// <returns>单个值的字符串</returns>
        [OperationContract]
        string ExecuteOne(string conStr, string sql);

        /// <summary>
        /// 获取指定的某天程序内存使用和CPU占用率
        /// </summary>
        /// <param name="checkDate"></param>
        /// <returns></returns>
        [OperationContract]
        List<MemoryAndCpuData> GetMemoryAndCpuData(DateTime checkDate);

        /*/// <summary>
        /// 可以直接传入params
        /// </summary>
        /// <param name="resourceName"></param>
        /// <param name="serviceName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        [OperationContract]
        string CallResourceServiceParams(string resourceName, string serviceName, params object[] parameters);*/
    }


    [DataContract]
    public class MachineTagModel
    {
        public bool IsSelected { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Type { get; set; }

        [DataMember]
        public string Address { get; set; }

        [DataMember]
        public string Value { get; set; }

        [DataMember]
        public string Quality { get; set; }

        [DataMember]
        public string AccessType { get; set; }
    }

    [DataContract]
    public class ResourceServiceModel
    {
        [DataMember]
        public string Name;

        [DataMember]
        public List<ServiceParameterModel> Parameters = new List<ServiceParameterModel>();
    }

    [DataContract]
    public class ServiceParameterModel
    {
        [JsonProperty("name")]
        [DataMember]
        public string Name;

        [JsonProperty("type")]
        [DataMember]
        public string Type;

        [JsonProperty("value")]
        [DataMember]
        public string Value;
    }

}
