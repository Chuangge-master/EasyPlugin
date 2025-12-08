using EasyPlugin.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyPlugin.Core
{
    public class PluginLogger : IPluginLogger
    {
        /// <summary>
        /// 插件日志事件，当有新的日志添加时触发
        /// </summary>
        public event Action<string> OnLogAdded;

        public void Log(string pluginName, string message)
        {
            var logMessage = $"{DateTime.Now}:[{pluginName}] {message}";
            OnLogAdded?.Invoke(logMessage);
        }
    }
}
