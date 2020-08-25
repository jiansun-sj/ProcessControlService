// ==================================================
// 文件名：ResourceClassRegister.cs
// 创建时间：2020/01/02 9:50
// ==================================================
// 最后修改于：2020/06/02 9:50
// 修改人：jians
// ==================================================

using System;
using System.Collections.Generic;
using log4net;

namespace ProcessControlService.ResourceFactory
{
    //各对象资源包注册表
    public class ResourceClassRegister
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ResourceClassRegister));

        private static readonly Dictionary<string, string> ResourceClassDic = new Dictionary<string, string>();

        private static readonly Dictionary<string, string> CustomizedTypeClassRegister =
            new Dictionary<string, string>();

        private static readonly Dictionary<string, string> ResourceTemplateDic =
            new Dictionary<string, string>();

        private static readonly Dictionary<string, string> Packages = new Dictionary<string, string>();

        public static void AddResource(string resourceClassName, string fullName, string packageName)
        {
            try
            {
                ResourceClassDic.Add(resourceClassName, fullName);
                Packages.Add(resourceClassName, packageName);
            }
            catch (Exception e)
            {
                Log.Error($"加入资源失败，异常为： {e.Message}.");
            }
        }

        public static void AddResourceTemplate(string resourceTemplateClassName, string fullName, string packageName)
        {
            ResourceTemplateDic.Add(resourceTemplateClassName, fullName);
            Packages.Add(resourceTemplateClassName, packageName);
        }

        public static void AddCustomizedType(string customizedTypeClassName, string fullName, string packageName)
        {
            CustomizedTypeClassRegister.Add(customizedTypeClassName, fullName);
            Packages.Add(customizedTypeClassName, packageName);
        }

        public static string GetResourceFullName(string resourceClassName)
        {
            return ResourceClassDic[resourceClassName];
        }

        public static string GetCustomizedTypeFullName(string customizedTypeClassName)
        {
            return CustomizedTypeClassRegister[customizedTypeClassName];
        }
        
        public static bool HasCustomizedType(string customizedTypeClassName)
        {
            return CustomizedTypeClassRegister.ContainsKey(customizedTypeClassName);
        }

        public static string GetPackageName(string resourceClassName)
        {
            return Packages[resourceClassName];
        }

        public static string GetResourceTemplateFullName(string resourceTemplateClassName)
        {
            return ResourceTemplateDic[resourceTemplateClassName];
        }
    }
}