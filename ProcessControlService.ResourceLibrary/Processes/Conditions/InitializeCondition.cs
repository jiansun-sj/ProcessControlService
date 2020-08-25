// ==================================================
// 文件名：InitializeCondition.cs
// 创建时间：2019/10/30 15:47
// 上海芸浦信息技术有限公司
// copyright@yumpoo
// ==================================================
// 最后修改于：2019/12/26 15:47
// 修改人：jians
// ==================================================

using System.Xml;
using log4net;

//using ProcessControlService.Models;

namespace ProcessControlService.ResourceLibrary.Processes.Conditions
{
    /// <summary>
    ///     触发条件 只运行一次
    ///     Created by David Dong
    ///     Created at 20180623
    /// </summary>
    public class InitializeCondition : Condition
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(InitializeCondition));
        
        private bool _first = true;

        public InitializeCondition(string name, Process owner)
            : base(name, owner)
        {
        }

        public override bool LoadFromConfig(XmlElement level1Item)
        {
            return true;
        }

        public override bool CheckReady()
        {
            if (!_first) return false;
            _first = false;
            return true;
        }
    }
}