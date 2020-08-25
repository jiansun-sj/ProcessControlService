using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ProcessControlService.Contracts.ProcessData
{
    [DataContract]
    public class ProcessInfoModel
    {
        /// <summary>
        /// Process名称
        /// </summary>
        [DataMember]
        public string ProcessName { get; set; }

        /// <summary>
        /// 使用参数化Container时，启动Process时需要给定Process所使用的Container名称。
        /// </summary>
        [DataMember] 
        public List<string> ContainerNames { get; set; } = new List<string>();

        /// <summary>
        /// Process触发条件
        /// </summary>
        [DataMember]
        public string ConditionType { get; set; }

        /// <summary>
        /// 当前正在运行实例数量
        /// </summary>
        [DataMember]
        public int RunningInstanceNumber { get; set; }

        /// <summary>
        /// 当前中断执行的数量
        /// </summary>
        [DataMember]
        public long BreakCounts { get; set; }

        /// <summary>
        /// 已触发生成实例次数
        /// </summary>
        [DataMember]
        public long TotalRunningTimes { get; set; }
    }
}