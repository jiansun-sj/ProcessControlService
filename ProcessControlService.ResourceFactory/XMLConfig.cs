using System;
using System.Xml;

namespace ProcessControlService.ResourceFactory
{
    public class XMLConfig
    {
        public XmlDocument ConfigDoc { get; set; }

        public string ConfigFileName { get; set; }
        public XmlElement ResourceElement { get; set; }
        
        /// <summary>
        /// 保存XML配置
        /// </summary>
        public void SaveConfiguration()
        {
            try
            {
                ConfigDoc?.Save(ConfigFileName);
            }
            catch (Exception)
            {

            }
        }
    }
}
