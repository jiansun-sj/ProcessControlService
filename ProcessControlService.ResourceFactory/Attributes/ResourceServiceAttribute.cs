// ==================================================
// 文件名：ResourceServiceAttribute.cs
// 创建时间：2020/08/03 15:21
// ==================================================
// 最后修改于：2020/08/03 15:21
// 修改人：jians
// ==================================================

using System;

namespace ProcessControlService.ResourceFactory.Attributes
{
    /// <summary>
    ///     资源服务特性， sunjian created 2020/8/3 长春
    /// </summary>
    public class ResourceServiceAttribute : Attribute
    {
        public ResourceServiceAttribute()
        {
        }

        /// <summary>
        ///     资源服务，调用方法输入参数，初始值设置，需要按照参数声明顺序写入参数。
        /// </summary>
        /// <param name="initialValues"></param>
        public ResourceServiceAttribute(params object[] initialValues)
        {
            ServiceParameters = initialValues;
        }

        /// <summary>
        ///     资源服务对应方法传参初始值
        /// </summary>
        public object[] ServiceParameters { get; }
    }
}