using System;
using System.Xml;
using System.Reflection;
//using ProcessControlService.Models;

namespace ProcessControlService.ResourceLibrary.Machines
{
    /// <summary>
    /// 报警抽象类
    /// </summary>
    public abstract class AlarmDefinition 
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(AlarmDefinition));

        protected string _alarmID;
        protected string _alarmGroup;
        protected string _alarmMessage;

        protected Machine _owner;

        public AlarmDefinition(Machine machine)
        {
            _owner = machine;
        }

        public string AlarmID
        {
            get { return _alarmID; }
        }

        public string Message
        {
            get { return _alarmMessage; }
        }

        public string Group
        {
            get { return _alarmGroup; }
        }


        public abstract bool LoadFromConfig(XmlNode node);

        public abstract AlarmSignalStatus UpdateStatus();

        // 装载数据源 Dongmin 20171230
        //<Alarm AlarmID="1" Type="Tag" TagName="Signal1" TrigTagValue="true" AlarmGroup="报警组1" AlarmMessage="传感器报警1"/>
        public static AlarmDefinition CreateFromConfig(XmlNode node, Machine machine)
        {
            // level --  "DataSource"
            if (node.NodeType == XmlNodeType.Comment)
                return null;

            try
            {
                XmlElement level1_item = (XmlElement)node;

                string strADType = level1_item.GetAttribute("Type");
                string strADName = level1_item.GetAttribute("Name");

                ///////////////////////////////////////
                // 使用反射创建数据源
                object obj = null;

                string AppPath = Assembly.GetExecutingAssembly().GetName().Name;
                string FullDSType = AppPath + ".Machines." + strADType;
                Type objType = Type.GetType(FullDSType, true);
                obj = Activator.CreateInstance(objType, new object[] { machine });
                ////////////////////////////////////////

                return (AlarmDefinition)obj;
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("加载报警{0}出错：{1}", node.Name, ex.Message));
                return null;
            }

        }
    }

    public enum AlarmSignalStatus
    {
        Untrigged = (short)0,
        Trigged = (short)1,
        Unknown = (short)2
    }

}
