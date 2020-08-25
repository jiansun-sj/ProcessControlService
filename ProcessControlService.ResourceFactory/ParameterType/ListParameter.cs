// ==================================================
// 文件名：ListParameter.cs
// 创建时间：// 17:20
// 上海芸浦信息技术有限公司
// copyright@yumpoo
// ==================================================
// 最后修改于：2020/01/18 17:20
// 修改人：jians
// ==================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace ProcessControlService.ResourceFactory.ParameterType
{
    /// <summary>
    /// 列表型基本类  add by sunjian 2020-05  
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    ///  更改为泛型类，实现IListParameter接口 sunjian 2020-06
    /// </remarks>
    public sealed class ListParameter<T> : Parameter,IListParameter
    {
        private List<T> _values=new List<T>();

        public bool HasValue => _values!=null&&_values.Count > 0;
        
        // ReSharper disable once MemberCanBePrivate.Global
        public string ListParameterType => _values.GetType().ToString();

        public ListParameter(string parameterName):base(parameterName)
        {
            StrType = typeof(T).ToString();
        }
        
        public object this[int index]
        {
            get
            {
                if (index>_values.Count)
                {
                    throw new ArgumentOutOfRangeException($"ParameterList:[{Name}]超出索引范围1.");
                }
                
                return _values[index];
                
            }
            set
            {
                if (_values.Count>index)
                {
                    throw new ArgumentOutOfRangeException($"ParameterList:[{Name}]超出索引范围1.");
                }
                
                _values[index] = (T)value;
            }
        }

        // ReSharper disable once MemberCanBePrivate.Global
        // 反射对应构造函数
        public ListParameter(string parameterName, string parameterType)
            : base(parameterName, parameterType)
        {
            //_basicValues = new List<T>();
        }

        public void SetValue(int index, object value)
        {
            _values[index] = (T)value;
        }
        
        public void Replace(IListParameter listParameter)
        {
            var parameter = (ListParameter<T>)listParameter;

            _values = parameter._values;
        }

        
        // ReSharper disable once MemberCanBePrivate.Global
        public void Add(T basicValue)
        {
            if (basicValue != null) _values.Add(basicValue);
        }

        public List<T> GetValue()
        {
            return HasValue ? _values:null;
        }

        public void Remove(int index)
        {
            if (_values.Count <= index)
            {
                _values.RemoveAt(index);
            }

            throw new ArgumentOutOfRangeException($"ParameterList:[{Name}]超出索引范围1.");
        }

        public void Add(object value)
        {
            _values.Add((T)value);
        }

        public object GetValue(int index)
        {
            throw new NotImplementedException();
        }

        public override string GetTypeString()
        {
            return ListParameterType;
        }

        public string GetValueInString()
        {
            var valueString = $"ListParameter:[{Name}]:\n";
            var i = 0;

            return _values.Aggregate(valueString,
                (current, basicValue) => current + $"第{++i}个参数值为：{basicValue}\n");
        }

        public override void Clear()
        {
            _values.Clear();
        }

        public IListParameter Clone()
        {
            var listParameter = new ListParameter<T>(Name, StrType){};

            foreach (var basicValue in _values)
            {
                listParameter.Add(basicValue);
            }

            return listParameter;
        }

        public IEnumerable<string> GetValueInStringList()
        {
            return _values.Select(a=>a.ToString()).ToList();
        }

        public Type ValueType => typeof(T);

        public int Count => _values.Count;
    }
}