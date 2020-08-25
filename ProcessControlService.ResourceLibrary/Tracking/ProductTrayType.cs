// ==================================================
// 文件名：ProductTrayType.cs
// 创建时间：2020/01/02 13:55
// ==================================================
// 最后修改于：2020/05/21 13:55
// 修改人：jians
// ==================================================

using System;
using System.Collections.Generic;
using System.Xml;
using log4net;
using ProcessControlService.ResourceFactory;
using ProcessControlService.ResourceFactory.ParameterType;

namespace ProcessControlService.ResourceLibrary.Tracking
{
    /// <summary>
    ///     托盘类型
    ///     Created By David Dong at 20180607
    /// </summary>
    public class ProductTrayType : ICustomType
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProductTrayType));

        // <Features>
        //	<Feature Name = "客户" Type="string"/>
        //</Features>
        private readonly Dictionary<string, string> _features = new Dictionary<string, string>(); // 产品特征类型集合

        public ProductTrayType(string Name)
        {
            this.Name = Name;
        }

        public short ProductCount { get; private set; }

        //set { _productCount = value; }
        public int RetentionSecondsSetting { get; private set; }

        public Dictionary<string, string> GetFeatures()
        {
            return _features;
        }

        #region ICustomizedType

        //private string _type;
        public string Type => "ProductTrayType";

        public string ComplexTypeName => "Products.ProductTrayType";

        public string Name { get; }

        public bool LoadFromConfig(XmlNode node)
        {
            var level0_item = (XmlElement) node;

            var strProductCount = level0_item.GetAttribute("ProductCount");
            ProductCount = Convert.ToInt16(strProductCount);

            if (level0_item.HasAttribute("RetentionSeconds"))
            {
                var strRetentionSecondsSetting = level0_item.GetAttribute("RetentionSeconds");
                RetentionSecondsSetting = Convert.ToInt32(strRetentionSecondsSetting);
            }

            foreach (XmlNode level1_node in node)
            {
                // Features
                if (level1_node.NodeType == XmlNodeType.Comment) continue;

                try
                {
                    var level1_item = (XmlElement) level1_node;

                    if (level1_item.Name.ToLower() == "features")
                    {
                        foreach (XmlNode level2_node in level1_node)
                        {
                            // Feature
                            if (level2_node.NodeType == XmlNodeType.Comment) continue;

                            var level2_item = (XmlElement) level2_node;

                            var FeatureName = level2_item.GetAttribute("Name");
                            var FeatureType = level2_item.GetAttribute("Type");

                            _features.Add(FeatureName, FeatureType);
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
                    Log.Error(string.Format("加载ProductTray出错：{0}", ex.Message));
                    return false;
                }
            }

            return true;
        }

        //public CustomizedTypeInstance CreateInstance()
        //{
        //    return (CustomizedTypeInstance)GetCurrentSpec().Clone();
        //}

        #endregion

        //#region "Tray Spec"
        //private Dictionary<string, TraySpec> _specs = new Dictionary<string, TraySpec>(); //产品规格集合
        //private string _currentSpecName = string.Empty; //当前规格名

        //private TraySpec GetCurrentSpec()
        //{
        //    if (_specs.ContainsKey(_currentSpecName))
        //    {
        //        return _specs[_currentSpecName];
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        //public TraySpec GetSpec(string SpecName)
        //{
        //    if (_specs.ContainsKey(SpecName))
        //    {
        //        return _specs[SpecName];
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        //private bool SetCurrentSpec(string SpecName)
        //{
        //    if (_specs.ContainsKey(SpecName))
        //    {
        //        _currentSpecName = SpecName;
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        //public List<string> GetAllSpecNames()
        //{
        //    return _specs.Keys.ToList<string>();
        //}
        //#endregion
        public object Clone()
        {
            throw new NotImplementedException();
        }
    }
}