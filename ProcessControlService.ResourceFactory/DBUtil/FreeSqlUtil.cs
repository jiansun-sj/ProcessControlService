// ==================================================
// 文件名：FreeSqlUtil.cs
// 创建时间：2020/07/15 17:38
// ==================================================
// 最后修改于：2020/07/15 17:38
// 修改人：jians
// ==================================================

using System;
using System.IO;
using FreeSql;

namespace ProcessControlService.ResourceFactory.DBUtil
{
    public static class FreeSqlUtil
    {
        public static readonly string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory + "ProcessRecord";

        public static readonly string FilePath = Path.Combine(BaseDirectory, "FWRecord.db");
        
        public static readonly IFreeSql FSql = new FreeSqlBuilder()
            .UseConnectionString(DataType.Sqlite, $@"Data Source={FilePath};Pooling=true;Min Pool Size=1")
            .UseAutoSyncStructure(true) //自动同步实体结构到数据库
            .Build();
    }
}