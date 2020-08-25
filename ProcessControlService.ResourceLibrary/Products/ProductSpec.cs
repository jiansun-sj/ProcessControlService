using System;
using System.Collections.Generic;
using ProcessControlService.ResourceFactory.ParameterType;

namespace ProcessControlService.ResourceLibrary.Products
{
    /// <summary>
    /// 产品类型对象实例
    /// Created By David Dong at 20180603
    /// </summary>
    public class ProductSpec : CustomizedTypeInstance
    {
        // 产品属性集合
        // 例如：  材料：A
        //         卷径：500mm
        //         宽幅  100mm 
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(ProductSpec));

        private string SpecName { get; set; }

        //private string _workStationName=string.Empty;
        public string WorkStationName
        {
            get;
            set;
        }
        
        //  <Feature Name = "长度" Value="1000"/>
		//  <Feature Name = "宽度" Value="300"/>
		//  <Feature Name = "材质" Value="Material_01"/>
        private readonly Dictionary<string, IBasicValue> _features = new Dictionary<string, IBasicValue>(); // 产品特征集合

        private Dictionary<string, IBasicValue> GetFeatures()
        {
            return _features;
        }

        public object GetFeature(string featureName)
        {
            return _features[featureName];
        }

        public ProductSpec()
        {
        }

        //从另一个规格创建
        private ProductSpec(ICustomType ownerProductType, string specName, string workStationName,Dictionary<string, IBasicValue> features) 
        {
            _ownerType = ownerProductType;
            SpecName = specName;
            WorkStationName = workStationName;

            foreach (var item in features)
            {
                _features.Add(item.Key, item.Value);
            }
        }

        //初始化一个规格
        public ProductSpec(ICustomType ownerProductType, string specName)
        {
            _ownerType = ownerProductType;
            SpecName = specName;

            var productType = (ProductType)ownerProductType;

            //初始化特征集合
            var features = productType.GetFeatures();
            foreach (var item in features)
            {
                var featureName = item.Key;
                var featureValue =  item.Value;
                _features.Add(featureName, featureValue);
            }
        }

        public void SetFeatureValueInString(string featureName, string strFeatureValue)
        {
            try
            {
                _features[featureName].SetValueInString(strFeatureValue);
            }
            catch (Exception ex)
            {
                Log.Error($"ProductSpec 设置特征:{featureName} 值:{strFeatureValue} 出错:{ex.Message}");
            }
        }

        public void SetFeatureValue(string featureName, string featureValue)
        {
            try
            {

                _features[featureName].SetValueInString(featureValue);
            }
            catch (Exception ex)
            {
                Log.Error($"ProductSpec 设置特征:{featureName} 值:{featureValue.ToString()} 出错:{ex.Message}");
            }
        }

        public string GetFeatureType(string featureName)
        {
            try
            {
                var resultValue = _features[featureName];
                return resultValue.SimpleType;
            }
            catch (Exception ex)
            {
                Log.Error($"ProductSpec 返回特征:{featureName}类型 出错:{ex.Message}");
                return null;
            }
        }

        public bool HasFeature(string featureName)
        {
            return _features.ContainsKey(featureName);
        }

        public bool MatchFeature(string featureName, IBasicValue featureValue)
        {
            try
            {
                return _features[featureName].Equals(featureValue);
            }
            catch (Exception ex)
            {
                Log.Error($"产品匹配特征：{featureName}错误：{ex}");
                return false;
            }
        }

        #region CustomizedTypeInstance
        public override object Clone()
        {
            var newSpec = new ProductSpec(_ownerType, SpecName,WorkStationName, _features);
            return newSpec;
        }

        public override string ParameterName => "ProductSpec";

        public override object ParameterValue
        {
            get => (ProductSpec)Clone();
            set => CopyFrom((ProductSpec)value);
        }

        private void CopyFrom(ProductSpec spec)
        {
            if (spec != null)
            {
                SpecName = spec.SpecName;
                WorkStationName = spec.WorkStationName;

                _features.Clear();

                var anotherSpecFeatures = spec.GetFeatures();
                foreach (var strFeature in anotherSpecFeatures.Keys)
                {
                    _features.Add(strFeature, anotherSpecFeatures[strFeature]);
                }
            }
            else
            {
                SpecName = string.Empty;
                _features.Clear();
            }

        }

        //override public string ClassPath => "ProcessControlService.ResourceFactory.Products.ProductType.ProductSpec";

        public override string ToJson()
        { //完整显示
            var strFeatureContent = string.Empty;
            if (_features.Count > 0)
            {
                var i = 1;
                foreach (var feature in _features)
                {
                    strFeatureContent += $"{{\"{feature.Key}\":{feature.Value}}}";
                    if (i >= _features.Count) continue;
                    strFeatureContent += ",";
                    i++;
                }
            }
            else
            {
                strFeatureContent = "\"NULL\"";
            }

            var strJson =
                $"{{\"SpecName\":\"{SpecName}\",\"WorkStation\":\"{WorkStationName}\",\"Features\":[{strFeatureContent}]}}";
            return strJson;
        }

        public override string ToString()
        {
            return ToJson();
        }

        #endregion
    }

}
