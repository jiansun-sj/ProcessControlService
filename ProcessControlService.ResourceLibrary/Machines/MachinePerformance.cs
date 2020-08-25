using System;
using System.Collections.Generic;
using System.Xml;

namespace ProcessControlService.ResourceLibrary.Machines
{
    /// <summary>
    /// 机器绩效OEE
    /// Created by David Dong 20170725
    /// </summary>
    public class MachinePerformance
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(MachinePerformance));

        private readonly Machine _owner;

        //private MachineStatusType _status;

        private readonly Dictionary<MachineStatusType, Tag> _statusTags = new Dictionary<MachineStatusType, Tag>();

        public MachinePerformance(Machines.Machine ownerMachine)
        {
            _owner = ownerMachine;
        }

        public bool LoadFromConfig(XmlNode node)
        {
            try
            {
                foreach (XmlNode level1Node in node)
                { // level1 --  "taggroup", "actions"
                    if (level1Node.NodeType == XmlNodeType.Comment)
                        continue;

                    XmlElement level1Item = (XmlElement)level1Node;

                    if (level1Item.Name.ToLower() == "statuslist")
                    {
                        #region StatusList

                        foreach (XmlNode level2_node in level1Node)
                        { // level2 --  "status"
                            //add
                            if (level2_node.NodeType == XmlNodeType.Comment)
                                continue;

                            XmlElement level2_item = (XmlElement)level2_node;
                            if (level2_item.Name.ToLower() == "status")
                            { // load Status

                                string strStatus = level2_item.GetAttribute("Name");
                                string strTag = level2_item.GetAttribute("Tag");

                                Tag link_tag = _owner.GetTag(strTag); ;
                                MachineStatusType type = (MachineStatusType)Enum.Parse(typeof(MachineStatusType), strStatus, false);
                                BindStatusTag(type, link_tag);

                            }
                        }

                        #endregion
                    }



                }// end of loop

                return true;
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("加载MachinePerformance出错：{0}", ex.Message));
                return false;
            }

        }

        private void BindStatusTag(MachineStatusType status, Tag tag)
        {
            if (_statusTags.ContainsKey(status))
            {
                _statusTags[status] = tag;
            }
            else
            {
                _statusTags.Add(status, tag);
            }
        }

        private void TrackMachineStatus()
        {

        }
    }

    public enum MachineStatusType
    {
        Running = 0,
        Stop = 1,
        Failure = 2,
        Block = 3,
        Starve = 4
    }
}
