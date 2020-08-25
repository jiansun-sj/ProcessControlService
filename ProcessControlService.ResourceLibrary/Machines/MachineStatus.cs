using System;
using System.Xml;
using ProcessControlService.ResourceLibrary.Machines;

namespace ProcessControlService.ResourceLibrary.Machines
{
    public class MachineStatus
    {
        private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(MachineStatus));

        private string _name;
        public string Name
        {
            get { return _name; }
        }

        private object _value;
        private bool _hasValue = false;

        private string _typeString;

        private object _defaultValue
        {
            get;
            set;
        }

        private Tag _linkTag;

        //private Machine _ownerMachine;

        public MachineStatus(string StatusName, string StatusType, Tag LinkTag, object DefaultValue = null)
        {
            _name = StatusName;

            _typeString = StatusType;

            _value = CreateValue(StatusType);

            if (_defaultValue == null)
            {
                _defaultValue = CreateValue(StatusType);
            }
            else
            {
                _defaultValue = DefaultValue;
            }

            //_ownerMachine = LinkMachine;
            _linkTag = LinkTag;                         
        }

        private object CreateValue(string strType)
        {
            //alter by gu 20170520
            _typeString = strType;
            switch (strType.ToLower())
            {
                case "bool":
                case "boolean":
                    return new bool();
                    //break;
                case "string":
                    return string.Empty;
                    //break;
                case "int32":
                    return new int();
                    //break;
                case "short":
                    return new short();
                    //break;
                case "float":
                    return new float();
                    //break;
                default:
                    throw new Exception("不支持创建此类型");
            }
        }
        //private VarEnum strType;

        //private object CreateValue(VarEnum strType)
        //{

        //    switch (strType)
        //    {
        //        case VarEnum.VT_BOOL:
        //            return new bool();
        //        case VarEnum.VT_LPSTR:
        //            return string.Empty;
        //        case VarEnum.VT_INT:
        //            return new Int32();
        //        case VarEnum.VT_I2:
        //            return new short();
        //        default:
        //            throw new Message("不支持创建此类型");
        //    }
        //}

        private object CopyValue(object Value)
        {
            try
            {            
                switch (_typeString.ToLower())
                {
                    case "bool":
                        bool obj = (bool)Value;
                        return obj;
                    case "string":
                        string strValue = (string)Value;
                        return strValue;
                    case "int32":
                        int intValue = (int)Value;
                        return intValue;
                    case "short":
                        short shtValue = (short)Value;
                        return shtValue;
                    default:
                        throw new Exception("不支持复制此类型");
                }
            }
            catch (Exception ex)
            {
                LOG.Error(string.Format("复制出错：{0}", ex));
                return null;
            }
            //return Value;
        }

        public static MachineStatus LoadFromConfig(XmlNode node,Machines.Machine machine)
        {
            try
            {
                MachineStatus status;

                XmlElement level1_item = (XmlElement)node;

                string strName = level1_item.GetAttribute("Name");
                string strType = level1_item.GetAttribute("Type");
                string strTag = level1_item.GetAttribute("Tag");

                object defaultValue = null;
                if (level1_item.HasAttribute("DefaultValue"))
                { //参数有默认值
                    // 默认值只可以是整数或字符串类型
                    if (strType.ToLower() == "string")
                    {
                        defaultValue = level1_item.GetAttribute("DefaultValue");
                    }
                    else if (strType.ToLower() == "int32")
                    {
                        defaultValue = Convert.ToInt32(level1_item.GetAttribute("DefaultValue"));
                    }
                    else
                    {
                        throw new Exception(string.Format("不支持机器状态{0}默认为此类型{1}", strName, strType));
                    }

                    Tag tag = machine.GetTag(strTag);
                    status = new MachineStatus(strName, strType, tag, defaultValue);

                }
                else
                {//参数无默认值
                    Tag tag = machine.GetTag(strTag);
                    status = new MachineStatus(strName, strType,tag);
                }

                return status;
            }
            catch (Exception ex)
            {
                LOG.Error(string.Format("从XML里装载参数失败:{0}",ex.Message));
                return null;
            }
        }

        public void SetValue(object Value)
        {
            // 类型检查
            try
            {
                this._value = CopyValue(Value);
                _hasValue = true;
            }
            catch (Exception ex)
            {
                LOG.Error(string.Format("设置参数{0}出错"+ex.Message ,Value.ToString()));
            }
        }

        public object GetValue()
        {
            if (_hasValue)
                return _value;
            else
                return _defaultValue;
        }

        public string GetValueString()
        {
            //alter by gu 20170520
            switch (_typeString.ToLower())
            {
                case "bool":
                    return _value.ToString();
                case "string":
                    return _value.ToString();
                case "int16":
                    return _value.ToString();
                case "int32":
                    return  _value.ToString();
                case "short":
                    return _value.ToString();
                case "float":
                    return _value.ToString();


                default:
                    throw new Exception("不支持创建此类型");
            }
        }

        new public System.Type GetType()
        {
            return _value.GetType();
        }

        public void UpdateData()
        {
            if(_linkTag!=null&&_linkTag.TagValue!=null)
                _value = _linkTag.TagValue;
        }
    }
   
}
