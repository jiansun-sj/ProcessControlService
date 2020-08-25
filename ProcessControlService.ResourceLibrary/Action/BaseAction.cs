// ==================================================
// 文件名：BaseAction.cs
// 创建时间：2019/11/13 16:00
// 上海芸浦信息技术有限公司
// copyright@yumpoo
// ==================================================
// 最后修改于：2019/12/26 16:00
// 修改人：jians
// ==================================================

using System;
using System.Collections.Generic;
using System.Xml;
using log4net;
using ProcessControlService.Contracts.ProcessData;
using ProcessControlService.ResourceFactory.ParameterType;

namespace ProcessControlService.ResourceLibrary.Action
{
    /// <summary>
    ///     所有动作的基本Action
    ///     Created by David Dong 20170407
    /// </summary>
    public abstract class BaseAction:IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(BaseAction));

        public IActionContainer ActionContainer;

        /// <summary>
        /// Action执行时记录的日志，可以在GUI流程日志中查询。
        /// </summary>
        /// <remarks>
        /// add by sunjian 2020-03
        /// </remarks>
        public List<Message> FeedBacks { get; }=new List<Message>();

        public void AddFeedBacks(Action<string> log,Message message)
        {
            FeedBacks.Add(message);
            log(message.Description);
        }

        protected BaseAction(string actionName)
        {
            Name = actionName;

            // ReSharper disable once VirtualMemberCallInConstructor
            CreateParameters();
        }

        public string Name { get; }

        public abstract void Execute();

        public abstract bool IsSuccessful();

        public virtual bool IsFinished()
        {
            return true;
        }

        public virtual void Break() { }

        public abstract object GetResult();

        /// <summary>
        /// 如果Xml中配置了输入输出参数，Action默认从Xml中加载输入输出参数 sunjian 2020-06
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public virtual bool LoadFromConfig(XmlNode node)
        {
            try
            {
                foreach (XmlNode level1Node in node)
                {
                    // level1 --  "Parameter"
                    if (level1Node.NodeType == XmlNodeType.Comment)
                        continue;

                    var level1Item = (XmlElement) level1Node;

                    if (string.Equals(level1Item.Name, "InParameter", StringComparison.CurrentCultureIgnoreCase))
                    {
                        Parameter.LoadParameterFromConfig(level1Item,ActionInParameterManager);
                    }

                    else if (string.Equals(level1Item.Name, "OutParameter",
                        StringComparison.CurrentCultureIgnoreCase))
                    {
                        Parameter.LoadParameterFromConfig(level1Item,ActionOutParameterManager);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Error("BaseAction装载参数出错：" + ex);
                return false;
            }
        }

        #region "Parameters"

        public ParameterManager ActionInParameterManager = new ParameterManager();
        public ParameterManager ActionOutParameterManager = new ParameterManager();


        /// <summary>
        ///     定义Action所需要的参数InParameter和OutParameter，Xml中为ActionParameter，该参数需要和一个Constant Value或者一个
        ///     Process Parameter相绑定，不可能单一存在。
        ///     Constant Value为常数值，也就是Default值，不会改变
        ///     Process Parameter为Process中定义的过程参数。
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        ///     <list type="number">
        ///         <item>
        ///             <description>
        ///                 sunjian 2019-12-24 增加xml注释
        ///             </description>
        ///         </item>
        ///     </list>
        /// </remarks>
        protected virtual bool CreateParameters()
        {
            return true;
        }
        
        #endregion

        public virtual BaseAction Clone()
        {
            var baseAction = (BaseAction) Activator.CreateInstance(GetType(),Name);
            baseAction.ActionContainer = ActionContainer;
            baseAction.ActionInParameterManager = ActionInParameterManager.Clone();
            baseAction.ActionOutParameterManager = ActionOutParameterManager.Clone();
            return baseAction;
        }
        
        public virtual void Dispose()
        {
            ActionInParameterManager.Dispose();
            ActionOutParameterManager.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}