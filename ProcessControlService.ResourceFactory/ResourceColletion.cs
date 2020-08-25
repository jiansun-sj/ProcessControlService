using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace ProcessControlService.ResourceFactory
{
    // T ResourceType
    public class ResourceColletion
    {
        private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(ResourceColletion));

        private Dictionary<string, IResource> _resourceList = new Dictionary<string, IResource>();

        //private ResourceType _type;
        private string _strResourceType;

        public ResourceColletion(string ResourceType)
        {
            _strResourceType = ResourceType;
        }

        public void Add(IResource item)
        {
            if (item.ResourceType == _strResourceType)
            {
                _resourceList.Add(item.ResourceName, item);
            }
            else
            {
                string errorMsg = string.Format("往资源池:{0}里添加资源:{1}类型不匹配", item.ResourceType, item.ResourceName);
                //LOG.Error(errorMsg);

                throw new Exception(errorMsg);
            }
        }

        public void Remove(IResource item)
        {
            if (item.ResourceType == _strResourceType)
            {
                _resourceList.Remove(item.ResourceName);
            }
            else
            {
                string errorMsg = string.Format("往资源池:{0}里删除资源:{1}类型不匹配", item.ResourceType, item.ResourceName);
                //LOG.Error(errorMsg);

                throw new Exception(errorMsg);
            }
        }

        public List<IResource> GetAllResources()
        {
            return _resourceList.Values.ToList();
        }
    }


}
