using EasyPlugin.Core;
using EasyPlugin.DataValidates;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Example_net472
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //日志事件注册
            PluginClient.RegisterLogHandler((msg) => Console.WriteLine(msg));
            Test1().GetAwaiter().GetResult();
            Test2().GetAwaiter().GetResult();
            Console.ReadLine();
        }
        async static Task Test1()
        {
            //插件创建
            var add = PluginClient.Create("加法1", typeof(AddPlugin));
            var multiply = PluginClient.Create("乘法1", typeof(MultiplyPlugin),500, validate: new TypeValidate(typeof(int[])));
            //插件执行
            var add_result = await PluginClient.Run(add, new int[] { 2, 3 });//参数直接传数据,也可以传PluginContext对象
            var multiply_result = await PluginClient.Run(multiply, new int[] { (int)add_result.Data, 5 });

            Console.WriteLine($"最终结果：{multiply_result.Data}");
        }
        async static Task Test2()
        {
            //插件创建
            var add1 = PluginClient.Create("加法1", typeof(AddPlugin));
            var add2 = PluginClient.Create("加法2", typeof(AddPlugin));
            var multiply = PluginClient.Create("乘法1", typeof(MultiplyPlugin));
            //插件执行,并行
            var add_result = await PluginClient.TogetherRun(new List<PluginProxy> { add1, add2 },new List<object> {
                new int[] { 2, 3 }, new int[] { 4, 5 } });

            var add1_data = (int) add_result[0].Data;
            var add2_data = (int) add_result[1].Data;
            Console.WriteLine($"加法1结果：{add1_data}");
            Console.WriteLine($"加法2结果：{add2_data}");
            
            var multiply_result = await PluginClient.Run(multiply, new int[] { add1_data, add2_data });

            Console.WriteLine($"最终结果：{multiply_result.Data}");
        }
    }
}
