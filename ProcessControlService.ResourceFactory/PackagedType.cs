using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessControlService.ResourceFactory
{
    public interface IPackagedType
    {
        PackagedTypeClass GetPackagedTypeClass();
    }

    public enum PackagedTypeClass
    {
        Resource,
        CustomizedType
    }
}
