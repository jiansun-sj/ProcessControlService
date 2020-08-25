// ==================================================
// 文件名：IParameter.cs
// 创建时间：2020/06/08 15:05
// ==================================================
// 最后修改于：2020/06/08 15:05
// 修改人：jians
// ==================================================

using System;

namespace ProcessControlService.ResourceFactory.ParameterType
{
    /// <summary>
    /// FW参数根接口 add by sunjian 2020-06
    /// </summary>
    public interface IParameter
    {
        string Name { get; }

        bool HasValue { get;}

        string StrType { get; }
        
        string GetValueInString();

        Type ValueType
        {
            get;
        }
    }
}