// ==================================================
// 文件名：MachineResourceTemplate.cs
// 创建时间：2020/06/16 13:38
// ==================================================
// 最后修改于：2020/08/10 13:38
// 修改人：jians
// ==================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using log4net;
using ProcessControlService.ResourceFactory;

namespace ProcessControlService.ResourceLibrary.ResourceTemplate
{
    /// <summary>
    ///     目标类型为Machine的资源模板，目前仅可以在模板内定义Actions
    /// </summary>
    /// <remarks>
    ///     修改记录
    ///     <list type="number">
    ///         <item>
    ///             <description>
    ///                 sunjian 2019-12-24 创建Machine的资源模板，增加模板化Action定义。
    ///             </description>
    ///         </item>
    ///     </list>
    /// </remarks>
    public class MachineResourceTemplate : IResourceTemplate
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MachineResourceTemplate));

        private readonly List<ResourceTemplateAction> _resourceTemplateActionsActions =
            new List<ResourceTemplateAction>();

        private string _targetMachineName = "";

        public bool HasActions;

        public MachineResourceTemplate(string templateName)
        {
            TemplateName = templateName;
        }

        public XmlNode ActionsNode { get; set; }

        public string TemplateName { get; set; }
        public string TargetResourceType { get; set; }

        public bool LoadFromConfig(XmlNode node)
        {
            try
            {
                var level0Item = (XmlElement) node;

                TargetResourceType = level0Item.GetAttribute("TargetResourceType");

                if (TargetResourceType.ToLower() != "machine" &&
                    TargetResourceType.ToLower() != "agv" + "control" + "center")
                    Log.Info("目前资源模板仅支持Machine和AgvControlCenter类资源");

                foreach (var level1Node in node.Cast<XmlNode>()
                    .Where(level1Node => level1Node.NodeType != XmlNodeType.Comment))
                    try
                    {
                        var level1Item = (XmlElement) level1Node;

                        //todo: 增加Alarms的模板化配置

                        switch (level1Item.Name.ToLower())
                        {
                            case "actions":
                                ActionsNode = level1Node;
                                LoadActionsForMachineTemplate(ActionsNode);
                                HasActions = true;
                                break;
                        }

                        //todo: 增加Events的模板化配置。
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"加载资源模板{TemplateName}出错：{ex.Message}");
                    }

                return true;
            }
            catch (Exception e)
            {
                Log.Error($"读取{nameof(MachineResourceTemplate)} : [{TemplateName}] Xml配置失败，exception：{e.Message}");
                return false;
            }
        }

        private void LoadActionsForMachineTemplate(IEnumerable level1Node)
        {
            foreach (XmlNode level2Node in level1Node)
            {
                if (level2Node.NodeType == XmlNodeType.Comment)
                    continue;

                var level2Item = (XmlElement) level2Node;

                // 动态创建Action
                var name = level2Item.GetAttribute("Name");
                // var actionType = level2Item.GetAttribute("Type");

                var action = new ResourceTemplateAction(name);
                // var action = (MachineAction) ActionsManagement.CreateAction(actionType, name);

                if (action.LoadFromConfig(level2Item))
                    // 加入到MachineResourceTemplate的Action集合
                    try
                    {
                        AddAction(action);
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"加载资源模板{TemplateName} 的Action:{name}出错:{ex}");
                    }
                else
                    Log.Error($"加载资源模板{TemplateName} 的Action:{name}出错");

                HasActions = true;
            }
        }

        private void AddAction(ResourceTemplateAction action)
        {
            _resourceTemplateActionsActions.Add(action);
        }

        /// <summary>
        ///     获得虚拟的资源模板Action
        /// </summary>
        /// <param name="actionName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public ResourceTemplateAction GetResourceTemplateAction(string actionName)
        {
            return _resourceTemplateActionsActions.FirstOrDefault(a => a.Name == actionName);
        }

        public void ChangeTargetMachineName(string targetMachineName)
        {
            _targetMachineName = targetMachineName;
        }

        public bool HasAction(string actionName)
        {
            return _resourceTemplateActionsActions.Any(a => a.Name == actionName);
        }
    }
}