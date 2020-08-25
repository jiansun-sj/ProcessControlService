// ==================================================
// 文件名：Message.cs
// 创建时间：// 17:08
// 上海芸浦信息技术有限公司
// copyright@yumpoo
// ==================================================
// 最后修改于：2020/05/02 17:08
// 修改人：jians
// ==================================================


using FreeSql.DataAnnotations;

namespace ProcessControlService.Contracts.ProcessData
{
    /// <summary>
    /// Process实例运行过程中产生的异常或反馈信息
    /// </summary>
    public class Message
    {
        [Column(IsPrimary = true,IsIdentity = true)]
        public long Id { get; set; }

        public string Description { get; set; }

        public Message()
        {
            
        }

        public Message(string description)
        {
            Description = description;
        }

        public Level Level { get; set; } = Level.Info;

        public string Pid { get; set; }
    }
}