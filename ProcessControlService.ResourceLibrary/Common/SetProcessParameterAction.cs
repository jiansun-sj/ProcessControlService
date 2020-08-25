// ==================================================
// 文件名：SetProcessParameterAction.cs
// 创建时间：// 18:20
// ==================================================
// 最后修改于：2019/12/23 18:20
// 修改人：jians
// ==================================================

using System;
using System.Xml;
using ProcessControlService.ResourceFactory;
using ProcessControlService.ResourceLibrary.Action;

namespace ProcessControlService.ResourceLibrary.Common
{

    /// <summary>
    /// 设置带有使用ResourceTemplate功能的Process的过程参数。
    /// </summary>
    /// <remarks>
    /// add by sunjian 2019-12-23
    /// </remarks>
    public class SetProcessParameterAction:BaseAction
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(SetProcessParameterAction));

        public SetProcessParameterAction(string actionName) : base(actionName)
        {


        }

        public override void Execute()
        {
            try
            {
                var processName= InParameters["ProcessName"].GetValue().ToString();
                var selectedResource = InParameters["ParameterValue"].GetValue().ToString();

                Log.Info($"开始执行{nameof(SetProcessParameterAction)},选中的资源为{selectedResource},需要设置的Process为{processName}");

                var process=(Processes.Process) ResourceManager.GetResource(processName);

                process.SetParametersAccordingToSelectedResource(selectedResource);

            }
            catch (Exception e)
            {
                Log.Error($"设置过程参数失败，{e.Message}");
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
            InParameters.Add(new Parameter("ProcessName","string"));
           // InParameters.Add(new Parameter("ParameterName", "string"));
            InParameters.Add(new Parameter("ParameterValue", "string"));
            
            return true;
        }
    }
}