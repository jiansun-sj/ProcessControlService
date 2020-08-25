// ==================================================
// 文件名：MemoryAndCpuData.cs
// 创建时间：2020/06/01 12:44
// ==================================================
// 最后修改于：2020/06/01 12:44
// 修改人：jians
// ==================================================

using System;
using System.Runtime.Serialization;

namespace ProcessControlService.Contracts.ProcessData
{
    /// <summary>
    /// FactoryWindow 执行耗费内存和Cpu占用率Model，传递给GUI。
    /// </summary>
    [DataContract]
    //[Table("MemoryAndCpuData")]
    public class MemoryAndCpuData
    {
        /// <summary>
        /// 记录时间
        /// </summary>
        [DataMember]
        //[PrimaryKey]
        public int RecordTimeIndex { get; set; }

        /// <summary>
        /// 内存使用
        /// </summary>
        [DataMember]
        public double Memory { get; set; }

        /// <summary>
        /// CPU占用率
        /// </summary>
        [DataMember]
        public double CpuUsage { get; set; }

        public DateTime RecordDate { get; set; }
    }
}