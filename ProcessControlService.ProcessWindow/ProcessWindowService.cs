using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using log4net;


namespace ProcessControlService.ProcessWindow
{
    public partial class ProcessWindowService : ServiceBase
    {
        private static readonly log4net.ILog LOG = log4net.LogManager.GetLogger(typeof(ProcessWindowService));

        public ProcessWindowService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            //using (System.IO.StreamWriter sw = new System.IO.StreamWriter("C:\\log.txt", true))
            //{
            //    sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ") + "Start.");
            //}
            //LOG.Info("进入服务");
            Program.StartProcessControlService(true); // 以Windows服务方式启动
        }

        protected override void OnStop()
        {
            //using (System.IO.StreamWriter sw = new System.IO.StreamWriter("C:\\log.txt", true))
            //{
            //    sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ") + "Break.");
            //}
            //LOG.Info(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss ") + "Break.");

            Program.Close();
        }
    }
}
