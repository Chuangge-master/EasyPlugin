using EasyPlugin.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EasyPlugin.Core
{
    public class PluginProxy : IPlugin
    {

        private readonly IPlugin _plugin;
        private readonly IPluginLogger _logger;

        public string Name { get; set; }
        public PluginProxy(IPlugin plugin, IPluginLogger logger)
        {
            Name = plugin.Name;
            _plugin = plugin;
            _logger = logger;
        }

        async public Task<PluginContext> ExecuteAsync(PluginContext context)
        {
            var result = new PluginContext();
            try
            {
                _logger.Log(Name,"开始运行");
                result = await _plugin.ExecuteAsync(context);
                if (result.Success)
                {
                    _logger.Log(Name, "运行成功");
                }
                else
                {
                    _logger.Log(Name, $"运行失败：{result.ErrorMessage}");
                }
                
            }
            catch (Exception ex)
            {
                result.Error(ex.Message);
                _logger.Log(Name, $"运行失败：{ex.Message}");
            }

            return result;
        }
    }
}
