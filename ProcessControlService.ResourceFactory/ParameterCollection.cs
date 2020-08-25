using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;

namespace ProcessControlService.ResourceFactory
{
    /// <summary>
    /// 参数集合类
    /// </summary>
    public class ParameterCollection : ICloneable
    {
        private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(ParameterCollection));

        private readonly Dictionary<string, Parameter> _parameters = new Dictionary<string, Parameter>();

        public bool HasParameter(string ParameterName)
        {
            return _parameters.ContainsKey(ParameterName);
        }

        //检查参数是否都有
        protected bool CheckParametersExist(Parameter[] Parameters)
        {
            // 检查参数数量
            if (Parameters.Length != _parameters.Count)
                return false;

            // 检查每个参数
            foreach (Parameter par in Parameters)
            {
                if (!_parameters.ContainsKey(par.Name))
                    return false;
            }

            return true;
        }

        //获得参数
        public Parameter GetParameter(string ParameterName)
        {
            try
            {
                return _parameters[ParameterName];
            }
            catch (Exception ex)
            {
                LOG.Error(string.Format("找不到输入参数{0}"+ex.Message ,  ParameterName));
                return null;
            }
        }

        //设置参数
        public Parameter this[string ParameterName]
        {
            get 
            {
                try
                {
                    return _parameters[ParameterName];
                }
                catch (Exception ex)
                {
                    LOG.Error(string.Format("找不到输入参数{0}"+ex.Message , ParameterName));
                    return null;
                }
            }
            set 
            {
                try
                {
                    _parameters[ParameterName] = value;
                }
                catch (Exception ex)
                {
                    LOG.Error(string.Format("找不到输入参数{0}" + ex.Message, ParameterName));
                }
            }
        }

        //以字符串方式设置参数值
        public bool SetParameterValue(string ParameterName, object Value)
        {
            try
            {
                _parameters[ParameterName].SetValue(Value);
                return true;
            }
            catch (Exception ex)
            {
                LOG.Error(string.Format("输入参数{0}设定出错：{1}", ParameterName, ex.Message));
                return false;
            }
        }

        //设置参数值
        public bool SetParameterValueInString(string ParameterName, string strValue)
        {
            try
            {
                _parameters[ParameterName].SetValueInString(strValue);
                return true;
            }
            catch (Exception ex)
            {
                LOG.Error(string.Format("输入参数{0}设定出错：{1}", ParameterName, ex.Message));
                return false;
            }
        }

        //增加参数
        public void Add(Parameter parameter)
        {
            _parameters.Add(parameter.Name, parameter);
        }

        /// <summary>
        /// Created by Dongmin 2017/03/10
        /// Last modified by Dongmin 2017/03/10
        /// 列出Action的所有输出参数
        /// </summary>
        /// <returns></returns>
        public List<Parameter> ListParameters()
        {
            return _parameters.Values.ToList();
        }


        public object Clone()
        {
            ParameterCollection newCollection = new ParameterCollection();

            foreach(string par in _parameters.Keys)
            {
                newCollection.Add(_parameters[par]);
            }

            return newCollection;
        }
    }
}
