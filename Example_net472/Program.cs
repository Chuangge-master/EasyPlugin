using EasyPlugin.Core;
using EasyPlugin.Exceptions;
using EasyPlugin.InternalPlugin;
using System;
using System.Threading.Tasks;

namespace Example_net472
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Test2().GetAwaiter().GetResult();
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

            // 连接节点
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
        async public static Task Test2()
        {
            // 准备执行上下文
            var context = new PluginContext();
            context.SetData("initial", "value");

            // 创建DAG管理器
            var dag = new PluginDag();

            // 创建工具节点
            var dataLoader = new DataProcessing
            {
                Id = "loader",
                Name = "数据加载器",
                InputKey = "initial",
                OutputKey = "rawData",
                ProcessingFunction = (input) => $"loader_{input}"
            };

            var dataProcessor1 = new DataProcessing
            {
                Id = "processor1",
                Name = "数据处理器1",
                InputKey = "rawData",
                OutputKey = "process1",
                ProcessingFunction = (input) => $"process1_{input}"
            };
            var dataProcessor2 = new DataProcessing
            {
                Id = "processor2",
                Name = "数据处理器2",
                InputKey = "rawData",
                OutputKey = "process2",
                ProcessingFunction = (input) => $"process2_{input}"
            };

            var dataCollector = new LambdaPlugin((ctx, output) =>
            {
                context.SetData(output, $"final_{ctx.GetData<string>("process1") }_{ctx.GetData<string>("process2")}");
            })
            {
                Id = "collector",
                Name = "数据收集器",
                OutputKey = "finalResult"
            };

            // 添加节点到DAG
            dag.AddNode(dataLoader);
            dag.AddNode(dataProcessor1);
            dag.AddNode(dataProcessor2);
            dag.AddNode(dataCollector);

            // 连接节点
            dag.ConnectNodes(dataLoader, dataProcessor1);
            dag.ConnectNodes(dataLoader, dataProcessor2);
            dag.ConnectNodes(dataProcessor1, dataCollector);
            dag.ConnectNodes(dataProcessor2, dataCollector);

            // 配置执行器
            var config = new ExecutionConfig
            {
                Mode = ExecutionMode.Parallel,
                MaxDegreeOfParallelism = 4,
                ContinueOnFailure = false
            };

            var executor = new PluginClient(dag, config);

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
