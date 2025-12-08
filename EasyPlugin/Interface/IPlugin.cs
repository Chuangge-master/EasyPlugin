using EasyPlugin.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EasyPlugin.Interface
{
    /// <summary>
    /// 插件节点接口
    /// </summary>
    public interface IPlugin
    {
        string Name { get; set; }
        Task<PluginContext> ExecuteAsync(PluginContext context);
    }
}
