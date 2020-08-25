// ==================================================
// 文件名：IBasicParameter.cs
// 创建时间：2020/06/08 9:22
// ==================================================
// 最后修改于：2020/06/08 9:22
// 修改人：jians
// ==================================================

namespace ProcessControlService.ResourceFactory.ParameterType
{
    /// <summary>
    /// 基本类型参数接口 add by sunjian 2020-06
    /// </summary>
    public interface IBasicParameter: IParameter
    {
        object GetValue();

        void SetValue(object value);

        void SetValueInString(string value);

        IBasicParameter Clone();

        bool Equals(object value);
    }
}