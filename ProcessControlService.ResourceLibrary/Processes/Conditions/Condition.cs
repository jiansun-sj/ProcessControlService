// ==================================================
// 文件名：Condition.cs
// 创建时间：2019/10/30 15:46
// 上海芸浦信息技术有限公司
// copyright@yumpoo
// ==================================================
// 最后修改于：2019/12/26 15:46
// 修改人：jians
// ==================================================

using System;
using System.Reflection;
using System.Xml;
using log4net;

namespace ProcessControlService.ResourceLibrary.Processes.Conditions
{
    public abstract class Condition
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Condition));

        protected Process Owner;

        public ConditionModel Model = new ConditionModel();

        protected Condition()
        {
            
        }
        
        protected Condition(string name, Process owner)
        {
            Owner = owner;
            Name = name;
        }

        public Process OwnerProcess => Owner;

        public string Name { get; set; }

        public abstract bool LoadFromConfig(XmlElement node);

        public abstract bool CheckReady();

        public virtual void Reset()
        {
            Model.Status = ConditionModel.ConditionStatus.NotStart;
        }
        
        /// <summary>
        ///     创建对象（当前程序集）
        /// </summary>
        /// <param name="node"></param>
        /// <param name="owner" />
        /// <returns>创建的对象，失败返回 null</returns>
        public static Condition CreateFromConfig(XmlNode node, Process owner)
        {
            try
            {
                var level1Item = (XmlElement) node;

                var strConditionType = level1Item.GetAttribute("Type");
                var strConditionName = level1Item.GetAttribute("Name");

                var appPath = Assembly.GetExecutingAssembly().GetName().Name;
                var fullActionType = appPath + ".Processes.Conditions." + strConditionType;
                var objType = Type.GetType(fullActionType, true);
                var obj = Activator.CreateInstance(objType, strConditionName, owner);

                return (Condition) obj;
            }
            catch (Exception ex)
            {
                Log.Error($"创建Condition：{node.Name}失败{ex.Message}.");
                return null;
            }
        }
    }
}