using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ProcessControlService.ResourceLibrary.Machines;

//using ProcessControlService.Models;

namespace ProcessControlService.ResourceLibrary.Machines
{
    
    /// <summary>
    /// 点标签报警 Dongmin 20170803
    /// XML配置例子
    /// <Alarms>
	///	    <Alarm AlarmID="1" Type="Tag" TagName="Signal1" TrigTagValue="true" AlarmGroup="报警组1" AlarmMessage="传感器报警1"/>
	///	    <Alarm AlarmID="2" Type="Tag" TagName="Level1" TrigType="High" TrigTagValue="5.0" AlarmGroup="报警组2" AlarmMessage="液位报警1"/>
	/// </Alarms>
    /// </summary>
    public class TagAlarmDefinition : AlarmDefinition
    {
        private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(TagAlarmDefinition));
        private Tag _alarmTag=null;
        private object _alarmTagTrigValue;
        private TrigType _alarmType;

        public enum TrigType
        {
            None = (short)0,
            Equal = (short)1,    // DongMin 20170803
            //TagOff = (short)2,    // DongMin 20170803
            High = (short)2,     // DongMin 20170803
            Low = (short)3       // DongMin 20170803
        }

        public enum AlarmCompareResult
        { 
            Equal = 0,
            GreatThan = 1,
            LessThan = 2,
            Unknown = 3
        }

        public TagAlarmDefinition(Machine machine) : base(machine)
        {
        }

        //static int alarmid = 3001;
        public override bool LoadFromConfig(XmlNode node)
        {
            string TagName=string.Empty;
            try
            {
                XmlElement level1_item = (XmlElement)node;

                if (!level1_item.HasAttribute("AlarmID"))   //检测是否有报警ALARM ID David 20170611
                {
                    throw new Exception("未找到报警ID");
                    
                }
                _alarmID = level1_item.GetAttribute("AlarmID");  //唯一代表报警
                TagName = level1_item.GetAttribute("TagName");
                // alarm type - David 20170803

                if (level1_item.HasAttribute("TrigType"))
                {
                    string strAlarmType = level1_item.GetAttribute("TrigType");
                    if (strAlarmType.ToLower() == "equal")
                    {
                        _alarmType = TrigType.Equal;
                    }
                    else if (strAlarmType.ToLower() == "high")
                    {
                        _alarmType = TrigType.High;
                       
                        

                    }
                    else if (strAlarmType.ToLower() == "low")
                    {
                        _alarmType = TrigType.Low;
                    }
                    else
                    {
                        _alarmType = TrigType.None;
                    }

                }
                else
                { //默认为相等
                    _alarmType = TrigType.Equal;
                }
                // load MonitorTags and Alarms
                
                _alarmTag = _owner.GetTag(TagName);

                string strAlarmTagTrigValue = level1_item.GetAttribute("TrigTagValue");
                _alarmTagTrigValue = _alarmTag.TranslateValueFromString(strAlarmTagTrigValue);
                //if ()

                _alarmGroup = level1_item.GetAttribute("AlarmGroup");
                _alarmMessage = level1_item.GetAttribute("AlarmMessage");


                return true;
            }
            catch (Exception ex)
            {
                LOG.Error(string.Format("装载报警{0}失败:{1}", TagName, ex.Message));
                return false;
            }
            
        }
       
        public override AlarmSignalStatus UpdateStatus()
        {
            try
            {
                AlarmCompareResult compareResult = CompareAlarmTagValue(_alarmTag.TagValue, _alarmTagTrigValue, _alarmTag.TagType);
                //if (_alarmTag.TagName.Contains("excode"))
                //{
                //    int a = 2;            Robin注释于20180203
                //}
                if (_alarmType == TrigType.Equal)
                {
                    //if (Tag.ValueEqual(_alarmTag, _alarmTagTrigValue))
                    //{

                    //    return AlarmSignalStatus.Triggered;
                    //}
                    //else
                    //    return AlarmSignalStatus.Nottriggered;
                    if (compareResult == AlarmCompareResult.Equal)
                        return AlarmSignalStatus.Trigged;
                    else
                        return AlarmSignalStatus.Untrigged;
                }
                else if (_alarmType == TrigType.High)
                {

                    if (compareResult == AlarmCompareResult.GreatThan)
                        return AlarmSignalStatus.Trigged;
                    else
                        return AlarmSignalStatus.Untrigged;
                }
                else if (_alarmType == TrigType.Low)
                {
                    if (compareResult == AlarmCompareResult.LessThan)
                        return AlarmSignalStatus.Trigged;
                    else
                        return AlarmSignalStatus.Untrigged;
                }
                else
                    return AlarmSignalStatus.Unknown;
            }
            catch
            {
                return AlarmSignalStatus.Unknown;
            }
        }

        // 比较报警设定值和标签值 0 - 相等； 1 - 大于； 2 - 小于
        private AlarmCompareResult CompareAlarmTagValue(object TagValue, object AlarmValue, string ValueType)
        {
            switch (ValueType)
            { 
                case "bool":
                    if ((bool)TagValue == (bool)AlarmValue)
                    {
                        return AlarmCompareResult.Equal;
                    }
                    else
                    {
                        return AlarmCompareResult.Unknown;
                    }
                case "int16":
                    if ((Int16)TagValue == (Int16)AlarmValue)
                    {
                        return AlarmCompareResult.Equal;
                    }
                    else if ((Int16)TagValue > (Int16)AlarmValue)
                    {
                        return AlarmCompareResult.GreatThan;
                    }
                    else
                    {
                        return AlarmCompareResult.LessThan;
                    }
                case "uint16":
                    if ((UInt16)TagValue == (UInt16)AlarmValue)
                    {
                        return AlarmCompareResult.Equal;
                    }
                    else if ((UInt16)TagValue > (UInt16)AlarmValue)
                    {
                        return AlarmCompareResult.GreatThan;
                    }
                    else 
                    {
                        return AlarmCompareResult.LessThan;
                    }

                case "int32":
                    if ((Int32)TagValue == (Int32)AlarmValue)
                    {
                        return AlarmCompareResult.Equal;
                    }
                    else if ((Int32)TagValue > (Int32)AlarmValue)
                    {
                        return AlarmCompareResult.GreatThan;
                    }
                    else 
                    {
                        return AlarmCompareResult.LessThan;
                    }

                case "float":
                    if ((float)TagValue == (float)AlarmValue)
                    {
                        return AlarmCompareResult.Equal;
                    }
                    else if ((float)TagValue > (float)AlarmValue)
                    {
                        return AlarmCompareResult.GreatThan;
                    }
                    else 
                    {
                        return AlarmCompareResult.LessThan;
                    }
                case "string":
                    if((string)TagValue==(string)AlarmValue)
                        return AlarmCompareResult.Equal;
                    else
                        return AlarmCompareResult.Unknown;
                default:
                    throw new Exception("不支持比较此类型");
            }

           
        }
     
    }

}
