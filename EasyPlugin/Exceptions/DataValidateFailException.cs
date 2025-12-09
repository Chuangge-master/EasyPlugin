using System;
using System.Collections.Generic;
using System.Text;

namespace EasyPlugin.Exceptions
{
    public class DataValidateFailException:Exception
    {
        public DataValidateFailException(string message) : base(message)
        {
            
        }
    }
}
