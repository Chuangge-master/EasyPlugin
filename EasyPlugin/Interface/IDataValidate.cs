using EasyPlugin.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyPlugin.Interface
{
    public interface IDataValidate
    {
        bool Validate(PluginContext context, out string errorMessage);
    }
}
