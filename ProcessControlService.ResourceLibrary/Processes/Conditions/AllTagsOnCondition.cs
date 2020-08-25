using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using ProcessControlService.ResourceFactory;
using ProcessControlService.ResourceFactory.ParameterType;
using ProcessControlService.ResourceLibrary.Machines;

namespace ProcessControlService.ResourceLibrary.Processes.Conditions
{
   

    /// <summary>
    /// 顾杨 
    /// 当所有点都为true的时候才触发process
    /// </summary>
    public class AllTagsOnCondition : Condition
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(AllTagsOnCondition));

        private readonly List<Tag> _tags = new List<Tag>();

        //private bool _lastStatus = false;
        private bool _currentStatus;

        private IBasicParameter _outMachineName;
        private IBasicParameter _outTagName;

        public AllTagsOnCondition(string name, Process owner)
            : base(name, owner)
        {
        }


        public override bool LoadFromConfig(XmlElement node)
        {
            try
            {
                foreach (XmlNode level1Node in node)
                { // level1 --  "TrigTags"
                    if (level1Node.NodeType == XmlNodeType.Comment)
                        continue;

                    var level1Item = (XmlElement)level1Node;

                    if (string.Equals(level1Item.Name, "TrigTags", StringComparison.CurrentCultureIgnoreCase))
                    {
                        foreach (XmlNode level2Node in level1Node)
                        {
                            if (level2Node.NodeType == XmlNodeType.Comment)
                                continue;

                            var level2Item = (XmlElement)level2Node;
                            if (!string.Equals(level2Item.Name, "TrigTag", StringComparison.CurrentCultureIgnoreCase)
                            ) continue; // level2 --  "TrigTag"
                            var strMachine = level2Item.GetAttribute("Container");//jiansun， 2019-11-23 由Machine改为Container。
                            var strTag = level2Item.GetAttribute("Tag");

                            var machine = (Machine)ResourceManager.GetResource(strMachine);
                            var tag = machine.GetTag(strTag);

                            if (tag != null)
                            {
                                if (tag.TagType != "bool")
                                {
                                    throw new Exception($"装载AllTagsOnCondition时Tag{strTag}不是BOOL量");
                                }

                                _tags.Add(tag);

                                //Tag _lastTag = (Tag)tag.Clone();
                                //_lastTags.Add(_lastTag);
                            }
                            else
                            {
                                throw new Exception($"装载AllTagsOnCondition时未找到Tag:{strTag}");
                            }
                        }

                    }
                    else if (string.Equals(level1Item.Name, "OutParameter", StringComparison.CurrentCultureIgnoreCase))
                    {
                        var strMachineName = level1Item.GetAttribute("Container");//jiansun 2019-11-23 由MachineName改为Container。
                        var strTagName = level1Item.GetAttribute("TagName");

                        _outMachineName = Owner.ProcessParameterManager.GetBasicParam(strMachineName);
                        _outTagName = Owner.ProcessParameterManager.GetBasicParam(strTagName);
                        if (_outMachineName == null || _outTagName == null)
                        {
                            throw new Exception($"未能在process参数里找到机器名{strMachineName}或标签名{strTagName}");
                        }
                    }

                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"装载AllTagsOnCondition时出错：{ex.Message}.");
                return false;
            }
        }

        // check if Tag value change
        public override bool CheckReady()
        {

            //_tag.Read();

            return AllTagsValueChangedToOn();
        }

        private bool AllTagsValueChangedToOn()
        {
            try
            {
                var data = new List<bool>();
                data.Clear();

                foreach (var tag in _tags)
                {
                    data.Add((bool)tag.TagValue);
                }

                _currentStatus = BitArrayAnd(data.ToArray());

                return _currentStatus;

                //if (_currentStatus != _lastStatus)
                //{
                //    if (_lastStatus == false)
                //    {
                //        _lastStatus = _currentStatus;
                //        return true;
                //    }
                //    _lastStatus = _currentStatus;
                //}
                //return false;

            }
            catch (Exception ex)
            {
                Log.Error($"检查TAG出错:{ex.Message}");
                return false;
            }

        }

        private static bool BitArrayAnd(IEnumerable<bool> data)
        {
            return data.All(bit => bit);
        }
    }



}
