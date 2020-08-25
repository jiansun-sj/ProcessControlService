// ==================================================
// 文件名：PackageLoader.cs
// 创建时间：2020/01/02 17:19
// 上海芸浦信息技术有限公司
// copyright@yumpoo
// ==================================================
// 最后修改于：2020/03/12 17:19
// 修改人：jians
// ==================================================

using System;
using System.Collections.Generic;
using System.Reflection;
using log4net;

namespace ProcessControlService.ResourceFactory
{
    public class PackageLoader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PackageLoader));
        //private string _assemblyName;

        public readonly bool LoadSucceeded;


        private readonly Assembly _assembly;

        public PackageLoader(string assemblyName)
        {
            //_assemblyName = AssemblyName;

            try
            {
                //string temp = "file:///C:/Debug/service/" + AssemblyName;  //路径
                _assembly = Assembly.LoadFrom(assemblyName);
                //LOG.Info(string.Format("PackageLoader的路径为{0}", _assembly.CodeBase));
                var typeList = _assembly.GetTypes();

                // finding package profile
                foreach (var type in typeList)
                    //if (type is typeof(BaseProfile))
                    //if(typeof(BaseProfile).IsSubclassOf(type))
                    if (type.Name == "PackageProfile")
                        CreateProfile(type);

                LoadSucceeded = true;
            }
            catch (Exception ex)
            {
                LoadSucceeded = false;
                Log.Error(ex);
            }
        }

        public BaseProfile Profile { get; private set; }

        private void CreateProfile(Type profileType)
        {
            try
            {
                //string AppPath = Assembly.GetExecutingAssembly().GetName().Name;
                //string FullActionType = AppPath + ".Action." + ActionType;
                //Type objType = Type.GetType(FullActionType, true);
                var obj = Activator.CreateInstance(profileType, _assembly);

                Profile = (BaseProfile) obj;
            }
            catch (Exception ex)
            {
                Log.Error($"创建Profile with Type{profileType}失败{ex}.");
            }
        }

        //public string GetExtendTypePath(string TypeName)
        //{
        //    foreach (var _type in typeList)
        //    {
        //        if (TypeName == _type.Name)
        //        { return _type.FullName; }
        //    }
        //    return string.Empty;
        //}

        public static List<string> GetAllProcessControlServiceAssembly()
        {
            var result = new List<string>();

            foreach (var item in AppDomain.CurrentDomain.GetAssemblies())
                if (item.FullName.Contains("ProcessControlService"))
                    result.Add(item.FullName);

            return result;
        }
    }
}