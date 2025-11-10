using AlertManagerWebhook.Models;

namespace AlertManagerWebhook.MessageBuilders;

public class DingtalkMessageBuilder : IMessageBuilder<DingtalkMessage>
{
    public DingtalkMessage? Build(AlertDetail alert)
    {
        var title = alert.IsFiring
            ? "# <font color=\"#FF0000\">🚨 触发告警</font>\n"
            : "# <font color=\"#008000\">✅ 告警恢复</font>\n";

        var sb = new System.Text.StringBuilder();
        sb.AppendLine(title);
        sb.AppendLine($"> **告警名称：** <font color=\"#FFA500\">{alert.Name}</font>  ");
        sb.AppendLine($"> **告警状态：** <font color=\"#FF0000\">{alert.Severity}</font>  ");
        sb.AppendLine($"> **告警实例：** {alert.Instance}  ");
        if (!string.IsNullOrEmpty(alert.Host))
            sb.AppendLine($"> **主机名称：** {alert.Host}  ");
        sb.AppendLine($"> **触发时间：** {alert.StartsAt:yyyy-MM-dd HH:mm:ss}  ");

        if (!alert.IsFiring)
        {
            sb.AppendLine($"> **恢复时间：** {alert.EndsAt:yyyy-MM-dd HH:mm:ss}  ");
        }

        sb.AppendLine($"---");
        sb.AppendLine(alert.IsFiring ? alert.Description : $"原告警内容：{alert.Description}");

        return new DingtalkMessage
        {
            Markdown = new DingtalkMarkdown
            {
                Title = alert.IsFiring ? "触发告警" : "告警恢复",
                Text = sb.ToString().TrimEnd()
            }
        };
    }
}
