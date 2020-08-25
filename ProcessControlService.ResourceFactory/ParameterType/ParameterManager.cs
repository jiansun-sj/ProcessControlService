// ==================================================
// 文件名：ParameterManager.cs
// 创建时间：2020/06/16 10:24
// ==================================================
// 最后修改于：2020/07/29 10:24
// 修改人：jians
// ==================================================

using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using ProcessControlService.Contracts.ProcessData;

namespace ProcessControlService.ResourceFactory.ParameterType
{
    /// <summary>
    ///     参数集合类，管理Process参数集和Action参数集 --- add by sunjian 2020-03
    /// </summary>
    public class ParameterManager : IDisposable /*: ICloneable*/
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ParameterManager));

        private readonly Dictionary<string, IDictionaryParameter> _dictionaryParameters =
            new Dictionary<string, IDictionaryParameter>();

        private readonly Dictionary<string, IListParameter> _listParameters = new Dictionary<string, IListParameter>();

        public readonly Dictionary<string, IBasicParameter> BasicParameters = new Dictionary<string, IBasicParameter>();

        public void Dispose()
        {
            Clear();
            GC.SuppressFinalize(this);
        }

        public bool HasParam(string parameterName)
        {
            return BasicParameters.ContainsKey(parameterName) ||
                   _listParameters.ContainsKey(parameterName) ||
                   _dictionaryParameters.ContainsKey(parameterName);
        }

        public bool ContainsKey(string parameterName)
        {
            return BasicParameters.ContainsKey(parameterName) || _listParameters.ContainsKey(parameterName) ||
                   _dictionaryParameters.ContainsKey(parameterName);
        }

        public string GetParameterType(string parameterName)
        {
            if (BasicParameters.ContainsKey(parameterName)) return "BasicParameter";

            if (_listParameters.ContainsKey(parameterName)) return "ListParameter";

            return _dictionaryParameters.ContainsKey(parameterName)
                ? "DictionaryParameter"
                : "NoSuchParameter";
        }

        #region 修改参数

        /// <summary>
        ///     设置基本类型参数值
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="value"></param>
        public void SetBasicParamValue(string parameterName, object value)
        {
            try
            {
                BasicParameters[parameterName].SetValue(value);
            }
            catch (Exception ex)
            {
                Log.Error($"输入参数{parameterName}设定出错：{ex.Message}");
            }
        }

        #endregion

        public ParameterManager Clone()
        {
            var newCollection = new ParameterManager();

            foreach (var basicParameter in BasicParameters.Keys.Select(key => BasicParameters[key]))
            {
                var parameter = basicParameter.Clone();

                newCollection.AddBasicParam(parameter);
            }

            foreach (var listParameter in _listParameters.Keys.Select(key => _listParameters[key]))
                newCollection.AddListParam(listParameter.Clone());

            foreach (var dictionaryParameter in _dictionaryParameters.Keys.Select(key => _dictionaryParameters[key]))
                newCollection.AddDictionaryParam(dictionaryParameter.Clone());

            return newCollection;
        }


        public Dictionary<string, IParameter> GetParam(IEnumerable<string> showParameters)
        {
            var parameters = new Dictionary<string, IParameter>();

            foreach (var showParameter in showParameters)
                if (BasicParameters.ContainsKey(showParameter))
                    parameters.Add(showParameter, BasicParameters[showParameter]);
                else if (_listParameters.ContainsKey(showParameter))
                    parameters.Add(showParameter, _listParameters[showParameter]);
                else if (_dictionaryParameters.ContainsKey(showParameter))
                    parameters.Add(showParameter, _dictionaryParameters[showParameter]);

            return parameters;
        }

        public void Clear()
        {
            BasicParameters.Clear();
            _listParameters.Clear();
            _dictionaryParameters.Clear();
        }

        public List<ParameterInfo> GetValueInString()
        {
            var instanceParameters = new List<ParameterInfo>();

            try
            {
                instanceParameters.AddRange(BasicParameters.Select(basicParameter =>
                    new ParameterInfo
                    {
                        Name = basicParameter.Key, ValueInString = basicParameter.Value.GetValueInString(),
                        Type = basicParameter.Value.StrType, Key = "Null"
                    }));

                foreach (var dictionaryParameter in _dictionaryParameters)
                    instanceParameters.AddRange(dictionaryParameter.Value.GetAllValueInStringDic().Select(
                        keyValuePair =>
                            new ParameterInfo
                            {
                                Name = dictionaryParameter.Key, Type = dictionaryParameter.Value.StrType,
                                ValueInString = keyValuePair.Value, Key = keyValuePair.Key
                            }));

                foreach (var listParameter in _listParameters)
                {
                    var index = 0;
                    instanceParameters.AddRange(listParameter.Value.GetValueInStringList().Select(basicValue =>
                        new ParameterInfo
                        {
                            Name = listParameter.Key,
                            Type = listParameter.Value.StrType,
                            ValueInString = basicValue,
                            Key = (index++).ToString()
                        }));
                }

                return instanceParameters;
            }
            catch (Exception e)
            {
                Log.Error($"查询ProcessInstance过程参数失败，异常为:[{e.Message}]");
                return instanceParameters;
            }
        }

        #region 增加参数

        public IBasicParameter this[string parameterName]
        {
            get =>
                BasicParameters.ContainsKey(parameterName)
                    ? BasicParameters[parameterName]
                    : null;
            set
            {
                //基本类型参数
                if (BasicParameters.ContainsKey(parameterName))
                    BasicParameters[parameterName].SetValue(value);
            }
        }

        /// <summary>
        ///     ListParameter类型参数根据数值索引设置和获取值
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="index"></param>
        public object this[string parameterName, int index]
        {
            get => _listParameters.ContainsKey(parameterName) ? _listParameters[parameterName].GetValue(index) : null;
            set
            {
                if (_listParameters.ContainsKey(parameterName)) _listParameters[parameterName].SetValue(index, value);
            }
        }

        /// <summary>
        ///     DictionaryParameter类型参数根据数值索引设置和获取值
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="key"></param>
        public object this[string parameterName, string key]
        {
            get => _dictionaryParameters.ContainsKey(parameterName)
                ? _dictionaryParameters[parameterName].GetValue(key)
                : null;
            set
            {
                if (_dictionaryParameters.ContainsKey(parameterName))
                    _dictionaryParameters[parameterName].SetValue(key, value);
            }
        }


        //增加参数
        public void AddBasicParam(IBasicParameter basicParameter)
        {
            if (ParamIsValid(basicParameter))
                BasicParameters.Add(basicParameter.Name, basicParameter);
        }

        public void AddListParam(IListParameter listParameter)
        {
            if (ParamIsValid(listParameter))
                _listParameters.Add(listParameter.Name, listParameter);
        }

        public void AddDictionaryParam(IDictionaryParameter dictionaryParameter)
        {
            if (ParamIsValid(dictionaryParameter))
                _dictionaryParameters.Add(dictionaryParameter.Name, dictionaryParameter);
        }

        private bool ParamIsValid(IParameter parameter)
        {
            if (parameter is null)
                return false;

            if (!ContainsKey(parameter.Name)) return true;

            Log.Error($"ParameterManager中已经有了名为：[{parameter.Name}]的参数，不可添加重复名称参数。");
            return false;
        }

        #endregion

        #region 增加参数

        //增加参数
        public void RemoveBasicParam<T>(BasicParameter<T> basicParameter)
        {
            if (BasicParameters.ContainsKey(basicParameter.Name))
                BasicParameters.Remove(basicParameter.Name);
            else
                throw new KeyNotFoundException($"BasicParameters不包含该键名:{basicParameter.Name}");
        }

        public void RemoveListParam<T>(ListParameter<T> listParameter)
        {
            if (_listParameters.ContainsKey(listParameter.Name))
                _listParameters.Remove(listParameter.Name);
            else
                throw new KeyNotFoundException($"ListParameters不包含该键名:{listParameter.Name}");
        }

        public void RemoveDictionaryParam<T>(DictionaryParameter<T> dictionaryParameter)
        {
            if (_dictionaryParameters.ContainsKey(dictionaryParameter.Name))
                _dictionaryParameters.Remove(dictionaryParameter.Name);
            else
                throw new KeyNotFoundException($"BasicParameters不包含该键名:{dictionaryParameter.Name}");
        }

        #endregion


        #region 获取参数

        public IBasicParameter GetBasicParam(string parameterName)
        {
            try
            {
                return BasicParameters[parameterName];
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("找不到输入参数{0}" + ex.Message, parameterName));
                return null;
            }
        }

        public IDictionaryParameter GetDictionaryParam(string parameterName)
        {
            try
            {
                return _dictionaryParameters[parameterName];
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("找不到输入参数{0}" + ex.Message, parameterName));
                return null;
            }
        }

        public IListParameter GetListParam(string parameterName)
        {
            try
            {
                return _listParameters[parameterName];
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("找不到输入参数{0}" + ex.Message, parameterName));
                return null;
            }
        }

        #endregion
    }
}