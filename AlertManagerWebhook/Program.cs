using System.Text.Json;
using AlertManagerWebhook.Models;
using AlertManagerWebhook.MessageBuilders;
using System.Text;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add configuration
builder.Services.Configure<WebhookConfig>(builder.Configuration.GetSection("WebhookConfig"));

builder.Services.AddTransient<LarkMessageBuilder>();
builder.Services.AddTransient<DingtalkMessageBuilder>();
builder.Services.AddTransient<MessageBuilderFactory>();
builder.Services.AddHttpClient();

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("AlertManagerWebhook");

app.MapGet("/", () => "Welcome to AlertManager Webhook");
// Webhook 主处理接口，根据 receiver 类型分发
app.MapPost("/{receiver}/{token}", async (HttpContext context,
    string receiver,
    string token,
    Notification notification,
    HttpClient httpClient) =>
{
    try
    {
        // Perform validation
        var (isValid, receiverEnum) = await ValidateRequest(context, receiver, token, notification, logger);
        if (!isValid)
        {
            return;
        }

        logger.LogInformation("Received notification for {ReceiverEnum}, alerts count: {AlertCount}, token: {TokenMasked}",
            receiverEnum, notification.Alerts.Length, token.Substring(0, Math.Min(5, token.Length)) + "***");

        // Get logger from DI
        var httpLogger = context.RequestServices.GetRequiredService<ILogger<Program>>();

        // Get webhook configuration
        var webhookConfig = context.RequestServices.GetRequiredService<IOptions<WebhookConfig>>().Value;

        foreach (var alert in notification.Alerts)
        {
            var detail = BuildAlertDetail(alert);
            string? url = null;
            object message;
            var messageBuilderFactory = context.RequestServices.GetRequiredService<MessageBuilderFactory>();

            switch (receiverEnum)
            {
                case Receiver.Lark:
                    url = webhookConfig.LarkBaseUrl + token;
                    var larkBuilder = messageBuilderFactory.GetMessageBuilder<LarkMessage>(receiverEnum);
                    message = larkBuilder.Build(detail);
                    break;
                case Receiver.Dingtalk:
                    url = webhookConfig.DingtalkBaseUrl + token;
                    var dingtalkBuilder = messageBuilderFactory.GetMessageBuilder<DingtalkMessage>(receiverEnum);
                    message = dingtalkBuilder.Build(detail);
                    break;
                default:
                    logger.LogWarning("Unsupported receiver type: {ReceiverType}", receiverEnum);
                    return;
            }


            logger.LogInformation("Sending {Receiver} message to {Url}", receiver, url);
            var ok = await SendToAsync(url, message, httpClient, httpLogger);
            if (ok)
            {
                logger.LogInformation("Message sent to {Receiver} successfully", receiver);
            }
            else
            {
                logger.LogInformation("Failed to send message to {Receiver}", receiver);
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Unhandled exception while processing webhook");
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("Internal server error");
    }
});

app.Run();

static async Task<(bool IsValid, Receiver ReceiverEnum)> ValidateRequest(HttpContext context, string receiver, string token, Notification notification, ILogger logger)
{
    // Validate receiver type
    if (!Enum.TryParse<Receiver>(receiver, true, out var receiverEnum))
    {
        logger.LogWarning("Unsupported receiver type: {Receiver}", receiver);
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Unsupported receiver type");
        return (false, default);
    }

    // Validate notification data
    if (notification == null || notification.Alerts == null || notification.Alerts.Length == 0)
    {
        logger.LogWarning("Invalid notification data");
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Invalid notification data");
        return (false, default);
    }

    // Validate token
    if (string.IsNullOrWhiteSpace(token))
    {
        logger.LogWarning("Empty token parameter");
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Token parameter is required");
        return (false, default);
    }

    return (true, receiverEnum);
}

static async Task<bool> SendToAsync<T>(string url, T message, HttpClient httpClient, ILogger logger)
{
    if (string.IsNullOrEmpty(url))
    {
        throw new ArgumentException("URL cannot be null or empty", nameof(url));
    }

    if (message is null)
    {
        throw new ArgumentNullException(nameof(message), "Message cannot be null");
    }

    try
    {
        var json = JsonSerializer.Serialize(message);
        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync(url, httpContent);
        if (!response.IsSuccessStatusCode)
        {
            var respContent = await response.Content.ReadAsStringAsync();
            logger.LogError("Failed to send message to {Url}. Status: {StatusCode}, Response: {ResponseContent}", url, response.StatusCode, respContent);
        }
        return response.IsSuccessStatusCode;
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error sending message to {Url}", url);
        return false;
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

// Add this class to make it accessible for testing
public partial class Program { }
