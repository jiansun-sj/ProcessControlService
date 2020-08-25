using System;
using System.Xml;
using ProcessControlService.ResourceFactory.ParameterType;

//using ProcessControlService.Models;

// 修改记录 
// 20170528 David 增加了Reference Tag的功能，在运行是根据变量的内容加载Tag
// 20170528 David 增加了带条件执行ActionItem的功能
namespace ProcessControlService.ResourceLibrary.Machines.Actions
{
   
    public abstract class TagActionItem
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(TagActionItem));

        public Tag Tag;
        protected IBasicParameter InParameter;//TagAction用的只有BasicParameter sunjian 2020-1-19
        protected IBasicParameter OutParameter;

        private IBasicParameter _conditionParameter; // add by David Dong 20170528
        private string _conditionValue; // add by David Dong 20170528

        //protected Object _constValue;
        protected string ConstValue;

        // Add by David 20170528
        public bool IsReferenceTag => IsRefTag;
        protected bool IsRefTag;
        protected string RefTagName;
        private IBasicParameter _refTagParameter;

        private MachineAction _action;

        public string TagName => Tag.TagName;

        /// <summary>
        /// Created By Dongmin 20170310
        ///  Last modified By Dongmin 20170310
        ///  Example: <ActionItem Type="ReadTag" Tag="BatteryID1" Parameter="ScannedBatteryID1" />
        ///  读取ActionItem
        /// </summary>
        /// <param name="node"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public virtual bool LoadFromConfig(XmlNode node, TagsAction action)
        {
            try
            {
                 // level --  "ActionItem"
                if (node.NodeType == XmlNodeType.Comment)
                    return true ;

                _action = action; // add by David 20170528
                
                var item = (XmlElement)node;

                var attribute = item.GetAttribute("Type");

                // 加载执行条件
                /*if (item.HasAttribute("ConditionParameter"))
                {
                    var strCondition = item.GetAttribute("ConditionParameter");
                    ConditionParameter = action.ActionInParameterManager.GetBasicParametereter(strCondition);

                    if (ConditionParameter == null)
                    {
                        Log.Error($"装载机器Action:{action.Name},节点{node.Name}时装载参数:{strCondition}出错");
                        return false;
                    }

                }*/

                /*if (item.HasAttribute("CondtionValue"))
                {
                    ConditionValue = item.GetAttribute("CondtionValue");
                }*/

                //// 加载Tag
                if (item.HasAttribute("Tag"))
                { // Tag action item
                    var tagName = item.GetAttribute("Tag");

                    // 找到Tag
                    var machine = action.OwnerMachine;
                    Tag = machine.GetTag(tagName);

                   

                    if (Tag == null)
                    {
                        Log.Error($"装载机器Action:{action.Name},节点{node.Name}时装载Tag:{tagName}出错");
                        return false;
                    }
                }
                /*else if (item.HasAttribute("ReferenceTag")) // Add by David 20170528
                { // Reference Tag
                    RefTagName = item.GetAttribute("ReferenceTag");
                    RefTagParameter = action.ActionInParameterManager.GetBasicParametereter(RefTagName);
                    if (RefTagParameter != null)
                    {
                        IsRefTag = true;
                    }
                }*/

                //// 加载参数
                string strParameter;
                //Parameter _parameter = null;

                // 找到Parameter
                if (item.HasAttribute("InParameter"))
                {
                    strParameter = item.GetAttribute("InParameter");
                    InParameter = action.ActionInParameterManager.GetBasicParam(strParameter);
                }

                if (item.HasAttribute("OutParameter"))
                {
                    strParameter = item.GetAttribute("OutParameter");
                    OutParameter = action.ActionOutParameterManager.GetBasicParam(strParameter);
                }
            

                if (item.HasAttribute("ConstValue"))
                {
                    ConstValue =item.GetAttribute("ConstValue");
                }


                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"装载机器Action:{action.Name},节点{node.Name}时出错:{ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获得参考Tag
        /// Action 运行时被调用，找到那个Tag
        /// Created by David 20170520
        /// </summary>
        /// <returns></returns>
        protected Tag GetReferenceTag()
        {
            try
            {
                // GetTag
                var value = _refTagParameter.GetValue().ToString();

                var machine = _action.OwnerMachine;
                Tag = machine.GetTag(value);

                return Tag;
            }
            catch (Exception ex)
            {
                Log.Error($"查找参考Tag:{TagName}失败:{ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 检测条件是否满足
        /// </summary>
        /// <returns></returns>
        public bool ExecuteConditionCheck()
        {
            if (_conditionParameter != null)
            {
                return (_conditionParameter.GetValue().ToString() == _conditionValue);
            }
            return true;
        }
    }

    /// <summary>
    ///  Created By Dongmin at 2017/03/10
    ///  Last modified by Dongmin at 2017/03/10
    ///  把Parameter写入Tag
    /// </summary>
    public class WriteTagActionItem : TagActionItem
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(WriteTagActionItem));

        /// <summary>
        /// 写点操作
        /// </summary>
        /// <returns>成功与否</returns>
        public bool Write(ParameterManager actionInParameterManager)
        {
            try
            {
                var tempTag = IsRefTag ? GetReferenceTag() : Tag; // 判断是否操作的是参考Tag

                return InParameter != null ? tempTag.Write(actionInParameterManager[InParameter.Name].GetValue()) 
                    : tempTag.WriteValueInString(ConstValue);
            }
            catch (Exception ex)
            {
                Log.Error($"机器{Tag.Owner.ResourceName}写点动作Tag:{Tag.TagName}操作失败:{ex.Message}");
                return false;
            }
        } 
    }

    /// <summary>
    ///  Created By Dongmin at 2017/03/10
    ///  Last modified by Dongmin at 2017/03/10
    ///  把Tag写入Parameter
    /// </summary>
    public class ReadTagActionItem : TagActionItem
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(ReadTagActionItem));

      
        /// <summary>
        /// 读点操作
        /// </summary>
        /// <returns>成功与否</returns>
        public bool Read(ParameterManager actionOutParameterManager)
        {
            try
            {
                var tempTag = IsRefTag ? GetReferenceTag() : Tag; // 判断是否操作的是参考Tag

                actionOutParameterManager[OutParameter.Name]?.SetValue(tempTag.TagValue);

                return true;
             
            }
            catch (Exception ex)
            {
                Log.Error($"机器{Tag.Owner.ResourceName}读点动作Tag:{Tag.TagName}操作失败:{ex.Message}");
                return false;
            }

        }

    }

    public class CheckTagActionItem : TagActionItem
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(CheckTagActionItem));

        public bool Check(ParameterManager actionInParameterManager)
        {
            try
            {
                var tempTag = IsRefTag ? GetReferenceTag() : Tag; // 判断是否操作的是参考Tag
              
                return InParameter != null ? Tag.ValueEqual(tempTag, actionInParameterManager[InParameter.Name].GetValue()) 
                    : Tag.ValueEqual(tempTag, ConstValue);
            }
            catch (Exception ex)
            {
                Log.Error($"机器{Tag.Owner.ResourceName}检查动作Tag:{Tag.TagName},参数:{InParameter?.Name} 操作失败:{ex.Message}");
                return false;
            }
             
        }
    }
  
}
