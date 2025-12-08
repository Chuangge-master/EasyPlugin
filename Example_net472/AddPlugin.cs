using EasyPlugin.Core;
using EasyPlugin.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example_net472
{
    public class AddPlugin : IPlugin
    {
        public string Name { get; set; } = "AddPlugin";

        async public Task<PluginContext> ExecuteAsync(PluginContext context)
        {
            await Task.Delay(1000);
            var data = (ValueTuple<int, int>)context.Data;
            var result = new PluginContext();
            result.Data = data.Item1 + data.Item2;
            return result;
        }
    }
}
