// ==================================================
// 文件名：ParameterExtensions.cs
// 创建时间：2020/06/10 10:19
// ==================================================
// 最后修改于：2020/06/10 10:19
// 修改人：jians
// ==================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace ProcessControlService.ResourceFactory.ParameterType
{
    /// <summary>
    /// 所有Parameter的拓展方法 add
    /// </summary>
    public static class ParameterExtensions
    {
        /// <summary>
        /// 转换DictionaryParameter的值称为任意类型的Dictionary（string，T）参数
        /// </summary>
        /// <param name="source"></param>
        /// <param name="valueSelector"></param>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TExpected"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Dictionary<string, TExpected> ToDictionary<TSource,TExpected>(this DictionaryParameter<TSource> source,
            Func<TSource, TExpected> valueSelector)
        {
            if (valueSelector==null)
            {
                throw new ArgumentNullException(nameof(valueSelector));
            }
            
            var allValue = source.GetAllValue();

            return allValue.ToDictionary(keyValuePair => keyValuePair.Key,
                keyValuePair => valueSelector(keyValuePair.Value));
        }
        
        public static Dictionary<string, TExpected> ToDictionary<TSource,TExpected>(this IDictionaryParameter source,
            Func<TSource, TExpected> valueSelector)
        {
            if (valueSelector==null)
                throw new ArgumentNullException(nameof(valueSelector));

            if (!(source is DictionaryParameter<TSource> dictionaryParameter))
                throw new ArgumentException($"{nameof(source)}不是指定类型[{typeof(TSource)}]的参数");
            
            var allValue = dictionaryParameter.GetAllValue();

            return allValue.ToDictionary(keyValuePair => keyValuePair.Key,
                keyValuePair => valueSelector(keyValuePair.Value));
        }
    }
}