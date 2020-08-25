using ProcessControlService.Contracts;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessControlService.ResourceLibrary.Machines
{
    public class LiveAlarmCollections
    {
        //private List<Alarm> _alarms = new List<Alarm>();
        private Dictionary<string, Alarm> _liveAlarms = new Dictionary<string, Alarm>();

        /// <summary>
        /// 改变alarm状态Trigged,添加。
        /// </summary>
        /// <param name="AlarmDef"></param>
        /// <param name="status"></param>
        public void ChangeAlarmStatus(AlarmDefinition AlarmDef, AlarmSignalStatus status)
        {
            if (status == AlarmSignalStatus.Trigged)
            {// 处于报警状态
                if (!_liveAlarms.Keys.Contains(AlarmDef.AlarmID)) 
                {//原来不存在此报警
                    Alarm new_alarm = Alarm.New(AlarmDef); //新增一条报警
                    _liveAlarms.Add(AlarmDef.AlarmID, new_alarm);
                }
                //2017年7月17日10:27:01  夏 注释 
                //else
                //{//原来存在此报警
                //    Alarm alarm = _liveAlarms[AlarmDef.AlarmID];
                //    if (alarm.Status != MachineAlarmModel.StatusType.Triggered)
                //    {
                //        alarm.Trig();  //触发一条报警
                //    }
                //}
            }
            else if (status == AlarmSignalStatus.Untrigged)
            {// 不处于报警状态
                if (_liveAlarms.Keys.Contains(AlarmDef.AlarmID))
                {//原来存在此报警
                    //2017年7月17日10:27:01  夏 注释 
                    //Alarm alarm = _liveAlarms[AlarmDef.AlarmID];
                    //if (alarm.Status == MachineAlarmModel.StatusType.Triggered || alarm.Status == MachineAlarmModel.StatusType.Confirmed)
                    //{
                    //    alarm.Reset(); //复位一条报警
                    //}

                    //if (alarm.Status == MachineAlarmModel.StatusType.Nottriggered) //原来报警已复位
                    //{
                        _liveAlarms.Remove(AlarmDef.AlarmID); //删除一条报警
                    //}
                }
            }
            //else if (status == AlarmModel.StatusType.Confirmed)
            //{
            //    if (_liveAlarms.Keys.Contains(AlarmDef.AlarmID))
            //    {
            //        Alarm alarm = _liveAlarms[AlarmDef.AlarmID];
            //        alarm.Confirm();

            //        if (alarm.Status == AlarmModel.StatusType.Nottriggered)
            //            _liveAlarms.Remove(AlarmDef.AlarmID);
            //    }
            //}
          
        }

        public List<MachineAlarmModel> GetAlarmModels()
        {
            List<MachineAlarmModel> alarm_models = new List<MachineAlarmModel>();

            foreach (Alarm alarm in _liveAlarms.Values)
            {
                alarm_models.Add(alarm.Model);
            }

            return alarm_models;
        }

        public Alarm GetAlarm(string AlarmID)
        {
            return _liveAlarms[AlarmID];
        }
     
    }
}
