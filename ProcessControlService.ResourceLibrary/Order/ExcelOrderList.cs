// ==================================================
// 文件名：ExcelOrderList.cs
// 创建时间：2019/11/16 15:49
// 上海芸浦信息技术有限公司
// copyright@yumpoo
// ==================================================
// 最后修改于：2019/12/26 15:49
// 修改人：jians
// ==================================================

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Xml;
using log4net;
using ProcessControlService.ResourceLibrary.Products;
using ProcessControlService.ResourceLibrary.ProductOrder;

namespace ProcessControlService.ResourceLibrary.Order
{
    /// <summary>
    ///     生成Excel格式的订单列表
    ///     Created by Dongmin 20180622
    /// </summary>
    public class ExcelOrderList : ProductOrder.OrderList
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(ExcelOrderList));

        private string _excelFile;
        private string _excelSheet;

        private readonly Dictionary<string, char> _featuresInExcel = new Dictionary<string, char>();
        private char _idColumn;

        private DataSet _orderDS;

        private short _startRow;

        private char _workStationColumn;

        public ExcelOrderList(string Name) : base(Name)
        {
        }


        private DataSet ExcelToDS(string FilePath, string ExcelSheet)
        {
            var strConn = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + FilePath + ";" +
                          "Extended Properties='Excel 8.0;HDR=YES;IMEX=1';";
            //string strConn = "Provider=Microsoft.ACE.OLEDB.12.0;" + "Data Source=" + FilePath + ";" + "Extended Properties='Excel 12.0;HDR=YES;IMEX=1';";
            var conn = new OleDbConnection(strConn);
            conn.Open();
            var strExcel = "";
            OleDbDataAdapter myCommand = null;
            DataSet ds = null;
            strExcel = string.Format("select * from [{0}$]", ExcelSheet);
            myCommand = new OleDbDataAdapter(strExcel, strConn);
            ds = new DataSet();
            myCommand.Fill(ds, "table1");
            return ds;
        }

        // Excel里的列标识转换成列号
        private short ExcelColumnLabelToNo(char ColumnLabel)
        {
            if (ColumnLabel > 'z' || ColumnLabel < 'a') return -1; //不合法

            var c1 = (short) ColumnLabel;
            var c2 = (short) 'a';


            return (short) (c1 - c2);
        }

        //private Int32 _currentOrderIndex = 0;

        #region "Spec List"

        private readonly List<ProductSpec> _order = new List<ProductSpec>(); //产品规格集合
        private readonly List<bool> _orderUsed = new List<bool>(); //产品规格是否被用过

        #endregion

        #region 接口实现

        public override bool LoadFromConfig(XmlNode node)
        {
            try
            {
                if (!base.LoadFromConfig(node)) return false;

                if (node.NodeType == XmlNodeType.Comment)
                    return true;

                var level1_item = (XmlElement) node;

                _excelFile = level1_item.GetAttribute("ExcelFile");
                _excelSheet = level1_item.GetAttribute("ExcelSheet");

                _startRow = Convert.ToInt16(level1_item.GetAttribute("StartRow"));

                _workStationColumn = Convert.ToChar(level1_item.GetAttribute("WorkStationColumn").ToLower());

                foreach (XmlNode level2_node in node)
                {
                    // Features
                    if (level2_node.NodeType == XmlNodeType.Comment) continue;

                    var level2_item = (XmlElement) level2_node;

                    if (level2_item.Name == "Features")
                    {
                        _idColumn = Convert.ToChar(level2_item.GetAttribute("IDColumn").ToLower());

                        foreach (XmlNode level3_node in level2_node)
                        {
                            // Feature
                            if (level3_node.NodeType == XmlNodeType.Comment) continue;

                            var level3_item = (XmlElement) level3_node;

                            if (level3_item.Name == "Feature")
                            {
                                var strFeatureName = level3_item.GetAttribute("Name");
                                var strColumn = Convert.ToChar(level3_item.GetAttribute("Column").ToLower());

                                _featuresInExcel.Add(strFeatureName, strColumn);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LOG.Error(string.Format("加载ExcelOrderList {0}出错：{1}", ResourceName, ex.Message));
                return false;
            }

            return true;
        }

        /*
        public override TrackingUnit GetLastOrder()
        {
            throw new NotImplementedException();
        }
        */

        public override ProductSpec GetLastProductType(string WorkStationName)
        {
            lock (this)
            {
                try
                {
                    for (var i = 0; i < _order.Count; i++)
                    {
                        if (_orderUsed[i]) continue;

                        var spec = _order[i];
                        if (spec.WorkStationName == WorkStationName)
                        {
                            _orderUsed[i] = true;
                            HandledOrderCount++; //已处理订单数加一
                            return spec;
                        }
                    }

                    return null;
                }
                catch (Exception ex)
                {
                    LOG.Error(string.Format("执行GetLastProductType出错：{0}", ex));
                    return null;
                }
            }
        }

        public override string ToJson()
        {
            var strSpec = string.Empty;

            if (_order.Count > 0)
            {
                var i = 1;
                foreach (var spec in _order)
                {
                    strSpec += string.Format("{{\"{0}\":{1}}}", spec.ParameterName, spec.ToJson());
                    if (i < _order.Count)
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

            var strJson = strSpec;
            return strJson;
        }

        public override void UpdateOrderList()
        {
            try
            {
                _orderDS = ExcelToDS(_excelFile, _excelSheet);

                TotalOrderCount = _orderDS.Tables[0].Rows.Count; //总订单数
                HandledOrderCount = 0; //已处理订单数

                foreach (DataRow row in _orderDS.Tables[0].Rows)
                {
                    // 每条订单记录
                    var i_idColumn = ExcelColumnLabelToNo(_idColumn);
                    var specID = (string) row[i_idColumn];

                    var newSpec = new ProductSpec(ProductType, specID);

                    var i_workStationColumn = ExcelColumnLabelToNo(_workStationColumn);
                    var workStationName = (string) row[i_workStationColumn];
                    newSpec.WorkStationName = workStationName;

                    foreach (var strFeatureName in ProductType.GetFeatures().Keys)
                    {
                        // 每一个产品特征
                        var c_excelColumn = _featuresInExcel[strFeatureName];
                        var i_excelColumn = ExcelColumnLabelToNo(c_excelColumn);

                        var strFeatureValue = Convert.ToString(row[i_excelColumn]);

                        newSpec.SetFeatureValueInString(strFeatureName, strFeatureValue);
                    }

                    _order.Add(newSpec); //增加到订单列表
                    _orderUsed.Add(false); //增加未使用标志
                }
            }
            catch (Exception ex)
            {
                LOG.Error(string.Format("UpdateOrderList 出错：{0}", ex.Message));
            }
        }

        #endregion
    }
}