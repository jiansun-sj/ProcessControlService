// ==================================================
// 文件名：TraySpec.cs
// 创建时间：2020/01/02 13:56
// ==================================================
// 最后修改于：2020/05/21 13:56
// 修改人：jians
// ==================================================

using System;
using System.Collections.Generic;
using log4net;
using ProcessControlService.ResourceFactory;
using ProcessControlService.ResourceFactory.ParameterType;
using ProcessControlService.ResourceLibrary.Products;

namespace ProcessControlService.ResourceLibrary.Tracking
{
    /// <summary>
    ///     托盘规格实例
    ///     Created By David Dong at 20180606
    /// </summary>
    public class TraySpec : CustomizedTypeInstance
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(TraySpec));

        private readonly Dictionary<string, object> _features = new Dictionary<string, object>(); // 产品特征集合

        public TraySpec()
        {
        }

        //从另一个规格创建
        public TraySpec(ICustomType ownerType, string specName, Dictionary<string, object> Features)
        {
            var trayType = (ProductTrayType) ownerType;

            _ownerType = ownerType;
            SpecName = specName;
            ProductCount = trayType.ProductCount;
            RetentionSecondsSetting = trayType.RetentionSecondsSetting;

            foreach (var feature in Features) _features.Add(feature.Key, feature.Value);
        }

        //初始化一个规格
        public TraySpec(ICustomType OwnerType, string SpecName)
        {
            var trayType = (ProductTrayType) OwnerType;

            _ownerType = OwnerType;
            this.SpecName = SpecName;
            ProductCount = trayType.ProductCount;
            RetentionSecondsSetting = trayType.RetentionSecondsSetting;


            //初始化特征集合
            var features = trayType.GetFeatures();
            foreach (var item in features)
            {
                var featureName = item.Key;
                var featureValue = item.Value;
                _features.Add(featureName, featureValue);
            }
        }

        private string SpecName { get; set; }

        public short ProductCount { get; }

        //set { _productCount = value; }
        public int RetentionSecondsSetting { get; }

        public Dictionary<string, object> GetFeatures()
        {
            return _features;
        }

        public object GetFeature(string featureName)
        {
            return _features[featureName];
        }

        public void SetFeatureValueInString(string featureName, string strFeatureValue)
        {
            try
            {
                _features[featureName]=strFeatureValue;
            }
            catch (Exception ex)
            {
                LOG.Error(string.Format("TraySpec 设置特征:{0} 值:{1} 出错:{2}", featureName, strFeatureValue, ex.Message));
            }
        }

        public void SetFeatureValue(string featureName, object featureValue)
        {
            try
            {
                _features[featureName]=featureValue;
            }
            catch (Exception ex)
            {
                LOG.Error($"TraySpec 设置特征:{featureName} 值:{featureValue} 出错:{ex.Message}");
            }
        }


        public Type GetFeatureType(string featureName)
        {
            try
            {
                var resultValue = _features[featureName];
                return resultValue.GetType();
            }
            catch (Exception ex)
            {
                LOG.Error($"TraySpec 返回特征:{featureName}类型 出错:{ex.Message}");
                return null;
            }
        }

        public bool HasFeature(string featureName)
        {
            return _features.ContainsKey(featureName);
        }


        //创建托盘规格，使之可以容纳产品
        public void SetSpecByProduct(Product product)
        {
            foreach (var feature in _features)
                if (product.ProductType.HasFeature(feature.Key))
                    _features[feature.Key]=product.ProductType.GetFeature(feature.Key);
        }

        #region CustomizedTypeInstance

        public override object Clone()
        {
            var newSpec = new TraySpec(_ownerType, SpecName, _features);
            //newSpec.ProductCount = ProductCount;
            return newSpec;
        }

        public override string ParameterName => "ProductTraySpec";

        public override object ParameterValue
        {
            get => (TraySpec) Clone();
            set => CopyFrom((TraySpec) value);
        }

        private void CopyFrom(TraySpec Spec)
        {
            if (Spec != null)
            {
                SpecName = Spec.SpecName;
                _features.Clear();

                var anotherSpecFeatures = Spec.GetFeatures();
                foreach (var strFeature in anotherSpecFeatures.Keys)
                    _features.Add(strFeature, anotherSpecFeatures[strFeature]);
            }
            else
            {
                SpecName = string.Empty;
                _features.Clear();
            }
        }

        //override public string ClassPath => "ProcessControlService.ResourceFactory.Products.ProductType.ProductSpec";

        public override string ToJson()
        {
            //完整显示
            var strFeatureContent = string.Empty;
            if (_features.Count > 0)
            {
                var i = 1;
                foreach (var feature in _features)
                {
                    strFeatureContent += $"{{\"{feature.Key}\":{feature.ToString()}}}";
                    if (i < _features.Count)
                    {
                        strFeatureContent += ",";
                        i++;
                    }
                }
            }
            else
            {
                strFeatureContent = "\"NULL\"";
            }

            var strJson = string.Format("{{\"SpecName\":\"{0}\",\"Features\":[{1}]}}", SpecName, strFeatureContent);
            return strJson;
        }

        public override string ToString()
        {
            return ToJson();
        }

        #endregion
    }
}