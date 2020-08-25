using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;
using Newtonsoft.Json;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using ProcessControlService.ResourceFactory;
using ProcessControlService.ResourceLibrary.Action;
using ProcessControlService.ResourceLibrary.Products;
using ProcessControlService.ResourceLibrary.Tracking;

namespace ProcessControlService.ResourceLibrary.Storages
{
    /// <summary>
    /// 基于数据库表结构的队列
    /// Dongmin 20180811
    /// </summary>
    public class DBTableQueue : Storage
    {
        private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(DBTableQueue));

        public DBTableQueue(string Name) : base(Name)
        {

        }

        #region DB Config

        private DataBaseHelper _dbHelper;

        private string _tableName;
        private string _fieldQueueName;
        private string _fieldPosition;
        private string _fieldItemID;

        #endregion

        #region Storage

        public override Int32 Count => GetDBItemCount();

        public Int16 LastPosition => GetDBLastPosition();

        //查询队列
        override public string GetPositionItemID(StoragePosition Pos)
        {
            return GetDBItemAtPosition(Pos.GetDiemensionValue());
        }

        //进入队列
        override public void Entry(TrackingUnit2 Item)
        {
            int _count = Count;
            Int16 lastPos = (Int16)((_count > 0)? LastPosition:0);

            if (lastPos >= Size)
            {
                throw new Exception(string.Format("队列{0}已满，无法插入",ResourceName));
            }

            Int16 insertPos = (Int16)(_count == 0 ? 0:(Int16)(lastPos + 1));

            PutIn(insertPos, Item);
        }

        //放入队列指定位置
        override public void PutIn(StoragePosition Pos, TrackingUnit2 Item)
        {
            if (Pos.DimensionCount == 1)
            {
                Int16 PosX = Pos.GetDiemensionValue(StoragePositionDimension.X);

                PutIn(PosX, Item);
            }
            else
            {
                throw new Exception(_storageName + "位置坐标维度错误");
            }
        }

        //放入队列指定位置
        public void PutIn(Int16 Pos, TrackingUnit2 Item)
        {
            if (Count < Size)
            {
                InsertDBItem(Pos, Item.ID);
            }
            else
            {
                throw new Exception(_storageName + "进队列出错,数量已满");
            }
        }

        //移出队列
        override public TrackingUnit2 Exit()
        {
            try
            {
                if (0 == Count)
                {
                    throw new Exception(string.Format("队列{0}是空，无法插入", ResourceName));
                }

                Dictionary<Int16, string> queue = GetDBItemList();
                string exitID = queue[0]; //记录头一个

                queue.Remove(0); //删除头一个
                DeleteDBItem(0);

                // 把剩余的Item向前移
                foreach (KeyValuePair<Int16, string> item in queue)
                {
                    UpdatePositionOfItem(item.Value, (short)(item.Key - 1));
                }

                Product product = Product.CreateProduct(exitID);
                return product;
            }
            catch (Exception ex)
            {
                LOG.Error(string.Format("DBTableQueuey {0}移出失败:{1}",ResourceName, ex));
                return null;
            }
        }

        //从指定位置拿出
        override public TrackingUnit2 TakeOut(StoragePosition Pos)
        {
            if (Pos.DimensionCount == 1)
            {
                int PosX = Pos.GetDiemensionValue(StoragePositionDimension.X);

                return TakeOut(PosX);
            }
            else
            {
                throw new Exception(_storageName + "位置坐标维度错误");
            }
        }

        //从指定位置拿出
        public TrackingUnit2 TakeOut(int index)
        {
            if (Count > 0)
            {
                //TrackingUnit2 temp = _queue[index];
                //_queue.RemoveAt(index);

                return null;
            }
            else
            {
                throw new Exception(_storageName + "出队列出错,数量为0");
            }
        }

        //清除队列
        override public void Clear()
        {
            DeleteAllDBItem();
        }

        public TrackingUnit2 GetItem(int index)
        {
            try
            {
                //return _queue[index];
                return null;
            }
            catch (Exception ex)
            {
                LOG.Error(string.Format("DBTableQueue：{0}获取数据失败{1}", ResourceName,ex));
                return null;
            }
        }
      
        override public string GetStatus()
        {
          
            DBTableQueueStatusModel statusModel = new DBTableQueueStatusModel(Size, GetDBItemList());

            DataContractJsonSerializer json = new DataContractJsonSerializer(statusModel.GetType());
            string szJson = "";
            using (MemoryStream stream = new MemoryStream())
            {
                json.WriteObject(stream, statusModel);
                szJson = Encoding.UTF8.GetString(stream.ToArray());
            }
            return szJson;
        }

        private void InitQueue()
        {

        }

        #endregion

        #region DB Query
        //获得整个队列
        private Dictionary<Int16,string> GetDBItemList()
        {
            Int16 ItemCount = GetDBItemCount();

            Dictionary<Int16, string> Items = new Dictionary<Int16, string>(); //初始化Item队列

            string sqlMySQL = string.Format("select {0},{1} from {2} where {3}='{4}'", _fieldPosition,_fieldItemID, _tableName, _fieldQueueName, ResourceName);

            if (_dbHelper != null)
            {
                DataSet ds = _dbHelper.GetDataSet(CommandType.Text, sqlMySQL);
                if (ds.Tables[0].Rows.Count != 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    { //遍历每行

                        Int16 pos = Convert.ToInt16(row[0]); // Position
                        string itemID = Convert.ToString(row[1]); // Item ID

                        if (pos >= 0 && pos < Size)
                        {
                            Items.Add(pos, itemID);
                        }
                    }
                }
                return Items;
            }

            return null;
        }

        //查询队列项目数量
        private Int16 GetDBItemCount()
        {
            try
            {
                List<object> values = new List<object>();
                string sqlMySQL = string.Format("select count(*) from {0} where {1}='{2}'", _tableName, _fieldQueueName, ResourceName);
                
                DataSet ds = _dbHelper.GetDataSet(CommandType.Text, sqlMySQL);
                Int16 resultCount = Convert.ToInt16(ds.Tables[0].Rows[0].ItemArray[0]);

                return resultCount;

            }
            catch (Exception ex)
            {
                LOG.Error("DBTableQueue获取Item数量失败:{0}", ex);
            }

            return 0;
        }

        //查询队列最大位置
        private Int16 GetDBLastPosition()
        {
            try
            {
                List<object> values = new List<object>();
                string sqlMySQL = string.Format("select max({0}) from {1} where {2}='{3}'", 
                    _fieldPosition, _tableName, _fieldQueueName, ResourceName);

                DataSet ds = _dbHelper.GetDataSet(CommandType.Text, sqlMySQL);
                return Convert.ToInt16(ds.Tables[0].Rows[0].ItemArray[0]);


            }
            catch (Exception ex)
            {
                LOG.Error("DBTableQueue获取最大位置失败:{0}", ex);
            }

            return 0;
        }

        //查询队列某个位置的项目
        private string GetDBItemAtPosition(Int16 Position)
        {
            string sqlMySQL = string.Format("select {0} from {1} where {2}='{3}'and {4} = {5}",  
                _fieldItemID, _tableName, _fieldQueueName, ResourceName, _fieldPosition, Position);

            if (_dbHelper != null)
            {
                DataSet ds = _dbHelper.GetDataSet(CommandType.Text, sqlMySQL);
                if (ds.Tables[0].Rows.Count != 0)
                {
                    string itemID = Convert.ToString(ds.Tables[0].Rows[0][0]); // Item ID
                    return itemID;
                }
                return string.Empty;
            }

            return null;
        }

        //替换队列某个位置
        private void UpdateDBItemAtPosition(Int16 Position, string ItemID)
        {
            try
            {
                List<object> values = new List<object>();
                string sqlMySQL = string.Format("Update {0} set {1}='{2}' where {3}={4} and {5}={6}",
                    _tableName, _fieldItemID,ItemID,_fieldQueueName, ResourceName, _fieldPosition, Position);

                _dbHelper.ExecuteNonQuery(CommandType.Text, sqlMySQL);

            }
            catch (Exception ex)
            {
                LOG.Error("DBTableQueue更新数据失败:{0}", ex);
            }
        }

        //替换项目的位置
        private void UpdatePositionOfItem(string ItemID,Int16 Position)
        {
            try
            {
                List<object> values = new List<object>();
                string sqlMySQL = string.Format("Update {0} set {1}={2} where {3}='{4}' and {5}='{6}'",
                    _tableName, _fieldPosition, Position, _fieldQueueName, ResourceName, _fieldItemID, ItemID);

                _dbHelper.ExecuteNonQuery(CommandType.Text, sqlMySQL);

            }
            catch (Exception ex)
            {
                LOG.Error("DBTableQueue更新数据失败:{0}", ex);
            }
        }

        //在队列某个位置插入
        private void InsertDBItem(Int16 Position,string ItemID)
        {
            try
            {
                List<object> values = new List<object>();
                string sqlMySQL = string.Format("Insert into {0}({1},{2},{3}) Values('{4}',{5},'{6}')", 
                    _tableName,_fieldQueueName, _fieldPosition, _fieldItemID, ResourceName,Position,ItemID);

                _dbHelper.ExecuteNonQuery(CommandType.Text, sqlMySQL);

            }
            catch (Exception ex)
            {
                LOG.Error("DBTableQueue插入数据失败:{0}", ex);
            }
        }

        //删除队列某个位置
        private void DeleteDBItem(Int16 Position)
        {
            try
            {
                List<object> values = new List<object>();
                string sqlMySQL = string.Format("Delete From {0} where {1}='{2}' and {3}={4}",
                    _tableName, _fieldQueueName, ResourceName, _fieldPosition, Position);

                _dbHelper.ExecuteNonQuery(CommandType.Text, sqlMySQL);

            }
            catch (Exception ex)
            {
                LOG.Error("DBTableQueue删除失败:{0}", ex);
            }
        }

        //删除整个队列
        private void DeleteAllDBItem()
        {
            try
            {
                List<object> values = new List<object>();
                string sqlMySQL = string.Format("Delete From {0} where {1}='{2}'", _tableName, _fieldQueueName, ResourceName);
             
                _dbHelper.ExecuteNonQuery(CommandType.Text, sqlMySQL);

            }
            catch (Exception ex)
            {
                LOG.Error("DBTableQueue删除失败:{0}", ex);
            }

        }

        #endregion

        #region "Resource definition"

        public override string ResourceType
        {
            get
            {
                return "DBTableQueue";
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
                XmlElement level0_item = (XmlElement)node;
             
                foreach (XmlNode level1_node in node)
                { // level1 --  "DBHelper", "QueueTable"
                    if (level1_node.NodeType == XmlNodeType.Comment)
                        continue;

                    XmlElement level1_item = (XmlElement)level1_node;

                    if (level1_item.Name.ToLower() == "DBHelper".ToLower())
                    {
                        _dbHelper = new DataBaseHelper();
                        if (!_dbHelper.LoadFromConfig(level1_node))
                        {
                            throw new Exception("装载DBhelper出错");
                        }
                    }
                    if (level1_item.Name.ToLower() == "QueueTable".ToLower())
                    {
                        _tableName = level1_item.GetAttribute("TableName");
                        _fieldQueueName = level1_item.GetAttribute("QueueNameField");
                        _fieldPosition = level1_item.GetAttribute("PositionField");
                        _fieldItemID = level1_item.GetAttribute("ItemIDField");

                    }
                }

                return base.LoadFromConfig(node);
            }
            catch (Exception ex)
            {
                LOG.Error(string.Format("加载Queue:{0}出错：{1}", ResourceName, ex.Message));
                return false;
            }
        }

        #endregion
  
    }

    [DataContract]
    class DBTableQueueStatusModel 
    {
        [DataMember]
        public Int32 Count = 0; //仓库货物数量

        [DataMember]
        public Int32 Size = 0; //仓库货物容量

        [DataMember]
        Dictionary<Int16, string> ItemList = new Dictionary<Int16, string>(); //部品列表

        public DBTableQueueStatusModel(Int32 Size,Dictionary<Int16, string> ItemList)
        {
            this.Count = ItemList.Count;
            this.Size = Size;

            this.ItemList = ItemList;
        }

    }
}
