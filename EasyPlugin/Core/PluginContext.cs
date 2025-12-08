using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace EasyPlugin.Core
{
    public class PluginContext
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public object Data { get; set; }

        public PluginContext()
        {
            Success = true;
        }
        public PluginContext OK()
        {
            Success = true;
            return this;
        }   
        public PluginContext Error(string message)
        {
            Success = false;
            ErrorMessage = message;
            return this;
        }
        public PluginContext SetData(object value)
        {
            this.Data = value;
            return this;
        }
    }
}
