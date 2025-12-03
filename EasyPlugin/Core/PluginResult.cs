using System;
using System.Collections.Generic;
using System.Text;

namespace EasyPlugin.Core
{
    public class PluginResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public Exception Exception { get; set; }
        /// <summary>
        /// 运行时间（ms）
        /// </summary>
        public string Runtime { get; set; }

        public static PluginResult Ok(string message = null) => new PluginResult { Success = true, Message = message };
        public static PluginResult Error(string message, Exception ex = null) => new PluginResult { Success = false, Message = message, Exception = ex };
    }
}
