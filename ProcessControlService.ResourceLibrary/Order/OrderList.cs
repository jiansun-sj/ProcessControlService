using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using ProcessControlService.ResourceFactory;
using ProcessControlService.ResourceLibrary.Action;
using ProcessControlService.ResourceLibrary.Tracking;
using ProcessControlService.ResourceLibrary.Products;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.IO;
using ProcessControlService.Contracts;

namespace ProcessControlService.ResourceLibrary.Order
{
    /// <summary>
    /// 订单基本类
    /// Created by Dongmin 20180218
    /// </summary>
    public abstract class OrderList : IResource,IResourceExportService, IActionContainer
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(OrderList));

        protected string OrderListName;

        protected ProductType ProductType; //订单产品类型

        protected int HandledOrderCount = 0; //已经处理的订单数量

        protected int TotalOrderCount = 0; //总订单数量

        protected OrderList(string name)
        {
            OrderListName = name;

            CreateActions();

        }

        #region "Resource definition"

        public virtual string ResourceName => OrderListName;

        public virtual string ResourceType => "OrderList";

        public virtual IResource GetResourceObject()
        {
            return this;
        }

        public virtual bool LoadFromConfig(XmlNode node)
        {
            try
            {

                if (node.NodeType == XmlNodeType.Comment)
                    return true;

                var level1Item = (XmlElement)node;

                var strProductType = level1Item.GetAttribute("ProductType");

                ProductType = (ProductType)CustomizedTypeManager.GetCustomizedType(strProductType);


                return true;

            }
            catch (Exception ex)
            {
                Log.Error($"加载OrderList {ResourceName}出错：{ex.Message}");
                return false;
            }
        }

        public virtual void FreeResource()
        {
            //StopWork();
        }

      
        public IResourceExportService GetExportService()
        {
            return this;
        }

        #endregion

        public RedundancyMode CurrentRedundancyMode { get; } = RedundancyMode.Unknown;


        #region "Resource Service"

        ////guyang  2018年2月28日09:57:58
        //public List<string> GetExportServiceNames()
        //{
        //    List<string> ServiceNames = new List<string>();

        //    ServiceNames.Add("GetOrderList");
        //    ServiceNames.Add("GetStatus");
        //    return ServiceNames;
        //    //return null;
        //}

        public List<ResourceServiceModel> GetExportServices()
        {
            var services = new List<ResourceServiceModel>();

            var newService1 = new ResourceServiceModel {Name = "GetOrderList", Parameters = null};
            services.Add(newService1);

            var newService2 = new ResourceServiceModel {Name = "GetStatus", Parameters = null};
            services.Add(newService2);

            return services;
        }

        public string CallExportService(string ServiceName,string strParameter)
        {
            switch (ServiceName)
            {
                case "GetOrderList":
                    return ToJson();
                case "GetStatus":
                    return GetStatus();
                default:
                    throw new Exception($"所调用接口{ServiceName}不存在.");
            }

            //return null;
        }

        public virtual string GetStatus()
        {
            OrderListStatusModel statusModel = new OrderListStatusModel(HandledOrderCount, TotalOrderCount);
            DataContractJsonSerializer json = new DataContractJsonSerializer(statusModel.GetType());
            string szJson = "";
            using (MemoryStream stream = new MemoryStream())
            {
                json.WriteObject(stream, statusModel);
                szJson = Encoding.UTF8.GetString(stream.ToArray());
            }
            return szJson;
        }

        #endregion

        #region "ActionContainer"

        protected ActionCollection Actions = new ActionCollection();

        public void AddAction(BaseAction action)
        {
            Actions.AddAction(action);
        }

        public BaseAction GetAction(string name)
        {
            return (OrderListAction)Actions.GetAction(name);
        }

        public virtual void ExecuteAction(string name)
        {
            var action = (OrderListAction)GetAction(name);
            action.Execute();
        }

        public string[] ListActionNames()
        {
            return Actions.ListActionNames();
        }

        protected virtual void CreateActions()
        {
            //add actions
            Actions.AddAction(new GetLastOrderAction(this, "GetLastOrderAction"));
            Actions.AddAction(new GetOrderListJsonAction(this, "GetOrderListJsonAction"));
            Actions.AddAction(new GetLastProductTypeAction(this, "GetLastProductTypeAction"));
            Actions.AddAction(new UpdateOrderListAction(this, "UpdateOrderListAction"));
        }


        #endregion
        
        // 获得最近的产品订单 -- 为了保持和 VehicleStorage 兼容
        /*
        public abstract TrackingUnit GetLastOrder();
        */

        // 获得最近的产品订单 -- 正解
        public abstract ProductSpec GetLastProductType(string workStationName);

        // 以json格式返回数据
        public abstract string ToJson();

        // 刷新订单
        public abstract void UpdateOrderList();
        
    }

    [DataContract]
    class OrderListStatusModel
    {
        [DataMember] private int _handledCount = 0; //已处理订单数

        [DataMember] private int _totalCount = 0; // 总订单数

        public OrderListStatusModel(int handledCount, int totalCount)
        {
            _handledCount = handledCount;
            _totalCount = totalCount;
        }

    }
}
