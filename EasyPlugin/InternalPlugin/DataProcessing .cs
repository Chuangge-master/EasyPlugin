using EasyPlugin.Attriibutes;
using EasyPlugin.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EasyPlugin.InternalPlugin
{
    /// <summary>
    /// 数据处理工具节点
    /// </summary>
    [PluginMeta("数据处理插件",
        description:"用于处理输入数据",
        version:"V1.0.0.0",
        author:"EasyPlugin")]
    public class DataProcessing : PluginBase
    {
        public string InputKey { get; set; }
        public string OutputKey { get; set; }
        public Func<object, object> ProcessingFunction { get; set; }

        public override async Task<PluginResult> ExecuteAsync(PluginContext context)
        {
            try
            {
                if (!context.ContainsKey(InputKey))
                {
                    return PluginResult.Error($"输入数据 '{InputKey}' 不存在");
                }

                var inputData = context.GetData<object>(InputKey);
                var outputData = ProcessingFunction?.Invoke(inputData);

                if (outputData != null && !string.IsNullOrEmpty(OutputKey))
                {
                    context.SetData(OutputKey, outputData);
                }

                return PluginResult.Ok($"处理完成，输出到: {OutputKey}");
            }
            catch (Exception ex)
            {
                return PluginResult.Error($"数据处理失败: {ex.Message}", ex);
            }
        }
    }
}
