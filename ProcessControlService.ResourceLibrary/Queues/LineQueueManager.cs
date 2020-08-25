// ==================================================
// 文件名：LineQueueCollection.cs
// 创建时间：2020/08/18 16:41
// ==================================================
// 最后修改于：2020/08/18 16:41
// 修改人：jians
// ==================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using log4net;
using Newtonsoft.Json;
using ProcessControlService.ResourceFactory;
using ProcessControlService.ResourceFactory.Attributes;
using ProcessControlService.ResourceFactory.ParameterType;
using ProcessControlService.ResourceLibrary.Action;
using ProcessControlService.ResourceLibrary.Products;
using ProcessControlService.ResourceLibrary.Tracking;

namespace ProcessControlService.ResourceLibrary.Queues
{
    public class LineQueueManager<T> : Resource, IActionContainer, IRedundancy, ILocation where T : ITrackUnit
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(LineQueueManager<>));

        private readonly Dictionary<string, Switch> _switches =
            new Dictionary<string, Switch>();

        private readonly Dictionary<string, FIFOQueue<T>> _trackUnitQueues =
            new Dictionary<string, FIFOQueue<T>>();

        private Type _trackUnitType=>typeof(T);

        public LineQueueManager(string resourceName) : base(resourceName)
        {
        }

        public FIFOQueue<T> this[string queueName]
        {
            get => !string.IsNullOrEmpty(queueName) && _trackUnitQueues.ContainsKey(queueName)
                ? _trackUnitQueues[queueName]
                : null;
            set
            {
                if (_trackUnitQueues.ContainsKey(queueName)) _trackUnitQueues[queueName] = value;

                throw new ArgumentOutOfRangeException($"AVI中不存在队列名：[{queueName}]");
            }
        }

        public string LocationId
        {
            get => ResourceName;
            set { }
        }

        public bool AcceptTrackUnit(ITrackUnit unit)
        {
            return unit.GetType() == _trackUnitType;
        }

        public bool CouldCreatePlace(ITrackUnit unit)
        {
            return false;
        }

        public bool HasOutput { get; set; } = false;
        
        /// <summary>
        ///     队列数据
        /// </summary>
        public int UnitCount => _trackUnitQueues.Count;
        
        public void RemoveUnit(ITrackUnit unit)
        {
            throw new NotImplementedException();
        }

        public ITrackUnit TakeUnit()
        {
            throw new NotImplementedException();
        }
        
        [JsonIgnore]
        public object LocationLocker { get; set; } = new object();

        public override bool LoadFromConfig(XmlNode node)
        {
            try
            {
                var xmlElement = XElement.Parse(node.OuterXml);
                
                var makeGenericType = typeof(FIFOQueue<>).MakeGenericType(typeof(T));

                //初始化队列
                var vehicleQueues = (from queue in xmlElement.Elements()
                    where queue.Name == "Queue"
                    let queueName = queue.Attribute("Name")?.Value
                    let queueSize=queue.Attribute("Size")?.Value
                    select new
                    {
                        QueueName=queueName,
                        Size=queueSize
                    }).ToList();

                foreach (var queueArgs in vehicleQueues)
                {
                    var lineQueue = (FIFOQueue<T>) Activator.CreateInstance(makeGenericType,queueArgs.QueueName);
                    if (int.TryParse(queueArgs.Size, out var queueSize))
                    {
                        lineQueue.QueueSize = queueSize;
                    }
                    
                    _trackUnitQueues.Add(queueArgs.QueueName, lineQueue);
                }

                //初始化路由
                var switches = (from route in xmlElement.Elements()
                    where route.Name == "Switch"
                    let switchName = route.Attribute("Name")?.Value
                    select new Switch(switchName)
                    {
                        Routes = (from a in route.Elements()
                                where a.Name == "Route"
                                let id = Convert.ToInt16(a.Attribute("Id")?
                                    .Value)
                                let queue = a.Attribute("Queue")?.Value
                                let isTerminal = a.Attribute("IsTerminal")?.Value.ToLower() == "true"
                                select new Route {Id = id, QueueName = queue, IsTerminal = isTerminal}).ToList()
                            .ToDictionary(a => a.Id, a => a)
                    }).ToList();

                //校验程序中是否已经生成路由中指定的队列。
                foreach (var @switch in switches)
                {
                    foreach (var keyValuePair in @switch.Routes.Where(keyValuePair =>
                        !_trackUnitQueues.ContainsKey(keyValuePair.Value.QueueName)))
                        throw new InvalidOperationException($"未配置指定的队列名：[{keyValuePair.Value}]");

                    _switches.Add(@switch.Name, @switch);
                }

                //从数据库中加载程序启动前产生的队列。
                foreach (var vehicleQueue in _trackUnitQueues)
                {
                    /*
                    var vehicles = AVICenter.FreeSql.Select<Vehicle>().Where(a => a.QueueName == vehicleQueue.Key)
                        .ToList();

                    if (vehicles.Count > 0)
                        vehicles.ForEach(a => vehicleQueue.Value.Enqueue(a.Vin, false /*不记录到数据库中#1#));
                */
                }

                return true;
            }
            catch (Exception e)
            {
                Log.Error($"初始化AVI队列失败,异常为：[{e.Message}]");
                return false;
            }
        }
        
        public override void FreeResource()
        {
        }

        public bool HasSwitch(string switchName)
        {
            return _switches.ContainsKey(switchName);
        }

        /// <summary>
        ///     根据路由入队
        /// </summary>
        /// <param name="switchName"></param>
        /// <param name="trackUnit"></param>
        /// <param name="routeId"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public void Enqueue(string switchName, ITrackUnit trackUnit, short routeId)
        {
            if (!_switches.ContainsKey(switchName))
                throw new ArgumentOutOfRangeException(nameof(switchName));

            var route = _switches[switchName][routeId];

            if (route == null) throw new ArgumentNullException($"查询不到路由名为Id为：[{routeId}]的对应队列名");

            var queueName = route.QueueName;

            //如果该路由点是工艺终点，不需要将VIN码存入到对应队列中。
            if (route.IsTerminal)
                return;

            if (_trackUnitQueues.ContainsKey(queueName)) _trackUnitQueues[queueName].PutUnit(trackUnit);
        }

        #region IActionContainer

        public void AddAction(BaseAction action)
        {
            throw new NotImplementedException();
        }

        public BaseAction GetAction(string actionName)
        {
            throw new NotImplementedException();
        }

        public void ExecuteAction(string name)
        {
            throw new NotImplementedException();
        }

        public string[] ListActionNames()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IRedundancy

        public void RedundancyModeChange(RedundancyMode mode)
        {
            CurrentRedundancyMode = mode;
        }

        public string BuildSyncData()
        {
            throw new NotImplementedException();
        }

        public void ExtractSyncData(string data)
        {
            throw new NotImplementedException();
        }

        public bool NeedDataSync { get; }

        public RedundancyMode CurrentRedundancyMode { get; set; }

        #endregion

        #region ResourceService

        /// <summary>
        ///     获取所有追踪对象队列
        /// </summary>
        /// <returns></returns>
        [ResourceService]
        public Dictionary<string, FIFOQueue<T>> GetAllQueues()
        {
            try
            {
                return _trackUnitQueues;
            }
            catch (Exception e)
            {
                return new Dictionary<string, FIFOQueue<T>>();
            }
        }

        [ResourceService]
        public int QueryQueueCount(string queueName)
        {
            if (_trackUnitQueues.ContainsKey(queueName))
            {
                return _trackUnitQueues[queueName].UnitCount;
            }

            return -1;
        }

        /// <summary>
        ///     获取TrackUnit所在队列
        /// </summary>
        /// <param name="trackUnitId"></param>
        /// <returns></returns>
        [ResourceService]
        public List<string> GetTrackUnitPosition(string trackUnitId)
        {
            var queueNames = new List<string>();

            foreach (var keyValuePair in _trackUnitQueues)
                if (keyValuePair.Value.HasQueueMember(trackUnitId.ToUpper()))
                    queueNames.Add(keyValuePair.Key);

            return queueNames;
        }

        [ResourceService]
        public void AddTestProduct()
        {
            var product = new Product($"LPH65BLI{new Random().Next(1001, 9999)}")
            {
                ProductType = (ProductType) CustomTypeManager.GetCustomizedType("TestProduct")
            };

            Enqueue("QueueA",product);
        }
        
        public ITrackUnit QueryUnit(string trackUnitId)
        {
            try
            {
                foreach (var keyValuePair in _trackUnitQueues)
                {
                    var queryUnit = keyValuePair.Value.QueryUnit(trackUnitId);
                    
                    if (queryUnit!=null)
                        return queryUnit;
                }

                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
        
        /// <summary>
        ///     TrackUnit出队
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        [ResourceService]
        public ITrackUnit Dequeue(string queueName)
        {
            try
            {
                var trackUnitQueue = _trackUnitQueues[queueName];

                var dequeue = trackUnitQueue.TakeUnit();

                return dequeue;
            }
            catch (Exception e)
            {
                Log.Error(e);

                return null;
            }
        }

        /// <summary>
        ///     根据队列名进队
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="trackUnit"></param>
        /// <returns></returns>
        [ResourceService]
        public string Enqueue(string queueName, ITrackUnit trackUnit)
        {
            try
            {
                var response = "";

                var enqueue = _trackUnitQueues[queueName].PutUnit(trackUnit);

                response = enqueue ? "Y" : "N";

                return response;
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                return "N";
            }
        }

        public bool PutUnit(ITrackUnit unit)
        {
            throw new NotImplementedException();
        }

        [ResourceService]
        public FIFOQueue<T> GetQueue(string queueName)
        {
             return _trackUnitQueues.ContainsKey(queueName) ? _trackUnitQueues[queueName] : null;
        }

        #endregion
    }
}