// ==================================================
// 文件名：IListParameter.cs
// 创建时间：2020/06/08 9:34
// ==================================================
// 最后修改于：2020/06/08 9:34
// 修改人：jians
// ==================================================

using System.Collections.Generic;

namespace ProcessControlService.ResourceFactory.ParameterType
{
    /// <summary>
    /// 列表型基本参数接口 add by sunjian 2020-06
    /// </summary>
    public interface IListParameter: IParameter
    {
        object this[int index] { get; set; }
        
        void Remove(int index);

        void Add(object value);

        object GetValue(int index);

        void SetValue(int index, object value);
        
        void Replace(IListParameter listParameter);

        IListParameter Clone();

        IEnumerable<string> GetValueInStringList();

        int Count { get; }
    }
}