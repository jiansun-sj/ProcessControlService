// ==================================================
// 文件名：ResourceTemplateAction.cs
// 创建时间：// 17:01
// 上海芸浦信息技术有限公司
// copyright@yumpoo
// ==================================================
// 最后修改于：2019/12/26 17:01
// 修改人：jians
// ==================================================

using System;
using System.Xml;
using log4net;
using ProcessControlService.ResourceFactory.ParameterType;
using ProcessControlService.ResourceLibrary.Action;

namespace ProcessControlService.ResourceLibrary.ResourceTemplate
{
    /// <summary>
    ///     模板生成的Action，并不是用来执行的，而是用来存储虚拟的参数值，用来给Process中的Step建立参数绑定关系的。
    /// </summary>
    /// <remarks>
    ///     <list type="number">
    ///         <item>
    ///             <description>
    ///                 created by sunjian 2019-12-26
    ///             </description>
    ///         </item>
    ///     </list>
    /// </remarks>
    public class ResourceTemplateAction : BaseAction
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MachineResourceTemplate));

        public ResourceTemplateAction(string actionName) : base(actionName)
        {
        }

        public override void Execute()
        {
            throw new NotImplementedException();
        }

        public override bool IsSuccessful()
        {
            throw new NotImplementedException();
        }

        public override object GetResult()
        {
            throw new NotImplementedException();
        }
        
        public override BaseAction Clone()
        {
            var basAction = new ResourceTemplateAction(Name)
            {
                ActionInParameterManager = ActionInParameterManager.Clone(),
                ActionOutParameterManager = ActionOutParameterManager.Clone(),
            };
            return basAction;
       
        }
    }
}