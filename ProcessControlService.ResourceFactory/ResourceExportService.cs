using System.Collections.Generic;
using ProcessControlService.Contracts;

namespace ProcessControlService.ResourceFactory
{
    public interface IResourceExportService
    {
        List<ResourceServiceModel> GetExportServices();

        //以json传递输入输出参数
        string CallExportService(string serviceName, string strParameter = null);
    }


}
