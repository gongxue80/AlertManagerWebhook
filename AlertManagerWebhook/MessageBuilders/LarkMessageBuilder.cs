using System.Text;
using AlertManagerWebhook.Models;

namespace AlertManagerWebhook.MessageBuilders;

public class LarkMessageBuilder : IMessageBuilder<LarkMessage>
{
    /// <summary>
    /// 构建 Lark 消息对象
    /// </summary>
    public LarkMessage? Build(Notification notification)
    {
        if (notification?.Alerts == null || notification.Alerts.Length == 0)
            return null;

        var alert = notification.Alerts[0];
        var isFiring = alert.Status == AlertStatus.Firing;
        var title = isFiring ? "🚨 告警触发" : "✅ 告警恢复";

        // 提取字段
        string alertName = alert.Labels.GetValueOrDefault("alertname", "未知");
        string severity = alert.Labels.GetValueOrDefault("severity", alert.Status.ToString());
        string instance = alert.Labels.GetValueOrDefault("instance", "未知");
        string host = alert.Labels.ContainsKey("host") ? alert.Labels["host"] : string.Empty;
        string description = alert.Annotations.GetValueOrDefault("description", "");
        string summary = alert.Annotations.GetValueOrDefault("summary", "");
        string details = string.IsNullOrEmpty(description) ? summary : description;

        // 用 StringBuilder 构建内容，分块插入
        var sb = new StringBuilder();
        sb.AppendLine($"**告警名称：** {alertName}");
        sb.AppendLine($"**告警状态：** {severity}");
        sb.AppendLine($"**告警实例：** {instance}");
        if (!string.IsNullOrEmpty(host))
            sb.AppendLine($"**主机名称：** {host}");
        sb.AppendLine($"**触发时间：** {alert.StartsAt:yyyy-MM-dd HH:mm:ss}");

        if (!isFiring)
        {
            sb.AppendLine($"**恢复时间：** {alert.EndsAt:yyyy-MM-dd HH:mm:ss}");
        }

        sb.AppendLine(isFiring ? details : $"原告警内容：{details}");

        // 构建 Lark 消息对象
        return new LarkMessage
        {
            Card = new LarkCard
            {
                Config = new LarkCardConfig { WideScreenMode = true },
                Header = new LarkCardHeader
                {
                    Title = new LarkCardHeaderTitle { Content = title },
                    Template = isFiring ? "red" : "green"
                },
                Elements =
                [
                    new LarkCardElement
                    {
                        Text = new LarkCardElementText { Content = sb.ToString().TrimEnd() }
                    }
                ]
            }
        };
    }
}
