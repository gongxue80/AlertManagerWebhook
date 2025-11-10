using System.Text.Json;
using AlertManagerWebhook.Models;
using AlertManagerWebhook.MessageBuilders;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 注册 HttpClient 服务
builder.Services.AddHttpClient();

var app = builder.Build();

// 获取 HttpClient 实例
var httpClientFactory = app.Services.GetRequiredService<IHttpClientFactory>();
var httpClient = httpClientFactory.CreateClient();

var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("AlertManagerWebhook");

// 注册消息构建器
var messageConfig = new Dictionary<Receiver, (string UrlKey, string Name)>
{
    [Receiver.Lark] = ("LarkUrl", "Lark"),
    [Receiver.Dingtalk] = ("DingtalkUrl", "Dingtalk")
};

app.MapGet("/", () => "Welcome to AlertManager Webhook");
// Webhook 主处理接口，根据 receiver 类型分发
app.MapPost("/{receiver}", async (HttpContext context, string receiver, Notification notification) =>
{
    if (!Enum.TryParse<Receiver>(receiver, true, out var receiverEnum) || !messageConfig.ContainsKey(receiverEnum))
    {
        logger.LogWarning($"Unsupported receiver type: {receiver}");
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Unsupported receiver type");
        return;
    }
    if (notification == null || notification.Alerts == null || notification.Alerts.Length == 0)
    {
        logger.LogWarning("Invalid notification data");
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Invalid notification data");
        return;
    }

    var configuration = context.RequestServices.GetRequiredService<IConfiguration>();

    logger.LogInformation($"Received notification for {receiverEnum}, alerts count: {notification.Alerts.Length}");

    var (urlKey, name) = messageConfig[receiverEnum];
    var url = configuration[urlKey];
    if (string.IsNullOrEmpty(url))
    {
        logger.LogWarning($"{urlKey} 配置缺失或为空");
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync($"{urlKey} 配置缺失或为空");
        return;
    }

    object? message = receiverEnum switch
    {
        Receiver.Lark => new LarkMessageBuilder().Build(notification),
        Receiver.Dingtalk => new DingtalkMessageBuilder().Build(notification),
        _ => null
    };

    if (message is null)
    {
        logger.LogWarning($"Failed to build {name} message");
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync($"Failed to build {name} message");
        return;
    }

    logger.LogInformation($"Sending {name} message to {url}");
    var ok = await SendTo(url, message);
    logger.LogInformation(ok ? $"Message sent to {name} successfully" : $"Failed to send message to {name}");
    context.Response.StatusCode = ok ? 200 : 500;
    await context.Response.WriteAsync(ok ? $"Message sent to {name} successfully" : $"Failed to send message to {name}");
});

app.Run();

async Task<bool> SendTo<T>(string url, T message)
{
    try
    {
        var json = JsonSerializer.Serialize(message);
        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync(url, httpContent);
        if (!response.IsSuccessStatusCode)
        {
            var respContent = await response.Content.ReadAsStringAsync();
            logger.LogError($"Failed to send message to {url}. Status: {response.StatusCode}, Response: {respContent}");
        }
        return response.IsSuccessStatusCode;
    }
    catch (Exception ex)
    {
        logger.LogError(ex, $"Error sending message to {url}");
        return false;
    }
}
