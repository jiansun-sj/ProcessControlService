using System;
using System.Xml;
using ProcessControlService.ResourceLibrary.Action;
using ProcessControlService.ResourceLibrary.Products;
using ProcessControlService.ResourceFactory;
using ProcessControlService.ResourceFactory.ParameterType;

namespace ProcessControlService.ResourceLibrary.Order
{
    /// <summary>
    /// Order List Action基础类
    /// Created By David Dong at 201702022
    /// </summary>
    public abstract class OrderListAction : BaseAction
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(OrderListAction));

        protected OrderList OwnerOrderList;
        public OrderList OwnerStorage => OwnerOrderList;

        protected OrderListAction(OrderList orderList, string name) : base(name)
        {
            OwnerOrderList = orderList;
        }


        #region "Core functions"

        //public abstract void Execute();

        //public abstract bool IsSuccessful();

        //public abstract object GetResult();

        public override bool LoadFromConfig(XmlNode node)
        {
            throw new NotImplementedException();
        }

        #endregion

    }

    /// <summary>
    /// 刷新订单操作Action
    /// Created by David Dong 20180623
    /// </summary>
    public class UpdateOrderListAction : OrderListAction
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(UpdateOrderListAction));

        public UpdateOrderListAction(OrderList orderList, string Name) : base(orderList, Name)
        {
        }

        #region "Core functions"

        public override void Execute()
        {
            OwnerOrderList.UpdateOrderList();
        }


        public override bool IsSuccessful()
        {
            return true;
        }

        public override object GetResult()
        {
            throw new NotImplementedException();
        }

        #endregion

        protected override bool CreateParameters()
        {
            return true;
        }

        public override BaseAction Create()
        {
            var basAction = new UpdateOrderListAction(OwnerOrderList,Name)
            {
                ActionInParameterManager = ActionInParameterManager.Clone(),
                ActionOutParameterManager = ActionOutParameterManager.Clone(),
            };
            return basAction;

        }
    }

    /// <summary>
    /// 查询最新订单操作Action
    /// Created by David Dong 20180218
    /// </summary>
    public class GetLastOrderAction : OrderListAction
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(GetLastOrderAction));

        public GetLastOrderAction(OrderList orderList,string name) : base(orderList, name)
        {
        }

        #region "Core functions"

        public override void Execute()
        {
           
            /*
            var order = OwnerOrderList.GetLastOrder();
            
            ActionOutParameterManager["LastOrder"].SetValue(order);
            */
           
        }
    

        public override bool IsSuccessful()
        {
            return true;
        }

        public override object GetResult()
        {
            throw new NotImplementedException();
        }

        #endregion

        protected override bool CreateParameters()   
        {
            //  参数1：跟踪项目ID号
            //ActionOutParameterManager.Add(new Parameter("LastOrder", "Object", "Tracking.TrackingUnit"));

            return true;
        }

        public override BaseAction Create()
        {
            var basAction = new GetLastOrderAction(OwnerOrderList,Name)
            {
                ActionInParameterManager = ActionInParameterManager.Clone(),
                ActionOutParameterManager = ActionOutParameterManager.Clone(),
            };
            return basAction;

        }
    }

    /// <summary>
    /// 查询最新订单产品类型操作Action 
    /// Created by David Dong 20180602
    /// </summary>
    public class GetLastProductTypeAction : OrderListAction
    {
        private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(GetLastProductTypeAction));

        public GetLastProductTypeAction(OrderList OrderList, string Name) : base(OrderList, Name)
        {
        }

        #region "Core functions"

        public override void Execute()
        {
            string _wsName = (string)ActionInParameterManager["WorkStationName"].GetValue();
            ProductSpec productSpec = OwnerOrderList.GetLastProductType(_wsName);

            ActionOutParameterManager["ResultProductType"].SetValue(productSpec);

        }


        public override bool IsSuccessful()
        {
            return true;
        }

        public override object GetResult()
        {
            throw new NotImplementedException();
        }

        #endregion

        protected override bool CreateParameters()
        {
            /*
            //  参数1：跟踪项目ID号
            ActionInParameterManager.Add(new Parameter("WorkStationName", "String"));

            //  参数1：跟踪项目ID号
            ActionOutParameterManager.Add(new Parameter("ResultProductType", "Object", "Products.ProductSpec"));
*/

            return true;
        }

        public override BaseAction Create()
        {
            var basAction = new GetLastProductTypeAction(OwnerOrderList, Name)
            {
                ActionInParameterManager = ActionInParameterManager.Clone(),
                ActionOutParameterManager = ActionOutParameterManager.Clone(),
            };
            return basAction;
        }
    }

    /// <summary>
    /// 以JSON格式获得
    /// </summary>
    public class GetOrderListJsonAction : OrderListAction
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(GetOrderListJsonAction));

        public GetOrderListJsonAction(OrderList OrderList, string Name) : base(OrderList, Name)
        {
        }

        #region "Core functions"

        public override void Execute()
        {
            string jsonStroage = OwnerOrderList.ToJson();
            ActionOutParameterManager["OrderListJson"].SetValue(jsonStroage);
        }

        public override bool IsSuccessful()
        {
            return true;
        }

        public override object GetResult()
        {
            throw new NotImplementedException();
        }

        #endregion

        protected override bool CreateParameters()
        {
            //  参数1：订单json输出
            /*
            ActionOutParameterManager.Add(new Parameter("OrderListJson", "String"));
*/

            return true;
        }

        public override BaseAction Create()
        {
            var basAction = new GetOrderListJsonAction(OwnerOrderList, Name)
            {
                ActionInParameterManager = ActionInParameterManager.Clone(),
                ActionOutParameterManager = ActionOutParameterManager.Clone(),
            };
            return basAction;
        }
    }
}
