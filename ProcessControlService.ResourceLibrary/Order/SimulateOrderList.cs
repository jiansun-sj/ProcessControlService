using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProcessControlService.ResourceLibrary.Products;
using ProcessControlService.ResourceLibrary.Tracking;
using ProcessControlService.ResourceFactory;
using System.Xml;
using Newtonsoft.Json;

namespace ProcessControlService.ResourceLibrary.Order
{
    /// <summary>
    /// 随机生成订单列表
    /// Created by Dongmin 20180218
    /// </summary>
    public class SimulateOrderList : OrderList
    {
        private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(SimulateOrderList));

        //private List<string> _orderSpecs; //订单里的产品类型集合

        private List<ProductSpec> _availSpecs = new List<ProductSpec>(); //产品规格集合

        public SimulateOrderList(string Name) : base(Name)
        {
          
        }

        #region 接口实现
        public override bool LoadFromConfig(XmlNode node)
        {
            if(!base.LoadFromConfig(node))
            {
                return false;
            }

            foreach (XmlNode level1_node in node)
            { // Specs
                if (level1_node.NodeType == XmlNodeType.Comment) continue;

                try
                {
                    XmlElement level1_item = (XmlElement)level1_node;

                    if (level1_item.Name.ToLower() == "specs")
                    {
                        foreach (XmlNode level2_node in level1_node)
                        { // Spec
                            if (level2_node.NodeType == XmlNodeType.Comment) continue;

                            XmlElement level2_item = (XmlElement)level2_node;

                            string strSpecName = level2_item.GetAttribute("Name");
 
                            ProductSpec newSpec = new ProductSpec(ProductType, strSpecName);

                            foreach (XmlNode level3_node in level2_node)
                            { // Spec feature
                                if (level3_node.NodeType == XmlNodeType.Comment) continue;

                                XmlElement level3_item = (XmlElement)level3_node;

                                string strFeatureName = level3_item.GetAttribute("Name");
                                string strFeatureValue = level3_item.GetAttribute("Value");

                                newSpec.SetFeatureValueInString(strFeatureName, strFeatureValue);
                            }

                            _availSpecs.Add(newSpec);

                        }
                    }
                    else
                    {
                        LOG.Error(string.Format("不识别的节点"));
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    LOG.Error(string.Format("加载SimulateOrderList出错：{0}", ex.Message));
                    return false;
                }
            }

            return true;
        }
        
        public override ProductSpec GetLastProductType(string WorkStationName)
        {
            try
            {
                //List<string> _orderSpecs = GetAllSpecNames(); //订单里的产品类型集合

                Random rd = new Random();
                int select = rd.Next(0, _availSpecs.Count);//随机数不能取上界值
                ProductSpec selectSpec = _availSpecs[select];

                return selectSpec;
            }
            catch (Exception ex)
            {
                LOG.Error(string.Format("执行GetLastProductType出错：{0}",ex));
                return null;
            }
   
        }

        public override string ToJson()
        {
            string strSpec = string.Empty;

            if (this._availSpecs.Count > 0)
            {
                int i = 1;
                foreach (ProductSpec spec in _availSpecs)
                {
                    strSpec += string.Format("{{\"{0}\":{1}}}", spec.ParameterName, spec.ToJson());
                    if (i < this._availSpecs.Count)
                    {
                        strSpec += ",";
                        i++;
                    }
                }
            }
            else
            {
                strSpec = "\"NULL\"";
            }

            string strJson = strSpec;
            return strJson;
        }

        public override void UpdateOrderList()
        {
            throw new NotImplementedException();
        }

    #endregion
}
}
