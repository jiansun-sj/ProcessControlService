// ==================================================
// 文件名：ConfigSelectedResourceAction.cs
// 创建时间：2020/06/05 10:17
// ==================================================
// 最后修改于：2020/06/05 10:17
// 修改人：jians
// ==================================================

using System;
using log4net;
using ProcessControlService.ResourceFactory.ParameterType;
using ProcessControlService.ResourceLibrary.Action;

namespace ProcessControlService.ResourceLibrary.Common
{
    /// <summary>
    /// 配置ResourceDic中的参数
    /// </summary>
    /// <remarks>
    ///     <list type="number">
    ///         <item>
    ///             <description> 添加从参数数组中选择下一个参数对象的动作 add by sunjian 2019-12</description>
    ///         </item>
    ///         <item>
    ///             <description> 添加从参数数组中选择下一个参数对象的动作 add by sunjian 2019-12</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    public class ConfigResourceDicAction : BaseAction
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ConfigResourceDicAction));

        public ConfigResourceDicAction(string name)
            : base(name)
        {
        }

        #region "Parameters"

        protected override bool CreateParameters()
        {
            ActionInParameterManager.AddBasicParam(new BasicParameter<string>("Key"));
            ActionInParameterManager.AddBasicParam(new BasicParameter<string>("Value"));
            ActionInParameterManager.AddDictionaryParam(new DictionaryParameter<string>("InDictionaryParameter"));
            ActionOutParameterManager.AddDictionaryParam(new DictionaryParameter<string>("OutDictionaryParameter"));
            return true;
        }
        
        #endregion

        #region "core functions"
        public override void Execute()
        {
            try
            {
                var key = ActionInParameterManager["Key"].GetValueInString();
                var value = ActionInParameterManager["Value"].GetValueInString();

                var inDictionaryParameter = ActionInParameterManager.GetDictionaryParam("InDictionaryParameter");

                if (inDictionaryParameter.ContainsKey(key,typeof(string)))
                {
                    inDictionaryParameter.SetValue(key,value);
                }
                else
                {
                    inDictionaryParameter.Add(key,value);
                }
                
                ActionOutParameterManager.GetDictionaryParam("OutDictionaryParameter").Replace(inDictionaryParameter);
            }
            catch (Exception ex)
            {
                Log.Error($"配置SelectedResource失败，异常为：[{ex}].");
            }
        }

        public override bool IsSuccessful()
        {
            return true;
        }

        public override bool IsFinished()
        {
            return true;
        }

        public override object GetResult()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}