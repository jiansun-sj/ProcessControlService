using System.Xml;

namespace ProcessControlService.ResourceFactory
{
    public interface IResourceTemplate
    {
        string TemplateName { get; set; }

        string TargetResourceType { get; set; }

        bool LoadFromConfig(XmlNode node);

        //List<string> GetRegisteredResources();


    }
}
