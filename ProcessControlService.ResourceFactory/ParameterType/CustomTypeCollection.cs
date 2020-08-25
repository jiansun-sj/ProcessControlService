using System;
using System.Collections.Generic;
using System.Linq;

namespace ProcessControlService.ResourceFactory.ParameterType
{
    // T ResourceType
    public class CustomTypeCollection
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(CustomTypeCollection));

        private readonly Dictionary<string, ICustomType> _customizedTypeList = new Dictionary<string, ICustomType>();

        //private ResourceType _type;
        private readonly string _strCustomizedType;

        public CustomTypeCollection(string customizedType)
        {
            _strCustomizedType = customizedType;
        }

        public void Add(ICustomType item)
        {
            if (item.Type == _strCustomizedType)
            {
                _customizedTypeList.Add(item.Name, item);
            }
            else
            {
                var errorMsg = $"往类型池:{item.Type}里添加类型:{item.Name}不匹配";
                //LOG.Error(errorMsg);

                throw new Exception(errorMsg);
            }
        }

        public void Remove(ICustomType item)
        {
            if (item.Type == _strCustomizedType)
            {
                _customizedTypeList.Remove(item.Name);
            }
            else
            {
                var errorMsg = $"往类型池:{item.Type}里删除类型:{item.Name}不匹配";

                throw new Exception(errorMsg);
            }
        }

        public List<ICustomType> GetAllTypes()
        {
            return _customizedTypeList.Values.ToList();
        }
    }
}
