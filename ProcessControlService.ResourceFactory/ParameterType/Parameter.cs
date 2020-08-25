// ==================================================
// 文件名：Parameter.cs
// 创建时间：2020/06/16 18:16
// ==================================================
// 最后修改于：2020/08/03 18:16
// 修改人：jians
// ==================================================

using System;
using System.Xml;
using log4net;

namespace ProcessControlService.ResourceFactory.ParameterType
{
    /// <summary>
    ///     Parameter抽象类，负责创建基础值BasicValue和给类参数（BasicParameter，ListParameter，DictionaryParameter）
    /// </summary>
    /// <remarks>
    ///     sunjian 2020/06/09 重组Paramter类
    ///     更改框架代码，更改为泛型类：
    ///     1. BasicParameter
    ///     2. ListParameter
    ///     3. DictionaryParameter
    ///     4. BasicValue
    ///     增加接口：
    ///     1. IBasicParameter
    ///     2. IListParameter
    ///     3. IDictionaryParameter
    ///     4. IBasicValue
    /// </remarks>
    public abstract class Parameter /*:ICloneable*/
    {
        protected static readonly ILog Log = LogManager.GetLogger(typeof(Parameter));

        protected Parameter()
        {
        }

        protected Parameter(string parameterName)
        {
            Name = parameterName;
        }

        protected Parameter(string parameterName, string parameterType)
        {
            Name = parameterName;
            StrType = parameterType;
        }

        public string StrType { get; set; }

        public string Name { get; }

        public static object CreateParameter(string name, string strType, Type parameterType)
        {
            var value = CreateValue(strType);

            var makeGenericType = parameterType.MakeGenericType(value.GetType());

            return (IParameter) Activator.CreateInstance(makeGenericType, name, strType);
        }

        public static IBasicValue CreateBasicValue(string parameterType, string defaultValue = "")
        {
            var value = CreateValue(parameterType);

            var basicValueType = typeof(BasicValue<>);

            var makeGenericType = basicValueType.MakeGenericType(value.GetType());

            if (string.IsNullOrEmpty(defaultValue) && value.GetType() != typeof(string))
                return (IBasicValue) Activator.CreateInstance(makeGenericType);

            var setValue = CreateValueFromString(parameterType, defaultValue);

            return (IBasicValue) Activator.CreateInstance(makeGenericType, setValue);
        }

        public static IBasicParameter CreateBasicParameter(string name, string strType)
        {
            return (IBasicParameter) CreateParameter(name, strType, typeof(BasicParameter<>));
        }

        public static IListParameter CreateListParameter(string name, string strType)
        {
            return (IListParameter) CreateParameter(name, strType, typeof(ListParameter<>));
        }

        public static IDictionaryParameter CreateDicParameter(string name, string strType)
        {
            return (IDictionaryParameter) CreateParameter(name, strType, typeof(DictionaryParameter<>));
        }

        /// <summary>
        /// 根据类型名创建值，支持基本类型，基本类型数组和实现了ICustomType接口的数据类型
        /// </summary>
        /// <param name="strType"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static object CreateValue(string strType, string value = "")
        {
            try
            {
                var isArray = false;
                
                var type = ConvertValueType(strType);

                //判断创建的类型是否为数组类型
                if (type.Contains("[]"))
                {
                    isArray = true;
                }

                //创建非数组类型，无初始值类型
                if (string.IsNullOrEmpty(value) && !isArray) return CreateValueWithoutInitValue(strType, type);

                if (!string.IsNullOrEmpty(value) && !isArray)
                    return CreateValueWithInitValue(strType, value, type);

                //创建数组型数据
                var primitiveType = type.Substring(0, type.Length - 2);
                
                var valueWithoutInitValue = CreateValueWithoutInitValue(strType, primitiveType);
                
                var type1 = valueWithoutInitValue.GetType();

                if (string.IsNullOrEmpty(value) && isArray)
                {
                    return Array.CreateInstance(type1,0);;
                }
                
                //带有设定值的数组
                var initValues = value.Split(',');

                var instance = Array.CreateInstance(type1, initValues.Length);

                for (var i = 0; i < initValues.Length; i++)
                {
                    var valueWithInitValue = CreateValueWithInitValue(strType, initValues[i], primitiveType);
                    instance.SetValue(valueWithInitValue,i);
                }
                

                return instance;

            }
            catch (Exception ex)
            {
                Log.Error($"创建类型{strType}失败：{ex}");
                return null;
            }
        }

        private static object CreateValueWithInitValue(string strType, string value, string type)
        {
            switch (type.ToLower())
            {
                case "bool":
                case "boolean":
                    return bool.Parse(value);

                case "string":
                    return value;
                case "int16":
                    return short.Parse(value);
                case "int32":
                case "int":
                    return int.Parse(value);
                case "int64":
                case "long":
                    return long.Parse(value);
                case "short":
                    return short.Parse(value);
                case "float":
                case "single":
                    return float.Parse(value);
                case "double":
                    return double.Parse(value);
                case "datetime": 
                    return DateTime.Parse(value);
                default:
                    throw new Exception($"类型：[{strType}]不能创建带有默认值的类型");
            }
        }

        /// <summary>
        /// 创建无初始值
        /// </summary>
        /// <param name="strType"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static object CreateValueWithoutInitValue(string strType, string type)
        {
            switch (type.ToLower())
            {
                case "bool":
                case "boolean":
                    return new bool();
                case "string":
                    return string.Empty;
                case "int16":
                    return new short();
                case "int32":
                case "int":
                    return new int();
                case "int64":
                case "long":
                    return new long();
                case "short":
                    return new short();
                case "float":
                case "single":
                    return new float();
                case "double":
                    return new double();
                case "datetime": 
                    return new DateTime();
                case "unknown":
                case "object": 
                    return new object();
                default:
                    return CustomizedType(strType);
            }
        }

        public static void LoadParameterFromConfig(XmlElement element, ParameterManager parameterManager)
        {
            if (element.HasAttribute("GenericType"))
            {
                var genericType = element.GetAttribute("GenericType").ToLower();

                switch (genericType)
                {
                    case "dictionary":
                        var dictionaryParameter = LoadDicParameterFromConfig(element);
                        parameterManager.AddDictionaryParam(dictionaryParameter);
                        break;

                    case "list":
                        var listParameter = LoadListParameterFromConfig(element);
                        parameterManager.AddListParam(listParameter);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(element.Name.ToLower());
                }
            }

            else
            {
                var basicParameter = LoadBasicParameterFromConfig(element);
                parameterManager.AddBasicParam(basicParameter);
            }
        }

        private static IListParameter LoadListParameterFromConfig(XmlElement element)
        {
            try
            {
                var strName = element.GetAttribute("Name");
                var strType = element.GetAttribute("Type");

                var listParameter =
                    (IListParameter) CreateParameter(strName, strType, typeof(ListParameter<>));

                var level2Node = (XmlNode) element;

                foreach (XmlElement xmlElement in level2Node)
                    if (string.Equals(xmlElement.Name, "Item",
                        StringComparison.CurrentCultureIgnoreCase))
                    {
                        var itemValue = xmlElement.GetAttribute("Value");
                        var basicValue = CreateValueFromString(strType, itemValue);

                        listParameter.Add(basicValue);
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException(
                            $"{xmlElement.Name}名称不合法，请检查配置手册，目前ProcessParameterArray仅支持ProcessParameterItem。");
                    }

                return listParameter;
            }
            catch (Exception ex)
            {
                Log.Error($"从XML里装载ParameterList参数失败:{ex.Message}");
                throw new InvalidOperationException(
                    $"从XML里装载ParameterList参数失败:{ex.Message}");
            }
        }

        private static IDictionaryParameter LoadDicParameterFromConfig(XmlElement element)
        {
            try
            {
                var strName = element.GetAttribute("Name");
                var strType = element.GetAttribute("Type");

                var instance =
                    (IDictionaryParameter) CreateParameter(strName, strType, typeof(DictionaryParameter<>));

                //var dictionaryParameter = new Dictionary<string, T>();

                var level2Node = (XmlNode) element;

                foreach (XmlElement xmlElement in level2Node)
                    if (string.Equals(xmlElement.Name, "Item",
                        StringComparison.CurrentCultureIgnoreCase))
                    {
                        var key = xmlElement.GetAttribute("Key");
                        var value = xmlElement.GetAttribute("Value");

                        var basicValue = CreateValueFromString(strType, value);

                        instance.Add(key, basicValue);
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException(
                            $"{xmlElement.Name}名称不合法，请检查配置手册，目前ProcessParameterArray仅支持ProcessParameterItem。");
                    }

                return instance;
            }
            catch (Exception ex)
            {
                Log.Error($"从XML里装载ParameterDictionary参数失败:{ex.Message}");
                throw new InvalidOperationException(
                    $"从XML里装载ParameterDictionary参数失败:{ex.Message}");
            }
        }
        
        /// <summary>
        ///     根据xml生成BasicParameter
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private static IBasicParameter LoadBasicParameterFromConfig(XmlElement element)
        {
            try
            {
                var strName = element.GetAttribute("Name");
                var strType = element.GetAttribute("Type");

                var parameter = (IBasicParameter) CreateParameter(strName, strType, typeof(BasicParameter<>));

                if (element.HasAttribute("DefaultValue"))
                {
                    //参数有默认值
                    // 默认值只可以是整数或字符串类型
                    var convertValueString =
                        CreateValueFromString(strType, element.GetAttribute("DefaultValue"));
                    parameter.SetValue(convertValueString);
                }
                /*else
                {
                    switch (strType.ToLower())
                    {
                        //参数无默认值
                        case "object" when element.HasAttribute("ComplexObjectType"):
                        {
                            // 复杂参数类型
                            var strComplexObjectType = element.GetAttribute("ComplexObjectType");
                            parameter = new BasicParameter(strName, strType, strComplexObjectType, element);

                            break;
                        }
                        case "customized" + "type" when element.HasAttribute("ComplexObjectType"):
                        {
                            // 定制化类型
                            var strComplexObjectType = element.GetAttribute("ComplexObjectType");
                            //ICustomizedType customizedType = CustomizedTypeManager.GetCustomizedType();
                            parameter = new BasicParameter(strName, "CustomizedType", strComplexObjectType, element);
                            break;
                        }
                    }
                }*/

                return parameter;
            }
            catch (Exception ex)
            {
                Log.Error($"从XML里装载ProcessParameter参数失败:{ex.Message}");
                throw new InvalidOperationException(
                    $"从XML里装载ProcessParameter参数失败:{ex.Message}");
            }
        }

        public static object CreateValueFromString(string strType, string strValue)
        {
            var type = ConvertValueType(strType);

            switch (type.ToLower())
            {
                case "bool":
                    return Convert.ToBoolean(strValue);
                case "boolean":
                    return Convert.ToBoolean(strValue);
                case "string":
                    return Convert.ToString(strValue);
                case "int16":
                    return Convert.ToInt16(strValue);
                case "int32":
                    return Convert.ToInt32(strValue);
                case "int":
                    return Convert.ToInt32(strValue);
                case "short":
                    return Convert.ToInt16(strValue);
                case "single":
                    return Convert.ToSingle(strValue);
                case "float":
                    return Convert.ToSingle(strValue);
                case "double":
                    return Convert.ToDouble(strValue);
                case "datetime": // add by Dongmin 20180218
                    return Convert.ToDateTime(strValue);
                default:
                    return CustomizedType(type);
            }
        }

        private static object CustomizedType(string type)
        {
            if (CustomTypeManager.HasType(type))
            {
                var type1 = CustomTypeManager.GetType(type);

                return Activator.CreateInstance(type1, default(string));
            }

            throw new ArgumentException($"不支持类型：[{type}]的转换。");
        }

        public static string ConvertValueType(string strLongType)
        {
            if (!strLongType.ToLower().Contains("system"))
                return strLongType;

            var dotPos = strLongType.LastIndexOf('.') + 1;
            return strLongType.Substring(dotPos, strLongType.Length - dotPos).Trim('[').Trim(']');
            //sunjian 2020/06/09 转换系统参数后需要裁剪掉方括号‘[’，‘]’；
        }
        
        public abstract void Clear();

        public abstract string GetTypeString();

    }
}