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
        async static public Task<PluginContext> Run(PluginProxy pluginProxy, object data)
        {
            PluginContext context;
            if(data is PluginContext) context = data as PluginContext;
            else context = new PluginContext().SetData(data);

            return await pluginProxy.ExecuteAsync(context);
        }
        async static public Task<PluginContext[]> TogetherRun(List<PluginProxy> pluginProxies, List<object> datas)
        {
            if(pluginProxies.Count != datas.Count) throw new Exception("插件数量和上下文数量不匹配");
            var contexts = new PluginContext[datas.Count];
            for (int i = 0; i < datas.Count; i++)
            {
                if (datas[i] is PluginContext) contexts[i] = datas[i] as PluginContext;
                else contexts[i] = new PluginContext().SetData(datas[i]);
            }
            //并行运行
            var tasks = new List<Task<PluginContext>>();
            for (int i = 0; i < pluginProxies.Count; i++)
            {
                tasks.Add(pluginProxies[i].ExecuteAsync(contexts[i]));
            }
            
            var result = await Task.WhenAll(tasks);

            return result;

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
