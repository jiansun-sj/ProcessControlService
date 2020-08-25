// ==================================================
// 文件名：ProductTray.cs
// 创建时间：2020/01/02 13:53
// ==================================================
// 最后修改于：2020/05/21 13:53
// 修改人：jians
// ==================================================

using System;
using System.Collections.Generic;
using log4net;
using ProcessControlService.ResourceLibrary.Products;

namespace ProcessControlService.ResourceLibrary.Tracking
{
    /// <summary>
    ///     托盘对象,只有相同属性的产品，可以放到同一个托盘里
    ///     Created By David Dong at 20180606
    /// </summary>
    public class ProductTray : UnitContainer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ProductTray));

        private readonly List<Product> _units = new List<Product>(); // 产品集合对象

        //private ProductTrayType _trayType;

        public ProductTray(TraySpec traySpec = null)
        {
            TraySpec = traySpec;
        }

        public new string Type => "ProductTray";

        public bool CouldContainProduct(Product product)
        {
            var productType = product.ProductType;

            foreach (var feature in TraySpec.GetFeatures())
                if (!feature.Value.Equals(productType.GetFeature(feature.Key)))
                    return false;
            return true;
        }

        public string ToJson()
        {
            var strJson = $"{{\"ProductTrayID\":\"{Id}\",\"Specs\":[{TraySpec.ToJson()}]}}";
            return strJson;
        }

        public override string ToString()
        {
            return ToJson();
        }

        #region TimeStamp

        private readonly DateTime _dateStart = DateTime.Now;

        public int RetentionSeconds //保留秒数
        {
            get
            {
                var dtNow = DateTime.Now;
                return Convert.ToInt32((dtNow - _dateStart).TotalSeconds);
            }
        }

        public bool OverRetentionTime() //超时
        {
            if (TraySpec != null) return RetentionSeconds > TraySpec.RetentionSecondsSetting;
            return false;
        }

        #endregion

        #region Tray spec

        public TraySpec TraySpec { get; set; }

        public bool IsNoSpec => TraySpec == null;

        public static ProductTray CreateProductTray(TraySpec traySpec)
        {
            var newProductTray = new ProductTray {TraySpec = (TraySpec) traySpec.Clone()};


            return newProductTray;
            //return null;
        }

        #endregion Tray spec

        #region UnitContainer

        public override short UnitCount => (short) _units.Count;

        protected override short ContainerSize => TraySpec.ProductCount;

        public override bool PutIn(ITrackUnit unit)
        {
            //if (UnitCount < ContainerSize)
            if (IsFull) return false;
            _units.Add((Product) unit);
            return true;
        }

        public override ITrackUnit TakeOut()
        {
            // 从头部拿掉一个
            if (_units.Count <= 0) return null;
            ITrackUnit result = _units[0];
            _units.RemoveAt(0);
            return result;
        }

        public override void TakeOut(ITrackUnit unit)
        {
            short removeIndex = 0;
            foreach (var product in _units)
            {
                if (product.Id == unit.Id) break;
                removeIndex++;
            }

            if (removeIndex <= _units.Count) _units.RemoveAt(removeIndex);
        }

        public override ITrackUnit GetCandidateUnit()
        {
            // 检查头部
            if (_units.Count <= 0) return null;
            ITrackUnit result = _units[0];
            return result;
        }


        public override bool AcceptUnit(ITrackUnit Unit)
        {
            try
            {
                if (Unit.Type != "Product")
                    //LOG.Error(string.Format("错误：托盘{0}放入的不是产品",ID));
                    return false;

                if (IsNoSpec)
                    //LOG.Error(string.Format("错误：托盘{0}没有规格", ID));
                    return false;

                if (IsFull)
                    //LOG.Error(string.Format("错误：托盘{0}已满", ID));
                    return false;

                var product = (Product) Unit;
                return CouldContainProduct(product);
            }
            catch (Exception ex)
            {
                Log.Error($"托盘{Id}产品{Unit.Id}检查出错{ex}");
                return false;
            }
        }

        public override bool HasUnit(ITrackUnit unit)
        {
            foreach (var product in _units)
                if (product.Id == unit.Id)
                    return true;

            return false;
        }

        #endregion
    }
}