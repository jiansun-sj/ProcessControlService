using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ProcessControlService.ResourceFactory;
using ProcessControlService.ResourceLibrary.Tracking;
using log4net;
using Newtonsoft.Json;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;


namespace ProcessControlService.ResourceLibrary.Storages
{
    /// <summary>
    /// 存储区组
    /// Created by Dongmin 20180817
    /// </summary>
    public class StorageGroup : Storage
    {
        private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(StorageGroup));

        private List<Storage> SubStorages = new List<Storage>(); // 并排子区域

        public StorageGroup(string Name) : base(Name)
        {
        }

        #region "Resource definition"

        public override string ResourceType
        {
            get
            {
                return "StorageGroup";
            }
        }

        public override IResource GetResourceObject()
        {
            return this;
        }

        public override bool LoadFromConfig(XmlNode node)
        {
            try
            {
                foreach (XmlNode level1_node in node)
                { // level1 --  "SubStorages"
                    if (level1_node.NodeType == XmlNodeType.Comment)
                        continue;


                    XmlElement level1_item = (XmlElement)level1_node;

                    if (level1_item.Name.ToLower() == "SubStorages".ToLower())
                    {
                        #region SubStorages
                        foreach (XmlNode level2_node in level1_node)
                        { // datasource
                            if (level2_node.NodeType == XmlNodeType.Comment)
                                continue;

                            XmlElement level2_item = (XmlElement)level2_node;
                            if (level2_item.Name.ToLower() == "SubStorage".ToLower())
                            {
                                string strStorageName = level2_item.GetAttribute("Name");
                                Storage subStorage = (Storage)ResourceManager.GetResource(strStorageName);

                                SubStorages.Add(subStorage);
                            }

                        }
                        #endregion
                    }

                }

                return base.LoadFromConfig(node);
            }
            catch (Exception ex)
            {
                LOG.Error(string.Format("加载StorageGroup{0}出错：{1}", ResourceName, ex.Message));
                return false;
            }
        }

        #endregion

        #region "Resource Service"

        override public string GetStatus()
        {
            StorageGroupStatusModel statusModel = new StorageGroupStatusModel(Count, Size, SubStorages);
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

        #region "Storage interface"

        override public Int32 Count //存货计数
        {
            get
            {
                int totalCount = 0;
                foreach (Storage storage in SubStorages)
                {
                    totalCount += storage.Count;
                }
                return totalCount;
            }
        }

        override public Int32 Size 
        {
            get
            {
                    int totalSize = 0;
                    foreach (Storage storage in SubStorages)
                    {
                        totalSize += storage.Size;
                    }
                    return totalSize;
            }
        }

        override public void Clear()
        {
            foreach (Storage storage in SubStorages)
            {
                storage.Clear();
            }
        }

        override public void Entry(TrackingUnit2 Item)
        { //根据规则进道，需要重写 -- Dongmin 20180817
            if (Item == null)
            {
                return ;
            }

            //int ID = entrayVehicleRule.GetNextEntryLane(Lanes, Item);
            //if (ID != -1)
            //{
            //    Lanes[ID - 1].Entry(Item);
            //    exitVehicleRule.AddVehicle(Item);
            //    return true;
            //}
            //else
            //{
            //    LOG.Info(string.Format("{0} Entry Faild", _storageName));
            //    return false;
            //}
        }

        // 进入一个指定子区域
        public void EntrySubGroup(Int16 SubGroupID, TrackingUnit2 Item)
        {
            try
            {
                SubStorages[SubGroupID].Entry(Item);
            }
            catch (Exception ex)
            {
                LOG.Error(string.Format("进入SubGroup出错;{0}",ex));
            }
        }

        override public TrackingUnit2 Exit()
        { //根据规则出道，需要重写 -- Dongmin 20180817
            //TrackingUnit body = null;
            //int ID = exitVehicleRule.GetNextExitLane(Lanes);
            //if (ID != -1)
            //{
            //    body = Lanes[ID - 1].Exit();
            //    LastExitID = ID;
            //    exitVehicleRule.DeleteVehicle(body);

            //}
            //return body;
            return null;
        }

        // 从一个指定子区域移出
        public TrackingUnit2 ExitSubGroup(Int16 SubGroupID)
        {
            try
            {
                return SubStorages[SubGroupID].Exit();
            }
            catch (Exception ex)
            {
                LOG.Error(string.Format("进入SubGroup出错;{0}", ex));
                return null;
            }
        }

        override protected void CreateActions()
        {
            // add actions
            _actions.AddAction(new EntryStorageGroupAction(this, "EntryStorageGroupAction"));
            _actions.AddAction(new ExitStorageGroupAction(this, "ExitStorageGroupAction"));

            base.CreateActions();
        }

        //override public int GetEntryLaneID(TrackingUnit2 Item)
        //{
        //    if (Item == null)
        //    { return -1; }
        //    int ID = entrayVehicleRule.GetNextEntryLane(Lanes, Item);
        //    return ID;
        //}

        //int LastExitID = -1;


        //public override void Remove(TrackingUnit body)
        //{
        //    if (LastExitID > 0)
        //    {
        //        Lanes[LastExitID - 1].Delete(body);
        //        exitVehicleRule.ChangeLot();
        //    }
        //}

        //public override int GetExitLaneID()
        //{
        //    return exitVehicleRule.GetNextExitLane(Lanes);
        //}

        //public override bool UpdateLaneBodys()
        //{
        //    try
        //    {
        //        foreach (var lane in Lanes)
        //        {
        //            lane.LoadBodyFromPLC();
        //        }
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        LOG.Info(string.Format("更新区域{0}车辆ID出错 {1}", ResourceName, ex.Message));
        //        return false;
        //    }

        //}


        //private string GetConfigFile()
        //{
        //    string appPath = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
        //    string xmlPath = appPath + "Config";

        //    var files = Directory.GetFiles(xmlPath, "*.xml");
        //    return files[0];
        //}

        //public void SetLaneProperty(Dictionary<string, string> dic)
        //{
        //    XmlDocument doc = new XmlDocument();
        //    string filename = GetConfigFile();
        //    doc.Load(filename);
        //    XmlNode node = doc.SelectSingleNode("root/Resources/Resource[@Name='" + ResourceName + "']/VehicleLanes/VehicleLane[@ID='" + dic["LaneID"] + "']");
        //    XmlElement ele = (XmlElement)node;
        //    int laneID = Convert.ToInt32(dic["LaneID"]);

        //    VehicleLane lane = Lanes[laneID - 1];

        //    if (dic.ContainsKey("Purpose"))
        //    {
        //        lane.Purpose = (LanePurpose)Enum.Parse(typeof(LanePurpose), dic["Purpose"]);
        //        ele.SetAttribute("Purpose", dic["Purpose"]);
        //    }
        //    if (dic.ContainsKey("IsContinueOut"))
        //    {
        //        lane.IsContinueOut = Convert.ToBoolean(dic["IsContinueOut"]);

        //        ele.SetAttribute("IsContinueOut", dic["IsContinueOut"]);

        //    }
        //    if (dic.ContainsKey("Size"))
        //    {
        //        lane.Length = Convert.ToInt32(dic["Size"]);
        //        ele.SetAttribute("Size", dic["Size"]);
        //    }
        //    if (dic.ContainsKey("Priority"))
        //    {
        //        lane.Priority = Convert.ToInt32(dic["Priority"]);
        //        ele.SetAttribute("Priority", dic["Priority"]);
        //    }
        //    doc.Save(filename);
        //}

        //public void SetLaneFeatures(Dictionary<string, string> dic)
        //{
        //    XmlDocument doc = new XmlDocument();
        //    string filename = GetConfigFile();
        //    doc.Load(filename);

        //    string strLaneID = dic["LaneID"];
        //    int laneID = Convert.ToInt32(dic["LaneID"]);

        //    VehicleLane lane = Lanes[laneID - 1];

        //    dic.Remove("LaneID");

        //    XmlNode Node = doc.SelectSingleNode("root/Resources/Resource[@Name='" + ResourceName + "']/VehicleLanes/VehicleLane[@ID='" + strLaneID + "']/Features");
        //    Node.RemoveAll();
        //    lane.Features.Clear();
        //    foreach (var item in dic.Keys)
        //    {
        //        Feature feature = new Feature(item, "string", dic[item]);
        //        lane.Features.AddFeature(feature);

        //        XmlElement featureNode = doc.CreateElement("Feature");
        //        featureNode.SetAttribute("VehicleFeature", item);
        //        featureNode.SetAttribute("FeatureType", "String");
        //        featureNode.SetAttribute("FeatureValue", dic[item]);
        //        Node.AppendChild(featureNode);
        //    }
        //    doc.Save(filename);
        //}

        //public void DeleteLaneFeatures(Dictionary<string, string> dic)
        //{
        //    int laneID = Convert.ToInt32(dic["LaneID"]);

        //    VehicleLane lane = Lanes[laneID - 1];

        //    dic.Remove("LaneID");

        //    foreach (var item in dic.Keys)
        //    {
        //        lane.Features.DeleteFeature(item);

        //    }
        //}

        //public void SetLevelingRules(Dictionary<string, string> dic)
        //{
        //    exitVehicleRule.SetLevelingRules(dic);

        //    XmlDocument doc = new XmlDocument();
        //    string filename = GetConfigFile();
        //    doc.Load(filename);
        //    XmlNode parNode = doc.SelectSingleNode("root/Resources/Resource[@Name='"
        //             + ResourceName + "']/OutRules");

        //    XmlElement node = (XmlElement)doc.SelectSingleNode("root/Resources/Resource[@Name='"
        //             + ResourceName + "']/OutRules/OutRule[@ID='" + dic["ID"] + "']");

        //    if (node is null)
        //    {
        //        XmlElement ele = doc.CreateElement("OutRule");
        //        ele.SetAttribute("ID", dic["ID"]);
        //        ele.SetAttribute("Type", dic["RuleName"]);
        //        ele.SetAttribute("FeatureName", dic["FeatureName"]);
        //        ele.SetAttribute("FeatureValue", dic["FeatureValue"]);
        //        ele.SetAttribute("Value", dic["UnmatchValue"]);
        //        parNode.AppendChild(ele);
        //    }
        //    else
        //    {
        //        node.SetAttribute("Type", dic["RuleName"]);
        //        node.SetAttribute("FeatureName", dic["FeatureName"]);
        //        node.SetAttribute("FeatureValue", dic["FeatureValue"]);
        //        node.SetAttribute("Value", dic["UnmatchValue"]);
        //    }
        //    doc.Save(filename);
        //}

        //public string GetLevelingRules()
        //{
        //    return JsonConvert.SerializeObject(exitVehicleRule.GetLevelingRules());
        //}

        //public bool RemoveLevelingRules(Dictionary<string, string> dic)
        //{
        //    exitVehicleRule.RemoveLevelingRules(dic);

        //    XmlDocument doc = new XmlDocument();
        //    string filename = GetConfigFile();
        //    doc.Load(filename);
        //    switch (dic["RuleName"])
        //    {
        //        case "MaxContinuesCheckRule":
        //            XmlNode node = doc.SelectSingleNode("root/Resources/Resource[@Name='"
        //             + ResourceName + "']/OutRules/OutRule[@ID='" +
        //             dic["ID"] + "']");
        //            if (node != null)
        //            {
        //                node.ParentNode.RemoveChild(node);
        //            }
        //            break;
        //        case "IntervalCheckRule":
        //            XmlNode intervalNode = doc.SelectSingleNode("root/Resources/Resource[@Name='"
        //             + ResourceName + "']/OutRules/OutRule[@ID='" +
        //             dic["ID"] + "']");
        //            if (intervalNode != null)
        //            {
        //                intervalNode.ParentNode.RemoveChild(intervalNode);
        //            }
        //            break;

        //    }
        //    doc.Save(filename);
        //    return true;
        //}

        #endregion

    }

    [DataContract]
    class StorageGroupStatusModel
    {
        [DataMember]
        Int32 Count = 0; //仓库货物数量

        [DataMember]
        Int32 Size = 0; //仓库货物容量

        [DataMember]
        List<string> Storages = new List<string>(); //部品列表

        public StorageGroupStatusModel(Int32 Count, Int32 Size, List<Storage> Storages)
        {
            this.Count = Count;
            this.Size = Size;

            foreach (Storage storage in Storages)
            {
                this.Storages.Add(storage.GetStatus());
            }
        }

    }
}
  
