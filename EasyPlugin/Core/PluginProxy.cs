using EasyPlugin.Exceptions;
using EasyPlugin.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasyPlugin.Core
{
    public class PluginProxy : IPlugin
    {

        private readonly IPlugin _plugin;
        private readonly IPluginLogger _logger;
        private readonly TimeSpan _timeout;
        private readonly IDataValidate _dataValidate;

        public string Name { get; set; }
        internal PluginProxy(IPlugin plugin, IPluginLogger logger, double timeout, IDataValidate dataValidate)
        {
            Name = plugin.Name;
            _plugin = plugin;
            _logger = logger;
            _timeout = TimeSpan.FromMilliseconds(timeout);
            _dataValidate = dataValidate;
        }

        async public Task<PluginContext> ExecuteAsync(PluginContext context)
        {
            var result = new PluginContext();
            try
            {
                _logger.Log(Name,"开始运行");
                if (_dataValidate != null && !_dataValidate.Validate(context, out string errorMessage))
                {
                    throw new DataValidateFailException(errorMessage);
                }
                using (var cts = new CancellationTokenSource(_timeout))
                {
                    var task = _plugin.ExecuteAsync(context);
                    var completedTask = await Task.WhenAny(task, Task.Delay(_timeout, cts.Token));
                    if (completedTask == task)
                    {
                        // 任务在超时前完成
                        result = await task;
                    }
                    else
                    {
                        throw new TimeoutException($"Plugin {Name} execution timed out after {_timeout.TotalMilliseconds} ms.");
                    }
                    if (result.Success)
                    {
                        _logger.Log(Name, "运行成功");
                    }
                    else
                    {
                        _logger.Log(Name, $"运行失败：{result.ErrorMessage}");
                    }
                }
            }
            catch (DataValidateFailException dvf_ex)
            {
                result.Error($"数据验证失败:{dvf_ex.Message}");
                _logger.Log(Name, $"数据验证失败:{dvf_ex.Message}");
            }
            catch (TimeoutException)
            {
                result.Error($"运行超出限制时间{_timeout.TotalMilliseconds}毫秒");
                _logger.Log(Name, $"运行超出限制时间{_timeout.TotalMilliseconds}毫秒");
            }
            catch (Exception ex)
            {
                result.Error(ex.Message);
                _logger.Log(Name, $"运行失败：{ex.Message}");
            }

            return result;
        }
    }
}
