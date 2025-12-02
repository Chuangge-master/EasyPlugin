using EasyPlugin.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example_net472
{
    public class HttpRequestPlugin : PluginBase
    {
        public string Url { get; set; }
        public string Method { get; set; } = "GET";
        public string ResponseKey { get; set; }

        public override async Task<PluginResult> ExecuteAsync(PluginContext context)
        {
            try
            {
                using (var client = new System.Net.Http.HttpClient())
                {
                    var response = await client.GetAsync(Url);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        context.SetData(ResponseKey, content);
                        return PluginResult.Ok($"HTTP请求成功，状态码: {(int)response.StatusCode}");
                    }
                    else
                    {
                        return PluginResult.Error($"HTTP请求失败，状态码: {(int)response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                return PluginResult.Error($"HTTP请求异常: {ex.Message}", ex);
            }
        }
    }
}
