// ==================================================
// 文件名：ProductCollection.cs
// 创建时间：2020/05/22 9:01
// ==================================================
// 最后修改于：2020/05/22 9:01
// 修改人：jians
// ==================================================

using ProcessControlService.ResourceLibrary.Products;

namespace ProcessControlService.ResourceLibrary.Storage
{
    /// <summary>
    /// 产品集合
    /// </summary>
    /// <remarks>
    /// 使用范围：
    /// 1.托盘上放置统一类产品集合
    /// </remarks>
    public class ProductCollection
    {
        public Product Product { get; set; }=new Product();

        public int Amount { get; set; } = 1;
    }
}