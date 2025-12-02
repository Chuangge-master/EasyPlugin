using EasyPlugin.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EasyPlugin.InternalPlugin
{
    /// <summary>
    /// 条件评估插件
    /// </summary>
    public class Conditional : PluginBase
    {
        public string ConditionKey { get; set; }
        public Func<object, bool> Condition { get; set; }

        public override async Task<PluginResult> ExecuteAsync(PluginContext context)
        {
            try
            {
                if (!context.ContainsKey(ConditionKey))
                {
                    return PluginResult.Error($"条件数据 '{ConditionKey}' 不存在");
                }

                var data = context.GetData<object>(ConditionKey);
                var result = Condition?.Invoke(data) ?? false;

                context.SetData($"{Id}_ConditionResult", result);

                return PluginResult.Ok($"条件评估结果: {result}");
            }
            catch (Exception ex)
            {
                return PluginResult.Error($"条件评估失败: {ex.Message}", ex);
            }
        }
    }
}
