using System;
using ProcessControlService.Contracts;

namespace ProcessControlService.ResourceLibrary.Machines
{
    public class Alarm
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(Alarm));

        private readonly MachineAlarmModel _model = new MachineAlarmModel();

        public MachineAlarmModel Model
        {
            get { return _model; }
        }

        private string TrigTime = string.Empty;

        //private string _uniqueID;

        //public string UniqueID
        //{
        //    get { return _uniqueID; }
        //}

        public string AlarmID
        {
            get { return _model.AlarmId; }
        }

        public MachineAlarmModel.StatusType Status
        {
            get { return _model.Status; }
        }

        public void Trig()
        {
            if (_model.Status == MachineAlarmModel.StatusType.UnTriggered || _model.Status == MachineAlarmModel.StatusType.Reset)
            {
                _model.Status = MachineAlarmModel.StatusType.Triggered;
                _model.TrigTime = DateTime.Now;
                TimeSpan ts = DateTime.Now - DateTime.Parse("1970-1-1");
                _model.UniqueId = ts.TotalMilliseconds.ToString();
                LogToDB();

                Log.Debug(string.Format("报警触发. AlarmID:{0},报警机器{1}，报警内容：{2}", AlarmID,_model.Group,_model.Message));
            }

        }

        public void Confirm()
        {
            if (_model.Status == MachineAlarmModel.StatusType.Triggered)
            {
                _model.Status = MachineAlarmModel.StatusType.Confirmed;

                LogToDB();

            }
            else if (_model.Status == MachineAlarmModel.StatusType.Reset)
            {
                _model.Status = MachineAlarmModel.StatusType.UnTriggered;

                LogToDB();

            }
            Log.Debug(string.Format("报警确认. AlarmID:{0}", AlarmID));

        }

        public void Reset()
        {
            if (_model.Status == MachineAlarmModel.StatusType.Triggered)
            {
                _model.Status = MachineAlarmModel.StatusType.Reset;

                LogToDB();

            }
            else if (_model.Status == MachineAlarmModel.StatusType.Confirmed)
            {
                _model.Status = MachineAlarmModel.StatusType.UnTriggered;

               LogToDB();

            }
            Log.Debug(string.Format("报警复位. AlarmID:{0}", AlarmID));

        }

        private void LogToDB()
        {
            Log.Debug(string.Format("报警记录到数据库的代码被注释 AlarmID:{0}", AlarmID));
            //try
            //{
            //    string sql = null;

            //    if (_model.Status == MachineAlarmModel.StatusType.Triggered)
            //    {
            //        TrigTime = DateTime.Now.ToString();
            //        string sql2 = "select AlarmID from tbl_alarms where UniqueID=@UniqueID ";
            //        MySqlParameter[] Para = new MySqlParameter[]
            //                            {
            //                                 new MySqlParameter("UniqueID", _model._uniqueID)
            //                            };
            //        if (MySqlUtil.GetDataSet(MySqlUtil.Conn, CommandType.Text, sql2, Para).Tables[0].Rows.Count == 0)
            //        {

            //            // sql = "INSERT INTO tbl_alarms(UniqueID,AlarmID,Status,TrigTime,UpdateTime) Values(@UniqueID,@AlarmID,@Status,@TrigTime,NOW())";
            //            sql = "INSERT INTO tbl_alarms(UniqueID,AlarmID,Status,TrigTime) Values(@UniqueID,@AlarmID,@Status,now())";
            //        }
            //        else
            //        {
            //            sql = "UPDATE tbl_alarms SET Status=@Status,UpdateTime=NOW(),AlarmID=@AlarmID,TrigTime=TrigTime WHERE UniqueID=@UniqueID ";

            //        }
            //    }
            //    else if (_model.Status == MachineAlarmModel.StatusType.Confirmed)
            //    {
            //        sql = "UPDATE tbl_alarms SET Status=@Status,ConfirmTime=NOW(),UpdateTime=NOW(), AlarmID=@AlarmID,TrigTime=TrigTime WHERE UniqueID=@UniqueID";
            //    }
            //    else if (_model.Status == MachineAlarmModel.StatusType.Reset)
            //    {
            //        sql = "UPDATE tbl_alarms SET `Status`=@Status,ResetTime=NOW(),UpdateTime=NOW(), AlarmID=@AlarmID,TrigTime=TrigTime WHERE UniqueID=@UniqueID";
            //    }
            //    //else if (_model.Status == MachineAlarmModel.StatusType.UnTriggered)
            //    //{
            //    //    sql = "UPDATE tbl_alarms SET `Status`=@Status,UntrigTime=NOW(),UpdateTime=NOW(), AlarmID=@AlarmID,TrigTime=@TrigTime WHERE UniqueID=@UniqueID";
            //    //}

            //    MySqlParameter[] Parameters = new MySqlParameter[]
            //                            {
            //                                 new MySqlParameter("UniqueID", _model._uniqueID),
            //                                new MySqlParameter("AlarmID", _model.AlarmID),
            //                                new MySqlParameter("Status", Status.ToString())
                                            
            //                            };

            //    MySqlUtil.ExecuteNonQuery(MySqlUtil.Conn, CommandType.Text, sql, Parameters);
            //}
            //catch (Message ex)
            //{
            //    LOG.Error(string.Format("报警：{0} 记录到数据库出错:{1}", _model.Message, ex.Message));
            //}
        }

        public static Alarm New(AlarmDefinition AlmDef)
        {
            Alarm _newAlarm = new Alarm();

            _newAlarm._model.AlarmId = AlmDef.AlarmID;
            _newAlarm._model.Message = AlmDef.Message;
            _newAlarm._model.Group = AlmDef.Group;
            
            _newAlarm.Trig();

            return _newAlarm;
        }

        private static string GenAlarmID(string AlarmID)
        {
            Random ran = new Random();
            int n = ran.Next(1000);

            string alarm_id = string.Format("{0}{1:D4}", AlarmID, n.ToString());
            return alarm_id;
        }
   
    }

    
}
