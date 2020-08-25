// ==================================================
// 文件名：IResource.cs
// 创建时间：2020/08/03 10:36
// ==================================================
// 最后修改于：2020/08/03 10:36
// 修改人：jians
// ==================================================

using System.Xml;

namespace ProcessControlService.ResourceFactory
{
    public interface IResource
    {
        object ResourceLocker { get; }
        
        string ResourceName
        { get; }

        string ResourceType
        { get; }

        IResource GetResourceObject();

        IResourceExportService GetExportService();

        bool LoadFromConfig(XmlNode node);

        void FreeResource();

    }
}