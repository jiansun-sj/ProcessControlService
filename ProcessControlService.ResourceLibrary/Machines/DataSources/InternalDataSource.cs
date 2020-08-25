using System;
using log4net;

namespace ProcessControlService.ResourceLibrary.Machines.DataSources
{
    /// <summary>
    ///     内部用DataSource
    ///     Created by DavidDong 2017/3/18
    /// </summary>
    public class InternalDataSource : DataSource
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(InternalDataSource));

        public InternalDataSource(string name, Machine machine)
            : base(name, machine)
        {
        }

        public override void Disconnect()
        {
            Connected = false;
        }

        protected override bool Connect()
        {
            Connected = true;
            return true;
        }

        // 检查通信连接 - David 20170709
        [Obsolete]
        protected override bool CheckConnected()
        {
            return Connected;
        }

        public override bool UpdateAllValue() => true;
        

        public override bool WriteTagToRealDevice(Tag tag, object value)
        {
            try
            {
                tag.TagValue = value;

                 Log.Debug($"数据源[{SourceName}]写入数据 Tag[{tag.TagName}] Address[{tag.Address}] Value[{value}] InternalDataSource.");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(
                    $"数据源[{SourceName}]写入数据出错 Tag[{tag.TagName}] Address[{tag.Address}] Value[{value}] Message[{ex.Message}]");
                return false;
            }
        }
    }
}