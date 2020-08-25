using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ProcessControlService.Contracts.ProcessData
{
    [DataContract]
    public class ProcessInstanceInfoModel
    {
        /// <summary>
        /// Process
        /// </summary>
        [DataMember]
        public string Index { get; set; }

        /// <summary>
        /// Process名称
        /// </summary>
        [DataMember]
        public string ProcessName { get; set; }

        /// <summary>
        /// ProcessInstance正在执行的Step
        /// </summary>
        [DataMember]
        public string CurrentStep { get; set; }

        [DataMember]
        public short CurrentStepId { get; set; }

        /// <summary>
        /// 当前正在执行的StepAction的Container
        /// </summary>
        [DataMember]
        public string Container { get; set; }

        /// <summary>
        /// ProcessInstance中的参数实时数据
        /// </summary>
        [DataMember]
        public List<ParameterInfo> ProcessInstanceParameters { get; set; }
    }
}