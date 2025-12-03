using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyPlugin.Core
{
    /// <summary>
    /// 插件上下文
    /// </summary>
    public class PluginContext
    {
        private readonly Dictionary<string, object> _data = new Dictionary<string, object>();

        public T GetData<T>(string key)
        {
            if (_data.ContainsKey(key) && _data[key] is T result)
            {
                return result;
            }
            return default(T);
        }

        public void SetData(string key, object value)
        {
            _data[key] = value;
        }

        public bool ContainsKey(string key) => _data.ContainsKey(key);
        public string[] Keys()
        {
            return _data.Keys.ToArray();
        }
    }
}
