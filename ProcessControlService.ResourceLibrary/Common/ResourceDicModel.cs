// ==================================================
// 文件名：SelectedResourceModel.cs
// 创建时间：2020/06/09 16:21
// ==================================================
// 最后修改于：2020/06/09 16:21
// 修改人：jians
// ==================================================

using ProcessControlService.ResourceFactory.ParameterType;

namespace ProcessControlService.ResourceLibrary.Common
{
    public class ResourceDicModel<T>
    {
        public string ResourceDictionaryName { get; set; }

        public DictionaryParameter<T> DictionaryParameter { get; set; }
    }
}