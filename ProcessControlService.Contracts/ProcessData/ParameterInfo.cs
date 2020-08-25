using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using FreeSql.DataAnnotations;
using ProcessControlService.Contracts.Annotations;

namespace ProcessControlService.Contracts.ProcessData
{
    [DataContract]
    public sealed class ParameterInfo:INotifyPropertyChanged
    {
        private string _name;
        private string _valueInString;
        private string _type;
        private string _key;

        [Column(IsPrimary = true,IsIdentity = true)]
        public long Id { get; set; }

        public string Pid { get; set; }
        
        [XmlAttribute]
        [DataMember]
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        [XmlAttribute]
        [DataMember]
        public string ValueInString
        {
            get => _valueInString;
            set
            {
                _valueInString = value;
                OnPropertyChanged(nameof(ValueInString));
            }
        }

        [XmlAttribute]
        [DataMember]
        public string Type
        {
            get => _type;
            set
            {
                _type = value;
                OnPropertyChanged(nameof(Type));
            }
        }

        [XmlAttribute]
        [DataMember]
        public string Key
        {
            get => _key;
            set
            {
                _key = value;
                OnPropertyChanged(nameof(Key));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}