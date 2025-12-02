using EasyPlugin.Attriibutes;
using EasyPlugin.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EasyPlugin.InternalPlugin
{
    [PluginMeta("Lambda表达式插件", "使用自定义的函数作为插件", "V1.0.0.0", "EasyPlugin")]
    public class LambdaPlugin : PluginBase
    {
        private Action<PluginContext, string> Func;
        public string OutputKey { get; set; }

        public LambdaPlugin(Action<PluginContext, string> func)
        {
            this.Func = func;
        }
        public override async Task<PluginResult> ExecuteAsync(PluginContext context)
        {
            try
            {
                Func(context, OutputKey);
                return PluginResult.Ok($"数据收集完成, 输出键: {OutputKey}");
            }
            catch (Exception ex)
            {
                return PluginResult.Error($"数据处理失败: {ex.Message}", ex);
            }
        }
    }
}
