// ==================================================
// 文件名：DictionaryParameter.cs
// 创建时间：2020/01/18 9:48
// ==================================================
// 最后修改于：2020/06/08 9:48
// 修改人：jians
// ==================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace ProcessControlService.ResourceFactory.ParameterType
{
    /// <summary>
    /// 字典型基本类  add by sunjian 2020-05  
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    ///  更改为泛型类，实现IDictionaryParameter接口 sunjian 2020-06
    /// </remarks>
    public class DictionaryParameter<T> : Parameter, IDictionaryParameter
    {
        private IDictionary<string, T> _values = new Dictionary<string, T>();

        public DictionaryParameter(string parameterName):base(parameterName)
        {
            StrType = typeof(T).ToString();
        }

        public DictionaryParameter(IDictionary<string, T> initValue)
        {
            _values = initValue;
        }
        
        public DictionaryParameter(string parameterName, string parameterType)
            : base(parameterName, parameterType)
        {
        }

        public string DictionaryParameterType => _values.GetType().ToString();

        public bool HasValue => _values.Count > 0;

        public object this[string key]
        {
            get
            {
                if (!_values.ContainsKey(key))
                    throw new ArgumentOutOfRangeException($"ParameterDictionary:[{Name}]超出索引范围.");

                return _values[key];
            }
            set
            {
                if (!_values.ContainsKey(key))
                    throw new ArgumentOutOfRangeException($"ParameterDictionary:[{Name}]超出索引范围.");

                _values[key] = (T)value;
            }
        }

        public object GetValue(string key)
        {
            return this[key];
        }
        
        public void Replace(IDictionaryParameter dictionaryParameter)
        {
            var parameter = (DictionaryParameter<T>)dictionaryParameter;

            _values=parameter._values;
        }

        public void Remove(string key)
        {
            if (_values.ContainsKey(key)) _values.Remove(key);

            throw new ArgumentOutOfRangeException($"ParameterDictionary:[{Name}]超出索引范围1.");
        }

        public bool ContainsKey(string key, Type type)
        {
            return _values.ContainsKey(key) && typeof(T) == type;
        }

        public void SetValue(string key, object value)
        {
            _values[key] = (T) value;
        }

        public IDictionaryParameter Clone()
        {
            var dictionaryParameter = new DictionaryParameter<T>(Name, StrType);

            foreach (var basicValue in _values) dictionaryParameter.Add(basicValue.Key, basicValue.Value);

            return dictionaryParameter;
        }
        
        public Dictionary<string, string> GetAllValueInStringDic()
        {
            return _values.ToDictionary(keys => keys.Key, values => values.Value.ToString());
        }

        public int Count => _values.Count;

        public void Add(string key, object value)
        {
            if (value != null) _values.Add(key, (T) value);
        }

        public void Add(string key, T value)
        {
            if (value != null) _values.Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return _values.ContainsKey(key);
        }

        public void Replace(Dictionary<string, T> value)
        {
            _values = value;
        }

        public IDictionary<string, T> GetAllValue()
        {
            return _values;
        }

        public override string GetTypeString()
        {
            return DictionaryParameterType;
        }

        public string GetValueInString()
        {
            var valueString = $"DictionaryParameter:[{Name}]:\n";
            var i = 0;

            return _values.Aggregate(valueString,
                (current, keyValue) => current + $"第{++i}个参数值为：Key={keyValue.Key},值为：Value={keyValue.Value}\n");
        }

        public Type ValueType => typeof(T);

        public override void Clear()
        {
            _values.Clear();
        }

        public IEnumerable<T> ToValueList()
        {
            return _values.Select(a=>a.Value).ToList();
        }
    }
}