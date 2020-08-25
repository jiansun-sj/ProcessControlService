using log4net;
using System;
using System.Collections.Generic;

namespace ProcessControlService.ResourceLibrary.Machines.DataSources
{
    public class DataSourceManagement
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DataSourceManagement));

        private static readonly Dictionary<string, Type> DataSourceTypeCollection = new Dictionary<string, Type>();

        public static void AddDataSourceType(string dataSourceType, Type type)
        {
            DataSourceTypeCollection.Add(dataSourceType, type);
        }

        public static DataSource CreateDataSource(string dataSourceTypeName, string dataSourceName, Machine machine)
        {
            try
            {
                var dataSourceType = DataSourceTypeCollection[dataSourceTypeName];
                var obj = Activator.CreateInstance(dataSourceType, dataSourceName, machine);

                var dataSource = (DataSource)obj;
                return dataSource;
            }
            catch (Exception ex)
            {
                Log.Error($"Create DataSource：{dataSourceName} Failed {ex.Message}.");
                return null;
            }
        }
    }
}