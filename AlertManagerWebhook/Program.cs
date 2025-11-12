using System.Text.Json;
using AlertManagerWebhook.Models;
using AlertManagerWebhook.MessageBuilders;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddTransient<LarkMessageBuilder>();
builder.Services.AddTransient<DingtalkMessageBuilder>();
builder.Services.AddHttpClient();

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("AlertManagerWebhook");

app.MapGet("/", () => "Welcome to AlertManager Webhook");
// Webhook 主处理接口，根据 receiver 类型分发
app.MapPost("/{receiver}/{token}", async (HttpContext context, string receiver, string token, Notification notification, HttpClient httpClient) =>
{
    if (!Enum.TryParse<Receiver>(receiver, true, out var receiverEnum))
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
    logger.LogInformation($"Received notification for {receiverEnum}, alerts count: {notification.Alerts.Length}");

    foreach (var alert in notification.Alerts)
    {
        var detail = BuildAlertDetail(alert);
        string? url = null;
        object? message = null;
        switch (receiverEnum)
        {
            case Receiver.Lark:
                url = $"https://open.feishu.cn/open-apis/bot/v2/hook/{token}";
                var larkBuilder = context.RequestServices.GetRequiredService<LarkMessageBuilder>();
                message = larkBuilder.Build(detail);
                break;
            case Receiver.Dingtalk:
                url = $"https://oapi.dingtalk.com/robot/send?access_token={token}";
                var dingtalkBuilder = context.RequestServices.GetRequiredService<DingtalkMessageBuilder>();
                message = dingtalkBuilder.Build(detail);
                break;
            default:
                url = null;
                message = null;
                break;
        }
        if (string.IsNullOrEmpty(url))
        {
            logger.LogWarning($"Unsupported receiver type: {receiverEnum}");
            return;
        }

        if (message is null)
        {
            logger.LogWarning($"Failed to build {receiver} message");
            return;
        }

        logger.LogInformation($"Sending {receiver} message to {url}");
        var ok = await SendToAsync(url, message, httpClient);
        logger.LogInformation(ok ? $"Message sent to {receiver} successfully" : $"Failed to send message to {receiver}");
    }
});

app.Run();

static async Task<bool> SendToAsync<T>(string url, T message, HttpClient httpClient)
{
    if (string.IsNullOrEmpty(url))
    {
        throw new ArgumentException("URL cannot be null or empty", nameof(url));
    }

    if (message is null)
    {
        throw new ArgumentNullException(nameof(message), "Message cannot be null");
    }

    var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("AlertManagerWebhook");
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
}

static AlertDetail BuildAlertDetail(Alert alert) => new AlertDetail
{
    IsFiring = alert.Status == AlertStatus.Firing,
    Name = alert.Labels.GetValueOrDefault("alertname", "未知"),
    Severity = alert.Status == AlertStatus.Firing ? alert.Labels.GetValueOrDefault("severity", string.Empty) : "normal",
    EnvName = alert.Labels.GetValueOrDefault("env", string.Empty),
    Project = alert.Labels.GetValueOrDefault("project", string.Empty),
    Instance = alert.Labels.GetValueOrDefault("instance", "未知"),
    Host = alert.Labels.GetValueOrDefault("host", string.Empty),
    Description = string.IsNullOrEmpty(alert.Annotations.GetValueOrDefault("description", string.Empty))
    ? alert.Annotations.GetValueOrDefault("summary", string.Empty)
    : alert.Annotations.GetValueOrDefault("description", string.Empty),
    StartsAt = alert.StartsAt.ToLocalTime(),
    EndsAt = alert.EndsAt.ToLocalTime()
};
