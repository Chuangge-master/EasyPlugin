using System;
using System.Collections.Generic;
using System.Text;

namespace EasyPlugin.Interface
{
    public interface IPluginLogger
    {
        event Action<string> OnLogAdded;
        void Log(string pluginName,string message);
    }
}
