// ==================================================
// 文件名：BasicParameter.cs
// 创建时间：2020/01/18 16:06
// ==================================================
// 最后修改于：2020/06/11 16:06
// 修改人：jians
// ==================================================

using System;

namespace ProcessControlService.ResourceFactory.ParameterType
{
    /// <summary>
    ///     基础类参数
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    ///  更改为抽象类,实现IBasicParameter接口  sunjian 2020-06-11
    /// </remarks>
    public sealed class BasicParameter<T> : Parameter, IBasicParameter
    {
        private IBasicValue _basicValue = new BasicValue<T>();

        public BasicParameter(string parameterName) : base(parameterName)
        {
            StrType = typeof(T).ToString();
        }

        public BasicParameter(string parameterName, BasicValue<T> value) : base(parameterName)
        {
            _basicValue = value;

            StrType = typeof(T).ToString();
        }
        
        public BasicParameter(string parameterName, string parameterType) : base(parameterName, parameterType)
        {
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public T Value
        {
            get => (T) _basicValue.GetValue();
            set => _basicValue.SetValue(value);
        }

        public bool HasValue => _basicValue != null && _basicValue.GetValue() != null;

        public void SetValue(object value)
        {
            _basicValue.SetValue((T) value);
        }

        public void SetValueInString(string value)
        {
            var setValue = CreateValueFromString(StrType, value);

            SetValue(setValue);
        }

        public IBasicParameter Clone()
        {
            return new BasicParameter<T>(Name, StrType)
            {
                _basicValue = _basicValue.Clone(),
            };
        }

        public string GetValueInString()
        {
            return Value == null ? "" : Value.ToString();
        }

        public Type ValueType => typeof(T);

        public object GetValue()
        {
            return _basicValue.GetValue();
        }

        public override bool Equals(object obj)
        {
            return _basicValue.Equals(obj);
        }

        public void SetValue(T value)
        {
            _basicValue.SetValue(value);
        }
        
        public string GetValuesInString()
        {
            return GetValue().ToString();
        }

        public override string GetTypeString()
        {
            return GetValue().GetType().ToString();
        }

        public new Type GetType()
        {
            return _basicValue.GetType();
        }

        public override void Clear()
        {
            _basicValue.SetValue(default);
        }

        private bool Equals(BasicParameter<T> other)
        {
            return Equals(_basicValue, other._basicValue);
        }

        public override int GetHashCode()
        {
            return _basicValue != null ? _basicValue.GetHashCode() : 0;
        }
    }
}