using EasyPlugin.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EasyPlugin.Core
{
    /// <summary>
    /// 执行模式枚举
    /// </summary>
    public enum ExecutionMode
    {
        /// <summary>
        /// 串行执行
        /// </summary>
        Sequential,  
        /// <summary>
        /// 并行执行
        /// </summary>
        Parallel
    }

    /// <summary>
    /// 执行配置
    /// </summary>
    public class ExecutionConfig
    {
        /// <summary>
        /// 执行模式
        /// </summary>
        public ExecutionMode Mode { get; set; } = ExecutionMode.Parallel;
        /// <summary>
        /// 并行度
        /// </summary>
        public int MaxDegreeOfParallelism { get; set; } = Environment.ProcessorCount;
        /// <summary>
        /// 失败是否继续执行
        /// </summary>
        public bool ContinueOnFailure { get; set; } = false;
    }
    /// <summary>
    /// 插件客户端，负责插件编排和用于执行
    /// </summary>
    public class PluginClient
    {
        private readonly PluginDag _dag;
        private readonly ExecutionConfig _config;

        /// <summary>
        /// 执行进度事件
        /// </summary>
        public event EventHandler<string> ExecutionProgress;
        /// <summary>
        /// 记录执行日志
        /// </summary>
        public string ExecutionLog { get; private set; } = string.Empty;

        public PluginClient(PluginDag dag, ExecutionConfig config = null)
        {
            _dag = dag;
            _config = config ?? new ExecutionConfig();
            ExecutionProgress += (sender, message) =>
            {
                ExecutionLog += $"\r\n[{DateTime.Now:HH:mm:ss}] {message}";
            };
        }
        /// <summary>
        /// 执行插件编排
        /// </summary>
        /// <param name="context">插件上下文</param>
        /// <returns>每个插件的执行结果</returns>
        public async Task<Dictionary<string, PluginResult>> ExecuteAsync(PluginContext context = null)
        {
            context = context ?? new PluginContext();
            var results = new Dictionary<string, PluginResult>();

            ExecutionLog = string.Empty;
            OnProgress("开始执行工具编排...");

            try
            {
                // 获取执行组
                var executionGroups = _dag.GetParallelGroups();
                OnProgress($"检测到 {executionGroups.Count} 个执行阶段");

                foreach (var group in executionGroups)
                {
                    OnProgress($"执行阶段包含 {group.Count} 个工具");

                    if (_config.Mode == ExecutionMode.Parallel && group.Count > 1)
                    {
                        await ExecuteGroupParallel(group, context, results);
                    }
                    else
                    {
                        await ExecuteGroupSequential(group, context, results);
                    }

                    // 检查是否继续执行
                    if (!_config.ContinueOnFailure && results.Any(r => !r.Value.Success))
                    {
                        OnProgress("遇到失败且配置为不继续执行，终止流程");
                        break;
                    }
                }
            }
            catch (CyclicDependencyException ex)
            {
                OnProgress($"编排错误: {ex.Message}");
                throw;
            }

            OnProgress("执行完成");
            return results;
        }
        /// <summary>
        /// 执行并行组
        /// </summary>
        /// <param name="group">并行组</param>
        /// <param name="context">插件上下文</param>
        /// <param name="results">组内每个插件的执行结果</param>
        /// <returns></returns>
        private async Task ExecuteGroupParallel(List<PluginBase> group, PluginContext context, Dictionary<string, PluginResult> results)
        {
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = _config.MaxDegreeOfParallelism
            };

            var parallelResults = new Dictionary<string, PluginResult>();
            var lockObject = new object();

            await Task.Run(() =>
            {
                Parallel.ForEach(group, options, node =>
                {
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    try
                    {
                        OnProgress($"并行执行工具: {node.Name}");
                        var result = node.ExecuteAsync(context).GetAwaiter().GetResult();

                        lock (lockObject)
                        {
                            parallelResults[node.Id] = result;
                            results[node.Id] = result;
                        }
                        sw.Stop();
                        result.Runtime = sw.ElapsedMilliseconds.ToString("f3");
                        OnProgress($"工具 {node.Name} 执行 {(result.Success ? "成功" : "失败")}");
                    }
                    catch (Exception ex)
                    {
                        var errorResult = PluginResult.Error($"执行失败: {ex.Message}", ex);
                        lock (lockObject)
                        {
                            parallelResults[node.Id] = errorResult;
                            results[node.Id] = errorResult;
                        }
                        sw.Stop();
                        errorResult.Runtime = sw.ElapsedMilliseconds.ToString("f3");
                        OnProgress($"工具 {node.Name} 执行异常: {ex.Message}");
                    }
                });
            });
        }
        /// <summary>
        /// 执行串行组
        /// </summary>
        /// <param name="group">串行组</param>
        /// <param name="context">插件上下文</param>
        /// <param name="results">组内每个插件的执行结果</param>
        /// <returns></returns>
        private async Task ExecuteGroupSequential(List<PluginBase> group, PluginContext context, Dictionary<string, PluginResult> results)
        {
            foreach (var node in group)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                try
                {
                    OnProgress($"串行执行工具: {node.Name}");
                    var result = await node.ExecuteAsync(context);
                    results[node.Id] = result;
                    sw.Stop();
                    result.Runtime = sw.ElapsedMilliseconds.ToString("f3");
                    OnProgress($"工具 {node.Name} 执行 {(result.Success ? "成功" : "失败")}");

                    if (!result.Success && !_config.ContinueOnFailure)
                    {
                        OnProgress($"工具 {node.Name} 执行失败，终止流程");
                        break;
                    }
                }
                catch (Exception ex)
                {
                    var errorResult = PluginResult.Error($"执行失败: {ex.Message}", ex);
                    results[node.Id] = errorResult;
                    sw.Stop();
                    errorResult.Runtime = sw.ElapsedMilliseconds.ToString("f3");
                    OnProgress($"工具 {node.Name} 执行异常: {ex.Message}");

                    if (!_config.ContinueOnFailure)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 触发进度事件
        /// </summary>
        /// <param name="message"></param>
        protected virtual void OnProgress(string message)
        {
            ExecutionProgress?.Invoke(this, message);
        }
        
    }
}
