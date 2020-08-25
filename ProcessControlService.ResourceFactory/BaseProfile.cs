// ==================================================
// 文件名：BaseProfile.cs
// 创建时间：2020/06/16 18:34
// ==================================================
// 最后修改于：2020/08/18 18:34
// 修改人：jians
// ==================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ProcessControlService.ResourceFactory.ParameterType;

namespace ProcessControlService.ResourceFactory
{
    public abstract class BaseProfile
    {
        private readonly Assembly _assembly;
        private readonly Type[] _typeList;

        //public abstract void CollectTypeInOtherPackage(BaseProfile profile);

        protected List<BaseProfile> OtherPackages = new List<BaseProfile>();

        public BaseProfile(Assembly assembly)
        {
            _assembly = assembly;

            _typeList = _assembly.GetTypes();

            InitResources();
        }

        //public abstract List<string> GetResourceNames();

        public List<Type> GetAllTypes()
        {
            return _typeList.ToList();
        }

        //获得所有资源类型
        public List<Type> GetAllResourceTypes()
        {
            var _resources = new List<Type>();

            try
            {
                //找到包里所有的资源
                foreach (var type in _typeList)
                    if (type.GetInterface("IResource") != null)
                        _resources.Add(type);

                return _resources;
            }
            catch (Exception)
            {
                return null;
            }
        }

        //获得所有资源模板类型
        public List<Type> GetAllResourceTemplateTypes()
        {
            var templateTypes = new List<Type>();

            try
            {
                //找到包里所有的资源
                templateTypes.AddRange(_typeList.Where(type => type.GetInterface("IResourceTemplate") != null));

                return templateTypes;
            }
            catch (Exception)
            {
                return null;
            }
        }


        //获得所有自定义类型,获取所有实现ICustomType接口的Class，sunjian 2020-08-18 updated
        public IEnumerable<Type> GetAllCustomizedTypes()
        {
            var customizedTypes = new List<Type>();

            try
            {
                //找到包里所有的类型定义 CustomValue
                //增加class类型判断，sunjian 2020-08
                customizedTypes.AddRange(_typeList.Where(type =>
                    type.IsClass && !type.IsAbstract && type.GetInterface(nameof(ICustomType)) != null));

                return customizedTypes;
            }
            catch (Exception)
            {
                return null;
            }
        }

        protected virtual void InitResources()
        {
        }

        public virtual void LoadAdditionalResource()
        {
        }

        public void AddOtherPackage(BaseProfile Profile)
        {
            OtherPackages.Add(Profile);
        }

        public abstract string GetPackageName();
    }
}