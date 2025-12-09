using EasyPlugin.Core;
using EasyPlugin.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace EasyPlugin.DataValidates
{
    public class TypeValidate : IDataValidate
    {
        private Type _type;
        public TypeValidate(Type type)
        {
            _type = type;
        }
        public bool Validate(PluginContext context, out string errorMessage)
        {
            errorMessage = "";
            if(context is null)
            {
                errorMessage = $"context 为空";
                return false;
            }
            if(context.Data is null)
            {
                errorMessage = $"context.Data 为空";
                return false;
            }
            if(context.Data.GetType() != _type)
            {
                errorMessage = $"数据类型不匹配，期望类型为{_type}，实际类型为{context.Data.GetType()}";
                return false;
            }
            return true;
        }
    }
}
