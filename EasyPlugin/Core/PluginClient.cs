using EasyPlugin.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EasyPlugin.Core
{
    public class PluginClient
    {
        private static readonly IPluginLogger _logger = new PluginLogger();

        async static public Task<PluginContext> Run(Func<Task<PluginContext>> task)
        {
            return await task();
        }
        async static public Task<PluginContext[]> TogetherRun(params Func<Task<PluginContext>>[] tasks)
        {
            var taskArray = new Task<PluginContext>[tasks.Length];

            for (int i = 0; i < tasks.Length; i++)
            {
                taskArray[i] = tasks[i]();
            }

            return await Task.WhenAll(taskArray);
        }
        public static PluginProxy Create(string name, Type pluginType, double timeout = 3000, IDataValidate validate = null)
        {
            return !(Activator.CreateInstance(pluginType) is IPlugin plugin) 
                ? throw new Exception("pluginType is not IPlugin") : new PluginProxy(plugin, _logger, timeout, validate) { Name = name};
        }
        public static void RegisterLogHandler(Action<string> onLogAdded)
        {
            ((PluginLogger)_logger).OnLogAdded += onLogAdded;
        }
    }
}
