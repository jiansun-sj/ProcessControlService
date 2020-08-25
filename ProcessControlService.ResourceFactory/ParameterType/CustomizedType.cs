// ==================================================
// 文件名：CustomizedType.cs
// 创建时间：2020/06/16 13:23
// ==================================================
// 最后修改于：2020/08/18 13:23
// 修改人：jians
// ==================================================

using System;
using System.Xml;

namespace ProcessControlService.ResourceFactory.ParameterType
{
    /// <summary>
    ///     定制化类型，需要在配置文件里定义该类型
    /// 所有实现该接口的类必须要有如下构造函数： ctor :: (string)
    /// </summary>
    public interface ICustomType : ICloneable
    {
        string Type { get; }

        string Name { get; }

        bool LoadFromConfig(XmlNode node);
    }
}