// ==================================================
// 文件名：Resource.cs
// 创建时间：2020/08/03 17:19
// ==================================================
// 最后修改于：2020/08/03 17:19
// 修改人：jians
// ==================================================

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using log4net;
using Newtonsoft.Json;
using ProcessControlService.Contracts;
using ProcessControlService.ResourceFactory.Attributes;
using ProcessControlService.ResourceFactory.ParameterType;
using Type = System.Type;

namespace ProcessControlService.ResourceFactory
{
    /// <summary>
    ///     Resource抽象类
    /// </summary>
    /// <remarks>
    ///     合并IResource和IResourceExportService接口
    ///     所有标注了CallResourceService特性的方法自动生成ResourceServiceModel模型
    /// </remarks>
    public abstract class Resource : IResource, IResourceExportService
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Resource));
        private readonly List<ResourceServiceModel> _resourceServiceModels = new List<ResourceServiceModel>();

        protected Resource(string resourceName)
        {
            ResourceName = resourceName;

            InitializeResourceServiceModels();
        }

        public object ResourceLocker { get; } = new object();
        public string ResourceName { get; }
        public string ResourceType => GetType().Name.Split('`')[0];

        public IResource GetResourceObject()
        {
            return this;
        }

        public IResourceExportService GetExportService()
        {
            return this;
        }

        public abstract bool LoadFromConfig(XmlNode node);

        public abstract void FreeResource();

        public List<ResourceServiceModel> GetExportServices()
        {
            return _resourceServiceModels;
        }

        public virtual string CallExportService(string serviceName, string strParameter = "")
        {
            try
            {
                var response = "";

                MethodInfo methodInfo;
                
                //调用无输入参数服务。
                if (string.IsNullOrEmpty(strParameter))
                {

                    methodInfo = GetType().GetMethod(serviceName,new Type[]{});

                    if (methodInfo == null)
                        throw new ArgumentNullException(nameof(methodInfo));

                    var methodInfoReturnType = methodInfo.ReturnType;

                    var invoke = methodInfo.Invoke(this, null);

                    response = methodInfoReturnType == typeof(void)
                        ? $"调用的服务返回类型为：[{typeof(void)}],方法名为：{methodInfo.Name}, 服务已调用。"
                        : JsonConvert.SerializeObject(invoke);
                    
                    return response;
                }

                //调用有参数资源服务
                var serviceParameterModels =
                    JsonConvert.DeserializeObject<List<ServiceParameterModel>>(strParameter);
                
                var parameters = new object[serviceParameterModels.Count];
                
                var types = new Type[serviceParameterModels.Count];

                for (var i = 0; i < serviceParameterModels.Count; i++)
                {
                    var serviceParameterModel = serviceParameterModels[i];
                    
                    //根据参数类型创建参数实例
                    parameters[i] =
                        Parameter.CreateValue(serviceParameterModel.Type, serviceParameterModel.Value);

                    types[i] = parameters[i].GetType();
                }

                methodInfo = GetType().GetMethod(serviceName,types);

                var o = methodInfo?.Invoke(this, parameters);

                response = methodInfo?.ReturnType == typeof(void)
                    ? $"调用的服务返回类型为：[{typeof(void)}],服务已调用。"
                    : JsonConvert.SerializeObject(o);

                return response;
            }
            catch (Exception e)
            {
                Log.Error($"调用资源服务异常，服务名：[{serviceName}],参数：[{strParameter}]，异常为：[{e}]");

                throw;
            }
        }

        /// <summary>
        ///     初始化资源服务名称和对应接收参数类型，sunjian 2020/8/3 长春
        /// </summary>
        /// <exception cref="Exception"></exception>
        private void InitializeResourceServiceModels()
        {
            try
            {
                var methodInfos = GetType().GetMethods();

                foreach (var methodInfo in methodInfos)
                {
                    var callResourceServiceAttribute =
                        (ResourceServiceAttribute) Attribute.GetCustomAttribute(methodInfo,
                            typeof(ResourceServiceAttribute));

                    if (callResourceServiceAttribute == null) continue;

                    var serviceParameterModels = new List<ServiceParameterModel>();

                    var parameterInfos = methodInfo.GetParameters();

                    var count = 0;

                    var initParameters = callResourceServiceAttribute.ServiceParameters;

                    var serviceParametersLength = initParameters?.Length;

                    if (serviceParametersLength > 0 && parameterInfos.Length != serviceParametersLength)
                        throw new Exception(
                            $"方法：[{methodInfo.DeclaringType}].[{methodInfo.Name}]接收[{parameterInfos.Length}]个参数，CallResourceService特性传入初始值:[{serviceParametersLength}]个参数");

                    foreach (var parameterInfo in parameterInfos)
                    {
                        var parameterInitValue = "";

                        if (serviceParametersLength > 0)
                        {
                            if (initParameters[count].GetType() != parameterInfo.ParameterType)
                                throw new Exception(
                                    $"CallResourceService特性中传入参数初始值：[{parameterInfo.Name}]:[{initParameters[count]}]类型不匹配,方法名：[{methodInfo.DeclaringType}].[{methodInfo.Name}]。");

                            parameterInitValue = initParameters[count].ToString();
                        }

                        var serviceParameterModel = new ServiceParameterModel
                        {
                            Name = parameterInfo.Name, Type = parameterInfo.ParameterType.Name,
                            Value = parameterInitValue
                        };

                        serviceParameterModels.Add(serviceParameterModel);

                        count++;
                    }

                    _resourceServiceModels.Add(new ResourceServiceModel
                    {
                        //设置资源参数别名
                        Name = methodInfo.Name,
                        Parameters = serviceParameterModels
                    });
                }
            }
            catch (Exception e)
            {
                Log.Error($"{ResourceName} 初始化ResourceServiceModel失败,异常为:{e}");
            }
        }
    }
}