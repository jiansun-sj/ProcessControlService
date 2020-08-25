using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using FreeSql.DataAnnotations;
using ProcessControlService.Contracts.Annotations;

namespace ProcessControlService.Contracts.ProcessData
{
    [XmlRoot]
    [DataContract]
    [Table(Name="ProcessRecord")]
    public class ProcessInstanceRecord:INotifyPropertyChanged
    {
        private string _processName;
        private string _pid;
        private DateTime _startTime;
        private DateTime _endTime;
        private ProcessStatus _processStatus = ProcessStatus.None;
        private string _breakStepName;
        private short _breakStepId;
        private List<Message> _messages = new List<Message>();
        private List<ParameterInfo> _parameters = new List<ParameterInfo>();

        /*[Column(IsPrimary = true,IsIdentity = true)]
        public long Id { get; set; }*/
        
        [XmlAttribute]
        [DataMember]
        public string ProcessName
        {
            get => _processName;
            set
            {
                _processName = value;
                OnPropertyChanged(nameof(ProcessName));
            }
        }

        [XmlAttribute]
        [DataMember]
        [Column(IsPrimary = true)]
        public string Pid
        {
            get => _pid;
            set
            {
                _pid = value;
                OnPropertyChanged(nameof(Pid));
            }
        }

        [XmlAttribute]
        [DataMember]
        public DateTime StartTime
        {
            get => _startTime;
            set
            {
                _startTime = value;
                OnPropertyChanged(nameof(StartTime));
            }
        }

        [XmlAttribute]
        [DataMember]
        public DateTime EndTime
        {
            get => _endTime;
            set
            {
                _endTime = value;
                OnPropertyChanged(nameof(EndTime));
            }
        }

        [XmlAttribute]
        [DataMember]
        public ProcessStatus ProcessStatus
        {
            get => _processStatus;
            set
            {
                _processStatus = value;
                OnPropertyChanged(nameof(ProcessStatus));
            }
        }

        [XmlAttribute]
        [DataMember]
        public string BreakStepName
        {
            get => _breakStepName;
            set
            {
                _breakStepName = value;
                OnPropertyChanged(nameof(BreakStepName));
            }
        }

        [XmlAttribute]
        [DataMember]
        public short BreakStepId
        {
            get => _breakStepId;
            set
            {
                _breakStepId = value;
                OnPropertyChanged(nameof(BreakStepId));
            }
        }

        [XmlArray]
        [DataMember]
        [Navigate(nameof(Message.Pid))]
        public List<Message> Messages
        {
            get => _messages;
            set
            {
                _messages = value;
                OnPropertyChanged(nameof(Messages));
            }
        }

        [XmlArray]
        [DataMember]
        [Navigate(nameof(ParameterInfo.Pid))]
        public List<ParameterInfo> Parameters
        {
            get => _parameters;
            set
            {
                _parameters = value;
                OnPropertyChanged(nameof(Parameters));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}