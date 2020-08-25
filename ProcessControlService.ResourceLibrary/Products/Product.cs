// ==================================================
// 文件名：Product.cs
// 创建时间：2020/06/16 19:11
// ==================================================
// 最后修改于：2020/08/18 19:11
// 修改人：jians
// ==================================================

using System.Xml;
using FreeSql.DataAnnotations;
using Newtonsoft.Json;
using ProcessControlService.ResourceFactory.ParameterType;
using ProcessControlService.ResourceLibrary.Tracking;

namespace ProcessControlService.ResourceLibrary.Products
{
    /// <summary>
    ///     产品对象
    ///     Created By David Dong at 20180603
    ///     updated by sunjian, 2020-08
    /// </summary>
    [Table(Name = "tb_product")]
    public class Product : ITrackUnit
    {
        /// <summary>
        ///     需要保留该构造函数用于，创建自定义参数
        /// </summary>
        public Product(string id)
        {
            Id = id;
        }

        public Product()
        {
        }

        public Product(string id, string productTypeName)
        {
            Id = id;

            var productType = (ProductType) CustomTypeManager.GetCustomizedType(productTypeName);

            ProductType =(ProductType) productType.Clone();
        }

        [Column(IsPrimary = true)] public string Id { get; set; }

        [JsonIgnore]
        public ILocation CurrentLocation { get; set; }

        public string Type => nameof(Product);

        [Column(IsIgnore = true)] public string Name { get; }

        public bool LoadFromConfig(XmlNode node)
        {
            return true;
        }

        public object Clone()
        {
            var newProduct = new Product(Id) {ProductType = ProductType};

            return newProduct;
        }

        public string ToJson()
        {
            var strJson = ProductType != null
                ? $"{{\"ProductID\":\"{Id}\",\"Specs\":[{ProductType.ToJson()}]}}"
                : $"{{\"ProductID\":\"{Id}\",\"Specs\":[{string.Empty}]}}";
            return strJson;
        }

        public override string ToString()
        {
            return ToJson();
        }

        #region Product spec

        public ProductType ProductType { get; set; }

        //根据规格创建产品
        public static Product CreateProduct(string id, string productTypeName)
        {
            var newProduct = new Product();

            var customizedType = CustomTypeManager.GetCustomizedType(productTypeName);

            newProduct.ProductType = (ProductType) customizedType.Clone();

            newProduct.Id = id;

            return newProduct;
        }

        //根据ID创建产品
        public static Product CreateProduct(string productId)
        {
            var newProduct = new Product {Id = productId};

            return newProduct;
        }

        public void SetFeature(string featureKey, object value)
        {
            ProductType.SetFeature(featureKey,value);
        }

        #endregion
    }
}