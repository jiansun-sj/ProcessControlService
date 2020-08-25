// ==================================================
// 文件名：CustomTypeManager.cs
// 创建时间：2020/06/16 10:00
// ==================================================
// 最后修改于：2020/08/18 10:00
// 修改人：jians
// ==================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using log4net;

namespace ProcessControlService.ResourceFactory.ParameterType
{
    /// <summary>
    ///     自定义类型管理器
    ///     Created By David Dong at 20180521
    /// </summary>
    public class CustomTypeManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CustomTypeManager));

        // 公共类型池
        private static readonly Dictionary<string, ICustomType> CustomTypeCollections =
            new Dictionary<string, ICustomType>();

        public static readonly Dictionary<string, Type> CustomTypes = new Dictionary<string, Type>();

        // 分类类型池
        private static readonly Dictionary<string, CustomTypeCollection> TypedCustomTypeCollections =
            new Dictionary<string, CustomTypeCollection>();

        // 增加分类类型池
        public static void AddCustomizedTypeCollection(string strType, CustomTypeCollection typedCollection)
        {
            TypedCustomTypeCollections.Add(strType, typedCollection);
        }
        
        public static void AddCustomizedType(string strType, Type customizedType)
        {
            CustomTypes.Add(strType, customizedType);
        }
        
        // 获得公共类型列表
        public static List<ICustomType> GetAllResources()
        {
            return CustomTypeCollections.Values.ToList();
        }

        // 增加自定义类型
        public static void AddCustomType(ICustomType customType)
        {
            try
            {
                TypedCustomTypeCollections[customType.Type].Add(customType); // 增加到分类类型池

                CustomTypeCollections.Add(customType.Name, customType); // 增加到公共类型池
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        // 删除自定义类型
        private static void RemoveCustomType(ICustomType customType)
        {
            try
            {
                TypedCustomTypeCollections[customType.Type].Remove(customType); // 从分类资源池

                CustomTypeCollections.Remove(customType.Name);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        // 获得自定义类型
        public static ICustomType GetCustomizedType(string customTypeName)
        {
            try
            {
                return CustomTypeCollections[customTypeName];
            }
            catch (Exception)
            {
                Log.Error($"获取资源:{customTypeName}出错");
                return null;
            }
        }

        // 从配置文件里加载自定义变量
        public static bool LoadCustomTypesInConfig(XmlNode node)
        {
            var resource = (XmlElement) node;

            // 如果资源Enable=false，则跳过不加载 
            if (resource.GetAttribute("Enable").ToLower() != "true") return true;

            var customizedTypeType = resource.GetAttribute("Type");
            var customizedTypeName = resource.GetAttribute("Name");
            Log.Info($"装载自定义类型{customizedTypeType},名称{customizedTypeName}...");


            var newType = CreateCustomizedType(customizedTypeType, customizedTypeName);
            if (newType != null)
            {
                if (newType.LoadFromConfig(node))
                {
                    AddCustomType(newType);
                    Log.Info($"装载自定义类型{customizedTypeName}成功");

                    return true;
                }

                Log.Warn($"装载自定义类型{customizedTypeName}失败");
                return false;
            }

            Log.Warn($"创建自定义类型{customizedTypeName}失败");
            return false;
        }

        // 创建自定义变量
        private static ICustomType CreateCustomizedType(string newType, string newName)
        {
            try
            {
                var resourcePath = ResourceClassRegister.GetCustomizedTypeFullName(newType);

                var packageName = ResourceClassRegister.GetPackageName(newType);

                var findingPath = resourcePath + "," + packageName;

                var resourceType = Type.GetType(findingPath, true);

                var obj = Activator.CreateInstance(resourceType, newName);

                return (ICustomType) obj;
            }
            catch (Exception ex)
            {
                Log.Error($"创建自定义类型:{newType} 名称{newName},失败:{ex}");
                return null;
            }
        }

        public static bool HasType(string type)
        {
            return CustomTypes.ContainsKey(type);
        }

        public static Type GetType(string type)
        {
            return CustomTypes[type];
        }
    }
}