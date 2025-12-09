# EasyPlugin 一个插件编排服务器

> 这个库旨在实现一个可以进行插件编排和运行的功能，让使用者只关注插件的制作和编排过程，无需考虑异常处理，日志输出，超时处理，数据校验等。

# 教程

## 自定义插件类

```csharp
//需要实现IPlugin接口，以下是示例
public class AddPlugin : IPlugin
{
    public string Name { get; set; } = "AddPlugin";

    async public Task<PluginContext> ExecuteAsync(PluginContext context)
    {
        await Task.Delay(1000);
        var data = (ValueTuple<int, int>)context.Data;
        var result = new PluginContext();
        result.Data = data.Item1 + data.Item2;
        return result;
    }
}
```

## 插件实例创建

```csharp
//插件实例的创建不使用new的方式，而是PluginClient.Create（name,type,timeout（可选，单位：毫秒，默认3000毫秒）, validate（可选）），这个方式创建出来的是一个插件代理类
var add1 = PluginClient.Create("加法1", typeof(AddPlugin));
//还可以设定超时时间和数据验证，其中TypeValidate是内置的一个类型校验，可以检验输入的context的数据是否是指定类型
var add1 = PluginClient.Create("加法1", typeof(AddPlugin), 3000, new TypeValidate(typeof(ValueTuple<int, int>)));
```

## 插件串行运行

```csharp
var add_result = await PluginClient.Run(
    async ()=> await add.ExecuteAsync(new PluginContext().SetData((2,3))));
var multiply_result = await PluginClient.Run(
    async () => await multiply.ExecuteAsync(new PluginContext().SetData(((int)add_result.Data, 3))));
```

## 插件并行运行

```csharp
var add_result = await PluginClient.TogetherRun(
    async () => await add1.ExecuteAsync(new PluginContext().SetData((2, 3))),
    async () => await add2.ExecuteAsync(new PluginContext().SetData((4, 5))));
```

## 运行日志添加事件注册

```csharp
PluginClient.RegisterLogHandler((msg)=> Console.WriteLine(msg)); // 运行日志输出到命令行窗口
```