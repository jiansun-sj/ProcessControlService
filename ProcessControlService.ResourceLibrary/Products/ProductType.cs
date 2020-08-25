// ==================================================
// 文件名：ProductType.cs
// 创建时间：2020/06/16 15:18
// ==================================================
// 最后修改于：2020/08/19 15:18
// 修改人：jians
// ==================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using FreeSql.DataAnnotations;
using log4net;
using Newtonsoft.Json;
using ProcessControlService.ResourceFactory.ParameterType;

namespace ProcessControlService.ResourceLibrary.Products
{
    /// <summary>
    ///     产品类型对象
    ///     Created By David Dong at 20180521
    /// </summary>
    [Table(Name = "tb_product_type")]
    public class ProductType : ICustomType
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProductType));

        //  <Features>
        //	<Feature Name = "长度" Type="Int16" />
        //	<Feature Name = "宽度" Type="Int16" />
        //	<Feature Name = "材质" Type="String" />
        //	<Feature Name = "客户" Type="String" />
        //	<Feature Name = "发货日期" Type="String" />
        //  </Features>
        [JsonMap]
        public readonly Dictionary<string, IBasicValue> Features = new Dictionary<string, IBasicValue>(); // 产品特征类型集合

        public ProductType(string name)
        {
            Name = name;
        }

        public object Clone()
        {
            var dictionary = (from feature in Features
                let value = feature.Value.Clone()
                select new
                {
                    Name = feature.Key,
                    BasicValue = value
                }).ToDictionary(arg => arg.Name, arg => arg.BasicValue);

            var productType = new ProductType(Name, dictionary);

            return productType;
        }

        public Dictionary<string, IBasicValue> GetFeatures()
        {
            return Features;
        }

        public IBasicValue GetFeature(string featureKey)
        {
            return Features.ContainsKey(featureKey) ? Features[featureKey] : null;
        }

        public void SetFeature(string featureKey, object value)
        {
            try
            {
                if (HasFeature(featureKey)) 
                    Features[featureKey].SetValue(value);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public bool HasFeature(string featureKey)
        {
            return Features.ContainsKey(featureKey);
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        #region ICustomizedType

        [JsonIgnore] public string Type => nameof(ProductType);

        [Column(IsPrimary = true)]
        [JsonProperty("SpecName")]
        public string Name { get; }

        public ProductType(string name, Dictionary<string, IBasicValue> features)
        {
            Name = name;
            Features = features;
        }

        public bool LoadFromConfig(XmlNode node)
        {
            foreach (var level1Node in node.Cast<XmlNode>()
                .Where(level1Node => level1Node.NodeType != XmlNodeType.Comment))
                try
                {
                    var level1Item = (XmlElement) level1Node;

                    if (level1Item.Name.ToLower() == "features")
                    {
                        foreach (XmlNode level2Node in level1Node)
                        {
                            // Feature
                            if (level2Node.NodeType == XmlNodeType.Comment) continue;

                            var level2Item = (XmlElement) level2Node;

                            var featureName = level2Item.GetAttribute("Name");
                            var featureType = level2Item.GetAttribute("Type");

                            Features.Add(featureName, Parameter.CreateBasicValue(featureType));
                        }
                    }
                    else
                    {
                        Log.Error("不识别的节点");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"加载ProductType出错：{ex.Message}");
                    return false;
                }

            return true;
        }

        #endregion
    }
}