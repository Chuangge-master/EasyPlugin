using System;
using System.Collections.Generic;
using System.Text;

namespace EasyPlugin.Exceptions
{
    /// <summary>
    /// 插件循环依赖异常
    /// </summary>
    public class CyclicDependencyException : Exception
    {
        public CyclicDependencyException(string message) : base(message) { }
    }
}
