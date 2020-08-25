// ==================================================
// 文件名：Tag.cs
// 创建时间：2020/06/11 20:19
// ==================================================
// 最后修改于：2020/06/11 20:19
// 修改人：jians
// ==================================================

using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml;
using log4net;

namespace ProcessControlService.ResourceLibrary.Machines
{
    public class Tag : ICloneable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Tag));

        public readonly Machine Owner;
        private float _convertBase;

        private float _convertRatio = 1.0f;

        private string _tagType;

        private object _tagValue;

        public string Address;

        public Tag(Machine owner)
        {
            Owner = owner;
        }

        public Tag(string name, Machine owner)
        {
            TagName = name;
            Owner = owner;
        }

        public Tag(string name, Machine owner, TagAccessType accessType)
        {
            TagName = name;
            Owner = owner;
            AccessType = accessType;
        }

        // variables
        public string TagName { get; set; }
        public Quality Quality { get; set; } = Quality.Unknown;

        public TagAccessType AccessType { get; private set; } = TagAccessType.ReadWrite;

        public Action<Tag> ValueChangedHander { get; set; }

        public string TagType
        {
            get => _tagType;

            set => _tagType = value;// CreateValue(value); //在该方法中对类型赋值
        }

        public object TagValue
        {
            get
            {
                // convert Tag accord convert ratio - David 20170616
                //if (Math.Abs(_convertRatio - 1.0f) < 0.0000000001) return _tagValue;
                //switch (_tagType)
                //{
                //    case "int16":
                //        return (short)((short)_tagValue * _convertRatio + _convertBase);
                //    case "uint16":
                //        return (ushort)((ushort)_tagValue * _convertRatio + _convertBase);
                //    case "int32":
                //        return (int)((int)_tagValue * _convertRatio + _convertBase);
                //    case "uint32":
                //        return (uint)((uint)_tagValue * _convertRatio + _convertBase);
                //    case "int64":
                //        return (long)((long)_tagValue * _convertRatio + _convertBase);
                //    case "uint64":
                //        return (ulong)((ulong)_tagValue * _convertRatio + _convertBase);
                //}

                //如果不需要转换
                return _tagValue;
            }
            set
            {
                LastUpdateTime = DateTime.Now;

                if ((_tagValue == null && value != null) ||
                    (_tagValue != null && (_tagValue is IComparable old) && (value is IComparable nval) && old.CompareTo(nval) != 0))
                {
                    _tagValue = value;
                    if (ValueChangedHander != null)
                    {
                        foreach (Action<Tag> Invocation in ValueChangedHander.GetInvocationList())
                        {
                            Task.Run(delegate { Invocation(this); });
                        }
                    }
                }
            }
        }

        public object Clone()
        {
            var newTag = new Tag(TagName, Owner)
            {
                TagType = TagType,
                TagValue = TagValue
            };
            //newTag.Owner = this.Owner;
            //newTag.TagName = this.TagName;

            return newTag;
        }

        public Tag()
        {
            
        }

        private object CreateValue(string strType)
        {
            switch (strType.ToLower())
            {
                case "bool":
                    _tagType = strType.ToLower();
                    return new bool();
                case "byte":
                    _tagType = strType.ToLower();
                    return new byte();
                case "sbyte":
                    _tagType = strType.ToLower();
                    return new sbyte();
                case "string":
                    _tagType = strType.ToLower();
                    return string.Empty;
                case "bytearray":
                    _tagType = strType.ToLower();
                    return string.Empty;
                case "uint16":
                case "ushort":
                    _tagType = "uint16";
                    return new ushort();
                case "short":
                case "int16":
                    _tagType = "int16";
                    return new short();
                case "int32":
                case "int":
                    _tagType = "int32";
                    return new int();
                case "uint32":
                case "uint":
                    _tagType = "uint32";
                    return new uint();
                case "long":
                case "int64":
                    _tagType = "int64";
                    return new long();
                case "ulong":
                case "uint64":
                    _tagType = "uint64";
                    return new ulong();
                case "float":
                case "single":
                    _tagType = "float";
                    return new float();
                case "double":
                    _tagType = "double";
                    return new double();
                default:
                    throw new Exception("不支持创建此类型");
            }
        }

        private object CopyValue(object value)
        {
            try
            {
                if (value != null)
                    switch (_tagType)
                    {
                        case "bool":
                            return Convert.ToBoolean(value);
                        case "byte":
                            return Convert.ToByte(value);
                        case "sbyte":
                            return Convert.ToSByte(value);
                        case "string":
                        case "bytearray":
                            return value.ToString();
                        case "int16":
                            return Convert.ToInt16(value);
                        case "uint16":
                            return Convert.ToUInt16(value);
                        case "int32":
                            return Convert.ToInt32(value);
                        case "uint32":
                            return Convert.ToUInt32(value);
                        case "int64":
                            return Convert.ToInt64(value);
                        case "uint64":
                            return Convert.ToUInt64(value);
                        case "float":
                            return Convert.ToSingle(value);
                        case "double":
                            return Convert.ToDouble(value);
                        default:
                            throw new Exception("不支持创建此类型");
                    }

                return null;
            }
            catch (Exception ex)
            {
                Log.Error($"复制出错：{ex},Tag:{TagName},Address:{Address}");
                return null;
            }
        }

        public void LoadFromConfig(XmlNode node)
        {
            try
            {
                var level1Item = (XmlElement)node;
                TagName = level1Item.GetAttribute("Name");
                if (Owner.ListTagNames().Contains(TagName))
                {
                    Log.Error($"装载Tag:{TagName}出错:TagName重复");
                    return;
                }

                if (level1Item.HasAttribute("Address")) Address = level1Item.GetAttribute("Address");

                if (level1Item.HasChildNodes && level1Item.FirstChild.Name == "Address")
                {
                    Address = level1Item.FirstChild.InnerText;
                }

                //TagType = (VarEnum)Enum.Parse(typeof(VarEnum), level1_item.GetAttribute("TagType"));
                TagType = level1Item.GetAttribute("TagType").ToLower();

                if (level1Item.HasAttribute("DefaultValue"))
                {
                    var strDefaultValue = level1Item.GetAttribute("DefaultValue");
                    _tagValue = strDefaultValue == "" ? null : TranslateValueFromString(strDefaultValue);
                }
                //_count = Convert.ToInt16(tag.GetAttribute("Count"));

                if (level1Item.HasAttribute("AccessType"))
                {
                    var strAccessType = level1Item.GetAttribute("AccessType");
                    if (strAccessType.ToLower() == "read")
                        AccessType = TagAccessType.Read;
                    else if (strAccessType.ToLower() == "write")
                        AccessType = TagAccessType.Write;
                    else
                        AccessType = TagAccessType.ReadWrite;
                }

                if (level1Item.HasAttribute("ConvertRatio"))
                {
                    var strConvertRatio = level1Item.GetAttribute("ConvertRatio");
                    _convertRatio = Convert.ToSingle(strConvertRatio);
                }

                if (level1Item.HasAttribute("ConvertBase"))
                {
                    var strConvertBase = level1Item.GetAttribute("ConvertBase");
                    _convertBase = Convert.ToSingle(strConvertBase);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"装载Tag:{TagName}出错:{ex.Message}");
            }
        }

        public object TranslateValueFromString(string strValue)
        {
            try
            {
                switch (TagType)
                {
                    case "bool":
                        switch (strValue)
                        {
                            case "0":
                                return false;
                            case "1":
                                return true;
                            default:
                                return Convert.ToBoolean(strValue);
                        }

                    case "string":
                    case "bytearray":
                        return strValue;
                    case "byte":
                        return Convert.ToByte(strValue);
                    case "sbyte":
                        return Convert.ToSByte(strValue);
                    case "int16":
                        return Convert.ToInt16(strValue);
                    case "uint16":
                        return Convert.ToUInt16(strValue);
                    case "int32":
                        return Convert.ToInt32(strValue);
                    case "uint32":
                        return Convert.ToUInt32(strValue);
                    case "int64":
                        return Convert.ToInt64(strValue);
                    case "uint64":
                        return Convert.ToUInt64(strValue);
                    case "float":
                        return Convert.ToSingle(strValue);
                    case "double":
                        return Convert.ToDouble(strValue);
                    default:
                        throw new Exception("不支持转换此类型");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"持转换此数据{strValue}出错：{ex}");
                return null;
            }
        }

        private string TranslateValueToString(object objValue)
        {
            try
            {
                switch (TagType)
                {
                    case "string":
                    case "bytearray":
                        return (string)objValue;

                    default:
                        return Convert.ToString(objValue);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"持转换此数据{objValue}出错：{ex}");
                return null;
            }
        }

        public static bool ValueEqual(object v1, object v2)
        {
            if (v1 == null || v2 == null)
                return false;

            return v1.ToString().Equals(v2.ToString());
        }

        public static bool ValueEqual(Tag tag1, Tag tag2)
        {
            return ValueEqual(tag1.TagValue, tag2.TagValue);
        }

        public static bool ValueEqual(Tag tag1, object defaultValue)
        {
            try
            {
                if (tag1?.TagValue == null || defaultValue == null) return false;

                if (tag1.TagType == "bool")
                {
                    //bool t1 = (Convert.ToBoolean(tag1.TagValue));
                    var t1 = (bool)tag1.TagValue;
                    //if (value is bool)
                    //{
                    //修改  夏  2017年6月1日10:52:56   类型转换时出错，不能强转
                    if (defaultValue.GetType().Name.ToLower().Equals("boolean"))
                        return ValueEqual(tag1._tagValue, defaultValue);

                    if (defaultValue.GetType().Name.ToLower().Equals("string"))
                    {
                        //bool t2 = ((int)defuValue != 0);
                        var t2 = Convert.ToBoolean(defaultValue);
                        return t1 == t2;
                    }

                    throw new Exception("不支持检查此类型的值");
                }

                return ValueEqual(tag1.TagValue, defaultValue);
            }
            catch (Exception ex)
            {
                Log.Error($"检查tag出错：{ex}");
                return false;
            }
        }

        public bool Write(object value)
        {
            if (AccessType == TagAccessType.ReadWrite || AccessType == TagAccessType.Write)
            {
                if (Owner != null) return Owner.WriteTag(this, value); //此处写操作已把值写到设备中
                //_TagValue = value;  内存型Tag只读型在此处也会被写成功。

                //_valueWrited = true;
                Log.Error(string.Format(TagName + "所属设备为空，写入失败"));
                return false;
            }

            Log.Error(string.Format(TagName + "不允许写入"));
            return false;
        }

        // add by David 20170528
        public bool WriteValueInString(string valueInString)
        {
            //Stopwatch sw=new Stopwatch();
            var tempValue = TranslateValueFromString(valueInString);
            return tempValue != null && Write(tempValue);
        }

        // add by David 20190807
        public string GetValueInString()
        {
            return TranslateValueToString(_tagValue);
        }

        private DateTime _lastUpdatetime;
        [DataMember]
        public DateTime LastUpdateTime
        {
            get => _lastUpdatetime;
            set
            {
                if (_lastUpdatetime == new DateTime())
                {
                    UpdateInterval = 1;
                }
                else
                {
                    UpdateInterval = (value - _lastUpdatetime).TotalMilliseconds;
                }
                _lastUpdatetime = value;
            }
        }

        [DataMember]
        public double UpdateInterval { get; set; }
    }

    public enum TagAccessType
    {
        Read = 0,
        Write = 1,
        ReadWrite = 2
    }

    public enum Quality
    {
        Good = 0,
        Bad = 1,
        Unknown = 2
    }

    [DataContract]
    public class TagModel
    {
        [DataMember] public string TagName;

        [DataMember] public string TagValue;

        public TagModel(Tag tag)
        {
            TagName = tag.TagName;
            TagValue = tag.GetValueInString();
        }
    }
}