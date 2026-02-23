using System.Text;
using AlertManagerWebhook.Models;

namespace AlertManagerWebhook.MessageBuilders;

public class DingtalkMessageBuilder : IMessageBuilder
{
    public object Build(AlertDetail alert)
    {
        var color = AlertSeverity.GetDingtalkColor(alert);
        var title = alert.IsFiring
            ? $"# <font color=\"{color}\">🚨 触发告警</font>\n"
            : $"# <font color=\"{color}\">✅ 告警恢复</font>\n";

        var sb = new StringBuilder();
        sb.AppendLine(title);
        sb.AppendLine($"> **告警名称：** <font color=\"#FFA500\">{alert.Name}</font>  ");
        sb.AppendLine($"> **告警状态：** <font color=\"{color}\">{alert.Severity}</font>  ");
        sb.AppendLine($"> **告警实例：** {alert.Instance}  ");
        if (!string.IsNullOrEmpty(alert.Host))
            sb.AppendLine($"> **主机名称：** {alert.Host}  ");
        if (!string.IsNullOrEmpty(alert.EnvName))
            sb.AppendLine($"> **环境名称：** {alert.EnvName}  ");
        if (!string.IsNullOrEmpty(alert.Project))
            sb.AppendLine($"> **项目名称：** {alert.Project}  ");
        sb.AppendLine($"> **触发时间：** {alert.StartsAt:yyyy-MM-dd HH:mm:ss zzz}  ");

        if (!alert.IsFiring)
        {
            sb.AppendLine($"> **恢复时间：** {alert.EndsAt:yyyy-MM-dd HH:mm:ss zzz}  ");
        }

        sb.AppendLine($"---");
        sb.AppendLine(alert.IsFiring ? alert.Description : $"原告警内容：{alert.Description}");

        return new DingtalkMessage
        {
            Markdown = new DingtalkMarkdown
            {
                Title = alert.IsFiring ? "触发告警" : "告警恢复",
                Text = sb.ToString().TrimEnd(),
            },
        };
    }
}
