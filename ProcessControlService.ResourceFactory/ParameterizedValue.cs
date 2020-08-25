using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessControlService.ResourceFactory
{
    public interface IParameterizedValue
    {
        string ParameterName { get; }
        object ParameterValue { get; set; }

        //Type GetType();

        //string ClassPath { get; }

        string ToJson(); //转换成JSON字符串

    }
}
