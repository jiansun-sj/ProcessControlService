// ==================================================
// 文件名：IBasicValue.cs
// 创建时间：2020/06/08 13:02
// ==================================================
// 最后修改于：2020/06/08 13:02
// 修改人：jians
// ==================================================

using System;

namespace ProcessControlService.ResourceFactory.ParameterType
{
    public interface IBasicValue
    {
        object GetValue();

        void SetValue(object value);

        Type Type { get; }
        
        string SimpleType { get; }
        
        void SetValueInString(string value);

        IBasicValue Clone();
    }
}