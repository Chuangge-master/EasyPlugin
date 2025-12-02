using EasyPlugin.Core;
using EasyPlugin.Exceptions;
using EasyPlugin.InternalPlugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EasyPlugin.InternalPlugin;

namespace Example_net472
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Test().GetAwaiter().GetResult();
            Console.ReadLine();

        }
        async public static Task Test()
        {
            // 创建DAG管理器
            var dag = new PluginDag();

            // 创建工具节点
            var dataLoader = new DataProcessing
            {
                Id = "loader",
                Name = "数据加载器",
                InputKey = "initial",
                OutputKey = "rawData",
                ProcessingFunction = (input) => $"processed_{input}"
            };

            var httpTool = new HttpRequestPlugin
            {
                Id = "http",
                Name = "API请求工具",
                Url = " https://api.example.com/data ",
                ResponseKey = "apiResponse"
            };

            var dataProcessor = new DataProcessing
            {
                Id = "processor",
                Name = "数据处理器",
                InputKey = "rawData",
                OutputKey = "finalResult",
                ProcessingFunction = (input) => $"final_{input}"
            };

            var conditionalTool = new Conditional
            {
                Id = "condition",
                Name = "条件判断工具",
                ConditionKey = "apiResponse",
                Condition = (data) => data != null && data.ToString().Length > 10
            };

            // 添加节点到DAG
            dag.AddNode(dataLoader);
            dag.AddNode(httpTool);
            dag.AddNode(dataProcessor);
            dag.AddNode(conditionalTool);

            // 建立依赖关系
            dag.ConnectNodes(dataLoader, dataProcessor);
            dag.ConnectNodes(httpTool, conditionalTool);

            // 配置执行器
            var config = new ExecutionConfig
            {
                Mode = ExecutionMode.Parallel,
                MaxDegreeOfParallelism = 4,
                ContinueOnFailure = false
            };

            var executor = new PluginClient(dag, config);

            // 准备执行上下文
            var context = new PluginContext();
            context.SetData("initial", "start_data");

            try
            {
                //打印图中工具信息
                Console.WriteLine("\n=== 图中工具信息 ===");
                foreach (var info in dag.GetGraphNodeInofs())
                {
                    Console.WriteLine(info);
                }
                ///打印图结构
                Console.WriteLine("\n=== 图结构 ===");
                Console.WriteLine(dag.ToString());

                // 执行工具
                var results = await executor.ExecuteAsync(context);

                //输入执行日志
                Console.WriteLine("\n=== 执行日志 ===");
                Console.WriteLine(executor.ExecutionLog);
                // 输出执行结果
                Console.WriteLine("\n=== 执行结果汇总 ===");
                foreach (var result in results)
                {
                    Console.WriteLine($"{result.Key}: {(result.Value.Success ? "成功" : "失败")} - {result.Value.Message}");
                }

                // 输出最终数据
                Console.WriteLine("\n=== 上下文数据 ===");
                Console.WriteLine($"最终结果: {context.GetData<string>("finalResult")}");
            }
            catch (CyclicDependencyException ex)
            {
                Console.WriteLine($"编排错误: {ex.Message}");
            }
        }
    }
}
