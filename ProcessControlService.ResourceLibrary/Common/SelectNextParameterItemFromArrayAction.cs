// ==================================================
// 文件名：SelectNextParameterItemFromArrayAction.cs
// 创建时间：2019/12/23 12:23
// 上海芸浦信息技术有限公司
// copyright@yumpoo
// ==================================================
// 最后修改于：2019/12/26 12:23
// 修改人：jians
// ==================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using log4net;
using ProcessControlService.ResourceFactory.ParameterType;
using ProcessControlService.ResourceLibrary.Action;

namespace ProcessControlService.ResourceLibrary.Common
{
    /// <summary>
    ///     从给定的参数数组中挑选下一个需要模块化执行Process的Machine参数
    /// </summary>
    /// <remarks>
    ///     <list type="number">
    ///         <item>
    ///             <description> 添加从参数数组中选择下一个参数对象的动作 add by sunjian 2019-12</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    public class SelectNextParameterItemFromArrayAction : BaseAction
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SelectNextParameterItemFromArrayAction));

        public SelectNextParameterItemFromArrayAction(string actionName) : base(actionName)
        {
        }

        public override void Execute()
        {
            try
            {
                
                
                var names =ActionInParameterManager.GetListParam("ListName");

                Debug.Assert(names != null, nameof(names) + " != null");
                var parameterItemNumber = names.Count;

                if (parameterItemNumber <= 0) Log.Error("获取下一个参数对象失败，因为没有从ProcessParameterArray检测到任何对象。");

                if (CommonResource.Count + 1 > parameterItemNumber) CommonResource.Count = 0;

                var nextMachineName = (string)names[CommonResource.Count];

                var resultDict=new Dictionary<string,string>()
                {
                    { "MachineA",nextMachineName /*new BasicValue("string","Controller1")*/},
                    { "Locker",nextMachineName/*new BasicValue("string","TestCallingMachine")*/}
                };

                ActionOutParameterManager.GetDictionaryParam("ResultItem").Replace(new DictionaryParameter<string>(resultDict));

                CommonResource.Count++;

                Log.Info(
                    $"执行{nameof(SelectNextParameterItemFromArrayAction)}成功，获取到的下一个参数项为[{nextMachineName}].");
            }
            catch (Exception e)
            {
                Log.Error($"获取下一个参数对象失败。{e.Message}");
            }
        }

        public override bool IsSuccessful()
        {
            return true;
        }

        public override object GetResult()
        {
            throw new NotImplementedException();
        }

        public override bool LoadFromConfig(XmlNode node)
        {
            throw new NotImplementedException();
        }

        protected override bool CreateParameters()
        {
            ActionInParameterManager.AddListParam(new ListParameter<string>("ListName"));
            ActionOutParameterManager.AddDictionaryParam(new DictionaryParameter<string>("ResultItem"));

            return true;
        }

        public override BaseAction Clone()
        {
            var basAction = new SelectNextParameterItemFromArrayAction(Name)
            {
                ActionInParameterManager = ActionInParameterManager.Clone(),
                ActionOutParameterManager = ActionOutParameterManager.Clone()
            };
            return basAction;

        }
    }
}