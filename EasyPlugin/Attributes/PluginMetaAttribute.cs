using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace EasyPlugin.Attriibutes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class PluginMetaAttribute : Attribute
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public string Author { get; set; }

        public PluginMetaAttribute(string name, string description = "", string version = "V1.0.0.0", string author = "Unknown")
        {
            Name = name;
            Description = description;
            Version = version;
            Author = author;
        }
    }
}
