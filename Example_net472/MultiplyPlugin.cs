using EasyPlugin.Core;
using EasyPlugin.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example_net472
{
    public class MultiplyPlugin: IPlugin
    {
        public string Name { get; set; }

        async public Task<PluginContext> ExecuteAsync(PluginContext context)
        {
            await Task.Delay(1000);
            var data = (int[])context.Data;
            var result = new PluginContext();
            result.Data = data[0] * data[1];
            return result;
        }
    }
}
