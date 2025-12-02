using EasyPlugin.Attriibutes;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EasyPlugin.Core
{
    /// <summary>
    /// 插件基类
    /// </summary>
    public abstract class PluginBase
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<PluginBase> Next { get; set; } = new List<PluginBase>();
        public List<PluginBase> Previous { get; set; } = new List<PluginBase>();
        public abstract Task<PluginResult> ExecuteAsync(PluginContext context);

        public virtual void Validate()
        {
            if (string.IsNullOrEmpty(Id))
                throw new InvalidOperationException("node must have an ID");
        }

        public void AddNext(PluginBase next)
        {
            if (!Next.Contains(next))
            {
                Next.Add(next);
                next.Previous.Add(this);
            }
        }

        public override string ToString()
        {
            var type = this.GetType();
            var metadataAttr = type.GetCustomAttribute<PluginMetaAttribute>();
            return $"{metadataAttr?.Name ?? type.Name}({Id}):{metadataAttr?.Version ?? "V1.0.0.0"}\n" +
                $"Description：{metadataAttr?.Description ?? ""} \n" +
                $"Author：{metadataAttr?.Author ?? ""} \n";
        }
    }
}
