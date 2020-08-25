// ==================================================
// 文件名：ResourceManager.cs
// 创建时间：2020/07/15 10:49
// ==================================================
// 最后修改于：2020/08/18 10:49
// 修改人：jians
// ==================================================

using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.IO;
using System.Linq;
using System.Xml;
using log4net;
using ProcessControlService.ResourceFactory.DBUtil;
using ProcessControlService.ResourceFactory.ParameterType;

namespace ProcessControlService.ResourceFactory
{
    public class ResourceManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ResourceManager));

        // 公共资源模板池
        private static readonly Dictionary<string, IResourceTemplate> ResourceTemplateCollections =
            new Dictionary<string, IResourceTemplate>();

        // 公共资源池
        private static readonly Dictionary<string, IResource> ResourceCollections = new Dictionary<string, IResource>();

        // 分类资源池
        private static readonly Dictionary<string, ResourceColletion> TypedResourceCollections =
            new Dictionary<string, ResourceColletion>();

        private static bool _resetNewRedundancySyncInterval;

        // 获得公共资源列表
        public static List<IResource> GetAllResources()
        {
            return ResourceCollections.Values.ToList();
        }

        // 获得分类资源列表
        public static IEnumerable<IResource> GetTypedResources(string resourceType)
        {
            return TypedResourceCollections[resourceType].GetAllResources();
        }

        // 从配置文件装载资源

        // 启动所有资源
        public static void StartAllResource()
        {
            //从主程序及扩展程序集里加载资源
            LoadAllPackages();

            //装载资源配置
            var appPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            var xmlPath = appPath + "Config";

            var files = Directory.GetFiles(xmlPath, "*.xml");

            foreach (var file in files)
            {
                Log.Info($"载入配置文件:{file}");
                LoadConfig(file);
            }

            //启动冗余检测
            if (EnableRedundancy)
            {
                if (_resetNewRedundancySyncInterval) Redundancy.ChangeSyncInterval(_redundancySyncInterval);
                Redundancy.StartWork(); //启动冗余检测
            }
            else
            {
                Log.Info("----------------------------------");
                Log.Info("| 关闭冗余模式,系统以Master运行! |");
                Log.Info("----------------------------------");
                Redundancy.ChangeMode(RedundancyMode.Master); //   Robin20180426

                foreach (var item in GetAllResources())
                    if (item is IWork worker)
                        worker.StartWork();
            }
        }

        public static void AddResource(IResource resource)
        {
            try
            {
                TypedResourceCollections[resource.ResourceType].Add(resource); // 增加到分类资源池

                ResourceCollections.Add(resource.ResourceName, resource); // 增加到公共资源池
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        private static void RemoveResource(IResource resource)
        {
            try
            {
                TypedResourceCollections[resource.ResourceType].Remove(resource); // 增加到分类资源池

                ResourceCollections.Remove(resource.ResourceName);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        public static bool HasResource(string resourceName)
        {
            return ResourceCollections.ContainsKey(resourceName);
        }

        public static IResource GetResource(string resourceName)
        {
            try
            {
                if (!ResourceCollections.ContainsKey(resourceName))
                {
                    Log.Info($"无法获取资源，FactoryWindow没有加载资源:{resourceName}");
                    return null;
                }

                return ResourceCollections[resourceName];
            }
            catch (Exception)
            {
                Log.Error($"获取资源:{resourceName}出错");
                return null;
            }
        }

        public static T GetResource<T>(string resourceName)
        {
            try
            {
                return (T) ResourceCollections[resourceName];
            }
            catch (Exception)
            {
                throw new Exception("Can`t find target resource");
            }
        }

        /// <summary>
        ///     get resource template
        /// </summary>
        /// <param name="resourceTemplateName"></param>
        /// <returns></returns>
        /// <remarks>
        ///     added by sunjian 2019-12-24
        /// </remarks>
        public static IResourceTemplate GetResourceTemplate(string resourceTemplateName)
        {
            try
            {
                return ResourceTemplateCollections[resourceTemplateName];
            }
            catch (Exception)
            {
                Log.Error($"获取资源模板:{resourceTemplateName}出错");
                return null;
            }
        }

        public static List<string> GetResourceNames(string resourceType)
        {
            return (from resource in ResourceCollections.Values
                where string.Equals(resource.ResourceType, resourceType, StringComparison.CurrentCultureIgnoreCase)
                select resource.ResourceName).ToList();
        }

        public static List<string> GetAllResourceNames()
        {
            return ResourceCollections.Values.Select(resource => resource.ResourceName).ToList();
        }

        public static List<IResource> GetResources(string resourceType)
        {
            return ResourceCollections.Values.Where(a => a.ResourceType == resourceType).ToList();
        }

        // 释放所有资源
        public static void FreeAllResources()
        {
            foreach (var res in ResourceCollections.Values) res.FreeResource();
        }

        private static IResource CreateResource(string strResourceType, string resourceName, string genericType)
        {
            // 根据类型加载资源 -- Dongmin 20171226
            try
            {
                var resourcePath = ResourceClassRegister.GetResourceFullName(strResourceType);

                var packageName = ResourceClassRegister.GetPackageName(strResourceType);

                var findingPath = resourcePath + "," + packageName;
                
                var resourceType = Type.GetType(findingPath, true);

                object obj;
                
                if (string.IsNullOrEmpty(genericType))
                {
                    obj = Activator.CreateInstance(resourceType, resourceName);
                }
                //sunjian 2020-08 增加泛型资源的创建，GenericType必须是实现了ICustomType接口的类
                else
                {
                    var genericObj = Parameter.CreateValue(genericType);

                    var makeGenericType = resourceType.MakeGenericType(genericObj.GetType());

                    obj = Activator.CreateInstance(makeGenericType,resourceName);
                }
                
                return (IResource) obj;
            }
            catch (Exception ex)
            {
                Log.Error($"创建资源类型:{strResourceType},名称:{resourceName}失败:{ex}");
                return null;
            }
        }

        private static void LoadAllPackages()
        {
            //////////////////////////////////////////////
            var localPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

            #region // 加载主资源包

            var strResourceLibrary = "ProcessControlService.ResourceLibrary.dll";
            Log.Info($"加载{strResourceLibrary}...");
            var resourceLibrary = new PackageLoader(localPath + strResourceLibrary); //Robin

            if (resourceLibrary.LoadSucceeded)
            {
                //获得包的描述文件
                var profile = resourceLibrary.Profile;

                ///////////////////////////////////////////////////////////////////
                //加载资源

                // 获取包里的所有资源
                var resourceTemplateTypes = profile.GetAllResourceTemplateTypes();

                // 初始化资源池
                foreach (var resourceTemplate in resourceTemplateTypes)
                    ResourceClassRegister.AddResourceTemplate(resourceTemplate.Name, resourceTemplate.FullName,
                        profile.GetPackageName());

                ///////////////////////////////////////////////////////////////////
                //加载资源

                // 获取包里的所有资源
                var resourceTypes = profile.GetAllResourceTypes();

                // 初始化资源池
                foreach (var resource in resourceTypes)
                {
                    var resourceClassName = resource.Name.Split('`')[0];

                    try
                    {
                        var resourceCollection = new ResourceColletion(resourceClassName);

                        TypedResourceCollections.Add(resourceClassName, resourceCollection);

                        ResourceClassRegister.AddResource(resourceClassName, resource.FullName, profile.GetPackageName());
                    }
                    catch (Exception e)
                    {
                        Log.Error($"资源：[{resourceClassName}]加载入资源池失败，异常为：{e.Message}.");
                        throw new DuplicateKeyException(resource, $"初始化资源池失败，异常为：{e.Message}.");
                    }
                }

                AddCustomizedType(profile);
            }

            //////////////////////////////////////////////////

            #endregion

            //////////////////////////////////////////////////

            #region // 加载扩展资源包

            var fullPath = localPath + "ExtendResources.xml";
            var doc = new XmlDocument();
            doc.Load(fullPath);
            var node = (XmlElement) doc.SelectSingleNode("Configs/ResourceModule");
            if (node != null)
                foreach (XmlNode level1Node in node)
                {
                    if (level1Node.NodeType == XmlNodeType.Comment) continue;

                    var item = (XmlElement) level1Node;

                    if (Convert.ToBoolean(item.Attributes["Enable"].InnerText))
                    {
                        var fileName = item.Attributes["Name"].InnerText;
                        Log.Info($"加载{fileName}...");
                        var extend1 = new PackageLoader(localPath + fileName); //Robin

                        if (extend1.LoadSucceeded)
                        {
                            var profile = extend1.Profile;

                            ///////////////////////////////////////////////////////////////////
                            //加载资源
                            var resourceTypes = profile.GetAllResourceTypes();

                            //初始化资源池
                            foreach (var resource in resourceTypes)
                            {
                                var resourceCollection = new ResourceColletion(resource.Name);
                                TypedResourceCollections.Add(resource.Name, resourceCollection);

                                //加到注册表
                                ResourceClassRegister.AddResource(resource.Name, resource.FullName,
                                    profile.GetPackageName());
                            }

                            ///////////////////////////////////////////////////////////////////
                            //加载自定义类型
                            AddCustomizedType(profile);
                        }
                        //////////////////////////////////////////////////

                        // 把各个资源包加到主资源包
                        resourceLibrary.Profile.AddOtherPackage(extend1.Profile);
                    }
                }

            #endregion

            // 主资源包执行加载附加资源
            resourceLibrary.Profile.LoadAdditionalResource();

            // 显示所有加载的程序集 -- Dongmin 20191015
            Log.Info("已加载程序集：");
            var loadAssemblies = PackageLoader.GetAllProcessControlServiceAssembly();
            short assIndex = 0;
            foreach (var assembly in loadAssemblies) Log.Info($"程序集{assIndex++}：{assembly}");
        }

        private static void AddCustomizedType(BaseProfile profile)
        {
            var customizedTypes = profile.GetAllCustomizedTypes();

            //初始化类型池
            foreach (var customizedType in customizedTypes)
            {
                var customizedTypeCollection = new CustomTypeCollection(customizedType.Name);
                CustomTypeManager.AddCustomizedTypeCollection(customizedType.Name,
                    customizedTypeCollection);

                CustomTypeManager.AddCustomizedType(customizedType.Name, customizedType);


                //加到注册表
                ResourceClassRegister.AddCustomizedType(customizedType.Name, customizedType.FullName,
                    profile.GetPackageName());
            }
        }

        // 装载入口
        private static void LoadConfig(string configFile)
        {
            try
            {
                // loading from xml file
                var doc = new XmlDocument();

                // Ignore Comments
                doc.Load(XmlReader.Create(configFile, new XmlReaderSettings {IgnoreComments = true}));

                // load  configuration
                var configs = doc.SelectSingleNode("root/Configs");
                if (null != configs)
                    foreach (XmlNode item in configs)
                    {
                        if (item.NodeType == XmlNodeType.Comment) continue;
                        if (item.Name == "Redundancy") // Get redundancy config -- Dongmin 20180319
                        {
                            var redundancy = (XmlElement) item;
                            if (redundancy.HasAttribute("Enable"))
                            {
                                var strEnableRedundancy = redundancy.GetAttribute("Enable");
                                EnableRedundancy = strEnableRedundancy.ToLower() == "true";
                            }

                            //增加xml冗余同步时间修改功能 --- jiansun 2019-11-10
                            if (redundancy.HasAttribute("SyncInterval"))
                            {
                                var strSyncInterval = redundancy.GetAttribute("SyncInterval");
                                _resetNewRedundancySyncInterval = true;
                                _redundancySyncInterval = strSyncInterval;
                            }
                        }
                        else if (item.Name == "DBConnection")
                        {
                            var dbConfig = (XmlElement) item;
                            if (dbConfig.HasAttribute("Name") && dbConfig.HasAttribute("Type") &&
                                dbConfig.HasAttribute("ConnectionString"))
                            {
                                //DataBaseHelperNew.LoadFromConfig(item);

                                var name = dbConfig.GetAttribute("Name");
                                var type = DataBaseHelper.ConvertDatabaseType(dbConfig.GetAttribute("Type"));
                                if (DataBaseHelper.NameType.Keys.Contains(name)) continue;
                                var connectionStr = dbConfig.GetAttribute("ConnectionString");
                                DataBaseHelper.NameType.Add(name, type);
                                DataBaseHelper.NameConnStr.Add(name, connectionStr);
                            }

                            else
                            {
                                throw new Exception("数据库配置错误,缺少Name,Type或ConnectionString");
                            }
                        }

                        else
                        {
                            throw new Exception("该节点不识别:" + item.Name);
                        }
                    }

                // Load Custom values
                var customValues = doc.SelectSingleNode("root/CustomizedTypes");
                if (null != customValues)
                    foreach (XmlNode item in customValues)
                    {
                        if (item.NodeType == XmlNodeType.Comment) continue;

                        // 装载自定义类型 -- Dongmin 20180602
                        if (!CustomTypeManager.LoadCustomTypesInConfig(item)) continue;
                    }

                // Load Resources
                var resources = doc.SelectSingleNode("root/Resources");
                if (resources != null)
                    foreach (XmlNode item in resources)
                    {
                        if (item.NodeType == XmlNodeType.Comment) continue;

                        //sunjian 2019-12-24 增加ResourceTemplate模板的配置。
                        //Resource Template必须在其他资源前面定义
                        //todo: Xml配置合法性检测必须要检查Resource Template的定义位置。
                        if (item.Name.ToLower() == "resource" + "templates")
                        {
                            foreach (var xmlElement in item.Cast<XmlNode>()
                                .Where(xmlElement => xmlElement.NodeType != XmlNodeType.Comment))
                                LoadResourceTemplateInConfig((XmlElement) xmlElement);

                            continue;
                        }

                        if (!LoadResourceInConfig(item, configFile, doc)) continue;
                    }
            }
            catch (Exception ex)
            {
                Log.Error($"装载配置文件{configFile}失败{ex.Message}");
            }
        }

        private static void LoadResourceTemplateInConfig(XmlElement resourceTemplateElement)
        {
            //todo:target resource type需要无视大小写
            var targetResourceType = resourceTemplateElement.GetAttribute("TargetResourceType");
            var resourceTemplateName = resourceTemplateElement.GetAttribute("Name");
            Log.Info($"装载资源模板{resourceTemplateName}，目标资源名称：[{targetResourceType}]...");

            var createResourceType = targetResourceType;
            if (string.Equals(targetResourceType, "AGVControlCenter", StringComparison.CurrentCultureIgnoreCase))
                createResourceType = "Machine";
            else if (string.Equals(targetResourceType, "VirtualAgv", StringComparison.CurrentCultureIgnoreCase))
                createResourceType = "Machine";
            else if (string.Equals(targetResourceType, "AgvResource", StringComparison.CurrentCultureIgnoreCase))
                createResourceType = "Machine";
            var newResource = CreateTemplateResource(createResourceType + "ResourceTemplate", resourceTemplateName);
            if (newResource != null)
            {
                if (newResource.LoadFromConfig(resourceTemplateElement))
                {
                    AddResourceTemplate(newResource);
                    Log.Info($"装载资源模板{targetResourceType}+ResourceTemplate: [{resourceTemplateName}]成功");

                    return;
                }

                Log.Warn($"装载资源模板{targetResourceType}+ResourceTemplate: [{resourceTemplateName}]失败");
                return;
            }

            Log.Warn($"创建资源{targetResourceType}+ResourceTemplate: [{resourceTemplateName}]失败");
        }

        private static void AddResourceTemplate(IResourceTemplate resourceTemplate)
        {
            try
            {
                ResourceTemplateCollections.Add(resourceTemplate.TemplateName, resourceTemplate); // 增加到公共资源池
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }


        /// <summary>
        ///     创建ResourceTemplate
        /// </summary>
        /// <param name="resourceTemplateType"></param>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        /// added by sunjian 2019-12-24
        private static IResourceTemplate CreateTemplateResource(string resourceTemplateType, string resourceName)
        {
            try
            {
                var resourcePath = ResourceClassRegister.GetResourceTemplateFullName(resourceTemplateType);

                var packageName = ResourceClassRegister.GetPackageName(resourceTemplateType);

                var findingPath = resourcePath + "," + packageName;
                var resourceType = Type.GetType(findingPath, true);

                var obj = Activator.CreateInstance(resourceType, resourceName);

                return (IResourceTemplate) obj;
            }
            catch (Exception ex)
            {
                Log.Error($"创建资源模板:{resourceTemplateType},名称:{resourceName}失败:{ex}");
                return null;
            }
        }

        public static bool LoadResourceInConfig(XmlNode node, string fileName, XmlDocument doc)
        {
            var resource = (XmlElement) node;

            // 如果资源Enable=false，则跳过不加载 
            if (resource.GetAttribute("Enable").ToLower() != "true") return true;

            var resourceType = resource.GetAttribute("Type");
            var resourceName = resource.GetAttribute("Name");
            
            var genericType = resource.GetAttribute("GenericType");

            Log.Info($"装载资源{resourceType}[{resourceName}]...");

            var newResource = CreateResource(resourceType, resourceName, genericType);
            
            if (newResource != null)
            {
                //装入配置文件
                if (newResource is XMLConfig config)
                {
                    config.ConfigFileName = fileName;
                    config.ConfigDoc = doc;
                    config.ResourceElement = resource;
                }

                if (newResource.LoadFromConfig(node))
                {
                    AddResource(newResource);
                    Log.Info($"装载资源{resourceType}[{resourceName}]成功");

                    return true;
                }

                Log.Warn($"装载资源{resourceType}[{resourceName}]失败");
                return false;
            }

            Log.Warn($"创建资源{resourceType}[{resourceName}]失败");
            return false;
        }


        #region "冗余"

        // 冗余
        public static bool EnableRedundancy;

        private static readonly Redundancy Redundancy = new Redundancy();
        private static string _redundancySyncInterval = "500";

        public static Redundancy GetRedundancy()
        {
            return Redundancy;
        }

        public static RedundancyMode GetRedundancyMode()
        {
            return Redundancy.Mode;
        }

        #endregion
    }
}