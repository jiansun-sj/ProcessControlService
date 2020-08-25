using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessControlService.ResourceLibrary.Machines
{
    public class MachineAlarmModel
    {
        public enum StatusType
        {
            Untrigged = (short)0,
            Trigged = (short)1,
            Confirmed = (short)2,
            Reset = (short)3,
        }


        //[DataMember]
        public string _uniqueID;

        //[DataMember]
        public string AlarmID;

        //[DataMember]
        public DateTime TrigTime;

        //[DataMember]
        public StatusType Status;

        //[DataMember]
        public string Message;

        //[DataMember]
        public string Group;


    }
}
