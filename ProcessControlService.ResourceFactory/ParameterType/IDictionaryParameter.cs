// ==================================================
// 文件名：IDictionaryParameter.cs
// 创建时间：2020/06/08 9:21
// ==================================================
// 最后修改于：2020/06/08 9:21
// 修改人：jians
// ==================================================

using System;
using System.Collections.Generic;

namespace ProcessControlService.ResourceFactory.ParameterType
{
    /// <summary>
    /// 字典型参数接口 add by sunjian 2020-06
    /// </summary>
    public interface IDictionaryParameter: IParameter
    {
        object this[string key] { get; set; }
        
        /// <summary>
        /// 移除指定键名的值。
        /// </summary>
        /// <param name="key"></param>
        void Remove(string key);
        
        /// <summary>
        /// 清空Dictionary Parameter中存储的数值。
        /// </summary>
        void Clear();
        
        /// <summary>
        /// 增加Dictionary Parameter包含的键值对。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void Add(string key, object value);

        /// <summary>
        /// 校验Dictionary Parameter 包含给出的键名。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool ContainsKey(string key);

        /// <summary>
        /// 校验Dictionary Parameter包含的键名和存储的数值类型。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        bool ContainsKey(string key, Type type);

        /// <summary>
        /// 修改指定键的值。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void SetValue(string key, object value);
        
        /// <summary>
        /// 获取指定键的值。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        object GetValue(string key);

        /// <summary>
        /// 替换Dictionary Parameter中存储的所有值。
        /// </summary>
        /// <param name="dictionaryParameter"></param>
        void Replace(IDictionaryParameter dictionaryParameter);
        
        

        /// <summary>
        /// Dictionary Parameter的克隆。
        /// </summary>
        /// <returns></returns>
        IDictionaryParameter Clone();

        /// <summary>
        /// 将Dictionary Parameter中的所有键值对转换为string值类型。
        /// </summary>
        /// <returns></returns>
        Dictionary<string,string> GetAllValueInStringDic();

        /// <summary>
        /// 包含的键对数量
        /// </summary>
        int Count { get; }
    }
}