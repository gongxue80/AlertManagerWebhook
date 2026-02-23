using System.Text;
using System.Text.Json;
using AlertManagerWebhook.MessageBuilders;
using AlertManagerWebhook.Models;
using Microsoft.Extensions.Options;

namespace AlertManagerWebhook;

public static class WebhookHandlers
{
    public static async Task HandleWebhookAsync(
        HttpContext context,
        string receiver,
        string token,
        Notification notification,
        HttpClient httpClient
    )
    {
        var logger = context
            .RequestServices.GetRequiredService<ILoggerFactory>()
            .CreateLogger("AlertManagerWebhook");

        // Validate request
        if (!ValidateReceiver(receiver, out var receiverEnum))
        {
            logger.LogWarning("Unsupported receiver type: {Receiver}", receiver);
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Unsupported receiver type");
            return;
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            logger.LogWarning("Empty token parameter");
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Token parameter is required");
            return;
        }

        if (notification.Alerts.Length == 0)
        {
            logger.LogWarning("Invalid notification data");
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Invalid notification data");
            return;
        }

        var webhookConfig = context
            .RequestServices.GetRequiredService<IOptions<WebhookConfig>>()
            .Value;
        var httpLogger = context
            .RequestServices.GetRequiredService<ILoggerFactory>()
            .CreateLogger("WebhookHandlers");

        logger.LogInformation(
            "Received notification for {ReceiverEnum}, alerts count: {AlertCount}, token: {TokenMasked}",
            receiverEnum,
            notification.Alerts.Length,
            token.Substring(0, Math.Min(5, token.Length)) + "***"
        );

        foreach (var alert in notification.Alerts)
        {
            var detail = BuildAlertDetail(alert);
            var (url, message) = BuildMessage(
                receiverEnum,
                token,
                detail,
                webhookConfig,
                context.RequestServices
            );

            logger.LogInformation("Sending {Receiver} message", receiver);
            var ok = await SendToAsync(url, message, httpClient, httpLogger);
            logger.LogInformation(
                ok
                    ? "Message sent to {Receiver} successfully"
                    : "Failed to send message to {Receiver}",
                receiver
            );
        }
    }

    static bool ValidateReceiver(string receiver, out Receiver receiverEnum) =>
        Enum.TryParse(receiver, true, out receiverEnum);

    static (string Url, object Message) BuildMessage(
        Receiver receiver,
        string token,
        AlertDetail alert,
        WebhookConfig config,
        IServiceProvider services
    )
    {
        IMessageBuilder builder = receiver switch
        {
            Receiver.Lark => services.GetRequiredService<LarkMessageBuilder>(),
            Receiver.Dingtalk => services.GetRequiredService<DingtalkMessageBuilder>(),
            _ => throw new NotSupportedException($"Unsupported receiver type: {receiver}"),
        };

        var url = receiver switch
        {
            Receiver.Lark => config.LarkBaseUrl + token,
            Receiver.Dingtalk => config.DingtalkBaseUrl + token,
            _ => throw new NotSupportedException($"Unsupported receiver type: {receiver}"),
        };

        return (url, builder.Build(alert));
    }

    static async Task<bool> SendToAsync<T>(
        string url,
        T message,
        HttpClient httpClient,
        ILogger logger
    )
    {
        try
        {
            var json = JsonSerializer.Serialize(message);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var respContent = await response.Content.ReadAsStringAsync();
                logger.LogError(
                    "Failed to send message to {Url}. Status: {StatusCode}, Response: {ResponseContent}",
                    url,
                    response.StatusCode,
                    respContent
                );
            }
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending message to {Url}", url);
            return false;
        }
    }

    static AlertDetail BuildAlertDetail(Alert alert) =>
        new()
        {
            IsFiring = alert.Status == AlertStatus.Firing,
            Name = alert.Labels.GetValueOrDefault("alertname", "未知"),
            Severity =
                alert.Status == AlertStatus.Firing
                    ? alert.Labels.GetValueOrDefault("severity", string.Empty)
                    : "normal",
            EnvName = alert.Labels.GetValueOrDefault("env", string.Empty),
            Project = alert.Labels.GetValueOrDefault("project", string.Empty),
            Instance = alert.Labels.GetValueOrDefault("instance", "未知"),
            Host = alert.Labels.GetValueOrDefault("host", string.Empty),
            Description = string.IsNullOrEmpty(
                alert.Annotations.GetValueOrDefault("description", string.Empty)
            )
                ? alert.Annotations.GetValueOrDefault("summary", string.Empty)
                : alert.Annotations.GetValueOrDefault("description", string.Empty),
            StartsAt = alert.StartsAt.ToLocalTime(),
            EndsAt = alert.EndsAt.ToLocalTime(),
        };
}
