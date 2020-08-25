// ==================================================
// 文件名：MachineModel.cs
// 创建时间：2020/06/10 16:13
// ==================================================
// 最后修改于：2020/06/10 16:13
// 修改人：jians
// ==================================================

using System.Collections.Generic;
using System.Runtime.Serialization;
using ProcessControlService.ResourceLibrary.Machines.DataSources;

namespace ProcessControlService.ResourceLibrary.Machines
{
    [DataContract]
    internal class MachineModel
    {
        [DataMember] public List<DataSourceModel> DataSources;

        [DataMember] public string MachineName;

        public MachineModel(Machine machine)
        {
            MachineName = machine.ResourceName;

            DataSources = new List<DataSourceModel>();

            foreach (var ds in machine.ListDataSource()) DataSources.Add(new DataSourceModel(ds));
        }
    }
}