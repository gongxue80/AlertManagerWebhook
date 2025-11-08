using AlertManagerWebhook.Models;

namespace AlertManagerWebhook.MessageBuilders;

public class DingtalkMessageBuilder : IMessageBuilder
{
    public object? Build(Notification notification)
    {
        if (notification?.Alerts == null || notification.Alerts.Length == 0)
            return null;

        var alert = notification.Alerts[0];
        var isFiring = alert.Status == "firing";
        var title = isFiring
            ? "# <font color=\"#FF0000\">🚨 触发告警</font>\n"
            : "# <font color=\"#008000\">✅ 告警恢复</font>\n";

        string alertName = alert.Labels.TryGetValue("alertname", out var name) ? name : "未知";
        string severity = alert.Labels.TryGetValue("serverity", out var sev) ? sev : "未知";
        string instance = alert.Labels.TryGetValue("instance", out var inst) ? inst : "未知";
        string host = alert.Labels.TryGetValue("host", out var h) ? h : "";
        string description = alert.Annotations.TryGetValue("description", out var desc) ? desc : "";
        string summary = alert.Annotations.TryGetValue("summary", out var s) ? s : "";
        string details = string.IsNullOrEmpty(description) ? summary : description;

        var sb = new System.Text.StringBuilder();
        sb.AppendLine(title);
        sb.AppendLine($"> **告警名称：** <font color=\"#FFA500\">{alertName}</font>  ");
        sb.AppendLine($"> **告警状态：** <font color=\"#FF0000\">{severity}</font>  ");
        sb.AppendLine($"> **告警实例：** {instance}  ");
        if (!string.IsNullOrEmpty(host))
            sb.AppendLine($"> **主机名称：** {host}  ");

        if (isFiring)
        {
            sb.AppendLine($"> **告警次数：** <font color=\"#FFA500\">{alert.Count}</font>  ");
            sb.AppendLine($"> **触发时间：** {alert.StartsAt:yyyy-MM-dd HH:mm:ss}  ");
        }
        else
        {
            sb.AppendLine($"> **开始时间：** {alert.StartsAt:yyyy-MM-dd HH:mm:ss}  ");
            sb.AppendLine($"> **恢复时间：** {alert.EndsAt:yyyy-MM-dd HH:mm:ss}  ");
        }

        sb.AppendLine($"---");
        sb.AppendLine(isFiring ? details : $"原告警内容：{details}");

        return new DingtalkMessage
        {
            Markdown = new DingtalkMarkdown
            {
                Title = isFiring ? "触发告警" : "告警恢复",
                Text = sb.ToString().TrimEnd()
            }
        };
    }
}
