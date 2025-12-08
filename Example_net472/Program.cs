using EasyPlugin.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Example_net472
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //Test1().GetAwaiter().GetResult();
            Test2().GetAwaiter().GetResult();
            Console.ReadLine();
        }
        async static Task Test1()
        {
            //日志事件注册
            PluginClient.SubscribeToLogs((msg)=> Console.WriteLine(msg));
            //插件创建
            var add = PluginClient.Create("加法1", typeof(AddPlugin));
            var multiply = PluginClient.Create("乘法1", typeof(MultiplyPlugin));
            //插件执行
            var add_result = await PluginClient.Run(
                async ()=> await add.ExecuteAsync(new PluginContext().SetData((2,3))));
            var multiply_result = await PluginClient.Run(
                async () => await multiply.ExecuteAsync(new PluginContext().SetData(((int)add_result.Data, 3))));

            Console.WriteLine($"最终结果：{multiply_result.Data}");
        }
        async static Task Test2()
        {
            //日志事件注册
            PluginClient.SubscribeToLogs((msg) => Console.WriteLine(msg));
            //插件创建
            var add1 = PluginClient.Create("加法1", typeof(AddPlugin));
            var add2 = PluginClient.Create("加法2", typeof(AddPlugin));
            var multiply = PluginClient.Create("乘法1", typeof(MultiplyPlugin));
            //插件执行,并行
            var add_result = await PluginClient.TogetherRun(
                async () => await add1.ExecuteAsync(new PluginContext().SetData((2, 3))),
                async () => await add2.ExecuteAsync(new PluginContext().SetData((4, 5))));

            var add1_data = (int) add_result[0].Data;
            var add2_data = (int) add_result[1].Data;
            Console.WriteLine($"加法1结果：{add1_data}");
            Console.WriteLine($"加法2结果：{add2_data}");

            var multiply_result = await PluginClient.Run(
                async () => await multiply.ExecuteAsync(
                    new PluginContext().SetData((add1_data, add2_data))));

            Console.WriteLine($"最终结果：{multiply_result.Data}");
        }
    }
}
