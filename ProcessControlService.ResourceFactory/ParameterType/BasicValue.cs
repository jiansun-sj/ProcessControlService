// ==================================================
// 文件名：BasicValue.cs
// 创建时间：2020/01/02 16:56
// ==================================================
// 最后修改于：2020/06/05 16:56
// 修改人：jians
// ==================================================

using System;
using System.Collections.Generic;
using log4net;
using Newtonsoft.Json;

namespace ProcessControlService.ResourceFactory.ParameterType
{
    /// <summary>
    ///     基本类型，可以在参数中使用
    /// </summary>
    public class BasicValue<T>: IBasicValue
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(BasicValue<T>));

        private T _value;

        public T Value => _value;

        public BasicValue()
        {
            _value = default;
        }

        public BasicValue(T initValue)
        {
            _value = initValue;
        }

        [JsonIgnore]
        public Type Type => typeof(T);
        
        public string SimpleType => Parameter.ConvertValueType(Type.ToString());

        public IBasicValue Clone()
        {
            var basicValueType = typeof(BasicValue<>);

            var makeGenericType = basicValueType.MakeGenericType(Type);

            return (IBasicValue) Activator.CreateInstance(makeGenericType, _value);
        }

        public static object CreateValueFromString(Type type, string strValue)
        {
            return Parameter.CreateValueFromString(type.Name.ToLower(), strValue);
        }

        public void SetValueInString(string strValue)
        {
            _value = (T) CreateValueFromString(_value.GetType(), strValue);
        }

        public object GetValue() 
        {
            return _value;
        }
        
        public void SetValue(T value)
        {
            _value = value;
        }

        public void SetValue(object value)
        {
            _value = (T)value;
        }
        
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            switch (obj)
            {
                case BasicValue<T> basicValue:
                    return _value.Equals(basicValue._value);
                
                case T value:
                    return _value.Equals(value);
                
                default:
                    return false;
            }
        }

        public override int GetHashCode()
        {
            return EqualityComparer<T>.Default.GetHashCode(_value);
        }

        public string ToJson()
        {
            if (_value is DateTime dateTimeValue) return $"\"{dateTimeValue:G}\"";
            return _value.ToString();
        }

        public override string ToString()
        {
            return _value.ToString();
        }
    }
}