using System;
using System.Collections.Generic;
using System.Xml;

namespace ProcessControlService.ResourceFactory
{
    public class Parameter : IParameterizedValue
    {
        private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(Parameter));

        #region interface
        public string ParameterName {
            get { return _name; }
        }

        public object ParameterValue {
            get { return GetValue(); }
            set
            {
                SetValue(value);
            }
        }


        public string ToJson()
        {
            throw new NotImplementedException();
        }
        #endregion

        private string _name;
        public string Name
        {
            get { return _name; }
        }

        //private object _value;
        private bool _hasValue = false;
        public bool HasValue
        {
            get { return _hasValue; }
        }

        private List<BasicValue> _processParameterList=new List<BasicValue>(); 
        private Dictionary<string,BasicValue> _processParameterDictionary=new Dictionary<string, BasicValue>();
        

        //private IParameterizedValue _valueObject; //参数化值 dongmin 20180219
        private BasicValue _basicValueObject; //simple value
        private object _complexValueObject; // complex value


        private string _strType;

        //private string _strComplexObjectType; //如果参数是复杂对象，此变量记录类的名称

        //private IParameterizedValue _defaultValueObject;
        private BasicValue _basicDefaultValueObject; //simple value
        private object _complexDefaultValueObject; // complex value

        public bool IsBasicValue
        { get; set; }

        public Parameter(string parameterName, string parameterType, string strComplexTypeName = null, object complexParameter=null) 
        {
            _name = parameterName;
            _strType = parameterType;

            //string strComplexTypeName1 = strComplexTypeName; //20180428注释Robin

            switch (parameterType.ToLower())
            {
                case "object":
                    // 参数为对象类型
                    _complexValueObject = CreateComplexObject(strComplexTypeName);
                    _complexDefaultValueObject = CreateComplexObject(strComplexTypeName);

                    IsBasicValue = false;
                    break;
                //sunjian 2019-12-23 增加Process Parameter Array的设定
                case "list":
                    _processParameterList = complexParameter as List<BasicValue>;

                    IsBasicValue = false;
                    break;
                //sunjian 2019-12-23 增加Process Parameter Array的设定
                case "dictionary":
                    _processParameterDictionary = complexParameter as Dictionary<string,BasicValue>;

                    IsBasicValue = false;
                    break;
                default:
                    // 参数为基本类型
                    _basicValueObject = new BasicValue(parameterType);   //dongmin 新增
                    _basicDefaultValueObject = new BasicValue(parameterType);

                    IsBasicValue = true;
                    break;
            }
        }


        private object CreateComplexObject(string complexObjectType)
        {
            try
            {
                const string packageName = "ProcessControlService.ResourceLibrary"; // 目前只支持在基础包里搜寻对象 Dongmin 20180219
                //strObjectType = "ProcessControlService.ResourceLibrary.Tracking.TrackingUnit"; // test only
                var strObjectType = packageName + "." + complexObjectType;
                var findingPath = strObjectType + "," + packageName;
                var newObjectType = Type.GetType(findingPath, true);

                //object NewObject = Activator.CreateInstance(NewObjectType, new object());
                var newObject = Activator.CreateInstance(newObjectType);
                return newObject;

            }
            catch (Exception ex)
            {
                LOG.Error($"创建复杂对象{complexObjectType}出错：{ex}");
                return null;
            }
        }

        public static Parameter CreateFromConfig(XmlNode node)
        {
            try
            {
                Parameter parameter;

                var level1Item = (XmlElement)node;


                switch (level1Item.Name.ToLower())
                {
                    case "process"+"parameter"+"array":

                        parameter = CreateProcessParameterArray(level1Item);

                        break;

                    case "process"+"parameter"+"dictionary":

                        parameter = CreateProcessParameterDictionary(level1Item);

                        break;


                    case "process"+"parameter":/*sunjian 2019-12-23 兼容之前版本，非ProcessParameterArray就使用之前的代码。*/
                        
                       

                    default:
                        parameter = CreateProcessParameter(level1Item);
                        break;
                        //throw new ArgumentOutOfRangeException($"ProcessParameters不存在{level1Item.Name}这样的配置名，请检查xml文本配置");
                        
                }

                return parameter;
            }
            catch (Exception ex)
            {
                LOG.Error($"从XML里装载参数失败:{ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 创建字典类过程参数
        /// </summary>
        /// <param name="level1Item"></param>
        /// <returns></returns>
        /// <remarks> sunjian added 2020-1-2 </remarks>
        private static Parameter CreateProcessParameterDictionary(XmlElement level1Item)
        {
            var strName = level1Item.GetAttribute("Name");
            var strType = level1Item.GetAttribute("Type");

            var dictionaryParameter = new Dictionary<string,BasicValue>();

            var level2Node = (XmlNode)level1Item;

            foreach (XmlElement xmlElement in level2Node)
            {
                if (string.Equals(xmlElement.Name, "ParameterKeyValuePair", StringComparison.CurrentCultureIgnoreCase))
                {
                    var key= xmlElement.GetAttribute("Key");
                    var value = xmlElement.GetAttribute("Value");
                    var basicValue = new BasicValue(strType, value);
                    dictionaryParameter.Add(key,basicValue);
                }
                else
                {
                    throw new ArgumentOutOfRangeException(
                        $"{xmlElement.Name}名称不合法，请检查配置手册，目前ProcessParameterArray仅支持ProcessParameterItem。");
                }
            }

            var parameter = new Parameter(strName, "Dictionary"/*Parameter的类型为list，但是list内参数的类型由Type中读取到的字符串来定义。*/, null,dictionaryParameter);

            return parameter;
        }

        /// <summary>
        /// 创建Array类型的ProcessParameter，目前仅支持BasicValue类
        /// </summary>
        /// <param name="level1Item"></param>
        /// <returns></returns>
        /// sunjian 2019-12-23
        private static Parameter CreateProcessParameterArray(XmlElement level1Item)
        {
            var strName = level1Item.GetAttribute("Name");
            var strType = level1Item.GetAttribute("Type");

            var listParameter=new List<BasicValue>();

            var level2Node = (XmlNode) level1Item;

            foreach (XmlElement xmlElement in level2Node)
            {
                if (string.Equals(xmlElement.Name, "ProcessParameterItem",StringComparison.CurrentCultureIgnoreCase) )
                {
                    var itemValue = xmlElement.GetAttribute("Value");
                    var basicValue=new BasicValue(strType,itemValue);
                    listParameter.Add(basicValue);
                }
                else
                {
                    throw new ArgumentOutOfRangeException(
                        $"{xmlElement.Name}名称不合法，请检查配置手册，目前ProcessParameterArray仅支持ProcessParameterItem。");
                }
            }

            var parameter = new Parameter(strName,"List"/*Parameter的类型为list，但是list内参数的类型由Type中读取到的字符串来定义。*/,null,listParameter);

            return parameter;
        }

        private static Parameter CreateProcessParameter(XmlElement level1Item)
        {
            Parameter parameter;
            var strName = level1Item.GetAttribute("Name");
            var strType = level1Item.GetAttribute("Type");
            if (level1Item.HasAttribute("DefaultValue"))
            {
                //参数有默认值
                // 默认值只可以是整数或字符串类型
                var defaultValue = BasicValue.ConvertValueString(strType, level1Item.GetAttribute("DefaultValue"));
                parameter = new Parameter(strName, strType);
                parameter.SetDefaultValue(defaultValue);
            }
            else
            {
                switch (strType.ToLower())
                {
                    //参数无默认值
                    case "object" when level1Item.HasAttribute("ComplexObjectType"):
                    {
                        // 复杂参数类型
                        var strComplexObjectType = level1Item.GetAttribute("ComplexObjectType");
                        parameter = new Parameter(strName, strType, strComplexObjectType);
                        break;
                    }
                    case "customized" + "type" when level1Item.HasAttribute("ComplexObjectType"):
                    {
                        // 定制化类型
                        var strComplexObjectType = level1Item.GetAttribute("ComplexObjectType");
                        //ICustomizedType customizedType = CustomizedTypeManager.GetCustomizedType();
                        parameter = new Parameter(strName, "CustomizedType", strComplexObjectType);
                        break;
                    }
                    default: // 简单参数类型
                        parameter = new Parameter(strName, strType);
                        break;
                }
            }

            return parameter;
        }

        public void SetValue(object value)
        {
            //sunjian 2019-01-02
            switch (value)
            {
                case List<BasicValue> parameterArray:
                    _processParameterList = parameterArray;

                    return;
                case Dictionary<string, BasicValue> dicKeyValue:
                    _processParameterDictionary = dicKeyValue;
                    break;
            }


            if (value != null)
            {
                _hasValue = true;
                 if (IsBasicValue)
                {
                    _basicValueObject.SetValue(value);
                }
                else
                {
                    _complexValueObject = value;
                }
            }
            else
            { // 清除
                _hasValue = false;

                if (IsBasicValue)
                {
                    _basicValueObject = null;
                }
                else
                {
                    _complexValueObject = null;
                }
            }
            
        }

        public void SetValueInString(string strValue)
        {
            SetValue(BasicValue.ConvertValueString(_strType, strValue));
        }

        public void SetDefaultValue(object value)
        {

            if (value != null)
            {
                if (IsBasicValue)
                {
                    _basicDefaultValueObject.SetValue(value);
                }
                else
                {
                    _complexDefaultValueObject = value;
                }
            }
            else
            { // 清除

                if (IsBasicValue)
                {
                    _basicDefaultValueObject = null;
                }
                else
                {
                    _complexDefaultValueObject = null;
                }
            }
        }

        public void SetDefaultValueInString(string strValue)
        {
            SetDefaultValue(BasicValue.ConvertValueString(_strType, strValue));
        }

        public object GetValue()
        {
            switch (_strType.ToLower())
            {
                case "list":
                    return _processParameterList.Count > 0 ? _processParameterList : null;
                case "dictionary":
                    return _processParameterDictionary.Count > 0 ? _processParameterDictionary : null;
            }


            if (_hasValue)
            {
                return IsBasicValue ? _basicValueObject.GetValue() : _complexValueObject;
            }

            return GetDefaultValue();
        }

        /*/// <summary>
        /// 提供ProcessParameterArray, 目前仅支持基本类型的list
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// sunjian 2019-12-23
        /// todo: 与其他类型的值的Parameter兼容,看后期list类型的Parameter使用广布广泛
        public List<BasicValue> GetParameterArray()
        {
            return _processParameterList.Count>0 ? _processParameterList : null;
        }*/

        //public BasicValue GetBasicValue()
        //{
        //    if (_hasValue)
        //    {
        //        if (IsBasicValue)
        //            return _basicValueObject;
        //        else
        //            return null;
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        //public void SetBasicValue(BasicValue bValue)
        //{
        //    if (IsBasicValue)
        //    {
        //        _basicValueObject = bValue.Clone();
        //        _hasValue = true;
        //    }

        //}

        public string GetValueInString()
        {
            object resultvalue = GetValue();

            if (resultvalue == null)
                return string.Empty;

            return resultvalue.ToString();
        }

        public object GetDefaultValue()
        {
            if (IsBasicValue)
                return _basicDefaultValueObject.GetValue();
            else
                return _complexDefaultValueObject;
        }

        public string GetDefaultValueInString()
        {
            object resultvalue = GetDefaultValue();

            if (resultvalue == null)
                return string.Empty;

            return resultvalue.ToString();

        }

        public new Type GetType()
        {
            if (IsBasicValue)
                return _basicValueObject.GetType();
            else
                return _complexDefaultValueObject.GetType();
        }

        public string GetTypeString()
        {
            return _strType;
        }

        public void Clear()
        {
            SetValue(null);
        }
    }

}
